using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* DDS Loader initially from: 
 * https://github.com/sarbian/DDSLoader
 * 
 * and then mucked about with
 * 
 * Used under the MIT license below
 */

/*
The MIT License (MIT)

Copyright (c) 2014 sarbian

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


namespace ASP
{
    class DDSTexture
    {
        public static uint ReadUInt(byte[] bytes, int offset)
        {
            uint value = (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
            return value;
        }

        public static int ReadInt(byte[] bytes, int offset)
        {
            int value = bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
            return value;
        }

        public static string ReadChars(byte[] bytes, int offset, int length)
        {
            string value = "";
            for (int i = 0; i < length; ++i)
            {
                value += (char)bytes[offset + i];
            }
            return value;
        }

        // DDS Texture loader inspired by
        // http://answers.unity3d.com/questions/555984/can-you-load-dds-textures-during-runtime.html#answer-707772
        // http://msdn.microsoft.com/en-us/library/bb943992.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/bb205578(v=vs.85).aspx
        // mipmapBias limits the number of mipmap when > 0
        public static Texture2D Load(byte[] bytes, bool normalMap)
        {
            const uint DDSD_MIPMAPCOUNT_BIT = 0x00020000;
            const uint DDPF_ALPHAPIXELS = 0x00000001;
            const uint DDPF_ALPHA = 0x00000002;
            const uint DDPF_FOURCC = 0x00000004;
            const uint DDPF_RGB = 0x00000040;
            const uint DDPF_YUV = 0x00000200;
            const uint DDPF_LUMINANCE = 0x00020000;
            const uint DDPF_NORMAL = 0x80000000;

            uint ddsMagic = ReadUInt(bytes, 0);
            if (Global.Debug3) Utils.Log("ddsMagic: {0}", ddsMagic);

            int headerSize = ReadInt(bytes, 4);
            if (Global.Debug3) Utils.Log("headerSize: {0}", headerSize);
            if (headerSize != 124)
            {
                Utils.LogError("Invalid header size: {0}", headerSize);
                return new Texture2D(1, 1);
            }

            int flags = ReadInt(bytes, 8);
            int height = ReadInt(bytes, 12);
            int width = ReadInt(bytes, 16);
            int pitchOrLinearSize = ReadInt(bytes, 20);
            int depth = ReadInt(bytes, 24);
            int mipMapCount = ReadInt(bytes, 28);

            if ((flags & DDSD_MIPMAPCOUNT_BIT) == 0) mipMapCount = 1;


            uint dds_pxlf_size = ReadUInt(bytes, 76);
            uint dds_pxlf_flags = ReadUInt(bytes, 80);
            string fourCC = ReadChars(bytes, 84, 4);
            uint dds_pxlf_RGBBitCount = ReadUInt(bytes, 88);
            uint pixelSize = dds_pxlf_RGBBitCount / 8;
            uint dds_pxlf_RBitMask = ReadUInt(bytes, 92);
            uint dds_pxlf_GBitMask = ReadUInt(bytes, 96);
            uint dds_pxlf_BBitMask = ReadUInt(bytes, 100);
            uint dds_pxlf_ABitMask = ReadUInt(bytes, 104);

            int caps = ReadInt(bytes, 104);
            int caps2 = ReadInt(bytes, 108);
            int caps3 = ReadInt(bytes, 112);
            int caps4 = ReadInt(bytes, 116);

            TextureFormat textureFormat = TextureFormat.ARGB32;
            bool isCompressed = false;

            bool alpha = (dds_pxlf_flags & DDPF_ALPHA) != 0;
            bool fourcc = (dds_pxlf_flags & DDPF_FOURCC) != 0;
            bool rgb = (dds_pxlf_flags & DDPF_RGB) != 0;
            bool alphapixel = (dds_pxlf_flags & DDPF_ALPHAPIXELS) != 0;
            bool luminance = (dds_pxlf_flags & DDPF_LUMINANCE) != 0;
            bool rgb888 = dds_pxlf_RBitMask == 0x000000ff && dds_pxlf_GBitMask == 0x0000ff00 && dds_pxlf_BBitMask == 0x00ff0000;
            bool bgr888 = dds_pxlf_RBitMask == 0x00ff0000 && dds_pxlf_GBitMask == 0x0000ff00 && dds_pxlf_BBitMask == 0x000000ff;
            bool rgb565 = dds_pxlf_RBitMask == 0x0000F800 && dds_pxlf_GBitMask == 0x000007E0 && dds_pxlf_BBitMask == 0x0000001F;
            bool argb4444 = dds_pxlf_ABitMask == 0x0000f000 && dds_pxlf_RBitMask == 0x00000f00 && dds_pxlf_GBitMask == 0x000000f0 && dds_pxlf_BBitMask == 0x0000000f;
            bool rbga4444 = dds_pxlf_ABitMask == 0x0000000f && dds_pxlf_RBitMask == 0x0000f000 && dds_pxlf_GBitMask == 0x000000f0 && dds_pxlf_BBitMask == 0x00000f00;

            if (fourcc)
            {
                // Texture dos not contain RGB data, check FourCC for format
                isCompressed = true;

                if (fourCC == "DXT1") textureFormat = TextureFormat.DXT1;
                else if (fourCC == "DXT5") textureFormat = TextureFormat.DXT5;
            }
            else if (rgb && (rgb888 || bgr888))
            {
                // RGB or RGBA format
                if (alphapixel) textureFormat = TextureFormat.RGBA32;
                else textureFormat = TextureFormat.RGB24;
            }
            else if (rgb && rgb565)
            {
                // Nvidia texconv B5G6R5_UNORM
                textureFormat = TextureFormat.RGB565;
            }
            else if (rgb && alphapixel && argb4444)
            {
                // Nvidia texconv B4G4R4A4_UNORM
                textureFormat = TextureFormat.ARGB4444;
            }
            else if (rgb && alphapixel && rbga4444)
            {
                textureFormat = TextureFormat.RGBA4444;
            }
            else if (!rgb && alpha != luminance)
            {
                // A8 format or Luminance 8
                textureFormat = TextureFormat.Alpha8;
            }
            else
            {
                Utils.LogError("Only DXT1, DXT5, A8, RGB24, BGR24, RGBA32, BGBR32, RGB565, ARGB4444 and RGBA4444 are supported");
                return new Texture2D(1, 1);
            }
            if (Global.Debug3)
            {
                Utils.Log("Load DDS Texture");
                Utils.Log("height: {0}", height);
                Utils.Log("width: {0}", width);
                Utils.Log("format: {0}", textureFormat);
            }

            byte[] data = new byte[bytes.Length - 128];
            for (int i = 0; i < data.Length; ++i)
                data[i] = bytes[i + 128];

            // Swap red and blue.
            if (!isCompressed && bgr888)
            {
                for (uint i = 0; i < data.Length; i += pixelSize)
                {
                    byte b = data[i + 0];
                    byte r = data[i + 2];

                    data[i + 0] = r;
                    data[i + 2] = b;
                }
            }

            //QualitySettings.masterTextureLimit = 0;
            // Work around for an >Unity< Bug.
            // if QualitySettings.masterTextureLimit != 0 (half or quarter texture rez)
            // and dwWidth and dwHeight divided by 2 (or 4 for quarter rez) are not a multiple of 4 
            // and we are creating a DXT5 or DXT1 texture
            // Then you get an Unity error on the "new Texture"
            int quality = QualitySettings.masterTextureLimit;

            // If the bug conditions are present then switch to full quality
            if (isCompressed && quality > 0 && (width >> quality) % 4 != 0 && (height >> quality) % 4 != 0) QualitySettings.masterTextureLimit = 0;

            Texture2D texture = new Texture2D(width, height, textureFormat, mipMapCount > 1);
            texture.LoadRawTextureData(data);

            return texture;
        }
    }
}
