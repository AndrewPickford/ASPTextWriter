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
        private static string[] _textureExtentionNames = { "png", "jpg", "tga", "mbm" };
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

        public static string CommaSeparatedList(List<string> list)
        {
            string text = string.Empty;
            for (int i = 0; i < list.Count; ++i)
            {
                text += list[i];
                if (i != list.Count - 1) text += ",";
            }

            return text;
        }

        public static string[] AddPrefix(string prefix, string[] array)
        {
            string[] newArray = new string[array.Length];

            for (int i = 0; i < array.Length; ++i)
            {
                newArray[i] = prefix + array[i];
            }

            return newArray;
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

        public static Texture2D LoadTextureFromUrl(string url, bool normalMap)
        {
            string fileName = GetTextureFileName(url);

            return LoadTexture(fileName, normalMap);
        }

        public static Texture2D LoadTexture(string fileName, bool normalMap)
        {
            string extension = System.IO.Path.GetExtension(fileName);
            Texture2D texture = null;

            byte[] bytes = FileCache.Instance.getData(fileName);
            if (bytes == null || (bytes != null && bytes.Length == 0))
            {
                Utils.LogError("Unable to load texture file: {0}", fileName);
                return new Texture2D(1, 1);
            }

            if (extension == ".mbm") texture = FromATM.MBMToTexture(bytes, normalMap);
            else if (extension == ".tga") texture = FromATM.TGAToTexture(bytes, normalMap);
            else
            {
                texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
            }

            if (normalMap)
            {
                bool convertToNormal = true;
                if (extension == ".mbm" && bytes[12] == 1) convertToNormal = false;

                if (convertToNormal)
                {
                    for (int i = 0; i < texture.width; ++i)
                    {
                        for (int j = 0; j < texture.height; ++j)
                        {
                            Color oldColor = texture.GetPixel(i, j);
                            Color newColor = new Color(oldColor.g, oldColor.g, oldColor.g, oldColor.r);
                            texture.SetPixel(i, j, newColor);
                        }
                    }
                }
            }

            texture.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            texture.Apply();

            return texture;
        }

        // compressed textures can be GetPixels32 readable even if they are
        // not GetPixel readable
        // require the texture url as well as some texture.names are empty
        public static Texture2D GetReadable32Texture(Texture2D texture, string url, bool normalMap)
        {
            Texture2D readable = texture;
            try
            {
                Color32[] test = readable.GetPixels32();
                if (Global.Debug1) Utils.Log("Texture: {0} readable 32", texture.name);
            }
            catch
            {
                if (Global.Debug1) Utils.Log("Texture: {0} not readable 32", texture.name);
                readable = Utils.LoadTextureFromUrl(url, normalMap);
            }

            return readable;
        }

        public static Texture2D GetReadableTexture(Texture2D texture, string url, bool normalMap)
        {
            Texture2D readable = texture;
            try
            {
                Color test = readable.GetPixel(0, 0);
                if (Global.Debug1) Utils.Log("Texture: {0} readable", texture.name);
            }
            catch
            {
                if (Global.Debug1) Utils.Log("Texture: {0} not readable", texture.name);

                readable = Utils.LoadTextureFromUrl(url, normalMap);
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
            Utils.Log("Material: {0}", material.name);

            Type materialType = typeof(Material);
            System.Reflection.FieldInfo[] fieldInfo = materialType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                Utils.Log("    {0}: {1}", info.Name, info.GetValue(material).ToString());
            }

            System.Reflection.PropertyInfo[] propertyInfo = materialType.GetProperties();
            foreach (System.Reflection.PropertyInfo info in propertyInfo)
            {
                Utils.Log("    {0}: {1}", info.Name, info.GetValue(material, null));
            }
        }

        public static void WriteTexture(Texture2D texture, string fileName)
        {
            Byte[] bytes = texture.EncodeToPNG();

            System.IO.File.WriteAllBytes(fileName, bytes);
        }

        public static string Reverse(string text)
        {
            if (text == null) return null;

            char[] array = text.ToCharArray();
            Array.Reverse(array);

            return new String(array);
        }

        public static void Log(string text, params System.Object[] vars)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();
            string methodName = method.Name;
            string className = method.ReflectedType.Name;

            Debug.Log(String.Format("[TWS] (" + className + "." + methodName + ") " + text, vars));
        }

        public static void LogError(string text, params System.Object[] vars)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();
            string methodName = method.Name;
            string className = method.ReflectedType.Name;

            Debug.LogError(String.Format("[TWS] (" + className + "." + methodName + ") " + text, vars));
        }

        public static T GetElement2D<T>(T[] array, int i, int j, int w)
        {
            return array[i + j * w];
        }

        public static void SetElement2D<T>(ref T[] array, T value, int i, int j, int w)
        {
            array[i + j * w] = value;
        }
    }
}
