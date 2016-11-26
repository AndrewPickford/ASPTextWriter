using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class RGB
    {
        public double r { get; private set; }
        public double g { get; private set; }
        public double b { get; private set; }

        public RGB(double red, double green, double blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        public RGB(UnityEngine.Color32 color)
        {
            r = (double) color.r / 255d;
            g = (double) color.g / 255d;
            b = (double) color.b / 255d;
        }

        public HSV toHSV()
        {
            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;

            double h = 0d;
            double s = 0d;
            double v = max;

            if (delta != 0d)
            {
                s = delta / max;

                double deltaR = (((max - r) / 6d) + (delta / 2d)) / delta;
                double deltaG = (((max - g) / 6d) + (delta / 2d)) / delta;
                double deltaB = (((max - b) / 6d) + (delta / 2d)) / delta;

                if (r == max) h = deltaB - deltaG;
                else if (g == max) h = 1d / 3d + deltaR - deltaB;
                else if (b == max) h = 2d / 3d + deltaG - deltaR;

                if (h < 0d) h += 1d;
                if (h > 1d) h -= 1d;
            }

            return new HSV(h, s, v);
        }

        public override string ToString()
        {
            return r.ToString() + ", " + g.ToString() + ", " + b.ToString();
        }
    }
}
