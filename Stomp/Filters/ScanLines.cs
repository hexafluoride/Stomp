using System;
using System.Drawing;

using Stomp;

namespace Stomp.Filters
{
    public class ScanLines : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "scan lines"; } }
        public string ScriptName { get { return "scanlines"; } }

        public event FilterMessageHandler OnMessage;

        public bool PreserveBrightness { get; set; }

        public ScanLines()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y += 2)
            {
                for (int x = 0; x < bmp.Width; x++)
                    bmp.SetPixel(x, y, Color.Black);
            }

            if (PreserveBrightness)
            {
                for (int y = 1; y < bmp.Height; y += 2)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        var clr = bmp.GetPixel(x, y);

                        int r = clr.R;
                        int g = clr.G;
                        int b = clr.B;

                        bmp.SetPixel(x, y, Color.FromArgb
                            (
                                Math.Min(r * 2, 255), 
                                Math.Min(g * 2, 255), 
                                Math.Min(b * 2, 255)
                            ));

                        // this is obviously not the optimal implementation
                        // TODO: actually double the perceived brightness instead of half-assing it
                    }
                }
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

