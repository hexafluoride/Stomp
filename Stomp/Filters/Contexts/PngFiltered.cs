using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Stomp;
using Stomp.Filters;

namespace Stomp.Filters.Contexts
{
    public class PngFiltered : IFilter
    {
        public bool IsContext { get { return true; } }
        public string HumanName { get { return "PNG filter"; } }
        public string ScriptName { get { return "png-filter-context"; } }

        public event FilterMessageHandler OnMessage;

        [ScriptAlias("inner-chain")]
        public FilterChain Chain { get; set; }
        [ScriptAlias("png-filter-gen")]
        public FilterTypeGen Behavior { get; set; }

        [ScriptAlias("constant-filter")]
        public FilterType ConstantType { get; set; }

        [ScriptAlias("run-length-max")]
        public int RunLengthMax { get; set; }
        [ScriptAlias("run-length-min")]
        public int RunLengthMin { get; set; }

        public Dictionary<FilterType, int> WeighedList = new Dictionary<FilterType, int>()
        {
            {FilterType.Average, 10},
            {FilterType.Paeth, 10},
            {FilterType.Sub, 5},
            {FilterType.Up, 5}
        };

        public PngFiltered()
        {
        }

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

            Stopwatch sw = Stopwatch.StartNew();
            var filtered = PngFilter.Encode(bmp, filters);
            SendMessage("Encoded in {0} seconds.", sw.ElapsedMilliseconds / 1000d);

            Chain.Apply(filtered);

            sw.Restart();

            PngFilter.Decode(filtered, filters, bmp);
            SendMessage("Decoded in {0} seconds.", sw.ElapsedMilliseconds / 1000d);

            sw.Stop();
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }

    public enum FilterTypeGen
    {
        Constant, Random
    }
}

