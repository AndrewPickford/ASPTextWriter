using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class RGB
    {
        public float r { get; private set; }
        public float g { get; private set; }
        public float b { get; private set; }

        public RGB(float red, float green, float blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        public RGB(UnityEngine.Color32 color)
        {
            r = (float) color.r / 255f;
            g = (float) color.g / 255f;
            b = (float) color.b / 255f;
        }

        public HSV toHSV()
        {
            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;

            float h = 0f;
            float s = 0f;
            float v = max;

            if (delta != 0f)
            {
                s = delta / max;

                float deltaR = (((max - r) / 6f) + (delta / 2f)) / delta;
                float deltaG = (((max - g) / 6f) + (delta / 2f)) / delta;
                float deltaB = (((max - b) / 6f) + (delta / 2f)) / delta;

                if (r == max) h = deltaB - deltaG;
                else if (g == max) h = 1f / 3f + deltaR - deltaB;
                else if (b == max) h = 2f / 3f + deltaG - deltaR;

                if (h < 0f) h += 1f;
                if (h > 1f) h -= 1f;
            }

            return new HSV(h, s, v);
        }

        public override string ToString()
        {
            return r.ToString() + ", " + g.ToString() + ", " + b.ToString();
        }
    }
}
