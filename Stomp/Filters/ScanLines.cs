using System;
using System.Drawing;

using Stomp;

namespace Stomp.Filters
{
    public class ScanLines : IFilter
    {
        public ScanLines()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y += 2)
            {
                for (int x = 0; x < bmp.Width; x++)
                    bmp.SetPixel(x, y, Color.Black);
            }
        }
    }
}

