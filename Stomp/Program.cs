using System;
using System.Drawing;

using Stomp.Filters;

namespace Stomp
{
    class MainClass
    {
        public static FilterChain Default = new FilterChain();

        public static void Main(string[] args)
        {
            FastBitmap bmp = new FastBitmap("/home/hexafluoride/test.jpg");
            bmp.Lock();

            Console.WriteLine("Opened test.jpg with size {0}({2})x{1}", bmp.Width, bmp.Height, bmp.Subwidth);

            Default.Append(
                new ScanLines() { PreserveBrightness = true },
                new ChromaShift() { RedShift = -10, GreenShift = 10, BlueShift = 30 },
                new RandomGaps() { GapCount = 30, RandomBehavior = true, MinGapLength = -100, MaxGapLength = 100 },
                new Saturation() { Intensity = 2 }
            );

            Default.Apply(bmp);

            bmp.Save("/home/hexafluoride/gapped.jpg");
        }

        public static void TestColor(FastBitmap bmp, int x, int y)
        {
            Color clr = bmp.GetPixel(x, y);

            Console.WriteLine("({0},{1}) is {2}, {3}, {4}", x, y, clr.R, clr.G, clr.B);
        }
    }
}
