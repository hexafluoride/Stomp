using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Stomp
{
    public class FastBitmap
    {
        private Bitmap InternalBitmap;
        private BitmapData Handle;

        private Random Random = new Random();

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

        public FastBitmap(int width, int height)
        {
            InternalBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        public void Save(string path)
        {
            Unlock();
            InternalBitmap.Save(path);
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
                        int index = (y * Handle.Stride) + (x * 4); // 32bpp, 4 bytes

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

        public byte[] GetRawBytes(int x, int y)
        {
            int index = (y * Handle.Stride);
            int len = 0;

            switch (InternalBitmap.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    len = 2;
                    break;
                case PixelFormat.Format24bppRgb:
                    len = 3;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    len = 4;
                    break;
                case PixelFormat.Format48bppRgb:
                    len = 6;
                    break;
                case PixelFormat.Format64bppPArgb:
                    len = 8;
                    break;
                default:
                    throw new Exception("Unsupported pixel format");
            }

            index += x * len;

            byte[] ret = new byte[len];
            Array.Copy(Data, index, ret, 0, len);

            return ret;
        }

        public void CreateGap(int start, int length, GapBehavior behavior = GapBehavior.Random)
        {
            byte[] gapped = new byte[Math.Abs(length)];

            int gap_pos = 0;

            if (length > 0)
            {
                Array.Copy(Data, start, gapped, 0, length);
                Array.Copy(Data, (start + length), Data, start, Data.Length - (start + length));

                gap_pos = Data.Length - length;
            }
            else
            {
                length = Math.Abs(length);

                Buffer.BlockCopy(Data, Data.Length - start, gapped, 0, length);
                Buffer.BlockCopy(Data, start, Data, (start + length), Data.Length - (start + length));

                gap_pos = start;
            }

            switch (behavior)
            {
                case GapBehavior.Black:
                    for (int i = gap_pos; i < gap_pos + length; i++)
                        Data[i] = 0;
                    
                    break;
                case GapBehavior.White:
                    for (int i = gap_pos; i < gap_pos + length; i++)
                        Data[i] = 255;

                    break;
                case GapBehavior.Random:
                    byte[] rnd = new byte[length];
                    Random.NextBytes(rnd);

                    Array.Copy(rnd, 0, Data, gap_pos, length);
                    break;
                case GapBehavior.Gapped:
                    Array.Copy(gapped, 0, Data, gap_pos, length);
                    break;
            }
        }

        public Dictionary<string, byte[]> SeparateChannels()
        {
            Dictionary<string, byte[]> ret = new Dictionary<string, byte[]>();

            switch (InternalBitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    {
                        byte[] r = new byte[Data.Length / 3];
                        byte[] g = new byte[Data.Length / 3];
                        byte[] b = new byte[Data.Length / 3];

                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int index = (y * Width) + x;
                                byte[] raw = GetRawBytes(x, y);

                                b[index] = raw[0];
                                g[index] = raw[1];
                                r[index] = raw[2];
                            }
                        }

                        ret.Add("R", r);
                        ret.Add("G", g);
                        ret.Add("B", b);

                        return ret;
                    }
                case PixelFormat.Format32bppArgb:
                    {
                        byte[] a = new byte[Data.Length / 4];
                        byte[] r = new byte[Data.Length / 4];
                        byte[] g = new byte[Data.Length / 4];
                        byte[] b = new byte[Data.Length / 4];

                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int index = (y * Width) + x;
                                byte[] raw = GetRawBytes(x, y);

                                b[index] = raw[0];
                                g[index] = raw[1];
                                r[index] = raw[2];
                                a[index] = raw[3];
                            }
                        }

                        ret.Add("R", r);
                        ret.Add("G", g);
                        ret.Add("B", b);
                        ret.Add("A", a);

                        return ret;
                    }
                default:
                    throw new Exception("Unsupported pixel format");
            }
        }

        public void WriteChannels(Dictionary<string, byte[]> channels)
        {
            switch (InternalBitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            string id = "";

                            switch (c)
                            {
                                case 0:
                                    id = "B";
                                    break;
                                case 1:
                                    id = "G";
                                    break;
                                case 2:
                                    id = "R";
                                    break;
                            }

                            if (!channels.ContainsKey(id))
                                continue;
                            
                            var channel = channels[id];

                            for (int i = 0; i < channel.Length; i++)
                            {
                                Data[(i * 3) + c] = channel[i];
                            }
                        }

                        break;
                    }
                case PixelFormat.Format32bppArgb:
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            string id = "";

                            switch (c)
                            {
                                case 0:
                                    id = "B";
                                    break;
                                case 1:
                                    id = "G";
                                    break;
                                case 2:
                                    id = "R";
                                    break;
                                case 3:
                                    id = "A";
                                    break;
                            }

                            if (!channels.ContainsKey(id))
                                continue;
                            
                            var channel = channels[id];

                            for (int i = 0; i < channel.Length; i++)
                            {
                                Data[(i * 4) + c] = channel[i];
                            }
                        }

                        break;
                    }
                default:
                    throw new Exception("Unsupported pixel format");
            }
        }
    }

    public static class ColorHelpers
    {
        public static Color Add(Color first, Color second)
        {
            return SafeRgb(
                first.R + second.R,
                first.G + second.G,
                first.B + second.B, true);
        }

        public static Color OverflowingAdd(Color first, Color second)
        {
            byte r = (byte)(first.R + second.R);
            byte g = (byte)(first.G + second.G);
            byte b = (byte)(first.B + second.B);

            return Color.FromArgb(r, g, b);
        }

        public static Color Delta(Color first, Color second)
        {
            byte r = (byte)(first.R - second.R);
            byte g = (byte)(first.G - second.G);
            byte b = (byte)(first.B - second.B);

            return Color.FromArgb(r, g, b);
        }

        public static Color Average(Color first, Color second)
        {
            return SafeRgb(
                (first.R + second.R) / 2,
                (first.G + second.G) / 2,
                (first.B + second.B) / 2, true);
        }

        public static Color PickPaeth(Color left, Color up, Color diagonal)
        {
            int p_r = left.R + up.R - diagonal.R;
            int p_g = left.G + up.G - diagonal.G;
            int p_b = left.B + up.B - diagonal.B;

            return SafeRgb(
                MinDelta(left.R, up.R, diagonal.R, p_r),
                MinDelta(left.G, up.G, diagonal.G, p_g),
                MinDelta(left.B, up.B, diagonal.B, p_b), true);
        }

        public static Color SafeRgb(int r, int g, int b, bool modulo = false)
        {
            if (modulo)
            {
                r = Math.Abs(r % 256);
                g = Math.Abs(g % 256);
                b = Math.Abs(b % 256);
            }
            else
            {
                r = Math.Min(Math.Max(0, r), 255);
                g = Math.Min(Math.Max(0, g), 255);
                b = Math.Min(Math.Max(0, b), 255);
            }

            return Color.FromArgb(r, g, b);
        }

        public static int MinDelta(int x, int y, int z, int t)
        {
            return Min(
                Math.Abs(x - t), 
                Math.Abs(y - t), 
                Math.Abs(z - t));
        }

        public static int Min(int x, int y, int z)
        {
            return Math.Min(x, Math.Min(y, z));
        }
    }

    public enum GapBehavior
    {
        Black, White, Random, Gapped
    }
}

