using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class BitmapColorDecal : ColorOverlay
        {
            static string _displayName = "Color Decal";
            static string _headerName = "COLOR DECAL";

            private string _url = string.Empty;
            private BitmapColorDecalGui _gui;

            public BitmapColorDecal()
            {
                _type = Type.BITMAP_COLOR_DECAL;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BITMAP_COLOR_DECAL;
                _url = string.Empty;

                base.load(node);

                if (node.HasValue("url")) _url = node.GetValue("url");
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("url", _url);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                BitmapDecal decal;
                if (!BitmapDecalCache.Instance.decals.TryGetValue(_url, out decal)) return;

                Image decalImage = new Image(decal.image);
                decalImage.scaleAlpha(_alpha);
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
                BitmapColorDecal colorDecal = new BitmapColorDecal();
                colorDecal.copyFrom(this);
                return colorDecal;
            }

            protected void copyFrom(BitmapColorDecal colorDecal)
            {
                base.copyFrom(colorDecal);

                _type = colorDecal._type;
                _url = colorDecal._url;
            }

            public override void cleanUp()
            {
                _gui = null;
            }

            public override string displayName()
            {
                return _displayName;
            }

            public override string headerName()
            {
                return _headerName;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new BitmapColorDecalGui(this);
                return _gui;
            }




            public class BitmapColorDecalGui : ColorOverlayGui
            {
                private IM.BitmapColorDecal _imBitmapColorDecal;
                private int _selectedSheet = 0;
                private int _selectedDecal = 0;
                private Vector2 _scrollPos;
                private List<Texture2D> _textures = null;

                public BitmapColorDecalGui(IM.BitmapColorDecal bitmapColorDecal)
                    : base(bitmapColorDecal)
                {
                    _imBitmapColorDecal = bitmapColorDecal;
                    _textures = null;
                }

                ~BitmapColorDecalGui()
                {
                    if (_textures != null)
                    {
                        for (int i = 0; i < _textures.Count; ++i)
                            UnityEngine.Object.Destroy(_textures[i]);
                    }
                }

                protected override void drawExtras1(TextureEditGUI gui)
                {
                    base.drawExtras1(gui);

                    GUILayout.Space(5f);
                    GUILayout.Label("Url: " + _imBitmapColorDecal._url, GUILayout.ExpandWidth(false));
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    if (BitmapDecalCache.Instance.colorSheets.Count == 0) return;

                    Color contentColor = GUI.contentColor;

                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(250), GUILayout.ExpandHeight(true));

                    header(gui, "COLOR DECAL");
                    GUILayout.Space(5f);

                    GUILayout.Label("Decal Sheets", gui.smallHeader);
                    GUILayout.Space(5f);

                    int oldSelectedSheet = _selectedSheet;
                    for (int i = 0; i < BitmapDecalCache.Instance.colorSheets.Count; ++i)
                    {
                        if (i == _selectedSheet) GUI.contentColor = Global.SelectedColor;
                        else GUI.contentColor = Global.NotSelectedColor;

                        if (GUILayout.Button(BitmapDecalCache.Instance.colorSheets[i].displayName, GUILayout.ExpandWidth(true))) _selectedSheet = i;
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
                        if (_selectedDecal >= BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals.Count) _selectedDecal = 0;
                    }

                    GUILayout.Space(10);

                    GUI.contentColor = Global.SelectedColor;
                    GUILayout.Label("Decals", gui.smallHeader);
                    GUILayout.Space(5);

                    if (_textures == null)
                    {
                        _textures = new List<Texture2D>();
                        for (int i = 0; i < BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals.Count; ++i)
                        {
                            Image image = BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals[i].image;
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
                            if (x > 0 && (x + _textures[i + 1].width) > 200)
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
                        _imBitmapColorDecal._url = BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals[_selectedDecal].url;
                        gui.setRemakePreview();
                    }
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    if (_imBitmapColorDecal._url == string.Empty) _imBitmapColorDecal._url = BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals[_selectedDecal].url;
                    BitmapDecalCache.Instance.getColorDecalIndex(_imBitmapColorDecal._url, out _selectedSheet, out _selectedDecal);
                }

                public override string buttonText()
                {
                    if (_imBitmapColorDecal._url == string.Empty) return "Color Decal";
                    else if (_imBitmapColorDecal._url.Length < 8) return _imBitmapColorDecal._url;
                    else return ".." + _imBitmapColorDecal._url.Substring(_imBitmapColorDecal._url.Length - 7, 7);
                }
            }
        }
    }
}
