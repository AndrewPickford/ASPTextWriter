using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class MultipleBaseTexture : BaseTexture
        {
            private string _textureDir = string.Empty;
            private string _textureNames = string.Empty;
            private string _normalMapNames = string.Empty;
            private List<TextureInfo> _mainTextInfo = null;
            private List<TextureInfo> _normalTextInfo = null;
            private int _selectedTexture = 0;
            private MultipleBaseTextureGui _gui;

            public MultipleBaseTexture()
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.MULTIPLE;
            }

            ~MultipleBaseTexture()
            {
                cleanUp();
            }

            public override IM.BaseTexture cloneBaseTexture()
            {
                IM.MultipleBaseTexture im = new IM.MultipleBaseTexture();

                im.copyFromBaseTexture(this);

                im._textureDir = _textureDir;
                im._textureNames = _textureNames;
                im._normalMapNames = _normalMapNames;

                if (_mainTextInfo != null)
                {
                    im._mainTextInfo = new List<TextureInfo>();
                    for (int i = 0; i < _mainTextInfo.Count; ++i)
                    {
                        TextureInfo info = _mainTextInfo[i].cloneUrl();
                        im._mainTextInfo.Add(info);
                    }
                }

                if (_normalTextInfo != null)
                {
                    im._normalTextInfo = new List<TextureInfo>();
                    for (int i = 0; i < _normalTextInfo.Count; ++i)
                    {
                        TextureInfo info = _normalTextInfo[i].cloneUrl();
                        im._normalTextInfo.Add(info);
                    }
                }

                im._selectedTexture = _selectedTexture;

                return im;
            }

            public override int width()
            {
                if (_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count &&
                    _mainTextInfo[_selectedTexture].texture != null) return _mainTextInfo[_selectedTexture].texture.width;
                return 0;
            }

            public override int height()
            {
                if (_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count &&
                    _mainTextInfo[_selectedTexture].texture != null) return _mainTextInfo[_selectedTexture].texture.height;
                return 0;
            }

            public override string mainUrl()
            {
                if (_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)
                    return _mainTextInfo[_selectedTexture].url;
                return string.Empty;
            }

            public override string normalMapUrl()
            {
                if (_normalTextInfo != null && _normalTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)
                    return _normalTextInfo[_selectedTexture].url;
                return string.Empty;
            }

            public override void set(KSPTextureInfo info)
            {
                string[] textureNamesArray = Utils.SplitString(_textureNames);
                string[] normalMapNamesArray = Utils.SplitString(_normalMapNames);

                string dirUrl = info.dirUrl;
                if (_textureDir != string.Empty) dirUrl = _textureDir;

                _mainTextInfo = new List<TextureInfo>();
                for (int i = 0; i < textureNamesArray.Length; ++i)
                {
                    TextureInfo texInfo = new TextureInfo();
                    texInfo.url = dirUrl + "/" + textureNamesArray[i];
                    texInfo.name = System.IO.Path.GetFileNameWithoutExtension(texInfo.url);
                    _mainTextInfo.Add(texInfo);
                }

                _normalTextInfo = new List<TextureInfo>();
                for (int i = 0; i < normalMapNamesArray.Length; ++i)
                {
                    TextureInfo texInfo = new TextureInfo();
                    texInfo.url = dirUrl + "/" + normalMapNamesArray[i];
                    texInfo.name = System.IO.Path.GetFileNameWithoutExtension(texInfo.url);
                    texInfo.normalMap = true;
                    _normalTextInfo.Add(texInfo);
                }

                if (_normalTextInfo.Count > 0) _hasNormalMap = true;
                else _hasNormalMap = false;

                if (_mainTextInfo.Count == 0)
                {
                    valid = false;
                    Utils.LogError("no main textures");
                    return;
                }

                if (_hasNormalMap && _mainTextInfo.Count != _normalTextInfo.Count)
                {
                    Utils.LogError("different numbers of mainTextures ({0}) and normalMaps ({1})", _mainTextInfo.Count, _normalTextInfo.Count);
                    valid = false;
                    return;
                }

                if (Global.Debug2) Utils.Log("valid multiple base texture, textures: {0}", _mainTextInfo.Count);

                valid = true;
            }

            private void setupImages()
            {
                if (_mainTextInfo[_selectedTexture].texture == null) _mainTextInfo[_selectedTexture].loadTexture();
                if (_mainTextInfo[_selectedTexture].pixels == null) _mainTextInfo[_selectedTexture].getPixels();

                if (_hasNormalMap)
                {
                    if (_normalTextInfo[_selectedTexture].texture == null) _normalTextInfo[_selectedTexture].loadTexture();
                    if (_normalTextInfo[_selectedTexture].pixels == null) _normalTextInfo[_selectedTexture].getPixels();
                }
            }

            public override void drawOnImage(ref Image image)
            {
                if (Global.Debug3) Utils.Log("draw abase texture");
                if (!(_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)) return;

                setupImages();
                image.resizeAndFill(_mainTextInfo[_selectedTexture].texture.width, _mainTextInfo[_selectedTexture].texture.height, _mainTextInfo[_selectedTexture].pixels);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with bounding box");
                if (!(_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)) return;

                setupImages();
                image.resizeAndFill(_mainTextInfo[_selectedTexture].texture.width, _mainTextInfo[_selectedTexture].texture.height, _mainTextInfo[_selectedTexture].pixels, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");
                if (!(_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)) return;

                setupImages();

                image.resizeAndFill(_mainTextInfo[_selectedTexture].texture.width, _mainTextInfo[_selectedTexture].texture.height, _mainTextInfo[_selectedTexture].pixels);
                normalMap.resizeAndFill(_normalTextInfo[_selectedTexture].texture.width, _normalTextInfo[_selectedTexture].texture.height, _normalTextInfo[_selectedTexture].pixels);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("draw base texture with normal map, bounding box");
                if (!(_mainTextInfo != null && _mainTextInfo.Count > 0 && _selectedTexture >= 0 && _selectedTexture < _mainTextInfo.Count)) return;

                setupImages();

                image.resizeAndFill(_mainTextInfo[_selectedTexture].texture.width, _mainTextInfo[_selectedTexture].texture.height, _mainTextInfo[_selectedTexture].pixels, boundingBox);
                normalMap.resizeAndFill(_normalTextInfo[_selectedTexture].texture.width, _normalTextInfo[_selectedTexture].texture.height, _normalTextInfo[_selectedTexture].pixels, boundingBox);
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BASE_TEXTURE;
                _method = Method.MULTIPLE;
                _textureDir = string.Empty;
                _textureNames = string.Empty;
                _normalMapNames = string.Empty;
                _selectedTexture = 0;

                if (node.HasValue("textureDir")) _textureDir = node.GetValue("textureDir");
                if (node.HasValue("textures")) _textureNames = node.GetValue("textures");
                if (node.HasValue("normals")) _normalMapNames = node.GetValue("normals");
                if (node.HasValue("selectedTexture")) _selectedTexture = int.Parse(node.GetValue("selectedTexture"));
            }

            public override void save(ConfigNode node)
            {
                saveBaseTexture(node);
                node.AddValue("textureDir", _textureDir);
                node.AddValue("textures", _textureNames);
                node.AddValue("normals", _normalMapNames);
                node.AddValue("selectedTexture", _selectedTexture);
            }

            public override ImageModifier clone()
            {
                ImageModifier im = cloneBaseTexture();
                return im;
            }

            public override void cleanUp()
            {
                if (_mainTextInfo != null)
                {
                    for (int i = 0; i < _mainTextInfo.Count; ++i)
                    {
                        _mainTextInfo[i].cleanUpTexture();
                        _mainTextInfo[i].cleanUp();
                    }
                }

                if (_normalTextInfo != null)
                {
                    for (int i = 0; i < _normalTextInfo.Count; ++i)
                    {
                        _normalTextInfo[i].cleanUpTexture();
                        _normalTextInfo[i].cleanUp();
                    }
                }

                _gui = null;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new MultipleBaseTextureGui(this);
                return _gui;
            }




            public class MultipleBaseTextureGui : ImageModifierGui
            {
                private MultipleBaseTexture _multipleBaseTexture;
                private Vector2 _scrollPosTex;
                private Vector2 _scrollPosNM;
                private Vector2 _scrollPos;

                public MultipleBaseTextureGui(MultipleBaseTexture multipleBaseTexture)
                {
                    _multipleBaseTexture = multipleBaseTexture;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    header(gui, "BASE TEXTURE (" + _multipleBaseTexture._method.ToString() + ")");

                    GUILayout.Label("Name: " + gui.kspTextureInfo().displayName);
                    GUILayout.Space(3);
                    GUILayout.Label("Shader: " + gui.kspTextureInfo().shader);
                    GUILayout.Space(3);

                    if (!(_multipleBaseTexture._selectedTexture >= 0 && _multipleBaseTexture._selectedTexture < _multipleBaseTexture._mainTextInfo.Count))
                    {
                        GUILayout.Label("No valid texture selected");
                        GUILayout.EndVertical();
                        return;
                    }

                    TextureInfo mainTex = _multipleBaseTexture._mainTextInfo[_multipleBaseTexture._selectedTexture]; 
                    TextureInfo normalTex = null;
                    if (_multipleBaseTexture._hasNormalMap) normalTex = _multipleBaseTexture._normalTextInfo[_multipleBaseTexture._selectedTexture]; 

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Label("Main Texture: " + mainTex.url);
                    if (mainTex.texture.width > 430 || mainTex.texture.height > 270)
                    {
                        _scrollPosTex = GUILayout.BeginScrollView(_scrollPosTex, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                        GUILayout.Box(mainTex.texture, GUI.skin.box, GUILayout.Width(mainTex.texture.width), GUILayout.Height(mainTex.texture.height));
                        GUILayout.EndScrollView();
                    }
                    else GUILayout.Box(mainTex.texture, GUILayout.Width(mainTex.texture.width), GUILayout.Height(mainTex.texture.height));
                    GUILayout.EndVertical();

                    if (normalTex != null)
                    {
                        GUILayout.Space(5);

                        GUILayout.BeginVertical();
                        GUILayout.Label("Normal Map: " + normalTex.url);
                        if (normalTex.texture.width > 430 || normalTex.texture.height > 270)
                        {
                            _scrollPosNM = GUILayout.BeginScrollView(_scrollPosNM, GUI.skin.box, GUILayout.MinWidth(450), GUILayout.MinHeight(290));
                            GUILayout.Box(normalTex.texture, GUI.skin.box, GUILayout.Width(normalTex.texture.width), GUILayout.Height(normalTex.texture.height));
                            GUILayout.EndScrollView();
                        }
                        else GUILayout.Box(normalTex.texture, GUILayout.Width(normalTex.texture.width), GUILayout.Height(normalTex.texture.height));
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.ExpandHeight(true));

                    int oldSelectedTexture = _multipleBaseTexture._selectedTexture;
                    for (int i = 0; i < _multipleBaseTexture._mainTextInfo.Count; ++i)
                    {
                        GUILayout.BeginHorizontal();

                        if (i == _multipleBaseTexture._selectedTexture) GUI.contentColor = Global.SelectedColor;
                        else GUI.contentColor = Global.NotSelectedColor;

                        if (GUILayout.Button(_multipleBaseTexture._mainTextInfo[i].name, GUILayout.ExpandWidth(true)))
                        {
                            _multipleBaseTexture._selectedTexture = i;
                            gui.setRemakePreview();
                            _multipleBaseTexture.setupImages();
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();
                }

                public override void initialise(TextureEditGUI gui)
                {
                    _multipleBaseTexture.setupImages();
                }

                public override string buttonText()
                {
                    return "Base Texture";
                }
            }
        }
    }
}