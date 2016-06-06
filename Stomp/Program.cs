using System;

using System.Drawing;

using Stomp.Filters;

namespace Stomp
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            FastBitmap bmp = new FastBitmap("/home/hexafluoride/test.jpg");
            bmp.Lock();

            Console.WriteLine("Opened test.jpg with size {0}({2})x{1}", bmp.Width, bmp.Height, bmp.Subwidth);

            ScanLines scan = new ScanLines();
            scan.Apply(bmp);

            ChromaShift filter = new ChromaShift() { RedShift = -10, GreenShift = 10, BlueShift = 30 };
            filter.Apply(bmp);

            for (int i = 0; i < bmp.Data.Length; i += 100000)
            {
                bmp.CreateGap(i, 10000, GapBehavior.Gapped);
            }

            bmp.Save("/home/hexafluoride/gapped.jpg");
        }

        public static void TestColor(FastBitmap bmp, int x, int y)
        {
            Color clr = bmp.GetPixel(x, y);

            Console.WriteLine("({0},{1}) is {2}, {3}, {4}", x, y, clr.R, clr.G, clr.B);
        }
    }
}
