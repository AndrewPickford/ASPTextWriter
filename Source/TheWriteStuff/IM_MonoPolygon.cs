using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ASP
{
    namespace IM
    {
        public abstract class MonoPolygon : MonoShape
        {
            protected List<Vertex> _vertices;
            protected double _scale;

            public MonoPolygon() :
                base()
            {
                _scale = 1d;
                _overlayRotates = false;
            }

            public override void load(ConfigNode node)
            {
                _scale = 1d;
                _vertices = new List<Vertex>(_vertices.Count);
                for (int i = 0; i < _vertices.Count; ++i)
                {
                    _vertices.Add(new Vertex(0, 0, 0));
                }

                base.load(node);

                if (node.HasValue("scale")) _scale = double.Parse(node.GetValue("scale"));
                for (int i = 0; i < _vertices.Count; ++i)
                {
                    if (node.HasValue("vertex_" + i.ToString() + "_x")) _vertices[i].x = double.Parse(node.GetValue("vertex_" + i.ToString() + "_x"));
                    if (node.HasValue("vertex_" + i.ToString() + "_y")) _vertices[i].y = double.Parse(node.GetValue("vertex_" + i.ToString() + "_y"));
                    if (node.HasValue("vertex_" + i.ToString() + "_rounding")) _vertices[i].rounding = int.Parse(node.GetValue("vertex_" + i.ToString() + "_rounding"));
                }
            }

            public override void save(ConfigNode node)
            {
                base.save(node);

                node.AddValue("scale", _scale);
                for (int i = 0; i < _vertices.Count; ++i)
                {
                    node.AddValue("vertex_" + i.ToString() + "_x", _vertices[i].x);
                    node.AddValue("vertex_" + i.ToString() + "_y", _vertices[i].y);
                    node.AddValue("vertex_" + i.ToString() + "_rounding", _vertices[i].rounding);
                }
            }

            protected void copyFrom(MonoPolygon polygon)
            {
                base.copyFrom(polygon);
                _scale = polygon._scale;
                _vertices = new List<Vertex>(polygon._vertices.Count);
                for (int i = 0; i < polygon._vertices.Count; ++i)
                {
                    _vertices.Add(new Vertex(polygon._vertices[i]));
                }
            }

            public override void drawImageGS()
            {
                if (Global.Debug3) Utils.Log("drawing grayscale");

                Polygon polygon = new Polygon();
                polygon.addVertices(_vertices);
                polygon.scale(_scale);
                polygon.rotate(_rotation);
                if (_mirrorX) polygon.mirrorX();
                if (_mirrorY) polygon.mirrorY();
                polygon.close();

                polygon.align();
                int w = (int)polygon.max.x + 2;
                int h = (int)polygon.max.y + 2;

                _origin.x = -(int)polygon.centre.x;
                _origin.y = -(int)polygon.centre.y;

                _gsImage = new ImageGS(w, h);
                _gsImage.clear();
                _gsImage.drawPolygon(polygon);
            }



            public abstract class MonoPolygonGui : MonoShapeGui
            {
                private IM.MonoPolygon _imMonoPolygon;
                private List<ValueSelector<double, DoubleField>> _vertexXselectors;
                private List<ValueSelector<double, DoubleField>> _vertexYselectors;
                private List<ValueSelector<int, IntField>> _vertexRoundingSelectors;
                private ValueSelector<double, DoubleField> _scaleSelector;

                public MonoPolygonGui(IM.MonoPolygon imMonoPolygon) :
                    base(imMonoPolygon)
                {
                    _imMonoPolygon = imMonoPolygon;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _scaleSelector = new ValueSelector<double, DoubleField>(_imMonoPolygon._scale, 0.1, 9999, 0.1, "Scale", Color.white);
                    _vertexXselectors = new List<ValueSelector<double, DoubleField>>(_imMonoPolygon._vertices.Count);
                    _vertexYselectors = new List<ValueSelector<double, DoubleField>>(_imMonoPolygon._vertices.Count);
                    _vertexRoundingSelectors = new List<ValueSelector<int, IntField>>(_imMonoPolygon._vertices.Count);
                    for (int i = 0; i < _imMonoPolygon._vertices.Count; ++i)
                    {
                        _vertexXselectors.Add(new ValueSelector<double, DoubleField>(_imMonoPolygon._vertices[i].x, -9999, 9999, 1, "X", Color.white));
                        _vertexYselectors.Add(new ValueSelector<double, DoubleField>(_imMonoPolygon._vertices[i].y, -9999, 9999, 1, "Y", Color.white));
                        _vertexRoundingSelectors.Add(new ValueSelector<int, IntField>(_imMonoPolygon._vertices[i].rounding, 0, 99, 1, "Rounding", Color.white));
                    }
                }

                public void drawVerticesSelectors(TextureEditGUI gui)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label("Master Scale", gui.smallHeader);
                    GUILayout.BeginHorizontal();
                    _scaleSelector.draw();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    for (int i = 0; i < _imMonoPolygon._vertices.Count; ++i)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.Label("Vertex "+ i.ToString(), gui.smallHeader);

                        GUILayout.BeginHorizontal();
                        _vertexXselectors[i].draw();
                        _vertexYselectors[i].draw();
                        _vertexRoundingSelectors[i].draw();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    checkChanged(ref _imMonoPolygon._scale, _scaleSelector.value(), gui);
                    for (int i = 0; i < _imMonoPolygon._vertices.Count; ++i)
                    {
                        checkChanged(ref _imMonoPolygon._vertices[i].x, _vertexXselectors[i].value(), gui);
                        checkChanged(ref _imMonoPolygon._vertices[i].y, _vertexYselectors[i].value(), gui);
                        checkChanged(ref _imMonoPolygon._vertices[i].rounding, _vertexRoundingSelectors[i].value(), gui);
                    }
                }
            }
        }
    }
}