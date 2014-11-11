using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEngine
{
    public static class ASP_Texture2D_Extension
    {
        public static void Fill(this Texture2D texture, Color color)
        {
            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    texture.SetPixel(i, j, color);
                }
            }
        }

        public static Color[] GetPixels(this Texture2D texture, ASP.Rectangle rect)
        {
            return texture.GetPixels(rect.x, rect.y, rect.w, rect.h);
        }

        public static void SetPixels(this Texture2D texture, ASP.Rectangle pos, Color[] pixels)
        {
            if (pos.x >= 0 && pos.y > 0 && (pos.x + pos.w) < texture.width && (pos.y + pos.h) < texture.height)
            {
                texture.SetPixels(pos.x, pos.y, pos.w, pos.h, pixels);
            }
            else
            {
                for (int i = 0; i < pos.w; ++i)
                {
                    for (int j = 0; j < pos.h; ++j)
                    {
                        int px = pos.x + i;
                        int py = pos.y + j;
                        if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        {
                            Color col = Color.Lerp(texture.GetPixel(px, py), pixels[i + j * pos.w], pixels[i + j * pos.w].a);
                            texture.SetPixel(px, py, col);
                        }
                    }
                }
            }
        }

        // basic idea from http://forum.unity3d.com/threads/drawtext-on-texture2d.220217/
        public static void DrawText(this Texture2D texture, string text, ASP.MappedFont font, Color color, int offsetX, int offsetY, bool applyAlpha = false, int alpha = 255)
        {
            ASP.CharacterMap cMap;
            int x = 0;
            int y = 0;
            bool escapeMode = false;

            foreach (char c in text)
            {
                if (c == '\\')
                {
                    escapeMode = !escapeMode;
                }

                if (escapeMode)
                {
                    if (c == 'n')
                    {
                        y = y + font.height;
                        x = 0;
                        escapeMode = false;
                    }
                }
                else
                {
                    if (font.characterMap.TryGetValue(c, out cMap) == false)
                    {
                        if (font.characterMap.TryGetValue('?', out cMap) == false) continue;
                    }

                    Color[] charPixels = font.texture.GetPixels(cMap.uv);
                    ASP.Utils.Recolor(ref charPixels, Color.black, color);

                    ASP.Rectangle cPos = new ASP.Rectangle();
                    cPos.x = offsetX + x + (int)cMap.vx;
                    cPos.y = texture.height - (offsetY + y - (int)cMap.vy + cMap.uv.h);
                    cPos.w = cMap.uv.w;
                    cPos.h = cMap.uv.h;

                    if (cMap.flipped)
                    {
                        charPixels = ASP.Utils.FlipXY(charPixels, cMap.uv.w, cMap.uv.h, false);
                        cPos.w = cMap.uv.h;
                        cPos.h = cMap.uv.w;
                        cPos.y = texture.height - (offsetY + y - (int)cMap.vy + cMap.uv.w);
                    }
                    else
                    {
                        charPixels = ASP.Utils.FlipVertically(charPixels, cPos.w, cPos.h);
                    }

                    Color[] texturePixels = texture.GetPixels(cPos);
                    ASP.Utils.Overlay(ref texturePixels, charPixels, cPos.w, cPos.h);

                    if (applyAlpha)
                    {
                        Debug.Log("text alpha");
                        for (int i = 0; i < charPixels.Length; ++i)
                        {
                            if (charPixels[i].r == color.r && charPixels[i].g == color.g && charPixels[i].b == color.b)
                            {
                                texturePixels[i].a = alpha;
                            }
                        }
                    }

                    texture.SetPixels(cPos, texturePixels);

                    x += (int)cMap.cw;
                }
            }
        }
    }
}