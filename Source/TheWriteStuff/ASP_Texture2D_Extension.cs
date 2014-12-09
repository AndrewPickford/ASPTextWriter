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
    }
}