using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEngine
{
    public static class ASP_Texture2D_Extension
    {
        public static Color[] GetPixelsFromCompressed(this Texture2D texture, ASP.Rectangle pos)
        {
            Texture2D temp = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, true);

            Color32[] pixels = texture.GetPixels32();
            temp.name = texture.name;
            temp.SetPixels32(pixels);

            return GetPixels(temp, pos);
        }

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

        public static Color[] GetPixels(this Texture2D texture, ASP.Rectangle pos)
        {
            if (pos.x >= 0 && pos.y >= 0 && pos.wx() < texture.width && pos.hy() < texture.height)
            {
                return texture.GetPixels(pos.x, pos.y, pos.w, pos.h);
            }
            else
            {
                Color[] pixels = new Color[pos.w * pos.h];
                for (int i = 0; i < pos.w; ++i)
                {
                    for (int j = 0; j < pos.h; ++j)
                    {
                        int px = pos.x + i;
                        int py = pos.y + j;
                        if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        {
                            pixels[i + j * pos.w] = texture.GetPixel(px, py);
                        }
                        else
                        {
                            pixels[i + j * pos.w] = Color.gray;
                        }
                    }
                }

                return pixels;
            }
        }

        public static void SetPixels(this Texture2D texture, ASP.Rectangle pos, Color[] pixels, ASP.Rectangle boundingBox)
        {
            if (pos.x >= 0 && pos.y >= 0 && pos.wx() < texture.width && pos.hy() < texture.height &&
                pos.x >= boundingBox.x && pos.wx() < boundingBox.wx() && pos.y >= boundingBox.y && pos.hy() < boundingBox.hy())
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
                            if (px >= boundingBox.x && px < boundingBox.wx() && py >= boundingBox.y && py < boundingBox.hy())
                            {
                                texture.SetPixel(px, py, pixels[i + j * pos.w]);
                            }
                        }
                    }
                }
            }
            texture.Apply();
        }

        public static void DrawText(this Texture2D texture, string text, ASP.MappedFont font, Color color, int offsetX, int offsetY)
        {
            ASP.Rectangle boundingBox = new ASP.Rectangle(0, 0, texture.width, texture.height);
            DrawText(texture, text, font, color, offsetX, offsetY, boundingBox);
        }

        // basic idea from http://forum.unity3d.com/threads/drawtext-on-texture2d.220217/
        public static void DrawText(this Texture2D texture, string text, ASP.MappedFont font, Color color, int offsetX, int offsetY, ASP.Rectangle boundingBox, bool applyAlpha = false, int alpha = 255)
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
                    Color[] texturePixelsOrig = texture.GetPixels(cPos);
                    ASP.Utils.Overlay(ref texturePixels, charPixels, cPos.w, cPos.h);

                    if (applyAlpha)
                    {
                        for (int i = 0; i < charPixels.Length; ++i)
                        {
                            if (charPixels[i].a > 0.1f)
                            {
                                texturePixels[i].a = alpha;
                            }
                            else
                            {
                                texturePixels[i].a = texturePixelsOrig[i].a;
                            }
                        }
                    }

                    texture.SetPixels(cPos, texturePixels, boundingBox);

                    x += (int)cMap.cw;
                }
            }
        }
    }
}