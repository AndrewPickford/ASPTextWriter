using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class NormalMap
    {
        public static Texture2D Create(Texture2D heightMap, float scale = 2.0f)
        {
            Texture2D normalMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.ARGB32, false);

            float topLeft, top, topRight, left, right, bottomLeft, bottom, bottomRight;
            Color color;
            for (int i = 0; i < heightMap.width; ++i)
            {
                for (int j = 0; j < heightMap.height; ++j)
                {
                    if (i == 0 || j == (heightMap.height - 1)) topLeft = 0.5f;
                    else topLeft = heightMap.GetPixel(i - 1, j + 1).grayscale;

                    if (j == (heightMap.height - 1)) top = 0.5f;
                    else top = heightMap.GetPixel(i, j + 1).grayscale;

                    if (i == (heightMap.width - 1) || j == (heightMap.height - 1)) topRight = 0.5f;
                    else topRight = heightMap.GetPixel(i + 1, j + 1).grayscale;

                    if (i == 0) left = 0.5f;
                    else left = heightMap.GetPixel(i - 1, j).grayscale;

                    if (i == (heightMap.width - 1)) right = 0.5f;
                    else right = heightMap.GetPixel(i + 1, j).grayscale;

                    if (i == 0 || j == 0) bottomLeft = 0.5f;
                    else bottomLeft = heightMap.GetPixel(i - 1, j - 1).grayscale;

                    if (j == 0) bottom = 0.5f;
                    else bottom = heightMap.GetPixel(i, j - 1).grayscale;

                    if (i == (heightMap.width - 1) || j == 0) bottomRight = 0.5f;
                    else bottomRight = heightMap.GetPixel(i + 1, j - 1).grayscale;

                    // sobel filter
                    float dX = (topRight + 2.0f * right + bottomRight) - (topLeft + 2.0f * left + bottomLeft);
                    float dY = (bottomLeft + 2.0f * bottom + bottomRight) - (topLeft + 2.0f * top + topRight);
                    float dZ = 1.0f / scale;

                    Vector3 normal = new Vector3(dX, dY, dZ);
                    normal.Normalize();

                    // change range from -1,+1 to 0,+1
                    normal = normal + new Vector3(1f, 1f, 1f);
                    normal = normal / 2f;

                    // change to unity normal format
                    color.r = normal.y;
                    color.g = normal.y;
                    color.b = normal.y;
                    color.a = normal.x;
                    normalMap.SetPixel(i, j, color);
                }
            }
            normalMap.Apply();

            return normalMap;
        }
    }
}
