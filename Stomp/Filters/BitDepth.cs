using System;
using System.Diagnostics;

using Stomp;

namespace Stomp.Filters
{
    public class BitDepth : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "bit depth reduction"; } }
        public string ScriptName { get { return "bit-depth"; } }

        public event FilterMessageHandler OnMessage;

        public int BitsPerChannel { get; set; }

        public BitDepth()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            int snap = (256 / (int)Math.Pow(2, BitsPerChannel - 1)) - 1;

            for (int i = 0; i < bmp.Data.Length; i++)
            {
                bmp.Data[i] = (byte)(bmp.Data[i] - (bmp.Data[i] % snap));
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

