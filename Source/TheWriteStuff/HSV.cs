using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class HSV
    {
        public float h { get; private set; }
        public float s { get; private set; }
        public float v { get; private set; }

        public HSV(float hue, float saturation, float value)
        {
            h = hue;
            s = saturation;
            v = value;
        }

        public HSV blend(HSV second, float fraction)
        {
            float h1 = this.h;
            float h2 = second.h;
            float d = h2 - h1;
            float f = fraction;

            if (h1 > h2)
            {
                float t = h1;
                h1 = h2;
                h2 = t;
                d = - d;
                f = 1 - f;

            }

            float H = 0f;
            float S = this.s + f * (second.s - this.s);
            float V = this.v + f * (second.s - this.v);

            if (d > 0.5f)
            {
                h1 = h1 + 1;
                H = h1 + f*(h2-h1);
            }
            else
            {
                H = h1 + f*d;
            }

            if (H < 0f) H += 1f;
            if (H >= 1f) H -= 1f;

            return new HSV(H, S, V);
        }

        public RGB toRGB()
        {
            if (s == 0f)
            {
                return new RGB(v, v, v);
            }

            float hh = h * 6f;
            if (hh == 6f) hh = 0f;

            int i = (int) hh;

            float v1 = v * (1-s);
            float v2 = v * (1 - s*(hh - i));
            float v3 = v * (1 - s*(1 - (hh - i)));

            float r = 0f;
            float g = 0f;
            float b = 0f;

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
