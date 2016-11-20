using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Circle : Shape
        {
            static string _displayName = "Circle";
            static string _headerName = "CIRCLE";

            private float _radius;
            private CircleGui _gui;

            public Circle()
            {
                _type = Type.CIRCLE;
                _radius = 10f;
            }

            public void setRadius(float radius)
            {
                _radius = radius;
            }

            public override void load(ConfigNode node)
            {
                _radius = 10f;

                base.load(node);
                if (node.HasValue("radius")) _radius = float.Parse(node.GetValue("radius"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("radius", _radius.ToString("F1"));
            }

            public ImageGS gsImage()
            {
                int size = (int)(_radius + _edgeWidth + 4);
                ImageGS image = new ImageGS(size, size);
                image.clear();
                image.drawCircleCentered(_radius, _edgeWidth);

                return image;
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                ImageGS gs = gsImage();
                Image circleImage = new Image((int) _radius, (int) _radius);
                circleImage.fill(_color);
                circleImage.rotateImage(_rotation);
                if (_mirror) image.flipHorizontally();

                image.blendImage(circleImage, _blendMethod, _position, _alphaOption, _textureAlpha, boundingBox);
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

                Image rectangleImage = new Image((int) _radius, (int) _radius);
                rectangleImage.fill(color);
                rectangleImage.fillAlpha(_color.a);
                rectangleImage.rotateImage(_rotation);
                if (_mirror) rectangleImage.flipHorizontally();

                backgroundImage.blendImage(rectangleImage, BlendMethod.PIXEL, _position, AlphaOption.OVERWRITE, 255, boundingBox);

                Image normalMapImage = backgroundImage.createNormalMap(_normalScale);

                if (image.width == normalMap.width && image.height == normalMap.height) normalMap.rescale(image.width, image.height);
                normalMap.overlay(normalMapImage, backgroundImage, 128, boundingBox);
            }

            public override ImageModifier clone()
            {
                IM.Circle circle = new IM.Circle();
                circle.copyFrom(this);
                return circle;
            }

            protected void copyFrom(IM.Circle circle)
            {
                base.copyFrom(circle);
                _radius = circle._radius;
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
                if (_gui == null) _gui = new CircleGui(this);
                return _gui;
            }




            public class CircleGui : ShapeGui
            {
                private IM.Circle _imCircle;
                private ValueSelector<float, FloatField> _radiusSelector;

                public CircleGui(IM.Circle circle)
                    : base(circle)
                {
                    _imCircle = circle;
                }

                ~CircleGui()
                {
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200));
                    header(gui, "CIRCLE");
                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();
                    _radiusSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    checkChanged(ref _imCircle._radius, _radiusSelector.value(), gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _radiusSelector = new ValueSelector<float, FloatField>(_imCircle._radius, 0.1f, 9999f, 0.1f, "Radius", Color.white);
                }

                public override string buttonText()
                {
                    return "Circle";
                }
            }
        }
    }
}