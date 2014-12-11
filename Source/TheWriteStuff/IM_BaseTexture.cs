using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class TextureInfo
        {
            public string url { get; set; }
            public Texture2D texture { get; set; }
            public Color32[] pixels { get; set; }
            public bool normalMap { get; set; }

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

            public TextureInfo clone()
            {
                IM.TextureInfo info = new IM.TextureInfo();
                info.url = url;
                info.normalMap = normalMap;

                return info;
            }

            public void loadTexture()
            {
                texture = Utils.LoadTextureFromUrl(url, normalMap);
                pixels = null;
            }

            public void getPixels()
            {
                if (texture == null) loadTexture();
                pixels = texture.GetPixels32();
            }

            public void cleanUp()
            {
                if (texture != null) UnityEngine.Object.Destroy(texture);
                pixels = null;
            }
        }

        public class BaseTexture : ImageModifier
        {
            static string _displayName = "Base Texture";

            private IM.TextureInfo _main = null;
            private IM.TextureInfo _normalMap = null;
            private string _name = string.Empty;
            private bool _hasNormalMap = false;
            private BaseTextureGui _gui;

            public BaseTexture()
            {
                _type = Type.BASE_TEXTURE;
            }

            ~BaseTexture()
            {
                cleanUp();
            }

            public IM.BaseTexture cloneBaseTexture()
            {
                IM.BaseTexture im = new IM.BaseTexture();

                if (_main != null) im._main = _main.clone();

                im._type = _type;
                im._name = _name;
                im._hasNormalMap = _hasNormalMap;

                if (_hasNormalMap) im._normalMap = _normalMap.clone();

                return im;
            }

            public void set(KSPTextureInfo info)
            {
                _main = new IM.TextureInfo();
                _main.url = info.mainUrl;

                if (info.hasNormalMap)
                {
                    _normalMap = new IM.TextureInfo();
                    _normalMap.url = info.normalMapUrl;
                    _normalMap.normalMap = true;
                    _hasNormalMap = true;
                }
                else
                {
                    _normalMap = null;
                    _hasNormalMap = false;
                }

                _name = System.IO.Path.GetFileNameWithoutExtension(_main.url);
            }

            public void setupTextures()
            {
                if (_main.pixels == null) _main.getPixels();
                if (_normalMap.pixels == null) _normalMap.getPixels();
            }

            public int width()
            {
                if (_main != null && _main.texture != null) return _main.texture.width;
                else return 0;
            }

            public int height()
            {
                if (_main != null && _main.texture != null) return _main.texture.height;
                else return 0;
            }

            public bool hasNormalMap()
            {
                return _hasNormalMap;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BASE_TEXTURE;
            }

            public override void save(ConfigNode node)
            {
                saveImageModifier(node);
            }

            public void drawOnImage(ref Image image)
            {
                if (Global.Debug3) Utils.Log("draw base texture");

                setupTextures();
                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with bounding box");

                setupTextures();
                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels, boundingBox);
            }

            public void drawOnImage(ref Image image, ref Image normalMap)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");

                setupTextures();

                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels);
                normalMap.resizeAndFill(_normalMap.texture.width, _normalMap.texture.height, _normalMap.pixels);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");

                setupTextures();

                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels, boundingBox);
                normalMap.resizeAndFill(_normalMap.texture.width, _normalMap.texture.height, _normalMap.pixels, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.BaseTexture im = this.cloneBaseTexture();
                return im;
            }

            public override void cleanUp()
            {
                if (_main != null) _main.cleanUp();
                if (_normalMap != null) _normalMap.cleanUp();
                _gui = null;
            }

            public override string displayName()
            {
                return _displayName;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new BaseTextureGui(this);
                return _gui;
            }




            public class BaseTextureGui : ImageModifierGui
            {
                private BaseTexture _baseTexture;
                private Vector2 _scrollPos;
                private Vector2 _scrollPosNM;

                public BaseTextureGui(BaseTexture baseTexture)
                {
                    _baseTexture = baseTexture;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    header(gui, "BASE TEXTURE");

                    GUILayout.Label("Name: " + gui.kspTextureInfo().displayName);
                    GUILayout.Space(3);
                    GUILayout.Label("Shader: " + gui.kspTextureInfo().shader);
                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Main Texture: " + _baseTexture._main.url);
                    if (_baseTexture._main.texture.width > 430 || _baseTexture._main.texture.height > 270)
                    {
                        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                        GUILayout.Box(_baseTexture._main.texture, GUI.skin.box, GUILayout.Width(_baseTexture._main.texture.width), GUILayout.Height(_baseTexture._main.texture.height));
                        GUILayout.EndScrollView();
                    }
                    else GUILayout.Box(_baseTexture._main.texture, GUILayout.Width(_baseTexture._main.texture.width), GUILayout.Height(_baseTexture._main.texture.height));
                    GUILayout.EndVertical();

                    if (_baseTexture._hasNormalMap)
                    {
                        GUILayout.Space(5);

                        GUILayout.BeginVertical();
                        GUILayout.Label("Normal Map: " + _baseTexture._normalMap.url);
                        if (_baseTexture._normalMap.texture.width > 430 || _baseTexture._normalMap.texture.height > 270)
                        {
                            _scrollPosNM = GUILayout.BeginScrollView(_scrollPosNM, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                            GUILayout.Box(_baseTexture._normalMap.texture, GUI.skin.box, GUILayout.Width(_baseTexture._normalMap.texture.width), GUILayout.Height(_baseTexture._normalMap.texture.height));
                            GUILayout.EndScrollView();
                        }
                        else GUILayout.Box(_baseTexture._normalMap.texture, GUILayout.Width(_baseTexture._normalMap.texture.width), GUILayout.Height(_baseTexture._normalMap.texture.height));
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                }

                public override void initialise(TextureEditGUI gui)
                {
                    _baseTexture.setupTextures();
                }

                public override string buttonText()
                {
                    return "Base Texture";
                }
            }
        }
    }
}
