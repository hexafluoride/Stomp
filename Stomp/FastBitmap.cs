using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Stomp
{
    public class FastBitmap
    {
        private Bitmap InternalBitmap;
        private BitmapData Handle;

        public byte[] Data;
        public bool Locked { get; set; }

        public int Width
        {
            get
            {
                return InternalBitmap.Width;
            }
        }

        public int Height
        {
            get
            {
                return InternalBitmap.Height;
            }
        }

        public int Subwidth
        {
            get
            {
                return Handle.Stride;
            }
        }

        public FastBitmap(string path)
        {
            InternalBitmap = new Bitmap(path);
        }

        public void Lock()
        {
            if (Locked)
                return;
        
            Handle = InternalBitmap.LockBits(
                new Rectangle(Point.Empty, InternalBitmap.Size), 
                ImageLockMode.ReadWrite, 
                InternalBitmap.PixelFormat);

            Data = new byte[Handle.Stride * InternalBitmap.Height];
            Marshal.Copy(Handle.Scan0, Data, 0, Data.Length);

            Locked = true;
        }

        public void Unlock(bool apply = true)
        {
            if(!Locked)
                return;

            if (apply)
                Marshal.Copy(Data, 0, Handle.Scan0, Data.Length);

            InternalBitmap.UnlockBits(Handle);

            Locked = false;
        }

        public Bitmap GetSnapshot()
        {
            if (Data == null || Data.Length == 0)
                throw new Exception("Buffer is empty");

            Bitmap bmp = new Bitmap(InternalBitmap);

            var handle = bmp.LockBits(
                 new Rectangle(Point.Empty, bmp.Size),
                 ImageLockMode.WriteOnly,
                 bmp.PixelFormat);
            
            Marshal.Copy(Data, 0, handle.Scan0, Data.Length);

            bmp.UnlockBits(handle);

            return bmp;
        }

        public Color GetPixel(int x, int y)
        {
            switch (InternalBitmap.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                    {
                        int index = (y * Handle.Stride) + (x * 2); // 16bpp, 2 bytes

                        byte first_half = Data[index]; // contains 1 alpha bit + 5 bits of red + 2 bits of green
                        byte second_half = Data[index + 1]; // contains 3 bits of green + 5 bits of blue

                        return Color.FromArgb(
                            (first_half & 0x80) == 0x80 ? 255 : 0,
                            (first_half & 0x7C) << 3,
                            (((first_half & 0x03) << 3) + ((second_half & 0xE0) >> 5)) << 3,
                            (second_half & 0x1F) << 3);
                    }
                case PixelFormat.Format16bppGrayScale:
                    {
                        int index = (y * Handle.Stride) + (x * 2); // 16bpp, 2 bytes

                        byte first_half = Data[index];

                        return Color.FromArgb(first_half, first_half, first_half);
                    }
                case PixelFormat.Format16bppRgb555:
                    {
                        int index = (y * Handle.Stride) + (x * 2); // 16bpp, 2 bytes

                        byte first_half = Data[index]; // contains 1 unused bit + 5 bits of red + 2 bits of green
                        byte second_half = Data[index + 1]; // contains 3 bits of green + 5 bits of blue

                        return Color.FromArgb(
                            (first_half & 0x7C) << 3,
                            (((first_half & 0x03) << 3) + ((second_half & 0xE0) >> 5)) << 3,
                            (second_half & 0x1F) << 3);
                    }
                case PixelFormat.Format16bppRgb565:
                    {
                        int index = (y * Handle.Stride) + (x * 2); // 16bpp, 2 bytes

                        byte first_half = Data[index]; // contains 5 bits of red + 3 bits of green
                        byte second_half = Data[index + 1]; // contains 3 bits of green + 5 bits of blue

                        return Color.FromArgb(
                            (first_half & 0xF8) << 3,
                            (((first_half & 0x07) << 3) + ((second_half & 0xE0) >> 5)) << 2,
                            (second_half & 0x1F) << 3);
                    }
                case PixelFormat.Format24bppRgb:
                    {
                        int index = (y * Handle.Stride) + (x * 3); // 24bpp, 3 bytes

                        return Color.FromArgb(
                            Data[index + 2],
                            Data[index + 1],
                            Data[index]);
                    }
                case PixelFormat.Format32bppArgb:
                    {
                        int index = (y * Handle.Stride) + (x * 3); // 32bpp, 4 bytes

                        return Color.FromArgb(
                            Data[index + 3],
                            Data[index + 2],
                            Data[index + 1],
                            Data[index]);
                    }
                default:
                    throw new Exception("Unsupported pixel format");
            }
        }

        public void SetPixel(int x, int y, Color clr)
        {
            switch (InternalBitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    {
                        int index = (y * Handle.Stride) + (x * 3); // 24bpp, 3 bytes

                        Data[index] = clr.B;
                        Data[index + 1] = clr.G;
                        Data[index + 2] = clr.R;

                        break;
                    }
                case PixelFormat.Format32bppArgb:
                    {
                        int index = (y * Handle.Stride) + (x * 3); // 32bpp, 4 bytes

                        Data[index] = clr.B;
                        Data[index + 1] = clr.G;
                        Data[index + 2] = clr.R;
                        Data[index + 3] = clr.A;

                        break;
                    }
                default:
                    throw new Exception("Unsupported pixel format");
            }
        }
    }
}

