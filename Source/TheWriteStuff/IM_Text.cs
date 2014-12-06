using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Text : ImageModifier
        {
            static string _displayName = "Text";

            internal string _text = string.Empty;
            internal string _fontName = "CAPSMALL_CLEAN";
            internal int _fontSize = 32;
            internal IntVector2 _position;
            internal bool _mirror = false;
            internal Color32 _color = new Color32(0, 0, 0, 255);
            internal AlphaOption _alphaOption = AlphaOption.TEXT_ONLY;
            internal float _normalScale = 2.0f;
            internal NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            internal BlendMethod _blendMethod = BlendMethod.PIXEL;
            internal TextDirection _textDirection = TextDirection.LEFT_RIGHT;

            private TextGui _gui;

            public Text()
            {
                _position = new IntVector2();
            }

            public override void load(ConfigNode node)
            {
                _text = string.Empty;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;
                _position = new IntVector2();
                _mirror = false;
                _color = new Color32(0, 0, 0, 255);
                _alphaOption = AlphaOption.TEXT_ONLY;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _blendMethod = BlendMethod.RGB;
                _textDirection = TextDirection.LEFT_RIGHT;

                if (node.HasValue("text")) _text = node.GetValue("text");
                if (node.HasValue("fontName")) _fontName = node.GetValue("fontName");
                if (node.HasValue("fontSize")) _fontSize = int.Parse(node.GetValue("fontSize"));
                if (node.HasValue("x")) _position.x = int.Parse(node.GetValue("x"));
                if (node.HasValue("y")) _position.y = int.Parse(node.GetValue("y"));
                if (node.HasValue("mirror")) _mirror = bool.Parse(node.GetValue("mirror"));
                if (node.HasValue("red")) _color.r = byte.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _color.g = byte.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _color.b = byte.Parse(node.GetValue("blue"));
                if (node.HasValue("alpha")) _color.a = byte.Parse(node.GetValue("alpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = int.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("textDirection")) _textDirection = (TextDirection)ConfigNode.ParseEnum(typeof(TextDirection), node.GetValue("textDirection"));
            }

            public override void save(ConfigNode node)
            {
                node.AddValue("type", "text");
                node.AddValue("text", _text);
                node.AddValue("fontName", _fontName);
                node.AddValue("fontSize", _fontSize);
                node.AddValue("x", _position.x);
                node.AddValue("y", _position.y);
                node.AddValue("mirror", _mirror);
                node.AddValue("red", _color.r);
                node.AddValue("green", _color.g);
                node.AddValue("blue", _color.b);
                node.AddValue("alpha", _color.a);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("textDirection", ConfigNode.WriteEnum(_textDirection));
            }

            public override void drawOnImage(ref Image image)
            {
                image.drawText(_text, _fontName, _fontSize, _position, _textDirection, _color, _mirror, _alphaOption, _blendMethod);
            }

            public override ImageModifier clone()
            {
                IM.Text im = new IM.Text();

                im._text = _text;
                im._fontName = _fontName;
                im._fontSize = _fontSize;
                im._position = new IntVector2(_position);
                im._mirror = _mirror;
                im._color = new Color32(_color.r, _color.g, _color.b, _color.a);
                im._alphaOption = _alphaOption;
                im._normalScale = _normalScale;
                im._normalOption = _normalOption;
                im._blendMethod = _blendMethod;
                im._textDirection = _textDirection;

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

            public override bool locked()
            {
                return false;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new TextGui(this);
                return _gui;
            }
        }

        public class TextGui : ImageModifierGui
        {
            private IM.Text _imText;
            private ValueSelector<byte, ByteField> _redSelector;
            private ValueSelector<byte, ByteField> _greenSelector;
            private ValueSelector<byte, ByteField> _blueSelector;
            private ValueSelector<byte, ByteField> _alphaSelector;
            private int _selectedFont = 0;
            private string[] _fontSizeGrid = null;
            private int _fontSizeSelection = 0;
            private Vector2 _fontScrollPos;

            public TextGui(IM.Text text)
            {
                _imText = text;
            }

            public override void drawBottom(TextureEditGUI gui)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                Header(gui, "TEXT");

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

                GUILayout.BeginHorizontal();
                Position(gui, ref _imText._position);
                GUILayout.Space(5);
                guiColorSelector(gui);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            public override void drawRight(TextureEditGUI gui)
            {
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
                    _fontSizeGrid = new string[FontCache.Instance.fontInfoArray[_selectedFont].sizes.Length];
                    for (int i = 0; i < _fontSizeGrid.Length; ++i)
                    {
                        _fontSizeGrid[i] = FontCache.Instance.fontInfoArray[_selectedFont].sizes[i].ToString();
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
                for (int i = 0; i < FontCache.Instance.fontInfoArray.Length; ++i)
                {
                    GUILayout.BeginHorizontal();

                    if (i == _selectedFont) GUI.contentColor = gui._selectedColor;
                    else GUI.contentColor = gui._notSelectedColor;

                    if (GUILayout.Button(FontCache.Instance.fontInfoArray[i].displayName, GUILayout.ExpandWidth(true)))
                    {
                        _selectedFont = i;
                        _fontSizeGrid = null;

                        int _lastFontSize = FontCache.Instance.fontInfoArray[oldSelectedFont].sizes[_fontSizeSelection];
                        _fontSizeSelection = FontCache.Instance.getFontSizeIndex(FontCache.Instance.fontInfoArray[_selectedFont].name, _lastFontSize);
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
                    _imText._fontName = FontCache.Instance.fontInfoArray[_selectedFont].name;
                    _imText._fontSize = FontCache.Instance.fontInfoArray[_selectedFont].sizes[_fontSizeSelection];
                }
            }

            private void guiColorSelector(TextureEditGUI gui)
            {
                GUILayout.BeginHorizontal();

                if (_redSelector.draw()) gui.setRemakePreview();
                GUILayout.Space(10f);
                if (_greenSelector.draw()) gui.setRemakePreview();
                GUILayout.Space(10f);
                if (_blueSelector.draw()) gui.setRemakePreview();
                GUILayout.Space(10f);
                if (_alphaSelector.draw()) gui.setRemakePreview();
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                if (_imText._color.r != _redSelector.value())
                {
                    _imText._color.r = _redSelector.value();
                    gui.setRemakePreview();
                }

                if (_imText._color.g != _greenSelector.value())
                {
                    _imText._color.g = _greenSelector.value();
                    gui.setRemakePreview();
                }

                if (_imText._color.b != _blueSelector.value())
                {
                    _imText._color.b = _blueSelector.value();
                    gui.setRemakePreview();
                }

                if (_imText._color.a != _alphaSelector.value())
                {
                    _imText._color.a = _alphaSelector.value();
                    gui.setRemakePreview();
                }
            }

            public override void initialise()
            {
                _redSelector = new ValueSelector<byte, ByteField>(_imText._color.r, 0, 255, 1, "Red", Color.red);
                _greenSelector = new ValueSelector<byte, ByteField>(_imText._color.g, 0, 255, 1, "Green", Color.green);
                _blueSelector = new ValueSelector<byte, ByteField>(_imText._color.b, 0, 255, 1, "Blue", Color.blue);
                _alphaSelector = new ValueSelector<byte, ByteField>(_imText._color.a, 0, 255, 1, "Alpha", Color.white);

                _selectedFont = FontCache.Instance.getFontIndexByName(_imText._fontName);
                if (_selectedFont < 0) _selectedFont = 0;
                _fontSizeSelection = FontCache.Instance.getFontSizeIndex(_imText._fontName, _imText._fontSize);
                if (_fontSizeSelection < 0) _fontSizeSelection = 0;
            }

            public override bool drawRightBar()
            {
                return true;
            }

            public override string buttonText()
            {
                if (_imText._text == string.Empty) return "Text";
                else if (_imText._text.Length < 6) return _imText._text;
                else return _imText._text.Substring(0, 5) + "..";
            }
        }
    }
}
