using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class BaseTextureInfo
    {
        public string mainUrl = string.Empty;
        public string normalMapUrl = string.Empty;
        public string displayName = string.Empty;
        public bool hasNormalMap = false;

        public static BaseTextureInfo CreateTextureInfo(Transform transform)
        {
            if (Global.Debug2) Utils.Log("create texture infos from transform");

            BaseTextureInfo baseTextureInfo = new BaseTextureInfo();
           
            Texture2D texture = transform.gameObject.renderer.material.mainTexture as Texture2D;
            baseTextureInfo.mainUrl = texture.name;

            texture = transform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (texture != null && texture.name != string.Empty)
            {
                baseTextureInfo.normalMapUrl = texture.name;
                baseTextureInfo.hasNormalMap = true;
            }
            else baseTextureInfo.hasNormalMap = false;

            baseTextureInfo.displayName = System.IO.Path.GetFileName(baseTextureInfo.mainUrl);

            return baseTextureInfo;
        }

        public BaseTextureInfo()
        {
            mainUrl = string.Empty;
            normalMapUrl = string.Empty;
            displayName = string.Empty;
            hasNormalMap = false;
        }
    }
}
