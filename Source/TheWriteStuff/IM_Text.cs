using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class Text : MonoOverlay
        {
            public enum TextEncoding { ASCII, ASCII_HEX };

            protected string _text;
            protected string _fontName;
            protected int _fontSize;
            protected int _extraLineSpacing;

            protected Text() :
                base()
            {
                _text = string.Empty;
                _fontName = "";
                _fontSize = 0;
                _extraLineSpacing = 0;
            }

            public override void load(ConfigNode node)
            {
                _text = string.Empty;
                _fontName = "";
                _fontSize = 0;
                TextEncoding encoding = TextEncoding.ASCII;

                base.load(node);

                if (node.HasValue("encoding")) encoding = (TextEncoding)ConfigNode.ParseEnum(typeof(TextEncoding), node.GetValue("encoding"));
                if (node.HasValue("text"))
                {
                    if (encoding == TextEncoding.ASCII_HEX) _text = Utils.DecodeStringHex(node.GetValue("text"));
                    else _text = node.GetValue("text");
                }
                if (node.HasValue("fontName")) _fontName = node.GetValue("fontName");
                if (node.HasValue("fontSize")) _fontSize = int.Parse(node.GetValue("fontSize"));
                if (node.HasValue("extraLineSpacing")) _extraLineSpacing = int.Parse(node.GetValue("extraLineSpacing"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);

                if (_text.Contains('\\') || _text[0] == ' ')
                {
                    node.AddValue("encoding", ConfigNode.WriteEnum(TextEncoding.ASCII_HEX));
                    node.AddValue("text", Utils.EncodeStringHex(_text));
                }
                else
                {
                    node.AddValue("encoding", ConfigNode.WriteEnum(TextEncoding.ASCII));
                    node.AddValue("text", _text);
                }
                node.AddValue("fontName", _fontName);
                node.AddValue("fontSize", _fontSize);
                node.AddValue("extraLineSpacing", _extraLineSpacing);
            }

            public void copyFrom(Text text)
            {
                base.copyFrom(text);
                _text = text._text;
                _fontName = text._fontName;
                _fontSize = text._fontSize;
                _extraLineSpacing = text._extraLineSpacing;
            }




            public abstract class TextGui : MonoOverlayGui
            {
                private IM.Text _imText;

                public TextGui(IM.Text text)
                    : base(text)
                {
                    _imText = text;
                }

                protected override void drawExtras1(TextureEditGUI gui)
                {
                    base.drawExtras1(gui);
                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Text", GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);

                    string oldText = _imText._text;
                    GUI.SetNextControlName("TextField");
                    _imText._text = GUILayout.TextField(_imText._text, GUILayout.ExpandWidth(true));
                    _imText._text = System.Text.RegularExpressions.Regex.Replace(_imText._text, @"[\r\n]", "");
                    if (oldText != _imText._text) gui.setRemakePreview();

                    GUILayout.EndHorizontal();
                }

                public override string buttonText()
                {
                    if (_imText._text == string.Empty) return "Bitmap Text";
                    else if (_imText._text.Length < 8) return _imText._text;
                    else return _imText._text.Substring(0, 7) + "..";
                }
            }
        }
    }
}
