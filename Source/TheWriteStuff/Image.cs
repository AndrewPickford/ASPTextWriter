﻿using System;
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
        public int length { get; private set; }
        public Color32[] pixels { get; private set; }

        public Image()
        {
            width = 0;
            height = 0;
            length = 0;
        }

        public Image(int w, int h)
        {
            resize(w, h);
        }

        public void resize(int w, int h)
        {
            width = w;
            height = h;
            length = width * height;

            pixels = new Color32[length];
        }

        public void resizeAndFill(int w, int h, Color32[] pix)
        {
            resize(w, h);

            for (int i = 0; i < length; ++i)
            {
                pixels[i] = pix[i];
            }
        }

        public void fillAlpha(byte alpha)
        {
            for (int i = 0; i < length; ++i)
                pixels[i].a = alpha;
        }

        public void recolor(Color32 from, Color32 to, bool checkAlpha)
        {
            for (int i = 0; i < length; ++i)
            {
                bool replace = true;
                if (pixels[i].r != from.r) replace = false;
                if (pixels[i].g != from.g) replace = false;
                if (pixels[i].b != from.b) replace = false;
                if (checkAlpha && pixels[i].a != from.a) replace = false;

                if (replace)
                {
                    pixels[i].r = to.r;
                    pixels[i].g = to.g;
                    pixels[i].b = to.b;
                    if (checkAlpha) pixels[i].a = to.a;
                }
            }
        }

        public void setPixel(int x, int y, Color color)
        {
            int p = x + y * width;
            pixels[p].r = (byte) (color.r * 255f);
            pixels[p].g = (byte) (color.g * 255f);
            pixels[p].b = (byte) (color.b * 255f);
            pixels[p].a = (byte) (color.a * 255f);
        }

        public void setPixel(int x, int y, Color32 color)
        {
            int p = x + y * width;
            pixels[p].r = color.r;
            pixels[p].g = color.g;
            pixels[p].b = color.b;
            pixels[p].a = color.a;
        }

        public void swapWH()
        {
            int t = width;

            width = height;
            height = t;
        }

        public void writeToFile(string fileName)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels32(pixels);
            Utils.WriteTexture(tex, fileName);
            UnityEngine.Object.Destroy(tex);
        }

        public void flipXY(bool positive)
        {
            Color32[] newPixels = new Color32[length];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color32 newColor = pixels[i + j * width];
                    if (positive) Utils.SetElement2D(ref newPixels, newColor, j, width - 1 - i, height);
                    else Utils.SetElement2D(ref newPixels, newColor, height - 1 - j, i, height);
                }
            }
            pixels = newPixels;
            swapWH();
        }

        public void flipVertically()
        {
            Color32[] newPixels = new Color32[length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    newPixels[i + (height - j - 1) * width] = pixels[i + j * width];
                }
            }
            pixels = newPixels;
        }

        public void flipHorizontally()
        {
            Color32[] newPixels = new Color32[length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    newPixels[(width - i - 1) + j * width] = pixels[i + j * width];
                }
            }
            pixels = newPixels;
        }

        public void rotate180()
        {
            Color32[] newPixels = new Color32[length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color32 newColor = Utils.GetElement2D(pixels, width - i - 1, height - j - 1, width);
                    Utils.SetElement2D(ref newPixels, newColor, i, j, width);
                }
            }
            pixels = newPixels;
        }

        public void overlay(Image overlay, IntVector2 position, bool keepAlpha)
        {
            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        if (overlay.pixels[i + j * overlay.width].a > 20)
                        {
                            pixels[px + py * width].r = overlay.pixels[i + j * overlay.width].r;
                            pixels[px + py * width].g = overlay.pixels[i + j * overlay.width].g;
                            pixels[px + py * width].b = overlay.pixels[i + j * overlay.width].b;
                            if (!keepAlpha) pixels[px + py * width].a = overlay.pixels[i + j * overlay.width].a;
                        }
                    }
                }
            }
        }

        public void blendRGB(Image overlay, IntVector2 position, bool keepAlpha)
        {
            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        Color32 overlayColor = Utils.GetElement2D(overlay.pixels, i, j, overlay.width);

                        if (overlayColor.a > 0)
                        {
                            Color32 color = Utils.GetElement2D(pixels, px, py, width);
                            Color32 newColor = Color32.Lerp(color, overlayColor, overlayColor.a);

                            pixels[px + py * width].r = newColor.r;
                            pixels[px + py * width].g = newColor.g;
                            pixels[px + py * width].b = newColor.b;
                            if (!keepAlpha) pixels[px + py * width].a = newColor.a;
                        }
                    }
                }
            }
        }

        public void blendHSV(Image overlay, IntVector2 position, bool keepAlpha)
        {
            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        Color32 overlayColor = Utils.GetElement2D(overlay.pixels, i, j, overlay.width);
                        if (overlayColor.a > 0)
                        {
                            Color32 color = Utils.GetElement2D(pixels, px, py, width);

                            RGB rgb1 = new RGB(color);
                            RGB rgb2 = new RGB(overlayColor);
                            HSV hsv1 = rgb1.toHSV();
                            HSV hsv2 = rgb2.toHSV();
                            HSV hsv3 = hsv1.blend(hsv2, overlayColor.a);
                            RGB rgb3 = hsv3.toRGB();

                            Color32 newColor = new Color(rgb3.r, rgb3.g, rgb3.b, overlayColor.a);

                            pixels[px + py * width].r = newColor.r;
                            pixels[px + py * width].g = newColor.g;
                            pixels[px + py * width].b = newColor.b;
                            if (!keepAlpha) pixels[px + py * width].a = newColor.a;
                        }
                    }
                }
            }
        }

        public void drawCharacter(char c, MappedFont font, ref IntVector2 position, TextDirection direction, Color32 color, bool mirror, AlphaOption alphaOption, BlendMethod blendMethod)
        {
            if (Global.Debug3) Utils.Log("char {0}, x {1}, y {2}", c, position.x, position.y);

            ASP.CharacterMap charMap;
            IntVector2 cPos = new IntVector2();

            if (font.characterMap.TryGetValue(c, out charMap) == false)
            {
                c = '?';
                if (font.characterMap.TryGetValue(c, out charMap) == false) return;
            }

            Image charImage = font.texture.GetImage(charMap.uv);
            charImage.recolor(Global.Black32, color, false);
 
            if (charMap.flipped) charImage.flipXY(false);
            else charImage.flipVertically();

            if (mirror) charImage.flipHorizontally();

            switch (direction)
            {
                case ASP.TextDirection.RIGHT_LEFT:
                    charImage.rotate180();
                    cPos.x = position.x - ((int)charMap.vx + (int)charMap.vw);
                    cPos.y = position.y - (font.size + (int)charMap.vy);
                    position.x -= (int) charMap.cw;
                    break;

                case ASP.TextDirection.UP_DOWN:
                    charImage.flipXY(true);
                    cPos.x = position.x + (font.size + (int)charMap.vy + (int)charMap.vh);
                    cPos.y = position.y - ((int)charMap.vx + (int)charMap.vw);
                    position.y -= (int)charMap.cw;
                    break;

                case ASP.TextDirection.DOWN_UP:
                    charImage.flipXY(false);
                    cPos.x = position.x - (font.size + (int)charMap.vy);
                    cPos.y = position.y + (int)charMap.vx;
                    position.y += (int)charMap.cw;
                    break;

                case ASP.TextDirection.LEFT_RIGHT:
                default:
                    cPos.x = position.x + (int)charMap.vx;
                    cPos.y = position.y + (font.size + (int)charMap.vy + (int)charMap.vh);
                    position.x += (int)charMap.cw;
                    break;
            }

            bool keepAlpha = false;
            if (alphaOption == AlphaOption.USE_TEXTURE) keepAlpha = true;
            switch (blendMethod)
            {
                case ASP.BlendMethod.HSV:
                    blendHSV(charImage, cPos, keepAlpha);
                    break;

                case ASP.BlendMethod.RGB:
                    blendRGB(charImage, cPos, keepAlpha);
                    break;

                case ASP.BlendMethod.PIXEL:
                default:
                    overlay(charImage, cPos, keepAlpha);
                    break;
            }
        }

        public void drawText(string text, string fontName, int fontSize, IntVector2 position, TextDirection direction, Color32 color, bool mirror, AlphaOption alphaOption, BlendMethod blendMethod)
        {
            MappedFont font = FontCache.Instance.getFontByNameSize(fontName, fontSize);
            if (font == null) font = FontCache.Instance.mappedList.First();

            drawText(text, font, position, direction, color, mirror, alphaOption, blendMethod);
        }

        public void drawText(string text, MappedFont font, IntVector2 position, TextDirection direction, Color32 color, bool mirror, AlphaOption alphaOption, BlendMethod blendMethod)
        {
            if (Global.Debug2) Utils.Log("text {0}, x {1}, y {2}", text, position.x, position.y);

            IntVector2 charPos = new IntVector2(position);
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
                                charPos.x += font.size;
                                charPos.y = 0;
                                break;

                            case ASP.TextDirection.LEFT_RIGHT:
                            case ASP.TextDirection.RIGHT_LEFT:
                            default:
                                charPos.y += font.size;
                                charPos.x = 0;
                                break;
                        }
                    }
                    if (c != '\\') escapeMode = false;
                }
                else drawCharacter(c, font, ref charPos, direction, color, mirror, alphaOption, blendMethod);
            }
        }
    }
}
