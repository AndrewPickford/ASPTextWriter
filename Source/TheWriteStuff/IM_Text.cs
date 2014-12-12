using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Text : MonoOverlay
        {
            static string _displayName = "Text";

            private string _text = string.Empty;
            private string _fontName = "CAPSMALL_CLEAN";
            private int _fontSize = 32;

            private TextGui _gui;

            public Text()
            {
                _type = Type.BITMAP_TEXT;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BITMAP_TEXT;
                _text = string.Empty;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;

                loadMonoOverlay(node);

                if (node.HasValue("text")) _text = node.GetValue("text");
                if (node.HasValue("fontName")) _fontName = node.GetValue("fontName");
                if (node.HasValue("fontSize")) _fontSize = int.Parse(node.GetValue("fontSize"));
            }

            public override void save(ConfigNode node)
            {
                saveMonoOverlay(node);
                node.AddValue("text", _text);
                node.AddValue("fontName", _fontName);
                node.AddValue("fontSize", _fontSize);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                Color32 color = new Color32(_red, _green, _blue, _alpha);
                image.drawText(_text, _fontName, _fontSize, _position, _rotation, color, _mirror, _alphaOption, _textureAlpha, _blendMethod, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                drawOnImage(ref image, boundingBox);

                if (_normalOption == NormalOption.USE_BACKGROUND) return;
               
                Image textImage = new Image(normalMap.width, normalMap.height);
                Color32 backgroudColor = new Color32(127, 127, 127, 0);
                textImage.fill(backgroudColor);

                Color32 color = Global.Gray32;
                if (_normalOption == NormalOption.RAISE_TEXT) color = Global.Black32;
                if (_normalOption == NormalOption.LOWER_TEXT) color = Global.White32;

                textImage.drawText(_text, _fontName, _fontSize, _position, _rotation, color, _mirror, AlphaOption.OVERWRITE, 255, BlendMethod.PIXEL);

                Image normalMapImage = textImage.createNormalMap(_normalScale);

                if (image.width == normalMap.width && image.height == normalMap.height) normalMap.rescale(image.width, image.height);
                normalMap.overlay(normalMapImage, textImage, 128, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.Text im = new IM.Text();

                im._type = _type;
                im._text = _text;
                im._fontName = _fontName;
                im._fontSize = _fontSize;

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
                if (_gui == null) _gui = new TextGui(this);
                return _gui;
            }




            public class TextGui : MonoOverlayGui
            {
                private IM.Text _imText;
                private int _selectedFont = 0;
                private string[] _fontSizeGrid = null;
                private int _fontSizeSelection = 0;
                private Vector2 _fontScrollPos;

                public TextGui(IM.Text text) : base(text)
                {
                    _imText = text;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    header(gui, "TEXT");

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Text", GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);

                    string oldText = _imText._text;
                    GUI.SetNextControlName("TextField");
                    _imText._text = GUILayout.TextField(_imText._text, GUILayout.ExpandWidth(true));
                    _imText._text = System.Text.RegularExpressions.Regex.Replace(_imText._text, @"[\r\n]", "");
                    if (oldText != _imText._text) gui.setRemakePreview();

                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    drawBottomMonoOverlay(gui);

                    GUILayout.EndVertical(); 
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    if (BitmapFontCache.Instance.fontInfo.Count == 0) return;

                    bool newFontSelection = false;
                    Color contentColor = GUI.contentColor;

                    GUILayout.BeginVertical(GUI.skin.box);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Size");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (_fontSizeGrid == null)
                    {
                        _fontSizeGrid = new string[BitmapFontCache.Instance.fontInfo[_selectedFont].sizes.Count];
                        for (int i = 0; i < _fontSizeGrid.Length; ++i)
                        {
                            _fontSizeGrid[i] = BitmapFontCache.Instance.fontInfo[_selectedFont].sizes[i].ToString();
                        }
                    }

                    int oldFontSizeSelection = _fontSizeSelection;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _fontSizeSelection = GUILayout.SelectionGrid(_fontSizeSelection, _fontSizeGrid, 6);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (oldFontSizeSelection != _fontSizeSelection) newFontSelection = true;

                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Font");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    _fontScrollPos = GUILayout.BeginScrollView(_fontScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.ExpandHeight(true));

                    int oldSelectedFont = _selectedFont;
                    for (int i = 0; i < BitmapFontCache.Instance.fontInfo.Count; ++i)
                    {
                        GUILayout.BeginHorizontal();

                        if (i == _selectedFont) GUI.contentColor = Global.SelectedColor;
                        else GUI.contentColor = Global.NotSelectedColor;

                        if (GUILayout.Button(BitmapFontCache.Instance.fontInfo[i].displayName, GUILayout.ExpandWidth(true)))
                        {
                            _selectedFont = i;
                            _fontSizeGrid = null;

                            int _lastFontSize = BitmapFontCache.Instance.fontInfo[oldSelectedFont].sizes[_fontSizeSelection];
                            _fontSizeSelection = BitmapFontCache.Instance.getFontSizeIndex(BitmapFontCache.Instance.fontInfo[_selectedFont].name, _lastFontSize);
                            if (_fontSizeSelection < 0) _fontSizeSelection = 0;

                            newFontSelection = true;
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();

                    GUILayout.EndVertical();

                    GUI.contentColor = contentColor;

                    if (newFontSelection)
                    {
                        gui.setRemakePreview();
                        _imText._fontName = BitmapFontCache.Instance.fontInfo[_selectedFont].name;
                        _imText._fontSize = BitmapFontCache.Instance.fontInfo[_selectedFont].sizes[_fontSizeSelection];
                    }
                }

                public override void initialise(TextureEditGUI gui)
                {
                    initialiseMonoOverlay(gui);

                    _selectedFont = BitmapFontCache.Instance.getFontIndexByName(_imText._fontName);
                    if (_selectedFont < 0) _selectedFont = 0;
                    _fontSizeSelection = BitmapFontCache.Instance.getFontSizeIndex(_imText._fontName, _imText._fontSize);
                    if (_fontSizeSelection < 0) _fontSizeSelection = 0;
                }

                public override string buttonText()
                {
                    if (_imText._text == string.Empty) return "Text";
                    else if (_imText._text.Length < 8) return _imText._text;
                    else return _imText._text.Substring(0, 7) + "..";
                }
            }
        }
    }
}
