using System;
using System.Drawing;
using System.IO;

using Stomp.Filters;
using Stomp.Filters.Contexts;

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
                new PngFiltered(new FilterChain(
                    new RandomBytes() { Rate = 0.0002 }
                )) { Behavior = FilterTypeGen.Constant, ConstantType = FilterType.Sub },

                new PngFiltered(new FilterChain(
                    new RandomBytes() { Rate = 0.0001 }
                )) { Behavior = FilterTypeGen.Constant, ConstantType = FilterType.Average },

                new PngFiltered(new FilterChain(
                    new RandomBytes() { Rate = 0.00001 }
                )) { Behavior = FilterTypeGen.Random }
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
