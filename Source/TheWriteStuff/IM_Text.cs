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
            internal byte _textureAlpha = 0;
            internal AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            internal float _normalScale = 2.0f;
            internal NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            internal BlendMethod _blendMethod = BlendMethod.RGB;
            internal Rotation _rotation = Rotation.R0;

            private TextGui _gui;

            public Text()
            {
                _position = new IntVector2();
            }

            public void setPosition(IntVector2 position)
            {
                _position.x = position.x;
                _position.y = position.y;
            }

            public override void load(ConfigNode node)
            {
                _text = string.Empty;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;
                _position = new IntVector2();
                _mirror = false;
                _color = new Color32(0, 0, 0, 255);
                _textureAlpha = 0;
                _alphaOption = AlphaOption.USE_TEXTURE;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _blendMethod = BlendMethod.RGB;
                _rotation = Rotation.R0;

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
                if (node.HasValue("textureAlpha")) _textureAlpha = byte.Parse(node.GetValue("textureAlpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = int.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("rotation")) _rotation = (Rotation)ConfigNode.ParseEnum(typeof(Rotation), node.GetValue("rotation"));
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
                node.AddValue("textureAlpha", _textureAlpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("rotation", ConfigNode.WriteEnum(_rotation));
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                image.drawText(_text, _fontName, _fontSize, _position, _rotation, _color, _mirror, _alphaOption, _blendMethod, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                drawOnImage(ref image, boundingBox);

                if (_normalOption != NormalOption.USE_BACKGROUND)
                {
                    Image textImage = new Image(normalMap.width, normalMap.height);
                    Color32 backgroudColor = new Color32(127, 127, 127, 0);
                    textImage.fill(backgroudColor);

                    Color32 color = Global.Gray32;
                    if (_normalOption == NormalOption.RAISE_TEXT) color = Color.black;
                    if (_normalOption == NormalOption.LOWER_TEXT) color = Color.white;

                    textImage.drawText(_text, _fontName, _fontSize, _position, _rotation, color, _mirror, AlphaOption.OVERWRITE, BlendMethod.PIXEL);

                    Image normalMapImage = textImage.createNormalMap(_normalScale);

                    if (image.width == normalMap.width && image.height == normalMap.height) normalMap.rescale(image.width, image.height);
                    normalMap.overlay(normalMapImage, textImage, 128, boundingBox);
                }
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
                im._textureAlpha = _textureAlpha;
                im._alphaOption = _alphaOption;
                im._normalScale = _normalScale;
                im._normalOption = _normalOption;
                im._blendMethod = _blendMethod;
                im._rotation = _rotation;

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
        }

        public class TextGui : ImageModifierGui
        {
            private IM.Text _imText;
            private ValueSelector<byte, ByteField> _redSelector;
            private ValueSelector<byte, ByteField> _greenSelector;
            private ValueSelector<byte, ByteField> _blueSelector;
            private ValueSelector<byte, ByteField> _alphaSelector;
            private ValueSelector<float, FloatField> _normalScaleSelector;
            private ValueSelector<byte, ByteField> _textureAlphaSelector;
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

                GUILayout.BeginHorizontal();
                positionSelector(gui, ref _imText._position);
                GUILayout.Space(5);

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                colorSelector(gui, ref _redSelector, ref _greenSelector, ref _blueSelector, ref _alphaSelector);
                GUILayout.Space(10f);
                rotationSelector(gui, ref _imText._rotation, ref _imText._mirror);
                GUILayout.Space(10f);
                blendMethodSelector(gui, ref _imText._blendMethod);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                _textureAlphaSelector.draw();
                GUILayout.Space(10f);
                alphaOptionSelector(gui, ref _imText._alphaOption);
                if (gui.hasNormalMap)
                {
                    GUILayout.Space(10f);
                    normalMapOptionSelector(gui, ref _imText._normalOption);
                    GUILayout.Space(10f);
                    _normalScaleSelector.draw();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                setColor(gui);
                if (_imText._normalScale != _normalScaleSelector.value()) _imText._normalScale = _normalScaleSelector.value();
                if (_imText._textureAlpha != _textureAlphaSelector.value()) _imText._textureAlpha = _textureAlphaSelector.value();
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

            private void setColor(TextureEditGUI gui)
            {
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

                _textureAlphaSelector = new ValueSelector<byte, ByteField>(_imText._textureAlpha, 0, 255, 1, "Texture Alpha", Color.white);
                _normalScaleSelector = new ValueSelector<float, FloatField>(_imText._normalScale, 0, 5.0f, 0.1f, "Normal Scale", Color.white);
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
