using System;

using System.Drawing;

namespace Stomp
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            FastBitmap bmp = new FastBitmap("/home/hexafluoride/test.jpg");
            bmp.Lock();

            Console.WriteLine("Opened test.jpg with size {0}({2})x{1}", bmp.Width, bmp.Height, bmp.Subwidth);

            TestColor(bmp, 0, 0);
            TestColor(bmp, 100, 200);
        }

        public static void TestColor(FastBitmap bmp, int x, int y)
        {
            Color clr = bmp.GetPixel(x, y);

            Console.WriteLine("{0}x{1} is {2}, {3}, {4}", x, y, clr.R, clr.G, clr.B);
        }
    }
}
