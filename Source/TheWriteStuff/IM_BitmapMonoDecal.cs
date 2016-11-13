using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class BitmapMonoDecal : MonoOverlay
        {
            static string _displayName = "Mono Decal";

            private string _url = string.Empty;
            private BitmapMonoDecalGui _gui;

            public BitmapMonoDecal()
            {
                _type = Type.BITMAP_MONO_DECAL;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BITMAP_MONO_DECAL;
                _url = string.Empty;

                loadMonoOverlay(node);

                if (node.HasValue("url")) _url = node.GetValue("url");
            }

            public override void save(ConfigNode node)
            {
                saveMonoOverlay(node);
                node.AddValue("url", _url);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                BitmapDecal decal;
                if (!BitmapDecalCache.Instance.decals.TryGetValue(_url, out decal)) return;

                Image decalImage = new Image(decal.image);
                Color32 color = new Color32(_red, _green, _blue, _alpha);
                decalImage.recolor(Global.Black32, color, false, true);
                decalImage.rotateImage(_rotation);
                if (_mirror) decalImage.flipHorizontally();

                image.blendImage(decalImage, _blendMethod, _position, _alphaOption, _textureAlpha, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                drawDecalOnImage(ref image, ref normalMap, _url, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.BitmapMonoDecal im = new IM.BitmapMonoDecal();

                im._type = _type;
                im._url = _url;

                im.copyFromMonoOverlay(this);

                return im;
            }

            public override void cleanUp()
            {
                _gui = null;
            }

            public override string displayName()
            {
                return _displayName;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new BitmapMonoDecalGui(this);
                return _gui;
            }




            public class BitmapMonoDecalGui : MonoOverlayGui
            {
                private IM.BitmapMonoDecal _imBitmapMonoDecal;
                private int _selectedSheet = 0;
                private int _selectedDecal = 0;
                private Vector2 _scrollPos;
                private List<Texture2D> _textures = null;

                public BitmapMonoDecalGui(IM.BitmapMonoDecal bitmapMonoDecal)
                    : base(bitmapMonoDecal)
                {
                    _imBitmapMonoDecal = bitmapMonoDecal;
                    _textures = null;
                }

                ~BitmapMonoDecalGui()
                {
                    if (_textures != null)
                    {
                        for (int i = 0; i < _textures.Count; ++i)
                            UnityEngine.Object.Destroy(_textures[i]);
                    }
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    header(gui, "MONO DECAL");

                    GUILayout.Label("Url: " + _imBitmapMonoDecal._url, GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);

                    drawBottomMonoOverlay(gui);

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    if (BitmapDecalCache.Instance.monoSheets.Count == 0) return;

                    Color contentColor = GUI.contentColor;
                    GUI.backgroundColor = Global.BackgroundColor;

                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(250), GUILayout.ExpandHeight(true));

                    GUILayout.Label("Decal Sheets");

                    GUILayout.Space(3);

                    int oldSelectedSheet = _selectedSheet;
                    for (int i = 0; i < BitmapDecalCache.Instance.monoSheets.Count; ++i)
                    {
                        if (i == _selectedSheet) GUI.contentColor = Global.SelectedColor;
                        else GUI.contentColor = Global.NotSelectedColor;

                        if (GUILayout.Button(BitmapDecalCache.Instance.monoSheets[i].displayName, GUILayout.ExpandWidth(true))) _selectedSheet = i;
                    }

                    if (_selectedSheet != oldSelectedSheet)
                    {
                        if (_textures != null)
                        {
                            for (int i = 0; i < _textures.Count; ++i)
                            {
                                UnityEngine.Object.Destroy(_textures[i]);
                            }
                            _textures = null;
                        }
                        if (_selectedDecal >= BitmapDecalCache.Instance.monoSheets[_selectedSheet].decals.Count) _selectedDecal = 0;
                    }

                    GUILayout.Space(10);

                    GUILayout.Label("Decals");

                    GUILayout.Space(3);

                    if (_textures == null)
                    {
                        _textures = new List<Texture2D>();
                        for (int i = 0; i < BitmapDecalCache.Instance.monoSheets[_selectedSheet].decals.Count; ++i)
                        {
                            Image image = new Image(BitmapDecalCache.Instance.monoSheets[_selectedSheet].decals[i].image);
                            image.recolor(Global.Black32, Global.White32, false, false);

                            Texture2D texture = new Texture2D(image.width, image.height, TextureFormat.ARGB32, false);
                            texture.SetPixels32(image.pixels);
                            texture.Apply();

                            _textures.Add(texture);
                        }
                    }

                    int oldSelectedDecal = _selectedDecal;
                    int x = 0;
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < _textures.Count; ++i)
                    {
                        if (i == _selectedDecal)
                        {
                            GUI.backgroundColor = Color.yellow;
                            if (GUILayout.Button(_textures[i], GUILayout.Width(_textures[i].width + 4), GUILayout.Height(_textures[i].height + 4))) _selectedDecal = i;
                            GUI.backgroundColor = Global.BackgroundColor;
                        }
                        else
                        {
                            if (GUILayout.Button(_textures[i], GUILayout.Width(_textures[i].width + 4), GUILayout.Height(_textures[i].height + 4))) _selectedDecal = i;
                        }

                        x += _textures[i].width + 5;
                        if (i < (_textures.Count - 1))
                        {
                            if (x > 0 && (x + _textures[i+1].width) > 200)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                x = 0;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndScrollView();

                    GUI.contentColor = contentColor;

                    if (oldSelectedSheet != _selectedSheet || oldSelectedDecal != _selectedDecal)
                    {
                        _imBitmapMonoDecal._url = BitmapDecalCache.Instance.monoSheets[_selectedSheet].decals[_selectedDecal].url;
                        gui.setRemakePreview();
                    }
                }

                public override void initialise(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);
                    initialiseMonoOverlay(gui);
                    
                    if (_imBitmapMonoDecal._url == string.Empty) _imBitmapMonoDecal._url = BitmapDecalCache.Instance.monoSheets[_selectedSheet].decals[_selectedDecal].url;
                    BitmapDecalCache.Instance.getMonoDecalIndex(_imBitmapMonoDecal._url, out _selectedSheet, out _selectedDecal);
                }

                public override string buttonText()
                {
                    if (_imBitmapMonoDecal._url == string.Empty) return "Mono Decal";
                    else if (_imBitmapMonoDecal._url.Length < 8) return _imBitmapMonoDecal._url;
                    else return ".." + _imBitmapMonoDecal._url.Substring(_imBitmapMonoDecal._url.Length - 7, 7);
                }
            }
        }
    }
}
