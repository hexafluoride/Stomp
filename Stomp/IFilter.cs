using System;

namespace Stomp
{
    public interface IFilter
    {
        void Apply(FastBitmap bitmap);
    }
}

