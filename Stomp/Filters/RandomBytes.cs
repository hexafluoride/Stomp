using System;

using Stomp;

namespace Stomp.Filters
{
    public class RandomBytes : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "random bytes"; } }
        public string ScriptName { get { return "random-bytes"; } }

        public event FilterMessageHandler OnMessage;

        public double Rate { get; set; } 
        public RandomBytes()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            Random rnd = new Random();
            int amount = (int)(bmp.Data.Length * Rate);

            while (amount-- > 0)
            {
                bmp.Data[rnd.Next(bmp.Data.Length)] = (byte)rnd.Next(256);
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

