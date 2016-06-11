using System;
using System.Collections.Generic;

using Stomp;

namespace Stomp.Filters
{
    public class ChromaShift : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "chroma shifter"; } }
        public string ScriptName { get { return "chroma-shift"; } }

        public event FilterMessageHandler OnMessage;

        [ScriptAlias("red-shift")]
        public int RedShift { get; set; }
        [ScriptAlias("green-shift")]
        public int GreenShift { get; set; }
        [ScriptAlias("blue-shift")]
        public int BlueShift { get; set; }

        public ChromaShift()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            var channels = bmp.SeparateChannels();

            var r_channel = channels["R"];
            var g_channel = channels["G"];
            var b_channel = channels["B"];

            byte[] temp = new byte[r_channel.Length];

            int r_offset = RedShift % r_channel.Length;
            int g_offset = GreenShift % g_channel.Length;
            int b_offset = BlueShift % b_channel.Length;

            Shift(r_channel, r_offset);
            Shift(g_channel, g_offset);
            Shift(b_channel, b_offset);

            bmp.WriteChannels(new Dictionary<string, byte[]>()
            {
                {"R", r_channel},
                {"G", g_channel},
                {"B", b_channel}
            });
        }

        private void Shift(byte[] array, int offset)
        {
            byte[] temp = new byte[array.Length];

            if (offset > 0)
            {
                Buffer.BlockCopy(array, 0, temp, offset, array.Length - offset);
                Buffer.BlockCopy(array, array.Length - offset, temp, 0, offset);
                Buffer.BlockCopy(temp, 0, array, 0, array.Length);
            }
            else
            {
                offset = Math.Abs(offset);

                Buffer.BlockCopy(array, offset, temp, 0, array.Length - offset);
                Buffer.BlockCopy(array, 0, temp, array.Length - offset, offset);
                Buffer.BlockCopy(temp, 0, array, 0, array.Length);
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

