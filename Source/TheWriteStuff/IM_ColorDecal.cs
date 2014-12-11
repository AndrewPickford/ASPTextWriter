using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class ColorDecal : Overlay
        {
            static string _displayName = "Color Decal";

            private string _url = string.Empty;
            private ColorDecalGui _gui;

            public ColorDecal()
            {
                _type = Type.COLOR_DECAL;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.COLOR_DECAL;
                _url = string.Empty;

                loadOverlay(node);

                if (node.HasValue("url")) _url = node.GetValue("url");
            }

            public override void save(ConfigNode node)
            {
                saveOverlay(node);
                node.AddValue("url", _url);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                BitmapDecal decal;
                if (!BitmapDecalCache.Instance.decals.TryGetValue(_url, out decal)) return;

                Image decalImage = new Image(decal.image);
                decalImage.scaleAlpha(_alpha);
                decalImage.rotateImage(_rotation);

                image.blendImage(decalImage, _blendMethod, _position, _alphaOption, _textureAlpha, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                drawDecalOnImage(ref image, ref normalMap, _url, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.ColorDecal im = new IM.ColorDecal();

                im._type = _type;
                im._url = _url;

                im.copyFromOverlay(this);

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
                if (_gui == null) _gui = new ColorDecalGui(this);
                return _gui;
            }




            public class ColorDecalGui : OverlayGui
            {
                private IM.ColorDecal _imColorDecal;
                private int _selectedSheet = 0;
                private int _selectedDecal = 0;
                private Vector2 _scrollPos;
                private List<Texture2D> _textures = null;

                public ColorDecalGui(IM.ColorDecal colorDecal)
                    : base(colorDecal)
                {
                    _imColorDecal = colorDecal;
                    _textures = null;
                }

                ~ColorDecalGui()
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

                    header(gui, "COLOR DECAL");

                    GUILayout.Label("Url: " + _imColorDecal._url, GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);

                    drawBottomOverlay(gui);

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    if (BitmapDecalCache.Instance.colorSheets.Count == 0) return;

                    Color contentColor = GUI.contentColor;
                    GUI.backgroundColor = Global.BackgroundColor;

                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.MinWidth(250), GUILayout.ExpandHeight(true));

                    GUILayout.Label("Decal Sheets");

                    GUILayout.Space(3);

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

                    GUILayout.Label("Decals");

                    GUILayout.Space(3);

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
                        _imColorDecal._url = BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals[_selectedDecal].url;
                        gui.setRemakePreview();
                    }
                }

                public override void initialise(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);
                    _selectedSheet = 0;
                    _selectedDecal = 0;

                    if (BitmapDecalCache.Instance.colorSheets.Count > 0) _imColorDecal._url = BitmapDecalCache.Instance.colorSheets[_selectedSheet].decals[_selectedDecal].url;
                }

                public override string buttonText()
                {
                    if (_imColorDecal._url == string.Empty) return "Color Decal";
                    else if (_imColorDecal._url.Length < 8) return _imColorDecal._url;
                    else return ".." + _imColorDecal._url.Substring(_imColorDecal._url.Length - 7, 7);
                }
            }
        }
    }
}
