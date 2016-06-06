using System;
using System.Collections.Generic;

using Stomp.Filters;

namespace Stomp
{
    public class FilterChain
    {
        public List<IFilter> Filters = new List<IFilter>();

        public FilterChain()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            foreach (var filter in Filters)
                filter.Apply(bmp);
        }

        public void Append(params IFilter[] filters)
        {
            foreach(var filter in filters)
                Filters.Add(filter);
        }
    }
}

