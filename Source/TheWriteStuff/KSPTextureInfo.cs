using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class KSPTextureInfo
    {
        private static string[] kspShaders = { "KSP/Diffuse", "KSP/Bumped", "KSP/Specular", "KSP/Bumped Specular" };

        public string mainUrl = string.Empty;
        public string normalMapUrl = string.Empty;
        public string displayName = string.Empty;
        public string dirUrl = string.Empty;
        public Texture2D mainTexture = null;
        public Texture2D normalMapTexture = null;
        public string shader = string.Empty;
        public bool hasNormalMap = false;
        public bool isSpecular = false;
        public bool isTransparent = false;

        private void setShader(Transform transform)
        {
            shader = transform.gameObject.renderer.material.shader.name;
            if (shader.IndexOf("Bumped") != -1) hasNormalMap = true;
            if (shader.IndexOf("Specular") != -1) isSpecular = true;

            bool knownShader = false;
            for (int i = 0; i < kspShaders.Length; ++i)
            {
                if (shader == kspShaders[i]) knownShader = true;
            }
            if (knownShader == false) Utils.LogError("Unknown shader: {0}", shader);
        }

        private string getUrl(Texture2D texture)
        {
            string url = string.Empty;
            GameDatabase.TextureInfo info = GameDatabase.Instance.databaseTexture.Find(x => x.texture == texture);

            if (info == null)
            {
                url = texture.name;
                if (Global.Debug3) Utils.Log("texture {0} using texture.name, info null", url);
            }

            if (info.name != string.Empty && texture.name != string.Empty)
            {
                if (info.name == texture.name)
                {
                    url = texture.name;
                    if (Global.Debug3) Utils.Log("texture {0} info.name == texture.name", url);
                }
                else
                {
                    url = info.name;
                    if (Global.Debug3) Utils.Log("texture {0} info.name != texture.name, using info.name", url);
                }
            }
            else if(info.name != string.Empty && texture.name == string.Empty)
            {
                url = info.name;
                if (Global.Debug3) Utils.Log("texture {0} using info.name, texture.name empty", url);
            }
            else if (info.name == string.Empty && texture.name != string.Empty)
            {
                url = texture.name;
                if (Global.Debug3) Utils.Log("texture {0} using texture.name, info.name empty", url);
            }

            return url;
        }

        public KSPTextureInfo(Transform transform)
        {
            if (Global.Debug2) Utils.Log("create texture infos from transform");

            setShader(transform);
           
            mainTexture = transform.gameObject.renderer.material.mainTexture as Texture2D;
            mainUrl = getUrl(mainTexture);
            dirUrl = System.IO.Path.GetDirectoryName(mainUrl);

            normalMapTexture = transform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (normalMapTexture != null)
            {
                normalMapUrl = getUrl(normalMapTexture);
                hasNormalMap = true;
            }
            else hasNormalMap = false;

            displayName = System.IO.Path.GetFileName(mainUrl);
        }
    }
}
