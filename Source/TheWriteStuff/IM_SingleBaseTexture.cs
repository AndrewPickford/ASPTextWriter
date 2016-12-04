using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class SingleBaseTexture : BaseTexture
        {
            protected TextureInfo _main;
            protected TextureInfo _normalMap;
            protected SingleBaseTextureGui _gui;

            protected abstract void setupImages();

            protected SingleBaseTexture() :
                base()
            {
                _main = null;
                _normalMap = null;
            }

            protected void copyFrom(SingleBaseTexture baseTexture)
            {
                base.copyFrom(baseTexture);
            }

            public override string mainUrl()
            {
                return _main.url;
            }

            public override string normalMapUrl()
            {
                return _normalMap.url;
            }

            public override int width()
            {
                if (_main != null && _main.texture != null) return _main.texture.width;
                else return 0;
            }

            public override int height()
            {
                if (_main != null && _main.texture != null) return _main.texture.height;
                else return 0;
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
            }

            public override void drawOnImage(ref Image image)
            {
                if (Global.Debug3) Utils.Log("draw abase texture");

                setupImages();
                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with bounding box");

                setupImages();
                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");

                setupImages();

                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels);
                normalMap.resizeAndFill(_normalMap.texture.width, _normalMap.texture.height, _normalMap.pixels);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");

                setupImages();

                image.resizeAndFill(_main.texture.width, _main.texture.height, _main.pixels, boundingBox);
                normalMap.resizeAndFill(_normalMap.texture.width, _normalMap.texture.height, _normalMap.pixels, boundingBox);
            }

            public override ImageModifier clone()
            {
                ImageModifier im = cloneBaseTexture();
                return im;
            }




            public class SingleBaseTextureGui : ImageModifierGui
            {
                private SingleBaseTexture _singleBaseTexture;
                private Vector2 _scrollPos;
                private Vector2 _scrollPosNM;

                public SingleBaseTextureGui(SingleBaseTexture singleBaseTexture)
                {
                    _singleBaseTexture = singleBaseTexture;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    header(gui, "BASE TEXTURE (" + _singleBaseTexture._method.ToString() + ")");

                    GUILayout.Label("Name: " + gui.kspTextureInfo().displayName);
                    GUILayout.Space(3);
                    GUILayout.Label("Shader: " + gui.kspTextureInfo().shader);
                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Main Texture: " + _singleBaseTexture._main.url);
                    if (_singleBaseTexture._main.texture.width > 430 || _singleBaseTexture._main.texture.height > 270)
                    {
                        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                        GUILayout.Box(_singleBaseTexture._main.texture, GUI.skin.box, GUILayout.Width(_singleBaseTexture._main.texture.width), GUILayout.Height(_singleBaseTexture._main.texture.height));
                        GUILayout.EndScrollView();
                    }
                    else GUILayout.Box(_singleBaseTexture._main.texture, GUILayout.Width(_singleBaseTexture._main.texture.width), GUILayout.Height(_singleBaseTexture._main.texture.height));
                    GUILayout.EndVertical();

                    if (_singleBaseTexture._hasNormalMap)
                    {
                        GUILayout.Space(5);

                        GUILayout.BeginVertical();
                        GUILayout.Label("Normal Map: " + _singleBaseTexture._normalMap.url);
                        if (_singleBaseTexture._normalMap.texture.width > 430 || _singleBaseTexture._normalMap.texture.height > 270)
                        {
                            _scrollPosNM = GUILayout.BeginScrollView(_scrollPosNM, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                            GUILayout.Box(_singleBaseTexture._normalMap.texture, GUI.skin.box, GUILayout.Width(_singleBaseTexture._normalMap.texture.width), GUILayout.Height(_singleBaseTexture._normalMap.texture.height));
                            GUILayout.EndScrollView();
                        }
                        else GUILayout.Box(_singleBaseTexture._normalMap.texture, GUILayout.Width(_singleBaseTexture._normalMap.texture.width), GUILayout.Height(_singleBaseTexture._normalMap.texture.height));
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
                    _singleBaseTexture.setupImages();
                }

                public override string buttonText()
                {
                    return "Base Texture";
                }
            }
        }
    }
}
