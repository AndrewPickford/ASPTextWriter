using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace ASP
{
    public class Utils
    {
        private static string[] _textureExtentionNames = { "png", "jpg", "mbm" };
        private static char[] _delimiters = { ',' };

        public static string[] SplitString(string text)
        {
            string[] splitText = text.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < splitText.Length; ++i)
            {
                string str = splitText[i];
                while (str.Length > 0 && str[0] == ' ')
                {
                    str.Remove(0, 1);
                }
            }

            return splitText;
        }
        

        public static string GetTextureFileName(string url)
        {
            string fileNameBase = KSPUtil.ApplicationRootPath + "GameData/" + url;
            foreach (string extention in _textureExtentionNames)
            {
                string fileName = fileNameBase + "." + extention;
                bool exists = System.IO.File.Exists(fileName);
                if (exists) return fileName;
            }

            return "";
        }

        public static Texture2D LoadNormalMapFromUrl(string url)
        {
            string fileName = GetTextureFileName(url);

            return LoadNormalMap(fileName);
        }

        public static Texture2D LoadNormalMap(string fileName)
        {
            Texture2D normalMap = LoadTexture(fileName);

#if DEBUG
            Debug.Log(String.Format("TWS: Converting {0} to normal map", fileName));
#endif

            for (int i = 0; i < normalMap.width; ++i)
            {
                for (int j = 0; j < normalMap.height; ++j)
                {
                    Color oldColor = normalMap.GetPixel(i, j);
                    Color newColor = new Color(0f, oldColor.g, 0f, oldColor.r);
                    normalMap.SetPixel(i, j, newColor);
                }
            }

            normalMap.Apply();

            return normalMap;
        }

        public static Texture2D LoadTextureFromUrl(string url)
        {
            string fileName = GetTextureFileName(url);

            return LoadTexture(fileName);
        }

        public static Texture2D LoadTexture(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);
            Texture2D texture = null;

            byte[] bytes = FileCache.Instance.getData(fileName);
            if (bytes == null || (bytes != null && bytes.Length == 0))
            {
                Debug.LogError(String.Format("TWS: Unable to load texture file: {0}", fileName));
                return new Texture2D(1, 1);
            }

            if (extension == ".mbm")
            {
                texture = FromATM.MBMToTexture(bytes, true);
            }
            else
            {
                texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
            }

            texture.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            texture.Apply();

            return texture;
        }

        // compressed textures can be GetPixels32 readable even if they are
        // not GetPixel readable
        public static Texture2D GetReadable32Texture(Texture2D texture, bool normalMap)
        {
            Texture2D readable = texture;
            try
            {
                Color32[] test = readable.GetPixels32();

#if DEBUG
                Debug.Log(String.Format("TWS: Texture: {0} readable 32", texture.name));
#endif
            }
            catch
            {
#if DEBUG
                Debug.Log(String.Format("TWS: Texture: {0} not readable 32", texture.name));
#endif
                if (normalMap) readable = Utils.LoadNormalMapFromUrl(texture.name);
                else readable = Utils.LoadTextureFromUrl(texture.name);
            }

            return readable;
        }

        public static Texture2D GetReadableTexture(Texture2D texture, bool normalMap)
        {
            Texture2D readable = texture;
            try
            {
                Color test = readable.GetPixel(0, 0);

#if DEBUG
                Debug.Log(String.Format("TWS: Texture: {0} readable", texture.name));
#endif
            }
            catch
            {
#if DEBUG
                Debug.Log(String.Format("TWS: Texture: {0} not readable", texture.name));
#endif

                if (normalMap) readable = Utils.LoadNormalMapFromUrl(texture.name);
                else readable = Utils.LoadTextureFromUrl(texture.name);
            }

            return readable;
        }

        public static int ParseIntWithLimits(string value, int min, int max)
        {
            int v = 0;
            int.TryParse(value, out v);

            if (v < min) v = min;
            if (v > max) v = max;

            return v;
        }

        public static float ParseFloatWithLimits(string value, float min, float max)
        {
            float v = 0f;
            float.TryParse(value, out v);

            if (v < min) v = min;
            if (v > max) v = max;

            return v;
        }

        public static string StringAdd(string value, int a)
        {
            int v = 0;
            int.TryParse(value, out v);

            v += a;

            return v.ToString();
        }

        public static string StringAdd(string value, float a)
        {
            float v = 0;
            float.TryParse(value, out v);

            v += a;

            return v.ToString();
        }

        public static string LimitIntString(string value, int min, int max)
        {
            return ParseIntWithLimits(value, min, max).ToString();
        }

        public static string LimitFloatString(string value, float min, float max)
        {
            return ParseFloatWithLimits(value, min, max).ToString();
        }

        public static string FindModelURL(string partName)
        {
            string name = "";
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");

            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs, (c => partName == c.name.Replace('_', '.')));
            if (config != null)
            {
                ConfigNode cn = config.config;
                var id = new UrlDir.UrlIdentifier(config.url);
                for (int i = 0; i < (id.urlDepth - 1); ++i)
                {
                    name += id[i] + "/";
                }
                string meshName = cn.GetValue("mesh");
                meshName = System.IO.Path.GetFileNameWithoutExtension(meshName);
                name += meshName;
            }

            return name;
        }

        public static string FindModelDir(string partName)
        {
            string name = "";
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");

            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs, (c => partName == c.name.Replace('_', '.')));
            if (config != null)
            {
                ConfigNode cn = config.config;
                var id = new UrlDir.UrlIdentifier(config.url);
                for (int i = 0; i < (id.urlDepth - 1); ++i)
                {
                    name += id[i];
                    if (i != (id.urlDepth - 2)) name += "/";
                }
            }

            return name;
        }

        public static void DebugMaterial(Material material)
        {
            Debug.Log(String.Format("TWS: Material: {0}", material.name));

            Type materialType = typeof(Material);
            System.Reflection.FieldInfo[] fieldInfo = materialType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                Debug.Log(String.Format("TWS:    {0}: {1}", info.Name, info.GetValue(material).ToString()));
            }

            System.Reflection.PropertyInfo[] propertyInfo = materialType.GetProperties();
            foreach (System.Reflection.PropertyInfo info in propertyInfo)
            {
                Debug.Log(String.Format("TWS:    {0}: {1}", info.Name, info.GetValue(material, null)));
            }
        }
    }
}
