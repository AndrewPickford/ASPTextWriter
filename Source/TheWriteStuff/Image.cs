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

        public void fill(Color32 color)
        {
            for (int i = 0; i < length; ++i)
            {
                pixels[i].r = color.r;
                pixels[i].g = color.g;
                pixels[i].b = color.b;
                pixels[i].a = color.a;
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
                    if (mask.pixels[i + j * width].a >= 10)
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
                            HSV hsv3 = hsv1.blend(hsv2, (float) overlayColor.a / 255f);
                            RGB rgb3 = hsv3.toRGB();

                            Color32 newColor = new Color(rgb3.r, rgb3.g, rgb3.b, (float) overlayColor.a / 255f);

                            pixels[px + py * width].r = newColor.r;
                            pixels[px + py * width].g = newColor.g;
                            pixels[px + py * width].b = newColor.b;

                            if (alphaOption == AlphaOption.OVERWRITE) pixels[px + py * width].a = textureAlpha;
                        }
                    }
                }
            }
        }

        public void drawCharacter(char c, BitmapFont font, ref IntVector2 position, Rotation rotation, Color32 color, bool mirror, AlphaOption alphaOption,
                                  byte textureAlpha, BlendMethod blendMethod, BoundingBox boundingBox = null)
        {
            if (Global.Debug3) Utils.Log("char {0}, x {1}, y {2}", c, position.x, position.y);

            ASP.BitmapChar charMap;
            IntVector2 cPos = new IntVector2();

            if (font.characterMap.TryGetValue(c, out charMap) == false)
            {
                c = '?';
                if (font.characterMap.TryGetValue(c, out charMap) == false) return;
            }

            Image charImage = new Image(charMap.image);
            charImage.recolor(Global.Black32, color, false, true);

            if (mirror) charImage.flipHorizontally();

            switch (rotation)
            {
                case Rotation.R180:
                    charImage.rotate180();
                    cPos.x = position.x - ((int)charMap.vx + (int)charMap.vw);
                    cPos.y = position.y - (font.size + (int)charMap.vy);
                    position.x -= (int) charMap.cw;
                    break;

                case Rotation.R270:
                    charImage.flipXY(true);
                    cPos.x = position.x + (font.size + (int)charMap.vy + (int)charMap.vh);
                    cPos.y = position.y - ((int)charMap.vx + (int)charMap.vw);
                    position.y -= (int)charMap.cw;
                    break;

                case Rotation.R90:
                    charImage.flipXY(false);
                    cPos.x = position.x - (font.size + (int)charMap.vy);
                    cPos.y = position.y + (int)charMap.vx;
                    position.y += (int)charMap.cw;
                    break;

                case Rotation.R0:
                default:
                    cPos.x = position.x + (int)charMap.vx;
                    cPos.y = position.y + (font.size + (int)charMap.vy + (int)charMap.vh);
                    position.x += (int)charMap.cw;
                    break;
            }

            switch (blendMethod)
            {
                case ASP.BlendMethod.HSV:
                    blendHSV(charImage, cPos, alphaOption, textureAlpha, boundingBox);
                    break;

                case ASP.BlendMethod.RGB:
                    blendRGB(charImage, cPos, alphaOption, textureAlpha, boundingBox);
                    break;

                case ASP.BlendMethod.PIXEL:
                default:
                    overlay(charImage, cPos, alphaOption, textureAlpha, boundingBox);
                    break;
            }
        }

        public void drawText(string text, string fontName, int fontSize, IntVector2 position, Rotation rotation, Color32 color, bool mirror, AlphaOption alphaOption,
                             byte textureAlpha, BlendMethod blendMethod, BoundingBox boundingBox = null)
        {
            BitmapFont font = BitmapFontCache.Instance.getFontByNameSize(fontName, fontSize);
            if (font == null) font = BitmapFontCache.Instance.fonts.First();

            drawText(text, font, position, rotation, color, mirror, alphaOption, textureAlpha, blendMethod, boundingBox);
        }

        public void drawText(string text, BitmapFont font, IntVector2 position, Rotation rotation, Color32 color, bool mirror, AlphaOption alphaOption,
                             byte textureAlpha, BlendMethod blendMethod, BoundingBox boundingBox = null)
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
                        switch (rotation)
                        {
                            case Rotation.R90:
                                charPos.x += font.size;
                                charPos.y = position.y;
                                break;
                            case Rotation.R270:
                                charPos.x -= font.size;
                                charPos.y = position.y;
                                break;

                            case Rotation.R0:
                                charPos.y -= font.size;
                                charPos.x = position.x;
                                break;
                            case Rotation.R180:
                            default:
                                charPos.y += font.size;
                                charPos.x = position.x;
                                break;
                        }
                    }
                    if (c != '\\') escapeMode = false;
                }
                else drawCharacter(c, font, ref charPos, rotation, color, mirror, alphaOption, textureAlpha, blendMethod, boundingBox);
            }
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

        public Image createNormalMap(float scale)
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

        public Color32 getPixelBilinear(float x, float y, float w, float h)
        {
            float red = 0f;
            float green = 0f;
            float blue = 0f;
            float alpha = 0f;
            float totalArea = 0f;

            float minX = x - w/2f;
            float minY = y - w/2f;
            float maxX = x + w/2f;
            float maxY = y + w/2f;
            int minPX = (int) minX;
            int minPY = (int) minY;
            int maxPX = (int) maxX;
            int maxPY = (int) maxY;

            for (int i = minPX; i <= maxPX; ++i)
            {
                for (int j = minPY; j <=maxPY; ++j)
                {
                    if (i >= 0 && i < width && j >= 0 && j < height)
                    {
                        float area = 1f;
                        if (i == minPX || j == minPY || (i + 1) > maxPX || (j + 1) > maxPY)
                        {
                            float pxMin = Math.Max(i, minX);
                            float pxMax = Math.Min(i + 1, maxX);
                            float pyMin = Math.Max(j, minY);
                            float pyMax = Math.Min(j + 1, maxY);
                            area = (pxMax - pxMin) * (pyMax - pyMin);
                        }

                        red = pixels[i + j * width].r * area;
                        green = pixels[i + j * width].g * area;
                        blue = pixels[i + j * width].b * area;
                        alpha = pixels[i + j * width].a * area;
                        totalArea += area;
                    }
                }
            }

            if (totalArea > 0f)
            {
                red /= totalArea;
                green /= totalArea;
                blue /= totalArea;
                alpha /= totalArea;
            }

            byte r = (byte)(Math.Max((int)(red / 255f), 255));
            byte g = (byte)(Math.Max((int)(green / 255f), 255));
            byte b = (byte)(Math.Max((int)(blue / 255f), 255));
            byte a = (byte)(Math.Max((int)(alpha / 255f), 255));

            return new Color32(r, g, b, a);
        }

        public void rescale(int newWidth, int newHeight)
        {
            if (Global.Debug2) Utils.Log("rescale from ({0},{1}) to ({2},{3})", width, height, newWidth, newHeight);

            if (newWidth == width && newHeight == height) return;

            Color32[] newPixels = new Color32[width * height];

            float widthRatio = (float) width / (float) newWidth;
            float heightRatio = (float) height / (float) newHeight;

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    float x = 0.5f + ((float)i / (float)width);
                    float y = 0.5f + ((float)j / (float)height);

                    newPixels[i + j * width] = getPixelBilinear(x, y, widthRatio, heightRatio);
                }
            }

            width = newWidth;
            height = newHeight;
            length = width * height;
            pixels = newPixels;
        }
    }
}
