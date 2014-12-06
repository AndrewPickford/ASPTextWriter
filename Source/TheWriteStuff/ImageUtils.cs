using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ImageUtils
    {
        public static void Recolor(ref Color[] image, Color from, Color to)
        {
            for (int i = 0; i < image.Length; i++)
            {
                if (image[i].r == from.r && image[i].g == from.g && image[i].b == from.b)
                {
                    image[i].r = to.r;
                    image[i].g = to.g;
                    image[i].b = to.b;
                }
            }
        }

        public static Color[] FlipXY(Color[] pixels, int width, int height, bool positive = true)
        {
            Color[] newPixels = new Color[pixels.Length];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color newColor = Utils.GetElement2D(pixels, i, j, width);

                    if (positive) Utils.SetElement2D(ref newPixels, newColor, j, width - 1 - i, height);
                    else Utils.SetElement2D(ref newPixels, newColor, height - 1 - j, i, height);
                }

            }

            return newPixels;
        }

        public static Color[] FlipVertically(Color[] pixels, int width, int height)
        {
            Color[] newPixels = new Color[pixels.Length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    newPixels[i + (height - j - 1) * width] = pixels[i + j * width];
                }
            }

            return newPixels;
        }

        public static Color[] FlipHorizontally(Color[] pixels, int width, int height)
        {
            Color[] newPixels = new Color[pixels.Length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    newPixels[(width - i - 1) + j * width] = pixels[i + j * width];
                }
            }

            return newPixels;
        }

        public static Color[] Rotate180(Color[] pixels, int width, int height)
        {
            Color[] newPixels = new Color[pixels.Length];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color newColor = Utils.GetElement2D(pixels, width - i - 1, height - j - 1, width);
                    Utils.SetElement2D(ref newPixels, newColor, i, j, width);
                }
            }

            return newPixels;
        }

        public static void BlendPixel(ref Color[] background, Color[] image, int width, int height)
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color imageColor = Utils.GetElement2D(image, i, j, width);
                    Color backgroundColor = Utils.GetElement2D(background, i, j, width);

                    if (imageColor.a == 1f) Utils.SetElement2D(ref background, imageColor, i, j, width);
                }
            }
        }

        public static void BlendRGB(ref Color[] background, Color[] image, int width, int height)
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color imageColor = Utils.GetElement2D(image, i, j, width);
                    Color backgroundColor = Utils.GetElement2D(background, i, j, width);
                    Color newColor = Color.Lerp(backgroundColor, imageColor, imageColor.a);

                    Utils.SetElement2D(ref background, newColor, i, j, width);
                }
            }
        }

        public static void BlendHSV(ref Color[] background, Color[] image, int width, int height)
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color imageColor = Utils.GetElement2D(image, i, j, width);
                    Color backgroundColor = Utils.GetElement2D(background, i, j, width);

                    if (imageColor.a == 1f) Utils.SetElement2D(ref background, imageColor, i, j, width);
                    else if (imageColor.a > 0f)
                    {
                        RGB rgb1 = new RGB(backgroundColor.r, backgroundColor.g, backgroundColor.b);
                        RGB rgb2 = new RGB(imageColor.r, imageColor.g, imageColor.b);
                        HSV hsv1 = rgb1.toHSV();
                        HSV hsv2 = rgb2.toHSV();
                        HSV hsv3 = hsv1.blend(hsv2, imageColor.a);
                        RGB rgb3 = hsv3.toRGB();

                        Color newColor = new Color(rgb3.r, rgb3.g, rgb3.b, imageColor.a);
                        Utils.SetElement2D(ref background, newColor, i, j, width);
                    }
                }
            }
        }
    }
}
