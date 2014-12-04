using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class Image
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public Color32[] pixels { get; private set; }

        public Image()
        {
            width = 0;
            height = 0;
        }

        public void resize(int w, int h)
        {
            width = w;
            height = h;

            pixels = new Color32[width * height];
        }

        public void resizeAndFill(int w, int h, Color32[] pix)
        {
            resize(w, h);

            for (int i = 0; i < width * height; ++i)
            {
                pixels[i] = pix[i];
            }
        }
    }
}
