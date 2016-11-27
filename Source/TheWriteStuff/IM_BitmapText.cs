using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class BitmapText : Text
        {
            static string _displayName = "Text";
            static string _headerName = "TEXT";

            private BitmapTextGui _gui;

            public BitmapText()
            {
                _type = Type.BITMAP_TEXT;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.BITMAP_TEXT;
                base.load(node);
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
            }

            public override void drawImageGS()
            {
                BitmapFont font = BitmapFontCache.Instance.getFontByNameSize(_fontName, _fontSize);
                if (font == null) font = BitmapFontCache.Instance.fonts.First();

                if (Global.Debug3) Utils.Log("font: {0}, size: {1}, text: {2}", font.name, font.size, _text);

                IntVector2 s = font.textExtent(_text);
                s.x += 4;
                s.y += 4;
                _gsImage = new ImageGS(s.x, s.y);

                _origin.x = 2;
                _origin.y = s.y - 2 - font.size;

                if (Global.Debug3) Utils.Log("image ({0}, {1}), pos ({2}, {3})", s.x, s.y, _origin.x, _origin.y);
                _gsImage.drawText(_text, font, _origin);
            }

            public override ImageModifier clone()
            {
                IM.BitmapText text = new IM.BitmapText();
                text.copyFrom(this);
                return text;
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
                if (_gui == null) _gui = new BitmapTextGui(this);
                return _gui;
            }




            public class BitmapTextGui : TextGui
            {
                private IM.BitmapText _imBitmapText;
                private int _selectedFont = 0;
                private string[] _fontSizeGrid = null;
                private int _fontSizeSelection = 0;
                private Vector2 _fontScrollPos;

                public BitmapTextGui(IM.BitmapText bitmapText) :
                    base(bitmapText)
                {
                    _imBitmapText = bitmapText;
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    if (BitmapFontCache.Instance.fontInfo.Count == 0) return;

                    bool newFontSelection = false;
                    Color contentColor = GUI.contentColor;

                    _fontScrollPos = GUILayout.BeginScrollView(_fontScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.ExpandHeight(true));

                    header(gui, _imBitmapText.headerName());
                    GUILayout.Space(5f);

                    GUILayout.Label("Size", gui.smallHeader);

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
                    _fontSizeSelection = GUILayout.SelectionGrid(_fontSizeSelection, _fontSizeGrid, 4);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (oldFontSizeSelection != _fontSizeSelection) newFontSelection = true;

                    GUILayout.Space(5);

                    GUILayout.Label("Font", gui.smallHeader);

                    int oldSelectedFont = _selectedFont;
                    for (int i = 0; i < BitmapFontCache.Instance.fontInfo.Count; ++i)
                    {
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
                    }

                    GUILayout.EndScrollView();

                    GUI.contentColor = contentColor;

                    if (newFontSelection)
                    {
                        gui.setRemakePreview();
                        _imBitmapText._fontName = BitmapFontCache.Instance.fontInfo[_selectedFont].name;
                        _imBitmapText._fontSize = BitmapFontCache.Instance.fontInfo[_selectedFont].sizes[_fontSizeSelection];
                    }
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _selectedFont = BitmapFontCache.Instance.getFontIndexByName(_imBitmapText._fontName);
                    if (_selectedFont < 0) _selectedFont = 0;

                    _fontSizeSelection = BitmapFontCache.Instance.getFontSizeIndex(_imBitmapText._fontName, _imBitmapText._fontSize);
                    if (_fontSizeSelection < 0) _fontSizeSelection = 0;
                }
            }
        }
    }
}
