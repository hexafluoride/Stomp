using System;
using System.Drawing;

using Stomp;
using Stomp.Filters;

namespace Stomp.Filters.Contexts
{
    public class PngFiltered : IFilter
    {
        public FilterChain Chain;

        public PngFiltered(FilterChain chain)
        {
            Chain = chain;
        }

        public void Apply(FastBitmap bmp)
        {
            int seed = 0;
            var filtered = PngFilter.Encode(bmp, out seed);

            Chain.Apply(filtered);

            PngFilter.Decode(filtered, seed, bmp);
        }
    }
}

