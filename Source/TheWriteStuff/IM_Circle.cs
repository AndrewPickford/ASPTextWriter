using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Circle : MonoShape
        {
            static string _displayName = "Circle";
            static string _headerName = "CIRCLE";

            private double _radius;
            private CircleGui _gui;

            public Circle() :
                base()
            {
                _type = Type.CIRCLE;
                _radius = 10f;
                _overlayRotates = false;
            }

            public void setRadius(double radius)
            {
                _radius = radius;
            }

            public override void load(ConfigNode node)
            {
                _radius = 10f;

                base.load(node);
                if (node.HasValue("radius")) _radius = double.Parse(node.GetValue("radius"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("radius", _radius.ToString("F1"));
            }

            public override void drawImageGS()
            {
                if (Global.Debug3) Utils.Log("drawing grayscale");

                int size = (int)(_radius * 2 + 2);
                _gsImage = new ImageGS(size, size);
                _gsImage.clear();
                _gsImage.drawCircleCentered(_radius);
                _origin.x = (int)(_radius + 1);
                _origin.y = (int)(_radius + 1);
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




            public class CircleGui : MonoShapeGui
            {
                private IM.Circle _imCircle;
                private ValueSelector<double, DoubleField> _radiusSelector;

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

                    _radiusSelector = new ValueSelector<double, DoubleField>(_imCircle._radius, 0.1, 9999, 0.1, "Radius", Color.white);
                }

                public override string buttonText()
                {
                    return "Circle";
                }
            }
        }
    }
}