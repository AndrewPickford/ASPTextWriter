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
            internal int _red = 0;
            internal int _green = 0;
            internal int _blue = 0;
            internal int _alpha = 255;
            internal AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            internal float _normalScale = 2.0f;
            internal NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            internal BlendMethod _blendMethod = BlendMethod.RGB;
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
                _red = 0;
                _green = 0;
                _blue = 0;
                _alpha = 255;
                _alphaOption = AlphaOption.USE_TEXTURE;
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
                if (node.HasValue("red")) _red = int.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _green = int.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _blue = int.Parse(node.GetValue("blue"));
                if (node.HasValue("alpha")) _alpha = int.Parse(node.GetValue("alpha"));
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
                node.AddValue("x", _position.x);
                node.AddValue("y", _position.y);
                node.AddValue("mirror", _mirror);
                node.AddValue("red", _red);
                node.AddValue("green", _green);
                node.AddValue("blue", _blue);
                node.AddValue("alpha", _alpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("textDirection", ConfigNode.WriteEnum(_textDirection));
            }

            public override void drawOnImage(ref Image image)
            {
            }

            public override ImageModifier clone()
            {
                IM.Text im = new IM.Text();

                im._text = _text;
                im._fontName = _fontName;
                im._fontSize = _fontSize;
                im._position = new IntVector2(_position);
                im._mirror = _mirror;
                im._red = _red;
                im._green = _green;
                im._blue = _blue;
                im._alpha = _alpha;
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
            private IM.Text _text;
            private ValueSelector<int, IntField> _redSelector;
            private ValueSelector<int, IntField> _greenSelector;
            private ValueSelector<int, IntField> _blueSelector;
            private ValueSelector<int, IntField> _alphaSelector;

            public TextGui(IM.Text text)
            {
                _text = text;
            }

            public override void draw(TextureEditGUI gui)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();

                GUILayout.Label("Text", GUILayout.ExpandWidth(false));
                GUILayout.Space(5);

                string oldText = _text._text;
                GUI.SetNextControlName("TextField");
                _text._text = GUILayout.TextField(_text._text, GUILayout.ExpandWidth(true));
                _text._text = System.Text.RegularExpressions.Regex.Replace(_text._text, @"[\r\n]", "");

                if (oldText != _text._text) gui.setRemakePreview();

                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                Position(gui, ref _text._position);
                GUILayout.Space(5);
                guiColorSelector(gui);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            private void guiColorSelector(TextureEditGUI gui)
            {
                GUILayout.BeginHorizontal();

                if (_redSelector.draw()) gui.setRemakePreview();
                GUILayout.Space(10f);
                if (_greenSelector.draw()) gui.setRemakePreview();
                GUILayout.Space(10f);
                if (_blueSelector.draw()) gui.setRemakePreview();
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }

            public override void initialise()
            {
                _redSelector = new ValueSelector<int, IntField>(_text._red, 0, 255, 1, "Red", Color.red);
                _greenSelector = new ValueSelector<int, IntField>(_text._green, 0, 255, 1, "Green", Color.green);
                _blueSelector = new ValueSelector<int, IntField>(_text._blue, 0, 255, 1, "Blue", Color.blue);
                _alphaSelector = new ValueSelector<int, IntField>(_text._alpha, 0, 255, 1, "Alpha", Color.white);
            }
        }
    }
}
