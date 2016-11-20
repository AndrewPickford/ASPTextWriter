using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class CurrentBaseTexture : SingleBaseTexture
        {
            public CurrentBaseTexture()
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.CURRENT;
            }

            ~CurrentBaseTexture()
            {
                cleanUp();
            }

            public override IM.BaseTexture cloneBaseTexture()
            {
                IM.CurrentBaseTexture im = new IM.CurrentBaseTexture();

                im.copyFrom(this);
                if (_main != null) im._main = _main.cloneTexture();
                if (_normalMap != null) im._normalMap = _normalMap.cloneTexture();

                return im;
            }

            public override void set(KSPTextureInfo info)
            {
                _main = new TextureInfo();
                _main.texture = info.mainTexture;
                _main.url = info.mainUrl;

                if (info.hasNormalMap)
                {
                    _normalMap = new TextureInfo();
                    _normalMap.texture = info.normalMapTexture;
                    _normalMap.url = info.normalMapUrl;
                    _normalMap.normalMap = true;
                    _hasNormalMap = true;
                }
                else
                {
                    _normalMap = null;
                    _hasNormalMap = false;
                }
            }

            protected override void setupImages()
            {
                if (_main.pixels == null) _main.getPixels();
                if (_normalMap.pixels == null) _normalMap.getPixels();
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.CURRENT;
            }

            public override void cleanUp()
            {
                if (_main != null) _main.cleanUp();
                if (_normalMap != null) _normalMap.cleanUp();
                _gui = null;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new SingleBaseTextureGui(this);
                return _gui;
            }
        }
    }
}