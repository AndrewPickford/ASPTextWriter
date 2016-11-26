using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEngine
{
    public static class ASP_Texture2D_Extension
    {
        public static ASP.Image GetImage(this Texture2D texture, ASP.Rectangle pos)
        {
            ASP.Image image = new ASP.Image(pos.w, pos.h);

            for (int i = 0; i < pos.w; ++i)
            {
                for (int j = 0; j < pos.h; ++j)
                {
                    int px = pos.x + i;
                    int py = pos.y + j;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height) image.setPixel(i, j, texture.GetPixel(px, py));
                    else image.setPixel(i, j, Color.gray);
                }
            }

            return image;
        }

        public static ASP.ImageGS GetImageMono(this Texture2D texture, ASP.Rectangle pos)
        {
            ASP.ImageGS image = new ASP.ImageGS(pos.w, pos.h);

            for (int i = 0; i < pos.w; ++i)
            {
                for (int j = 0; j < pos.h; ++j)
                {
                    int px = pos.x + i;
                    int py = pos.y + j;
                    byte v = 0;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height) v = (byte)(texture.GetPixel(px, py).a * 255f);
                    image.setPixel(i, j, v);
                }
            }

            return image;
        }
    }
}