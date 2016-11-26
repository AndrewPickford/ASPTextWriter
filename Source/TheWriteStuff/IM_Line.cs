using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Line : MonoShape
        {
            static string _displayName = "Line";
            static string _headerName = "LINE";

            private DoubleVector2 _length;
            private double _width;
            private int _rounding;
            private LineGui _gui;

            public Line() :
                base()
            {
                _type = Type.LINE;

                _length = new DoubleVector2();
                _length.x = 20d;
                _length.y = 20d;
                _width = 2d;
                _rounding = 0;
            }

            public void setLength(int x, int y)
            {
                _length.x = x;
                _length.y = y;
            }

            public override void load(ConfigNode node)
            {
                _length.x = 0d;
                _length.y = 0d;
                _width = 0d;
                _rounding = 0;

                base.load(node);
                if (node.HasValue("length_x")) _length.x = double.Parse(node.GetValue("length_x"));
                if (node.HasValue("length_y")) _length.y = double.Parse(node.GetValue("length_y"));
                if (node.HasValue("width")) _width = double.Parse(node.GetValue("width"));
                if (node.HasValue("rounding")) _rounding = int.Parse(node.GetValue("rounding"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("length_x", _length.x);
                node.AddValue("length_y", _length.y);
                node.AddValue("width", _width);
                node.AddValue("rounding", _rounding);
            }

            public override void drawImageGS()
            {
                if (Global.Debug3) Utils.Log("drawing grayscale");

                List<Vertex> vertices = new List<Vertex>(4);
                double dx = 0;
                double dy = 0;
                if (_length.x == 0) dx = _width;
                else if (_length.y == 0) dy = _width;
                else
                {
                    double d = Math.Sqrt(_length.x * _length.x + _length.y * _length.y);
                    dx = _width * _length.y / d;
                    dy = _width * _length.x / d;
                }

                vertices.Add(new Vertex(dx, -dy, _rounding));
                vertices.Add(new Vertex(-dx, dy, _rounding));
                vertices.Add(new Vertex(_length.x - dx, _length.y + dy, _rounding));
                vertices.Add(new Vertex(_length.x + dx, _length.y - dy, _rounding));

                Polygon polygon = new Polygon();
                polygon.addVertices(vertices);
                polygon.close();

                _offset.x = -(int)polygon.minX;
                _offset.y = -(int)polygon.minY;

                polygon.align();
                int w = (int)polygon.maxX + 2;
                int h = (int)polygon.maxY + 2;

                _gsImage = new ImageGS(w, h);
                _gsImage.clear();
                _gsImage.drawPolygon(polygon);
            }

            public override ImageModifier clone()
            {
                IM.Line line = new IM.Line();
                line.copyFrom(this);
                return line;
            }

            protected void copyFrom(IM.Line line)
            {
                base.copyFrom(line);
                _length.x = line._length.x;
                _length.y = line._length.y;
                _width = line._width;
                _rounding = line._rounding;
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
                if (_gui == null) _gui = new LineGui(this);
                return _gui;
            }




            public class LineGui : MonoShapeGui
            {
                private IM.Line _imLine;
                private ValueSelector<double, DoubleField> _lengthXSelector;
                private ValueSelector<double, DoubleField> _lengthYSelector;
                private ValueSelector<double, DoubleField> _widthSelector;
                private ValueSelector<int, IntField> _roundingSelector;

                public LineGui(IM.Line line)
                    : base(line)
                {
                    _imLine = line;
                }

                ~LineGui()
                {
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200));
                    header(gui, _imLine.headerName());

                    GUILayout.Space(5f);
                    GUILayout.Label("End Offset", gui.smallHeader);
                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();
                    _lengthXSelector.draw();
                    _lengthYSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    _widthSelector.draw();
                    _roundingSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    checkChanged(ref _imLine._length.x, _lengthXSelector.value(), gui);
                    checkChanged(ref _imLine._length.y, _lengthYSelector.value(), gui);
                    checkChanged(ref _imLine._width, _widthSelector.value(), gui);
                    checkChanged(ref _imLine._rounding, _roundingSelector.value(), gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _lengthXSelector = new ValueSelector<double, DoubleField>(_imLine._length.x, -9999, 9999, 1, "End X", Color.white);
                    _lengthYSelector = new ValueSelector<double, DoubleField>(_imLine._length.y, -9999, 9999, 1, "End Y", Color.white);
                    _widthSelector = new ValueSelector<double, DoubleField>(_imLine._width, 0.1, 999, 0.1, "Width", Color.white);
                    _roundingSelector = new ValueSelector<int, IntField>(_imLine._rounding, 0, 99, 1, "Rounding", Color.white);
                }

                public override string buttonText()
                {
                    return "Line";
                }
            }
        }
    }
}
