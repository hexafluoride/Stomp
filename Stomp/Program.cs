﻿using System;
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

            ScriptEngine script = new ScriptEngine();
            script.Register(new RandomGaps());
            script.Register(new BitDepth());
            script.Register(new ChromaShift());
            script.Register(new RandomBytes());
            script.Register(new Saturation());
            script.Register(new ScanLines());
            script.Register(new PngFiltered());

            Default = script.Parse(File.ReadAllText(args[1]));

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
