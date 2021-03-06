﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Rectangle : MonoShape
        {
            static string _displayName = "Rectangle";
            static string _headerName = "RECTANGLE";

            private DoubleVector2 _end;
            private double _height;
            private int _rounding;
            private double _scale;
            private RectangleGui _gui;

            public Rectangle() :
                base()
            {
                _type = Type.RECTANGLE;

                _end = new DoubleVector2();
                _end.x = 30;
                _end.y = 0;
                _height = 10;
                _rounding = 0;
                _scale = 1;
                _overlayRotates = false;
                _overlayMirrors = false;
            }

            public override void load(ConfigNode node)
            {
                _end.x = 0;
                _end.y = 0;
                _height = 0;
                _rounding = 0;
                _scale = 1;

                base.load(node);
                if (node.HasValue("end_x")) _end.x = double.Parse(node.GetValue("end_x"));
                if (node.HasValue("end_y")) _end.y = double.Parse(node.GetValue("end_y"));
                if (node.HasValue("height")) _height = double.Parse(node.GetValue("height"));
                if (node.HasValue("rounding")) _rounding = int.Parse(node.GetValue("rounding"));
                if (node.HasValue("scale")) _scale = double.Parse(node.GetValue("scale"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("end_x", _end.x);
                node.AddValue("end_y", _end.y);
                node.AddValue("height", _height);
                node.AddValue("rounding", _rounding);
                node.AddValue("scale", _scale);
            }

            public override void drawImageGS()
            {
                List<Vertex> vertices = new List<Vertex>(4);
                double dx = 0;
                double dy = 0;
                if (_end.x == 0) dx = _height;
                else if (_end.y == 0) dy = _height;
                else
                {
                    double d = Math.Sqrt(_end.x * _end.x + _end.y * _end.y);
                    dx = _height * _end.y / d;
                    dy = _height * _end.x / d;
                }

                vertices.Add(new Vertex(0, 0, _rounding));
                vertices.Add(new Vertex(-dx, dy, _rounding));
                vertices.Add(new Vertex(_end.x - dx, _end.y + dy, _rounding));
                vertices.Add(new Vertex(_end.x, _end.y, _rounding));

                Polygon polygon = new Polygon();
                polygon.addVertices(vertices);
                polygon.scale(_scale);
                polygon.rotate(_rotation);
                if (_mirrorX) polygon.mirrorX();
                if (_mirrorY) polygon.mirrorY();
                polygon.close();

                if (Global.Debug3) Utils.Log("drawing rectangle ({0}, {1}), ({2}, {3})", polygon.vertices[0].x, polygon.vertices[0].y, polygon.vertices[2].x, polygon.vertices[2].y);

                _origin.x = (int)(2 - Math.Floor(polygon.min.x));
                _origin.y = (int)(2 - Math.Floor(polygon.min.y));

                polygon.align();
                int w = (int)polygon.max.x + 2;
                int h = (int)polygon.max.y + 2;

                _gsImage = new ImageGS(w, h);
                _gsImage.clear();
                _gsImage.drawPolygon(polygon);
            }

            public override ImageModifier clone()
            {
                IM.Rectangle rectangle = new IM.Rectangle();
                rectangle.copyFrom(this);
                return rectangle;
            }

            protected void copyFrom(IM.Rectangle rectangle)
            {
                base.copyFrom(rectangle);
                _end.x = rectangle._end.x;
                _end.y = rectangle._end.y;
                _height = rectangle._height;
                _rounding = rectangle._rounding;
                _scale = rectangle._scale;
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
                if (_gui == null) _gui = new RectangleGui(this);
                return _gui;
            }




            public class RectangleGui : MonoShapeGui
            {
                private IM.Rectangle _imRectangle;
                private ValueSelector<double, DoubleField> _endXSelector;
                private ValueSelector<double, DoubleField> _endYSelector;
                private ValueSelector<double, DoubleField> _heightSelector;
                private ValueSelector<int, IntField> _roundingSelector;
                private ValueSelector<double, DoubleField> _scaleSelector;

                public RectangleGui(IM.Rectangle rectangle)
                    : base(rectangle)
                {
                    _imRectangle = rectangle;
                }

                ~RectangleGui()
                {
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200));
                    header(gui, _imRectangle.headerName());

                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label("Master Scale", gui.smallHeader);
                    GUILayout.BeginHorizontal();
                    _scaleSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.BeginHorizontal();
                    _endXSelector.draw();
                    _endYSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    _heightSelector.draw();
                    _roundingSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    checkChanged(ref _imRectangle._end.x, _endXSelector.value(), gui);
                    checkChanged(ref _imRectangle._end.y, _endYSelector.value(), gui);
                    checkChanged(ref _imRectangle._height, _heightSelector.value(), gui);
                    checkChanged(ref _imRectangle._rounding, _roundingSelector.value(), gui);
                    checkChanged(ref _imRectangle._scale, _scaleSelector.value(), gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _endXSelector = new ValueSelector<double, DoubleField>(_imRectangle._end.x, -9999, 9999, 1, "End X", Color.white);
                    _endYSelector = new ValueSelector<double, DoubleField>(_imRectangle._end.y, -9999, 9999, 1, "End Y", Color.white);
                    _heightSelector = new ValueSelector<double, DoubleField>(_imRectangle._height, 0.1, 999, 0.1, "Height", Color.white);
                    _roundingSelector = new ValueSelector<int, IntField>(_imRectangle._rounding, 0, 99, 1, "Rounding", Color.white);
                    _scaleSelector = new ValueSelector<double, DoubleField>(_imRectangle._scale, 0.1, 9999, 0.1, "Scale", Color.white);
                }

                public override string buttonText()
                {
                    return "Rectangle";
                }
            }
        }
    }
}
