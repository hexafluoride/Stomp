using System;

using Stomp;

namespace Stomp.Filters
{
    public class RandomBytes : IFilter
    {
        public double Ratio { get; set; } 
        public RandomBytes()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            Random rnd = new Random();
            int amount = (int)(bmp.Data.Length * Ratio);

            while (amount-- > 0)
            {
                bmp.Data[rnd.Next(bmp.Data.Length)] = (byte)rnd.Next(256);
            }
        }
    }
}

