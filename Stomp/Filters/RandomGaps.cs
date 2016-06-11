using System;
using System.Collections.Generic;

using Stomp;

namespace Stomp.Filters
{
    public class RandomGaps : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "random gaps"; } }
        public string ScriptName { get { return "random-gaps"; } }

        public event FilterMessageHandler OnMessage;

        public int GapCount { get; set; }

        public int MinGapLength { get; set; }
        public int MaxGapLength { get; set; }

        public GapBehavior Behavior { get; set; }

        public RandomGaps()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            Random rnd = new Random();

            List<KeyValuePair<int, int>> gaps = new List<KeyValuePair<int, int>>();

            for(int i = 0; i < GapCount; i++)
            {
                gaps.Add(new KeyValuePair<int, int>(
                        rnd.Next(bmp.Data.Length), 
                        rnd.Next(MinGapLength, MaxGapLength)));
            }

            bool random = (Behavior == GapBehavior.Random);

            foreach (var gap in gaps)
            {
                if (random)
                    Behavior = (GapBehavior)rnd.Next(Enum.GetNames(typeof(GapBehavior)).Length - 1); // skip Random

                CreateGap(bmp, gap.Key, gap.Value, Behavior);
            }
        }

        public void CreateGap(FastBitmap bitmap, int start, int length, GapBehavior behavior = GapBehavior.RandomBytes)
        {
            byte[] gapped = new byte[Math.Abs(length)];

            int gap_pos = 0;

            if (length > 0)
            {
                Array.Copy(bitmap.Data, start, gapped, 0, length);
                Array.Copy(bitmap.Data, (start + length), bitmap.Data, start, bitmap.Data.Length - (start + length));

                gap_pos = bitmap.Data.Length - length;
            }
            else
            {
                length = Math.Abs(length);

                Buffer.BlockCopy(bitmap.Data, bitmap.Data.Length - start, gapped, 0, length);
                Buffer.BlockCopy(bitmap.Data, start, bitmap.Data, (start + length), bitmap.Data.Length - (start + length));

                gap_pos = start;
            }

            switch (behavior)
            {
                case GapBehavior.Black:
                    for (int i = gap_pos; i < gap_pos + length; i++)
                        bitmap.Data[i] = 0;

                    break;
                case GapBehavior.White:
                    for (int i = gap_pos; i < gap_pos + length; i++)
                        bitmap.Data[i] = 255;

                    break;
                case GapBehavior.RandomBytes:
                    byte[] rnd = new byte[length];
                    Random.NextBytes(rnd);

                    Array.Copy(rnd, 0, bitmap.Data, gap_pos, length);
                    break;
                case GapBehavior.Gapped:
                    Array.Copy(gapped, 0, bitmap.Data, gap_pos, length);
                    break;
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }

    public enum GapBehavior
    {
        Black, White, RandomBytes, Gapped, Random
    }
}

