using System;
using System.Diagnostics;

using Stomp;

namespace Stomp.Filters
{
    public class BitDepth : IFilter
    {
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
    }
}

