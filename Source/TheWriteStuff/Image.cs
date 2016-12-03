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

        public Image(IntVector2 size)
        {
            resize(size);
        }

        public Image(Image image)
        {
            resize(image.width, image.height);
            for (int i = 0; i < image.length; ++i)
                pixels[i] = image.pixels[i];
        }

        public void resize(int w, int h)
        {
            width = w;
            height = h;
            length = width * height;

            pixels = new Color32[length];
        }

        public void resize(IntVector2 size)
        {
            resize(size.x, size.y);
        }

        public void clear()
        {
            for (int i = 0; i < length; ++i)
            {
                pixels[i].r = 0;
                pixels[i].g = 0;
                pixels[i].b = 0;
                pixels[i].a = 0;
            }
        }

        public void resizeAndFill(int w, int h, Color32[] pix)
        {
            resize(w, h);

            for (int i = 0; i < length; ++i)
            {
                pixels[i] = pix[i];
            }
        }

        public void resizeAndFill(int w, int h, Color32[] pix, BoundingBox boundingBox)
        {
            if (boundingBox != null && boundingBox.valid && boundingBox.use)
            {
                resize(w, h);

                for (int i = 0; i < width; ++i)
                {
                    for (int j = 0; j < height; ++j)
                    {
                        if (boundingBox.inBox(i,j)) pixels[i + j * width] = pix[i + j *width];
                    }
                }
            }
            else resizeAndFill(w, h, pix);
        }

        public void fillAlpha(byte alpha)
        {
            for (int i = 0; i < length; ++i)
                pixels[i].a = alpha;
        }

        public void scaleAlpha(byte alpha)
        {
            for (int i = 0; i < length; ++i)
            {
                int newAlpha = Math.Min(((int)pixels[i].a * (int)alpha) / 255, 255);
                pixels[i].a = (byte) newAlpha;
            }
        }

        public void fill(Color32 color, bool fillAlpha = true)
        {
            for (int i = 0; i < length; ++i)
            {
                pixels[i].r = color.r;
                pixels[i].g = color.g;
                pixels[i].b = color.b;
                if (fillAlpha) pixels[i].a = color.a;
            }
        }

        public void fillFromGrayScale(ImageGS gs, Color32 color)
        {
            int w = Math.Min(width, gs.width);
            int h = Math.Min(height, gs.height);
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    if (gs.getPixel(i, j) > 0)
                    {
                        pixels[i + j * width].r = color.r;
                        pixels[i + j * width].g = color.g;
                        pixels[i + j * width].b = color.b;

                        int a = ((int)gs.getPixel(i, j) * (int)color.a / 255);
                        pixels[i + j * width].a = (byte) a;
                    }
                }
            }
        }

        public void recolor(Color32 from, Color32 to, bool checkAlpha, bool replaceAlpha)
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
                    if (replaceAlpha)
                    {
                        int a = (int)pixels[i].a * (int)to.a / 255;
                        pixels[i].a = (byte)Math.Min(a, 255);
                    }
                }
            }
        }

        public void recolorScaledByAlpha(Color32 low, Color32 high)
        {
            int dr = high.r - low.r;
            int dg = high.g - low.g;
            int db = high.b - low.b;
            int da = high.a - low.a;
            for (int i = 0; i < length; ++i)
            {
                if (pixels[i].a > 0)
                {
                    int v = pixels[i].a;
                    pixels[i].r = (byte)(dr * v / 255 + low.r);
                    pixels[i].g = (byte)(dg * v / 255 + low.g);
                    pixels[i].b = (byte)(db * v / 255 + low.b);
                    pixels[i].a = (byte)(da * v / 255 + low.a);
                }
            }
        }

        public void recolorScaledByGray(Color32 low, Color32 high)
        {
            int dr = high.r - low.r;
            int dg = high.g - low.g;
            int db = high.b - low.b;
            int da = high.a - low.a;
            for (int i = 0; i < length; ++i)
            {
                if (pixels[i].a > 0)
                {
                    int r = pixels[i].r;
                    int b = pixels[i].b;
                    int g = pixels[i].g;
                    int a = pixels[i].a;
                    int v = (int) Math.Sqrt((r*r + g*g + b*b) * a / (3*255));
                    pixels[i].r = (byte)(dr * v / (3 * 255) + low.r);
                    pixels[i].g = (byte)(dg * v / (3 * 255) + low.g);
                    pixels[i].b = (byte)(db * v / (3 * 255) + low.b);
                    pixels[i].a = (byte)(da * v / (3 * 255) + low.a);
                }
            }
        }

        public void edges(int scale = 1)
        {
            Color32[] newPixels = new Color32[length];

            int topLeft, top, topRight, left, right, bottomLeft, bottom, bottomRight;
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    int g = getGrayscale(i, j);
                    if (i == 0 || j == (height - 1)) topLeft = 127;
                    else topLeft = getGrayscale(i - 1, j + 1);

                    if (j == (height - 1)) top = 127;
                    else top = getGrayscale(i, j + 1);

                    if (i == (width - 1) || j == (height - 1)) topRight = 127;
                    else topRight = getGrayscale(i + 1, j + 1);

                    if (i == 0) left = 127;
                    else left = getGrayscale(i - 1, j);

                    if (i == (width - 1)) right = 127;
                    else right = getGrayscale(i + 1, j);

                    if (i == 0 || j == 0) bottomLeft = 127;
                    else bottomLeft = getGrayscale(i - 1, j - 1);

                    if (j == 0) bottom = 127;
                    else bottom = getGrayscale(i, j - 1);

                    if (i == (width - 1) || j == 0) bottomRight = 127;
                    else bottomRight = getGrayscale(i + 1, j - 1);

                    topLeft -= g;
                    top -= g;
                    topRight -= g;
                    left -= g;
                    right -= g;
                    bottomLeft -= g;
                    bottom -= g;
                    bottomRight -= g;

                    int d = topLeft * topLeft + top * top + topRight * topRight + left * left + right * right + bottomLeft * bottomLeft + bottom * bottom + bottomRight * bottomRight;
                    d = Math.Min(255, (int) ((Math.Sqrt(d / 8) * scale / 2) + 127));
                    newPixels[i + j * width].r = (byte)d;
                    newPixels[i + j * width].g = (byte)d;
                    newPixels[i + j * width].b = (byte)d;
                    newPixels[i + j * width].a = pixels[i + j * width].a;
                }
            }
            pixels = newPixels;
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
            if (p < 0 || p >= length) return;

            pixels[p].r = color.r;
            pixels[p].g = color.g;
            pixels[p].b = color.b;
            pixels[p].a = color.a;
        }

        public byte getGrayscale(int x, int y)
        {
            int gray = (pixels[x + y * width].r + pixels[x + y * width].g + pixels[x + y * width].b)/3;
            return (byte) gray;
        }

        public void swapWH()
        {
            int t = width;

            width = height;
            height = t;
        }

        public void drawFromGS(ImageGS gs, Color32 color)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int p = i + j * width;
                    int a = (color.a * gs.getPixel(i, j)) / 255;

                    if (a > 0)
                    {
                        pixels[p].r = color.r;
                        pixels[p].g = color.g;
                        pixels[p].b = color.b;
                        pixels[p].a = (byte)a;
                    }
                }
            }
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

        public void setMinMax(out int minX, out int minY, out int maxX, out int maxY, BoundingBox boundingBox)
        {
            minX = 0;
            maxX = width - 1;
            minY = 0;
            maxY = height - 1;
            if (boundingBox != null && boundingBox.valid && boundingBox.use)
            {
                boundingBox.fillLimits(ref minX, ref minY, ref maxX, ref maxY);
                if (maxX >= width) maxX = width - 1;
                if (maxY >= height) maxY = height - 1;
            }
        }

        public void overlay(Image overlay, Image mask, byte min, BoundingBox boundingBox)
        {
            int minX, minY, maxX, maxY;
            setMinMax(out minX, out minY, out maxX, out maxY, boundingBox);

            for (int i = minX; i <= maxX; ++i)
            {
                for (int j = minY; j <= maxY; ++j)
                {
                    if (mask.pixels[i + j * width].a >= min)
                    {
                        pixels[i + j * width].r = overlay.pixels[i + j * width].r;
                        pixels[i + j * width].g = overlay.pixels[i + j * width].g;
                        pixels[i + j * width].b = overlay.pixels[i + j * width].b;
                        pixels[i + j * width].a = overlay.pixels[i + j * width].a;
                    }
                }
            }
        }

        public void overlay(Image overlay, IntVector2 position, AlphaOption alphaOption, byte textureAlpha, BoundingBox boundingBox = null)
        {
            int minX, minY, maxX, maxY;
            setMinMax(out minX, out minY, out maxX, out maxY, boundingBox);

            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= minX && px <= maxX && py >= minY && py <= maxY)
                    {
                        if (overlay.pixels[i + j * overlay.width].a > 127)
                        {
                            pixels[px + py * width].r = overlay.pixels[i + j * overlay.width].r;
                            pixels[px + py * width].g = overlay.pixels[i + j * overlay.width].g;
                            pixels[px + py * width].b = overlay.pixels[i + j * overlay.width].b;

                            if (alphaOption == AlphaOption.OVERWRITE) pixels[px + py * width].a = textureAlpha;
                        }
                    }
                }
            }
        }

        public void blendRGB(Image overlay, IntVector2 position, AlphaOption alphaOption, byte textureAlpha, BoundingBox boundingBox = null)
        {
            int minX, minY, maxX, maxY;
            setMinMax(out minX, out minY, out maxX, out maxY, boundingBox);

            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= minX && px <= maxX && py >= minY && py <= maxY)
                    {
                        Color32 overlayColor = Utils.GetElement2D(overlay.pixels, i, j, overlay.width);

                        if (overlayColor.a > 0)
                        {
                            Color32 oldColor = Utils.GetElement2D(pixels, px, py, width);
                            Color32 newColor = Color32.Lerp(oldColor, overlayColor, (float) overlayColor.a / 255f);

                            pixels[px + py * width].r = newColor.r;
                            pixels[px + py * width].g = newColor.g;
                            pixels[px + py * width].b = newColor.b;

                            if (alphaOption == AlphaOption.OVERWRITE) pixels[px + py * width].a = textureAlpha;
                        }
                    }
                }
            }
        }

        public void blendSSR(Image overlay, IntVector2 position, AlphaOption alphaOption, byte textureAlpha, BoundingBox boundingBox = null)
        {
            int minX, minY, maxX, maxY;
            setMinMax(out minX, out minY, out maxX, out maxY, boundingBox);

            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= minX && px <= maxX && py >= minY && py <= maxY)
                    {
                        Color32 overlayColor = Utils.GetElement2D(overlay.pixels, i, j, overlay.width);

                        if (overlayColor.a > 0)
                        {
                            Color32 oldColor = Utils.GetElement2D(pixels, px, py, width);
                            double r = ((oldColor.r * oldColor.r * (255 - overlayColor.a)) / 255d) + ((overlayColor.r * overlayColor.r * overlayColor.a) / 255d);
                            double g = ((oldColor.g * oldColor.g * (255 - overlayColor.a)) / 255d) + ((overlayColor.g * overlayColor.g * overlayColor.a) / 255d);
                            double b = ((oldColor.b * oldColor.b * (255 - overlayColor.a)) / 255d) + ((overlayColor.b * overlayColor.b * overlayColor.a) / 255d);
                            r = Math.Sqrt(r);
                            g = Math.Sqrt(g);
                            b = Math.Sqrt(b);
                            pixels[px + py * width].r = (byte) r;
                            pixels[px + py * width].g = (byte) g;
                            pixels[px + py * width].b = (byte) b;

                            if (alphaOption == AlphaOption.OVERWRITE) pixels[px + py * width].a = textureAlpha;
                        }
                    }
                }
            }
        }

        public void blendHSV(Image overlay, IntVector2 position, AlphaOption alphaOption, byte textureAlpha, BoundingBox boundingBox = null)
        {
            int minX, minY, maxX, maxY;
            setMinMax(out minX, out minY, out maxX, out maxY, boundingBox);

            for (int i = 0; i < overlay.width; ++i)
            {
                for (int j = 0; j < overlay.height; ++j)
                {
                    int px = position.x + i;
                    int py = position.y + j;
                    if (px >= minX && px <= maxX && py >= minY && py <= maxY)
                    {
                        Color32 overlayColor = Utils.GetElement2D(overlay.pixels, i, j, overlay.width);
                        if (overlayColor.a > 0)
                        {
                            Color32 oldColor = Utils.GetElement2D(pixels, px, py, width);

                            RGB rgb1 = new RGB(oldColor);
                            RGB rgb2 = new RGB(overlayColor);
                            HSV hsv1 = rgb1.toHSV();
                            HSV hsv2 = rgb2.toHSV();
                            HSV hsv3 = hsv1.blend(hsv2, (double) overlayColor.a / 255d);
                            RGB rgb3 = hsv3.toRGB();

                            pixels[px + py * width].r = Math.Min((byte) (rgb3.r * 255d), (byte) 255);
                            pixels[px + py * width].g = Math.Min((byte) (rgb3.g * 255d), (byte) 255);
                            pixels[px + py * width].b = Math.Min((byte) (rgb3.b * 255d), (byte) 255);

                            if (alphaOption == AlphaOption.OVERWRITE) pixels[px + py * width].a = textureAlpha;
                        }
                    }
                }
            }
        }

        public void blendImage(Image image, BlendMethod blendMethod, IntVector2 position, AlphaOption alphaOption, byte alpha, BoundingBox boundingBox = null)
        {
            switch (blendMethod)
            {
                case ASP.BlendMethod.HSV:
                    blendHSV(image, position, alphaOption, alpha, boundingBox);
                    break;

                case ASP.BlendMethod.RGB:
                    blendRGB(image, position, alphaOption, alpha, boundingBox);
                    break;

                case ASP.BlendMethod.SSR:
                    blendSSR(image, position, alphaOption, alpha, boundingBox);
                    break;

                case ASP.BlendMethod.PIXEL:
                default:
                    overlay(image, position, alphaOption, alpha, boundingBox);
                    break;
            }
        }
 
        public void rotate(int rotation, ref IntVector2 origin)
        {
            if (rotation == 0) return;
            else if (rotation == 90)
            {
                flipXY(false);
                int oy = origin.y;
                origin.y = origin.x;
                origin.x = width - oy; 
            }
            else if (rotation == 180)
            {
                rotate180();
                origin.x = width - origin.x;
                origin.y = height - origin.y;
            }
            else if (rotation == 270)
            {
                flipXY(true);
                int ox = origin.x;
                origin.x = origin.y;
                origin.y = height - ox;
            }
            else rotateExact(rotation, ref origin);
        }

        public void rotateExact(double rotation, ref IntVector2 origin)
        {
            const int subPixels = 8;
            const int subPixelThreshold = 56;

            int size = width;
            if (height > size) size = height;
            size = (int)(size * 2.5d);

            long[,,] map = new long[size, size, 5];
            Array.Clear(map, 0, map.Length);

            IntVector2 originNew = new IntVector2(size / 2, size / 2);
            IntVector2 min = new IntVector2(size, size);
            IntVector2 max = new IntVector2(0, 0);
            IntVector2 rpos = new IntVector2(0, 0);

            double sr = Math.Sin(2 * Math.PI * (rotation / 360d));
            double cr = Math.Cos(2 * Math.PI * (rotation / 360d));

            if (Global.Debug3)
            {
                Utils.Log("Rotate {0} degrees", rotation);
                Utils.Log("Size: {0} x {1} -> {2} x {3}", width, height, size, size);
                Utils.Log("Centres: ({0}, {1}) -> ({2}, {3})", origin.x, origin.y, originNew.x, originNew.y);
            }


            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (pixels[i + j * width].a > 0)
                    {
                        byte r = pixels[i + j * width].r;
                        byte g = pixels[i + j * width].g;
                        byte b = pixels[i + j * width].b;
                        byte a = pixels[i + j * width].a;

                        for (int k = 0; k < subPixels; ++k)
                        {
                            double x0 = i - origin.x + ((k + 0.5d) / (double)subPixels);
                            for (int l = 0; l < subPixels; ++l)
                            {
                                double y0 = j - origin.y + ((l + 0.5d) / (double)subPixels);
                                double x1 = x0 * cr - y0 * sr + originNew.x;
                                double y1 = x0 * sr + y0 * cr + originNew.y;
                                rpos.x = (int)(x1);
                                rpos.y = (int)(y1);

                                map[rpos.x, rpos.y, 0] += r * r * a;
                                map[rpos.x, rpos.y, 1] += g * g * a;
                                map[rpos.x, rpos.y, 2] += b * b * a;
                                map[rpos.x, rpos.y, 3] += a;
                                map[rpos.x, rpos.y, 4] += 1;

                                if (rpos.x < min.x) min.x = rpos.x;
                                if (rpos.x > max.x) max.x = rpos.x;
                                if (rpos.y < min.y) min.y = rpos.y;
                                if (rpos.y > max.y) max.y = rpos.y;
                            }
                        }
                    }
                }
            }

            resize(max.x - min.x + 1, max.y - min.y + 1);
            if (Global.Debug3) Utils.Log("New size: {0} x {1}", width, height);
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    int x = i + min.x;
                    int y = j + min.y;

                    if (map[x, y, 3] > 0 && map[x, y, 4] > 0)
                    {
                        byte r = (byte)Math.Sqrt(map[x, y, 0] / map[x, y, 3]);
                        byte g = (byte)Math.Sqrt(map[x, y, 1] / map[x, y, 3]);
                        byte b = (byte)Math.Sqrt(map[x, y, 2] / map[x, y, 3]);

                        byte a = 0;
                        if (map[x, y, 4] >= subPixelThreshold) a = (byte)Math.Min(255, (map[x, y, 3] / map[x, y, 4]));
                        else a = (byte)Math.Min(255, (map[x, y, 3] / subPixelThreshold));

                        pixels[i + j * width].r = r;
                        pixels[i + j * width].g = g;
                        pixels[i + j * width].b = b;
                        pixels[i + j * width].a = a;
                    }
                }
            }

            origin.x = originNew.x - min.x;
            origin.y = originNew.y - min.y;
        }

        public void clip(BoundingBox boundingBox)
        {
            if (!boundingBox.valid || !boundingBox.use) return;
            if (boundingBox.x == 0 && boundingBox.y == 0 && boundingBox.w == width && boundingBox.h == height) return;

            Color32[] newPixels = new Color32[boundingBox.w * boundingBox.h];

            for (int i = 0; i < boundingBox.w; ++i)
            {
                for (int j = 0; j < boundingBox.h; ++j)
                {
                    int px = boundingBox.x + i;
                    int py = boundingBox.y + j;
                    if (px >= 0 && px < width && py >= 0 && py < height) newPixels[i + j * boundingBox.w] = pixels[px + py * width];
                }
            }

            pixels = newPixels;
            width = boundingBox.w;
            height = boundingBox.h;
            length = width * height;
        }

        public Image createNormalMap(double scale)
        {
            if (Global.Debug2) Utils.Log("create normalMap {0},{1}", width, height);
            Image normalMap = new Image(width, height);

            int topLeft, top, topRight, left, right, bottomLeft, bottom, bottomRight;
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (i == 0 || j == (height - 1)) topLeft = 127;
                    else topLeft = getGrayscale(i - 1, j + 1);

                    if (j == (height - 1)) top = 127;
                    else top = getGrayscale(i, j + 1);

                    if (i == (width - 1) || j == (height - 1)) topRight = 127;
                    else topRight = getGrayscale(i + 1, j + 1);

                    if (i == 0) left = 127;
                    else left = getGrayscale(i - 1, j);

                    if (i == (width - 1)) right = 127;
                    else right = getGrayscale(i + 1, j);

                    if (i == 0 || j == 0) bottomLeft = 127;
                    else bottomLeft = getGrayscale(i - 1, j - 1);

                    if (j == 0) bottom = 127;
                    else bottom = getGrayscale(i, j - 1);

                    if (i == (width - 1) || j == 0) bottomRight = 127;
                    else bottomRight = getGrayscale(i + 1, j - 1);

                    // sobel filter
                    int dX = (topRight + 2 * right + bottomRight) - (topLeft + 2 * left + bottomLeft);
                    int dY = (bottomLeft + 2 * bottom + bottomRight) - (topLeft + 2 * top + topRight);
                    int dZ = (int) (255f / scale);

                    Vector3 normal = new Vector3(dX, dY, dZ);
                    normal.Normalize();

                    // change range from -1,+1 to 0,+1
                    normal = normal + new Vector3(1f, 1f, 1f);
                    normal = normal / 2f;

                    // change to unity normal format
                    normalMap.pixels[i + j * width].r = (byte)(Math.Min(normal.y * 255f, 255f));
                    normalMap.pixels[i + j * width].g = (byte)(Math.Min(normal.y * 255f, 255f));
                    normalMap.pixels[i + j * width].b = (byte)(Math.Min(normal.y * 255f, 255f));
                    normalMap.pixels[i + j * width].a = (byte)(Math.Min(normal.x * 255f, 255f));
                }
            }
            return normalMap;
        }

        public Color32 getPixelBilinear(double x, double y, double w, double h)
        {
            double red = 0d;
            double green = 0d;
            double blue = 0d;
            double alpha = 0d;
            double totalArea = 0d;

            double minX = (x - w / 2d) * width;
            double minY = (y - w / 2d) * height;
            double maxX = (x + w / 2d) * width;
            double maxY = (y + w / 2d) * height;
            int minPX = (int) minX;
            int minPY = (int) minY;
            int maxPX = (int) maxX;
            int maxPY = (int) maxY;

            for (int i = minPX; i <= maxPX; ++i)
            {
                for (int j = minPY; j <= maxPY; ++j)
                {
                    if (i >= 0 && i < width && j >= 0 && j < height)
                    {
                        double area = 0d;
                        if (i == minPX || j == minPY || (i + 1) > maxPX || (j + 1) > maxPY)
                        {
                            double pxMin = Math.Max(i, minX);
                            double pxMax = Math.Min(i + 1, maxX);
                            double pyMin = Math.Max(j, minY);
                            double pyMax = Math.Min(j + 1, maxY);
                            area = (pxMax - pxMin) * (pyMax - pyMin);
                        }
                        else area = 1d;

                        red += pixels[i + j * width].r * area;
                        green += pixels[i + j * width].g * area;
                        blue += pixels[i + j * width].b * area;
                        alpha += pixels[i + j * width].a * area;
                        totalArea += area;
                    }
                }
            }

            if (totalArea > 0d)
            {
                red /= totalArea;
                green /= totalArea;
                blue /= totalArea;
                alpha /= totalArea;
            }

            byte r = (byte)(Math.Min((int)red, 255));
            byte g = (byte)(Math.Min((int)green, 255));
            byte b = (byte)(Math.Min((int)blue, 255));
            byte a = (byte)(Math.Min((int)alpha, 255));

            return new Color32(r, g, b, a);
        }

        public void rescale(int newWidth, int newHeight)
        {
            if (Global.Debug2) Utils.Log("rescale from ({0},{1}) to ({2},{3})", width, height, newWidth, newHeight);

            if (newWidth == width && newHeight == height) return;

            Color32[] newPixels = new Color32[newWidth * newHeight];

            double pixelWidth = 1f / (double)newWidth;
            double pixelHeight = 1f / (double)newHeight;

            for (int i = 0; i < newWidth; ++i)
            {
                for (int j = 0; j < newHeight; ++j)
                {
                    double x = (((double)i + 0.5d) / (double)newWidth);
                    double y = (((double)j + 0.5d) / (double)newHeight);

                    newPixels[i + j * newWidth] = getPixelBilinear(x, y, pixelWidth, pixelHeight);
                }
            }

            width = newWidth;
            height = newHeight;
            length = width * height;
            pixels = newPixels;
        }
    }
}
