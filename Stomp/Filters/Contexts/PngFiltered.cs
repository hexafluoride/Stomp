using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

using Stomp;
using Stomp.Filters;

namespace Stomp.Filters.Contexts
{
    public class PngFiltered : IFilter
    {
        public FilterChain Chain;
        public FilterTypeGen Behavior { get; set; }

        public FilterType ConstantType { get; set; }

        public int RunLengthMax { get; set; }
        public int RunLengthMin { get; set; }

        public Dictionary<FilterType, int> WeighedList = new Dictionary<FilterType, int>()
        {
            {FilterType.Average, 10},
            {FilterType.Paeth, 10},
            {FilterType.Sub, 5},
            {FilterType.Up, 5}
        };

        public PngFiltered(FilterChain chain)
        {
            Chain = chain;
        }

        public FilterType[] GenerateFilterArray(FastBitmap bmp)
        {
            Random rnd = new Random();

            if (Behavior == FilterTypeGen.Constant)
                return Enumerable.Repeat(ConstantType, bmp.Height).ToArray();

            int total = WeighedList.Values.Sum();
            KeyValuePair<FilterType, int>[] array = WeighedList.ToArray();

            FilterType[] ret = new FilterType[bmp.Height];

            int run_count = 0;

            for (int y = 0; y < bmp.Height; y++)
            {
                if (run_count-- <= 0)
                {
                    run_count = rnd.Next(RunLengthMin, RunLengthMax);
                }
                else
                {
                    ret[y] = ret[y - 1];
                    continue;
                }

                int counter = 0;
                int random = rnd.Next(total);

                for (int i = 0; i < array.Length; i++)
                {
                    counter += array[i].Value;
                    if (counter > random)
                    {
                        ret[y] = array[i].Key;
                        break;
                    }
                }
            }

            return ret;
        }

        public void Apply(FastBitmap bmp)
        {
            var filters = GenerateFilterArray(bmp);
            var filtered = PngFilter.Encode(bmp, filters);

            Chain.Apply(filtered);

            PngFilter.Decode(filtered, filters, bmp);
        }
    }

    public enum FilterTypeGen
    {
        Constant, Random
    }
}

