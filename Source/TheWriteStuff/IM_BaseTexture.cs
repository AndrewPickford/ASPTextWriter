using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class BaseTexture : ImageModifier
        {
            public enum Method { AUTO, CURRENT, MULTIPLE };

            static string _displayName = "Base Texture";

            public bool valid { get; protected set; }
            protected Method _method = Method.AUTO;
            protected bool _hasNormalMap = false;

            public abstract IM.BaseTexture cloneBaseTexture();
            public abstract int width();
            public abstract int height();
            public abstract string mainUrl();
            public abstract string normalMapUrl();
            public abstract void set(KSPTextureInfo info);
            public abstract void drawOnImage(ref Image image);
            public abstract void drawOnImage(ref Image image, ref Image normalMap);

            public static IM.BaseTexture CreateBaseTexture(ConfigNode node)
            {
                IM.BaseTexture baseTexture = null;

                Method method = Method.AUTO;
                if (node.HasValue("method")) method = (Method)ConfigNode.ParseEnum(typeof(Method), node.GetValue("method"));
                if (Global.Debug3) Utils.Log("method: {0}", method);

                switch (method)
                {
                    case Method.CURRENT:
                        baseTexture = new IM.CurrentBaseTexture();
                        break;

                    case Method.MULTIPLE:
                        baseTexture = new IM.MultipleBaseTexture();
                        break;

                    default:
                    case Method.AUTO:
                        baseTexture = new IM.AutoBaseTexture();
                        break;
                }

                return baseTexture;
            }

            public override string displayName()
            {
                return _displayName;
            }

            public bool hasNormalMap()
            {
                return _hasNormalMap;
            }

            protected void saveBaseTexture(ConfigNode node)
            {
                saveImageModifier(node);
                node.AddValue("method", _method);
            }

            protected void copyFromBaseTexture(BaseTexture baseTexture)
            {
                copyFromImageModifer(baseTexture);
                _method = baseTexture._method;
                _hasNormalMap = baseTexture._hasNormalMap;
            }




            public class TextureInfo
            {
                public string url { get; set; }
                public Texture2D texture { get; set; }
                public Color32[] pixels { get; set; }
                public bool normalMap { get; set; }
                public string name { get; set; }

                public TextureInfo()
                {
                    url = string.Empty;
                    texture = null;
                    pixels = null;
                    normalMap = false;
                }

                ~TextureInfo()
                {
                    cleanUp();
                }

                public TextureInfo cloneUrl()
                {
                    TextureInfo info = new TextureInfo();
                    info.url = url;
                    info.normalMap = normalMap;
                    info.name = name;

                    return info;
                }

                public TextureInfo cloneTexture()
                {
                    TextureInfo info = new TextureInfo();
                    info.url = url;
                    info.normalMap = normalMap;
                    info.texture = texture;
                    info.name = name;

                    return info;
                }

                public void loadTexture()
                {
                    if (Global.Debug3) Utils.Log("load texture {0}", url);
                    texture = Utils.LoadTextureFromUrl(url, normalMap);
                    pixels = null;
                }

                public void getPixels()
                {
                    pixels = texture.GetPixels32();
                }

                public void cleanUpTexture()
                {
                    if (texture != null) UnityEngine.Object.Destroy(texture);
                    texture = null;
                }

                public void cleanUp()
                {
                    pixels = null;
                }
            }

        }
    }
}
