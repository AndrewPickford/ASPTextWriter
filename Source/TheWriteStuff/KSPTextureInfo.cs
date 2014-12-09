using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class KSPTextureInfo
    {
        private static string[] kspShaders = { "KSP/Bumped Specular" };

        public string mainUrl = string.Empty;
        public string normalMapUrl = string.Empty;
        public string displayName = string.Empty;
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

        public KSPTextureInfo(Transform transform)
        {
            if (Global.Debug2) Utils.Log("create texture infos from transform");

            setShader(transform);
           
            Texture2D texture = transform.gameObject.renderer.material.mainTexture as Texture2D;
            mainUrl = texture.name;

            texture = transform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (texture != null && texture.name != string.Empty)
            {
                normalMapUrl = texture.name;
                hasNormalMap = true;
            }
            else hasNormalMap = false;

            displayName = System.IO.Path.GetFileName(mainUrl);
        }

        public KSPTextureInfo(string baseTextureDirUrl, string textureName, string normalMapName, Transform transform)
        {
            setShader(transform);

            string url = baseTextureDirUrl;
            if (url == string.Empty)
            {
                Texture2D texture = transform.gameObject.renderer.material.mainTexture as Texture2D;
                url = System.IO.Path.GetDirectoryName(texture.name);
            }

            mainUrl = url + "/" + textureName;
            displayName = textureName;

            if (normalMapName == string.Empty) hasNormalMap = false;
            else
            {
                normalMapUrl = url + "/" + normalMapName;
                hasNormalMap = true;
            }
        }
    }
}
