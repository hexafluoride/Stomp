using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Stomp
{
    public class PngFilter
    {
        public static Random Seeder = new Random();

        public static FastBitmap Encode(FastBitmap input, FilterType[] filters)
        {
            FastBitmap ret = new FastBitmap(input.Width, input.Height);
            ret.Lock();

            // encode the first line with zero subtype
            for (int x = 0; x < input.Width; x++)
                ret.SetPixel(x, 0, input.GetPixel(x, 0));

            Parallel.For(1, input.Height, y =>
            {
                FilterType type = filters[y];

                switch (type)
                {
                    case FilterType.None:
                        for (int x = 0; x < input.Width; x++)
                            ret.SetPixel(x, y, input.GetPixel(x, y));
                        break;
                    case FilterType.Sub:
                        ret.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);
                            Color left = input.GetPixel(x - 1, y);

                            Color delta = ColorHelpers.Delta(original, left);

                            ret.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Up:
                        for (int x = 0; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);
                            Color up = input.GetPixel(x, y - 1);

                            Color delta = ColorHelpers.Delta(original, up);

                            ret.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Average:
                        ret.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);

                            Color left = input.GetPixel(x - 1, y);
                            Color up = input.GetPixel(x, y - 1);

                            Color avg = ColorHelpers.Average(left, up);
                            Color delta = ColorHelpers.Delta(original, avg);

                            ret.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Paeth:
                        ret.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);

                            Color left = input.GetPixel(x - 1, y);
                            Color up = input.GetPixel(x, y - 1);
                            Color diagonal = input.GetPixel(x - 1, y - 1);

                            Color paeth = ColorHelpers.PickPaeth(left, up, diagonal);
                            Color delta = ColorHelpers.Delta(original, paeth);

                            ret.SetPixel(x, y, delta);
                        }
                        break;
                    default:
                        throw new Exception("This should never ever happen");
                }
            });

            return ret;
        }

        public static FastBitmap Decode(FastBitmap input, FilterType[] filters, FastBitmap output = null)
        {
            if (output == null)
            {
                output = new FastBitmap(input.Width, input.Height);
                output.Lock();
            }

            // decode the first line with zero subtype
            for (int x = 0; x < input.Width; x++)
                output.SetPixel(x, 0, input.GetPixel(x, 0));

            for(int y = 1; y < input.Height; y++)
            {
                FilterType type = filters[y];

                switch (type)
                {
                    case FilterType.None:
                        for (int x = 0; x < input.Width; x++)
                            output.SetPixel(x, y, input.GetPixel(x, y));
                        break;
                    case FilterType.Sub:
                        output.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);
                            Color left = output.GetPixel(x - 1, y);

                            Color delta = ColorHelpers.OverflowingAdd(original, left);

                            output.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Up:
                        for (int x = 0; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);
                            Color up = output.GetPixel(x, y - 1);

                            Color delta = ColorHelpers.OverflowingAdd(up, original);

                            output.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Average:
                        output.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);

                            Color left = output.GetPixel(x - 1, y);
                            Color up = output.GetPixel(x, y - 1);

                            Color avg = ColorHelpers.Average(left, up);
                            Color delta = ColorHelpers.OverflowingAdd(avg, original);

                            output.SetPixel(x, y, delta);
                        }
                        break;
                    case FilterType.Paeth:
                        output.SetPixel(0, y, input.GetPixel(0, y));

                        for (int x = 1; x < input.Width; x++)
                        {
                            Color original = input.GetPixel(x, y);

                            Color left = output.GetPixel(x - 1, y);
                            Color up = output.GetPixel(x, y - 1);
                            Color diagonal = output.GetPixel(x - 1, y - 1);

                            Color paeth = ColorHelpers.PickPaeth(left, up, diagonal);
                            Color delta = ColorHelpers.OverflowingAdd(paeth, original);

                            output.SetPixel(x, y, delta);
                        }
                        break;
                    default:
                        throw new Exception("This should never ever happen");
                }
            }

            return output;
        }
    }

    public enum FilterType
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4
    }
}

