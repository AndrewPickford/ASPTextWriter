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
 * The MBMToTexture function from Active Texture Management
 * Modified a bit
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
        public static Texture2D MBMToTexture(string fileName, bool mipmaps)
        {
            bool isNormalMap = false;
            FileStream mbmStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            mbmStream.Position = 4;

            uint width = 0, height = 0;
            for (int b = 0; b < 4; b++)
            {
                width >>= 8;
                width |= (uint)(mbmStream.ReadByte() << 24);
            }
            for (int b = 0; b < 4; b++)
            {
                height >>= 8;
                height |= (uint)(mbmStream.ReadByte() << 24);
            }
            mbmStream.Position = 12;
            if (mbmStream.ReadByte() == 1)
            {
                isNormalMap = true;
            }
            

            mbmStream.Position = 16;
            int format = mbmStream.ReadByte();
            mbmStream.Position += 3;

            int imageSize = (int)(width * height * 3);
            TextureFormat texformat = TextureFormat.RGB24;
            bool alpha = false;
            if (format == 32)
            {
                imageSize += (int)(width * height);
                texformat = TextureFormat.ARGB32;
                alpha = true;
            }
            if (isNormalMap)
            {
                texformat = TextureFormat.ARGB32;
            }

            Byte[] bytes = new Byte[imageSize];
            mbmStream.Read(bytes, 0, imageSize);
            mbmStream.Close();

            Texture2D texture = new Texture2D((int) width, (int) height, texformat, mipmaps);
            Color32[] colors = new Color32[width * height];
            int n = 0;
            for (int i = 0; i < width * height; i++)
            {
                colors[i].r = bytes[n++];
                colors[i].g = bytes[n++];
                colors[i].b = bytes[n++];
                if (alpha)
                {
                    colors[i].a = bytes[n++];
                }
                else
                {
                    colors[i].a = 255;
                }
            }

            texture.SetPixels32(colors);

            return texture;
        }

    }
}
