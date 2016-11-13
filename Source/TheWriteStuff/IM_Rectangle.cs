using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Rectangle : MonoOverlay
        {
            static string _displayName = "Rectangle";

            private IntVector2 _size;
            private RectangleGui _gui;

            public Rectangle()
            {
                _type = Type.RECTANGLE;
                _size = new IntVector2();
                _size.x = 10;
                _size.y = 10;
            }

            public void setSize(IntVector2 size)
            {
                _size.x = size.x;
                _size.y = size.y;
            }

            public override void load(ConfigNode node)
            {
                _type = Type.RECTANGLE;
                _size.x = 0;
                _size.y = 0;

                loadOverlay(node);

                if (node.HasValue("width")) _size.y = int.Parse(node.GetValue("height"));
                if (node.HasValue("height")) _size.y = int.Parse(node.GetValue("height"));
            }

            public override void save(ConfigNode node)
            {
                saveOverlay(node);
                node.AddValue("width", _size.x);
                node.AddValue("height", _size.y);
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                Image rectangleImage = new Image(_size);
                Color32 color = new Color32(_red, _green, _blue, _alpha);
                rectangleImage.fill(color);
                rectangleImage.rotateImage(_rotation);
                if (_mirror) rectangleImage.flipHorizontally();

                image.blendImage(rectangleImage, _blendMethod, _position, _alphaOption, _textureAlpha, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                drawOnImage(ref image, boundingBox);

                if (_normalOption == NormalOption.USE_BACKGROUND) return;

                Image backgroundImage = new Image(normalMap.width, normalMap.height);
                Color32 backgroudColor = new Color32(127, 127, 127, 0);
                backgroundImage.fill(backgroudColor);

                Color32 color = Global.Gray32;
                if (_normalOption == NormalOption.RAISE) color = Global.White32;
                if (_normalOption == NormalOption.LOWER) color = Global.Black32;

                Image rectangleImage = new Image(_size);
                rectangleImage.fill(color);
                rectangleImage.fillAlpha(_alpha);
                rectangleImage.rotateImage(_rotation);
                if (_mirror) rectangleImage.flipHorizontally();

                backgroundImage.blendImage(rectangleImage, BlendMethod.PIXEL, _position, AlphaOption.OVERWRITE, 255, boundingBox);

                Image normalMapImage = backgroundImage.createNormalMap(_normalScale);

                if (image.width == normalMap.width && image.height == normalMap.height) normalMap.rescale(image.width, image.height);
                normalMap.overlay(normalMapImage, backgroundImage, 128, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.Rectangle im = new IM.Rectangle();

                im._type = _type;
                im._size = new IntVector2(_size);

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
                if (_gui == null) _gui = new RectangleGui(this);
                return _gui;
            }




            public class RectangleGui : MonoOverlayGui
            {
                private IM.Rectangle _imRectangle;
                private ValueSelector<int, IntField> _widthSelector;
                private ValueSelector<int, IntField> _heightSelector;

                public RectangleGui(IM.Rectangle rectangle)
                    : base(rectangle)
                {
                    _imRectangle = rectangle;
                }

                ~RectangleGui()
                {
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    header(gui, "RECTANGLE");

                    GUILayout.Space(5);

                    drawBottomMonoOverlay(gui);

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    Color contentColor = GUI.contentColor;
                    GUI.backgroundColor = Global.BackgroundColor;

                    GUILayout.BeginVertical(GUI.skin.box);

                    GUILayout.Label("Rectangle");

                    GUILayout.Space(3);

                    GUILayout.BeginHorizontal();

                    _widthSelector.draw();
                    GUILayout.Space(10f);
                    _heightSelector.draw();
                    GUILayout.Space(10f);

                    if (_imRectangle._size.x != _widthSelector.value())
                    {
                        _imRectangle._size.x = _widthSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_imRectangle._size.y != _heightSelector.value())
                    {
                        _imRectangle._size.y = _heightSelector.value();
                        gui.setRemakePreview();
                    }

                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndVertical();

                    GUI.contentColor = contentColor;
                }

                public override void initialise(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);
                    initialiseMonoOverlay(gui);

                    _widthSelector = new ValueSelector<int, IntField>(_imRectangle._size.x, 1, 9999, 1, "width", Color.white);
                    _heightSelector = new ValueSelector<int, IntField>(_imRectangle._size.y, 1, 9999, 1, "height", Color.white);
                }

                public override string buttonText()
                {
                    return "Rectangle Size";
                }
            }















        }
    }
}
