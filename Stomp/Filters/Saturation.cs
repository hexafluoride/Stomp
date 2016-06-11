using System;
using System.Drawing;

using Stomp;

namespace Stomp.Filters
{
    public class Saturation : IFilter
    {
        public bool IsContext { get { return false; } }
        public string HumanName { get { return "saturation"; } }
        public string ScriptName { get { return "saturate"; } }

        public event FilterMessageHandler OnMessage;

        [ScriptAlias("intensity")]
        public double Intensity { get; set; }

        public Saturation()
        {
        }

        public void Apply(FastBitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var color = bmp.GetPixel(x, y);

                    bmp.SetPixel(x, y, Color.FromArgb
                        (
                            Saturate(color.R, Intensity),
                            Saturate(color.G, Intensity),
                            Saturate(color.B, Intensity)
                        ));
                }
            }
        }

        public int Saturate(int color, double intensity)
        {
            if (color < 128)
            {
                return (int)Math.Max(0, color - ((color * intensity) / 2));
            }
            else
            {
                return (int)Math.Min(255, color + ((color * intensity) / 2));
            }
        }

        public void SendMessage(string str, params object[] format)
        {
            if (OnMessage != null)
                OnMessage(string.Format(str, format), this);
        }
    }
}

