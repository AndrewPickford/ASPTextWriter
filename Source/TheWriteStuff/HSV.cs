using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class HSV
    {
        public double h { get; private set; }
        public double s { get; private set; }
        public double v { get; private set; }

        public HSV(double hue, double saturation, double value)
        {
            h = hue;
            s = saturation;
            v = value;
        }

        public HSV blend(HSV second, double fraction)
        {
            double h1 = this.h;
            double h2 = second.h;
            double d = h2 - h1;
            double f = fraction;

            if (h1 > h2)
            {
                double t = h1;
                h1 = h2;
                h2 = t;
                d = - d;
                f = 1 - f;

            }

            double H = 0d;
            double S = this.s + f * (second.s - this.s);
            double V = this.v + f * (second.s - this.v);

            if (d > 0.5)
            {
                h1 = h1 + 1;
                H = h1 + f*(h2-h1);
            }
            else
            {
                H = h1 + f*d;
            }

            if (H < 0d) H += 1f;
            if (H >= 1d) H -= 1f;

            return new HSV(H, S, V);
        }

        public RGB toRGB()
        {
            if (s == 0d)
            {
                return new RGB(v, v, v);
            }

            double hh = h * 6d;
            if (hh == 6d) hh = 0d;

            int i = (int) hh;

            double v1 = v * (1-s);
            double v2 = v * (1 - s*(hh - i));
            double v3 = v * (1 - s*(1 - (hh - i)));

            double r = 0d;
            double g = 0d;
            double b = 0d;

            switch (i)
            {
                case 0:
                    r = v;
                    g = v3;
                    b = v1;
                    break;

                case 1:
                    r = v2;
                    g = v;
                    b = v1;
                    break;
                   
                case 2:
                    r = v1;
                    g = v;
                    b = v3;
                    break;

                case 3:
                    r = v1;
                    g = v2; 
                    b = v;
                    break;

                case 4:
                    r = v3;
                    g = v1;
                    b = v;
                    break;

                default:
                    r = v;
                    g = v1;
                    b = v2;
                    break;
            }

            return new RGB(r, g, b);
        }

        public override string ToString()
        {
            return h.ToString() + ", " + s.ToString() + ", " + v.ToString();
        }
    }
}
