using System;
using System.Drawing;
using System.IO;

using Stomp.Filters;

namespace Stomp
{
    class MainClass
    {
        public static FilterChain Default = new FilterChain();

        public static void Main(string[] args)
        {
            FastBitmap bmp = new FastBitmap(args[0]);
            bmp.Lock();

            Console.WriteLine("Opened {0} with size {1}({3})x{2}", Path.GetFileName(args[0]), bmp.Width, bmp.Height, bmp.Subwidth);

            Default.Append(
                new BitDepth() { BitsPerChannel = 3 },
                new ScanLines() { PreserveBrightness = true },
                new ChromaShift() { RedShift = -10, GreenShift = 10, BlueShift = 30 },
                new RandomGaps() { GapCount = 30, RandomBehavior = true, MinGapLength = -100, MaxGapLength = 100 },
                new Saturation() { Intensity = 2 }
            );

            Default.Apply(bmp);

            bmp.Save(args[0] + "-glitched.png");
        }

        public static void TestColor(FastBitmap bmp, int x, int y)
        {
            Color clr = bmp.GetPixel(x, y);

            Console.WriteLine("({0},{1}) is {2}, {3}, {4}", x, y, clr.R, clr.G, clr.B);
        }
    }
}
