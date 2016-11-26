using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Triangle : MonoPolygon
        {
            static string _displayName = "Triangle";
            static string _headerName = "TRIANGLE";

            private TriangleGui _gui;

            public Triangle() :
                base()
            {
                _type = Type.TRIANGLE;

                _vertices = new List<Vertex>(3);
                _vertices.Add(new Vertex(0, 0, 0));
                _vertices.Add(new Vertex(20, 20, 0));
                _vertices.Add(new Vertex(40, 0, 0));
            }

            public override void load(ConfigNode node)
            {
                base.load(node);
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
            }

            public override ImageModifier clone()
            {
                IM.Triangle triangle = new IM.Triangle();
                triangle.copyFrom(this);
                return triangle;
            }

            protected void copyFrom(Triangle triangle)
            {
                base.copyFrom(triangle);
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
                if (_gui == null) _gui = new TriangleGui(this);
                return _gui;
            }




            public class TriangleGui : MonoPolygonGui
            {
                private IM.Triangle _imTriangle;

                public TriangleGui(IM.Triangle triangle)
                    : base(triangle)
                {
                    _imTriangle = triangle;
                }

                ~TriangleGui()
                {
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    header(gui, _imTriangle.headerName());

                    drawVerticesSelectors(gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);
                }

                public override string buttonText()
                {
                    return "Triangle";
                }
            }
        }
    }
}
