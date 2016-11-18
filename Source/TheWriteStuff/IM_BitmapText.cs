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
            static string _displayName = "Bitmap Text";
            static string _headerName = "BITMAP TEXT";

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

                loadText(node);
            }

            public override void save(ConfigNode node)
            {
                saveText(node);
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
               
                Image textImage = new Image(image.width, image.height);
                Color32 backgroudColor = new Color32(127, 127, 127, 0);
                textImage.fill(backgroudColor);

                Color32 color = Global.Gray32;
                if (_normalOption == NormalOption.RAISE) color = Global.White32;
                if (_normalOption == NormalOption.LOWER) color = Global.Black32;

                textImage.drawText(_text, _fontName, _fontSize, _position, _rotation, color, _mirror, AlphaOption.OVERWRITE, 255, BlendMethod.PIXEL);

                BoundingBox bBox = new BoundingBox(boundingBox);
                if (image.width != normalMap.width || image.height != normalMap.height)
                {
                    textImage.rescale(normalMap.width, normalMap.height);
                    bBox.x = (int)((float)bBox.x * (float)normalMap.width / (float)image.width);
                    bBox.w = (int)((float)bBox.w * (float)normalMap.width / (float)image.width);
                    bBox.y = (int)((float)bBox.y * (float)normalMap.height / (float)image.height);
                    bBox.h = (int)((float)bBox.h * (float)normalMap.height / (float)image.height);
                }

                Image normalMapImage = textImage.createNormalMap(_normalScale);
                normalMap.overlay(normalMapImage, textImage, 128, bBox);
            }

            public override ImageModifier clone()
            {
                IM.BitmapText im = new IM.BitmapText();
                im.copyFromText(this);
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

            protected override string headerName()
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
                    initialiseText(gui);

                    _selectedFont = BitmapFontCache.Instance.getFontIndexByName(_imBitmapText._fontName);
                    if (_selectedFont < 0) _selectedFont = 0;

                    _fontSizeSelection = BitmapFontCache.Instance.getFontSizeIndex(_imBitmapText._fontName, _imBitmapText._fontSize);
                    if (_fontSizeSelection < 0) _fontSizeSelection = 0;
                }
            }
        }
    }
}
