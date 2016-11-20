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
            protected string _text = string.Empty;
            protected string _fontName = string.Empty;
            protected int _fontSize = 0;

            public override void load(ConfigNode node)
            {
                _text = string.Empty;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;

                base.load(node);

                if (node.HasValue("text")) _text = node.GetValue("text");
                if (node.HasValue("fontName")) _fontName = node.GetValue("fontName");
                if (node.HasValue("fontSize")) _fontSize = int.Parse(node.GetValue("fontSize"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("text", _text);
                node.AddValue("fontName", _fontName);
                node.AddValue("fontSize", _fontSize);
            }

            public void copyFrom(Text text)
            {
                base.copyFrom(text);
                _text = text._text;
                _fontName = text._fontName;
                _fontSize = text._fontSize;
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
