using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class AutoBaseTexture : SingleBaseTexture
        {
            public AutoBaseTexture()
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.AUTO;
            }

            ~AutoBaseTexture()
            {
                cleanUp();
            }

            public override IM.BaseTexture cloneBaseTexture()
            {
                IM.AutoBaseTexture im = new IM.AutoBaseTexture();

                im.copyFromSingleBaseTexture(this);
                if (_main != null) im._main = _main.cloneUrl();
                if (_normalMap != null) im._normalMap = _normalMap.cloneUrl();

                return im;
            }

            public override void set(KSPTextureInfo info)
            {
                _main = new TextureInfo();
                _main.url = info.mainUrl;

                if (info.hasNormalMap)
                {
                    _normalMap = new TextureInfo();
                    _normalMap.url = info.normalMapUrl;
                    _normalMap.normalMap = true;
                    _hasNormalMap = true;
                }
                else
                {
                    _normalMap = null;
                    _hasNormalMap = false;
                }

                valid = true;
            }

            protected override void setupImages()
            {
                if (_main.texture == null) _main.loadTexture();
                if (_main.pixels == null) _main.getPixels();

                if (_hasNormalMap)
                {
                    if (_normalMap.texture == null) _normalMap.loadTexture();
                    if (_normalMap.pixels == null) _normalMap.getPixels();
                }
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.AUTO;
            }

            public override void cleanUp()
            {
                if (_main != null) 
                {
                    _main.cleanUpTexture();
                    _main.cleanUp();
                }

                if (_normalMap != null)
                {
                    _normalMap.cleanUpTexture();
                    _normalMap.cleanUp();
                }
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
