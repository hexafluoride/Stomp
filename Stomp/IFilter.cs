using System;

namespace Stomp
{
    public delegate void FilterMessageHandler(string message, object sender);

    public interface IFilter
    {
        event FilterMessageHandler OnMessage;

        bool IsContext { get; }
        string HumanName { get; }
        string ScriptName { get; } // ;)

        void Apply(FastBitmap bitmap);
    }
}

