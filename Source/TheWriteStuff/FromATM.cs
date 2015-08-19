/*
 * 
 * Compresses the Textures after loading in KSP
 * The MIT License (MIT)
 * Copyright (c) 2013 Ryan Bray
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
 * associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions
 * of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

/*
 * Initial MBMToTexture and TGAToTexture functions from Active Texture Management
 * Modified a fair bit by A. Pickford
 * 
 */


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    class FromATM
    {
        public static Texture2D MBMToTexture(byte[] bytes, bool normalMap)
        {
            bool isNormalMap = false;

            uint width = 0, height = 0;
            for (int b = 0; b < 4; b++)
            {
                width >>= 8;
                width |= (uint)(bytes[4+b] << 24);
            }
            for (int b = 0; b < 4; b++)
            {
                height >>= 8;
                height |= (uint)(bytes[8+b] << 24);
            }
            if (bytes[12] == 1)
            {
                isNormalMap = true;
            }
            
            int format = bytes[16];

            TextureFormat texformat = TextureFormat.RGB24;
            bool alpha = false;
            int mult = 3;
            if (format == 32 || isNormalMap)
            {
                texformat = TextureFormat.ARGB32;
                alpha = true;
                mult = 4;
            }
            if (normalMap) texformat = TextureFormat.ARGB32;

            Texture2D texture = new Texture2D((int)width, (int)height, texformat, false);
            Color32[] colors = new Color32[width * height];
            for (int i = 0; i < width * height; i++)
            {
                colors[i].r = bytes[20 + i*mult];
                colors[i].g = bytes[20 + i*mult + 1];
                colors[i].b = bytes[20 + i*mult + 2];
                if (alpha)
                {
                    colors[i].a = bytes[20 + i*mult + 3];
                }
                else
                {
                    colors[i].a = 255;
                }
            }

            texture.SetPixels32(colors);

            return texture;
        }

        private static Color32[] TGAType2(byte[] bytes, int width, int height, bool alpha)
        {
            Color32[] colors = new Color32[width * height];

            int mult = 3;
            if (alpha) mult = 4;
            for (int i = 0; i < width * height; i++)
            {
                colors[i].b = bytes[18 + i*mult];
                colors[i].g = bytes[18 + i*mult + 1];
                colors[i].r = bytes[18 + i*mult + 2];
                if (alpha)
                {
                    colors[i].a = bytes[18 + i*mult + 3];
                }
                else
                {
                    colors[i].a = 255;
                }
            }

            return colors;
        }

        private static Color32[] TGAType10(byte[] bytes, int width, int height, bool alpha)
        {
            Color32[] colors = new Color32[width * height];

            int i = 0;
            int run = 0;
            int n = 18;
            while (i < width * height)
            {
                run = bytes[n++];
                if ((run & 0x80) != 0)
                {
                    run = (run ^ 0x80) + 1;
                    colors[i].b = bytes[n++];
                    colors[i].g = bytes[n++];
                    colors[i].r = bytes[n++];

                    if (alpha) colors[i].a = bytes[n++];
                    else colors[i].a = 255;
                    
                    i++;
                    for (int c = 1; c < run; c++, i++)
                    {
                        colors[i] = colors[i - 1];
                    }
                }
                else
                {
                    run += 1;
                    for (int c = 0; c < run; c++, i++)
                    {
                        colors[i].b = bytes[n++];
                        colors[i].g = bytes[n++];
                        colors[i].r = bytes[n++];

                        if (alpha) colors[i].a = bytes[n++];
                        else colors[i].a = 255;
                    }
                }
            }

            return colors;
        }

        public static Texture2D TGAToTexture(byte[] bytes, bool normalMap)
        {
            byte imgType = bytes[2];
            int width = bytes[12] | (bytes[13] << 8);
            int height = bytes[14] | (bytes[15] << 8);

            int depth = bytes[16];
            bool alpha = false;
            TextureFormat format = TextureFormat.RGB24;
            if (depth == 32)
            {
                alpha = true;
                format = TextureFormat.ARGB32;
            }
            if (normalMap) format = TextureFormat.ARGB32;

            Utils.Log("TGAToTexture: Loading TGA file format {0}", imgType);

            Color32[] colors;
            if (imgType == 2) colors = TGAType2(bytes, width, height, alpha);
            else if (imgType == 10) colors = TGAType10(bytes, width, height, alpha);
            else
            {
                Utils.LogError("TGAToTexture: Unsupported TGA format {0}", imgType);
                colors = new Color32[width * height];
            }

            Texture2D texture = new Texture2D(width, height, format, false);
            texture.SetPixels32(colors);

            return texture;
        }
    }
}
