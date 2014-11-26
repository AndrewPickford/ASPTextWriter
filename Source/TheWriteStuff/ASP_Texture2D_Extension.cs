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

        public static void DrawText(this Texture2D texture, string text, ASP.MappedFont font, Color color, int offsetX, int offsetY, bool mirror,
                                    ASP.TextDirection direction)
        {
            ASP.Rectangle boundingBox = new ASP.Rectangle(0, 0, texture.width, texture.height);
            DrawText(texture, text, font, color, offsetX, offsetY, mirror, direction, boundingBox);
        }

        // basic idea from http://forum.unity3d.com/threads/drawtext-on-texture2d.220217/
        public static void DrawText(this Texture2D texture, string text, ASP.MappedFont font, Color color, int offsetX, int offsetY, bool mirror,
                                    ASP.TextDirection direction, ASP.Rectangle boundingBox, ASP.BlendMethod blend = ASP.BlendMethod.RGB,
                                    bool applyAlpha = false, float alpha = 1f)
        {
            ASP.CharacterMap cMap;
            int x = 0;
            int y = 0;
            bool escapeMode = false;

            string textToWrite = string.Empty;
            if (mirror) textToWrite = ASP.Utils.Reverse(text);
            else textToWrite = text;

            foreach (char c in textToWrite)
            {
                if (c == '\\')
                {
                    escapeMode = !escapeMode;
                }

                if (escapeMode)
                {
                    if (c == 'n')
                    {
                        switch (direction)
                        {
                            case ASP.TextDirection.DOWN_UP:
                            case ASP.TextDirection.UP_DOWN:
                                x += font.size;
                                y = 0;
                                break;

                            case ASP.TextDirection.LEFT_RIGHT:
                            case ASP.TextDirection.RIGHT_LEFT:
                            default:
                                y += font.size;
                                x = 0;
                                break;
                        }
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
                    ASP.ImageUtils.Recolor(ref charPixels, Color.black, color);

                    ASP.Rectangle cPos = new ASP.Rectangle(0, 0, cMap.uv.w, cMap.uv.h);

                    if (cMap.flipped)
                    {
                        charPixels = ASP.ImageUtils.FlipXY(charPixels, cPos.w, cPos.h, false);
                        cPos.swapWH();
                    }
                    else charPixels = ASP.ImageUtils.FlipVertically(charPixels, cPos.w, cPos.h);
                    
                    if (mirror) charPixels = ASP.ImageUtils.FlipHorizontally(charPixels, cPos.w, cPos.h);

                    switch (direction)
                    {
                        case ASP.TextDirection.RIGHT_LEFT:
                            charPixels = ASP.ImageUtils.Rotate180(charPixels, cPos.w, cPos.h);
                            cPos.x = offsetX - x - ((int)cMap.vx + (int)cMap.vw);
                            cPos.y = offsetY + y - (font.size + (int)cMap.vy);
                            break;

                        case ASP.TextDirection.UP_DOWN:
                            charPixels = ASP.ImageUtils.FlipXY(charPixels, cPos.w, cPos.h, false);
                            cPos.swapWH();
                            cPos.x = offsetX - x + (font.size + (int)cMap.vy + (int)cMap.vh);
                            cPos.y = offsetY - y - ((int)cMap.vx + (int)cMap.vw);
                            break;

                        case ASP.TextDirection.DOWN_UP:
                            charPixels = ASP.ImageUtils.FlipXY(charPixels, cMap.uv.w, cMap.uv.h, true);
                            cPos.swapWH();
                            cPos.x = offsetX + x - (font.size + (int)cMap.vy);
                            cPos.y = offsetY + y + (int)cMap.vx;
                            break;

                        case ASP.TextDirection.LEFT_RIGHT:
                        default:
                            cPos.x = offsetX + x + (int)cMap.vx;
                            cPos.y = offsetY - y + (font.size + (int)cMap.vy + (int)cMap.vh);
                            break;
                    }

                    Color[] texturePixels = texture.GetPixels(cPos);
                    Color[] texturePixelsOrig = texture.GetPixels(cPos);

                    switch (blend)
                    {
                        case ASP.BlendMethod.HSV:
                            ASP.ImageUtils.BlendHSV(ref texturePixels, charPixels, cPos.w, cPos.h);
                            break;

                        case ASP.BlendMethod.RGB:
                            ASP.ImageUtils.BlendRGB(ref texturePixels, charPixels, cPos.w, cPos.h);
                            break;

                        case ASP.BlendMethod.PIXEL:
                        default:
                            ASP.ImageUtils.BlendPixel(ref texturePixels, charPixels, cPos.w, cPos.h);
                            break;
                    }
                    

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

                    switch (direction)
                    {
                        case ASP.TextDirection.DOWN_UP:
                        case ASP.TextDirection.UP_DOWN:
                            y += (int)cMap.cw;
                            break;

                        case ASP.TextDirection.LEFT_RIGHT:
                        case ASP.TextDirection.RIGHT_LEFT:
                        default:
                            x += (int)cMap.cw;
                            break;
                    }
                }
            }
        }

        public static void Rescale(this Texture2D texture, int width, int height)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    float x = (float)i / (float) width;
                    float y = (float)j / (float) height;

                    pixels[i + j* width] = texture.GetPixelBilinear(x, y);
                }
            }

            texture.Resize(width, height);
            texture.SetPixels(pixels);
        }

    }
}