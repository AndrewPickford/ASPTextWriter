using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ASP
{
    public class ImageGS
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public int length { get; private set; }
        public byte[] pixels { get; private set; }

        public ImageGS()
        {
            width = 0;
            height = 0;
            length = 0;
        }

        public ImageGS(int w, int h)
        {
            resize(w, h);
        }

        public ImageGS(IntVector2 size)
        {
            resize(size);
        }

        public ImageGS(ImageGS image)
        {
            resize(image.width, image.height);
            for (int i = 0; i < image.length; ++i)
                pixels[i] = image.pixels[i];
        }

        public ImageGS(Image image)
        {
            resize(image.width, image.height);
            for (int i = 0; i < image.length; ++i)
            {
                pixels[i] = image.pixels[i].a;
            }
        }

        public void resize(int w, int h)
        {
            width = w;
            height = h;
            length = width * height;

            pixels = new byte[length];
        }

        public void resize(IntVector2 size)
        {
            resize(size.x, size.y);
        }

        public void clear()
        {
            for (int i = 0; i < length; ++i)
                pixels[i] = 0;
        }

        public void setPixel(int i, int j, byte v)
        {
            int p = i + j * width;
            if (p < 0 || p >= length) return;

            pixels[p] = v;
        }

        public byte getPixel(int i, int j)
        {
            return pixels[i + j * width];
        }

        public void fill(byte v)
        {
            for (int i = 0; i < length; ++i)
            {
                pixels[i] = v;
            }
        }

        public void flipXY(bool positive)
        {
            Byte[] newPixels = new byte[length];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte v = pixels[i + j * width];
                    if (positive) Utils.SetElement2D(ref newPixels, v, j, width - 1 - i, height);
                    else Utils.SetElement2D(ref newPixels, v, height - 1 - j, i, height);
                }
            }
            pixels = newPixels;
            swapWH();
        }

        public void flipVertically()
        {
            Byte[] newPixels = new byte[length];

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
            Byte[] newPixels = new byte[length];

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
            Byte[] newPixels = new byte[length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Byte v = Utils.GetElement2D(pixels, width - i - 1, height - j - 1, width);
                    Utils.SetElement2D(ref newPixels, v, i, j, width);
                }
            }
            pixels = newPixels;
        }

        public void swapWH()
        {
            int t = width;

            width = height;
            height = t;
        }

        public void drawCircleCentered(double radius)
        {
            double x = (int)(width / 2);
            double y = (int)(height / 2);
            drawCircle(x, y, radius);
        }

        public void drawCircle(double x, double y, double r)
        {
            double minX = x - r - 1;
            double minY = y - r - 1;
            double maxX = x + r + 1;
            double maxY = y + r + 1;
            double r2 = r * r;
            double r2in = r2 - 2 * r + 1;
            double r2out = r2 + 2 * r + 1;
            int subPixels = 16;

            if (minX < 0) minX = 0;
            if (maxX > width - 1) maxX = width - 1;
            if (minY < 0) minY = 0;
            if (maxY > height - 1) maxY = height - 1;

            for (int i = (int)minX; i <= (int)maxX; ++i)
            {
                for (int j = (int)minY; j <= (int)maxY; ++j)
                {
                    double dx = i - x + 0.5;
                    double dy = j - y + 0.5;
                    double d2 = dx * dx + dy * dy;

                    if (d2 < r2in)
                    {
                        setPixel(i, j, 255);
                    }
                    else if (d2 > r2out) continue;
                    else
                    {
                        int c = 0;
                        for (int k = 0; k < subPixels; ++k)
                        {
                            for (int l = 0; l < subPixels; ++l)
                            {
                                dx = i + ((double)k / subPixels) + 0.5 / subPixels - x;
                                dy = j + ((double)l / subPixels) + 0.5 / subPixels - y;
                                d2 = dy * dy + dx * dx;
                                if (d2 <= r2) c++;
                            }
                        }
                        if (c > 255) c = 255;
                        setPixel(i, j, (byte)c);
                    }
                }
            }
        }

        public void drawPolygon(Polygon polygon)
        {
            int subPixels = 16;
            int[] subPixelMap = new int[width];
            int minY = (int)polygon.min.y;
            int maxY = (int)polygon.max.y;
            if (minY < 0) minY = 0;
            if (maxY > height - 1) maxY = height - 1;

            for (int i = 0; i < width; ++i)
                subPixelMap[i] = 0;

            List<double> segments = new List<double>();
            for (int j = minY; j <= maxY; ++j)
            {
                int minX = width;
                int maxX = 0;

                for (int l = 0; l < subPixels; ++l)
                {
                    double y = j + ((double)l / subPixels) + 0.5 / subPixels;
                    segments = polygon.intersectsAlongY(y);
                    if (segments.Count == 0) continue;

                    for (int i = 0; i < segments.Count; i += 2)
                    {
                        int x0 = (int)(segments[i] * subPixels);
                        int x1 = (int)(segments[i + 1] * subPixels);
                        if (x0 < 0) x0 = 0;
                        if (x1 > (width * subPixels - 1)) x1 = width * subPixels - 1;

                        for (int k = x0; k < x1; ++k)
                        {
                            int a = k / subPixels;
                            ++subPixelMap[a];
                        }
                    }
                    if (minX > (int)segments[0]) minX = (int)segments[0];
                    if (maxX < (int)segments[segments.Count - 1]) maxX = (int)segments[segments.Count - 1];
                }

                for (int i = minX; i <= maxX; ++i)
                {
                    setPixel(i, j, (byte) Math.Min(255, subPixelMap[i]));
                    subPixelMap[i] = 0;
                }
            }

        }

        public void overlay(ImageGS overlay, IntVector2 position)
        {
            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px < 0 || px >= width || py < 0 || py >= height) continue;
                    if (overlay.getPixel(i, j) > 0) setPixel(px, py, overlay.getPixel(i, j));
                }
            }
        }

        public void drawCharacter(char c, BitmapFont font, ref IntVector2 position)
        {
            if (Global.Debug4) Utils.Log("char {0}, x {1}, y {2}", c, position.x, position.y);

            ASP.BitmapChar charMap;
            IntVector2 cPos = new IntVector2();

            if (font.characterMap.TryGetValue(c, out charMap) == false)
            {
                c = '?';
                if (font.characterMap.TryGetValue(c, out charMap) == false) return;
            }

            cPos.x = position.x + (int)charMap.vx;
            cPos.y = position.y + (font.size + (int)charMap.vy + (int)charMap.vh);
            position.x += (int)charMap.cw;

            overlay(charMap.gsImage, cPos);
        }

        public void drawText(string text, BitmapFont font, IntVector2 position)
        {
            if (Global.Debug2) Utils.Log("text {0}, x {1}, y {2}", text, position.x, position.y);

            IntVector2 charPos = new IntVector2(position);
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
                        charPos.y -= font.size;
                        charPos.x = position.x;
                    }
                    if (c != '\\') escapeMode = false;
                }
                else drawCharacter(c, font, ref charPos);
            }
        }
    }
}