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
        public bool RandomBehavior { get; set; }

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

            foreach (var gap in gaps)
            {
                if (RandomBehavior)
                    Behavior = (GapBehavior)rnd.Next(Enum.GetNames(typeof(GapBehavior)).Length);

                bmp.CreateGap(gap.Key, gap.Value, Behavior);
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

