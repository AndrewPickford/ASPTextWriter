using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class Quadrilateral : MonoPolygon
        {
            static string _displayName = "Quadrilateral";
            static string _headerName = "Quadrilateral";

            private QuadrilateralGui _gui;

            public Quadrilateral() :
                base()
            {
                _type = Type.QUADRILATERAL;
                _vertices = new List<Vertex>(4);
                _vertices.Add(new Vertex(0, 0, 0));
                _vertices.Add(new Vertex(0, 20, 0));
                _vertices.Add(new Vertex(20, 20, 0));
                _vertices.Add(new Vertex(20, 0, 0));
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
                IM.Quadrilateral quadrilateral = new IM.Quadrilateral();
                quadrilateral.copyFrom(this);
                return quadrilateral;
            }

            protected void copyFrom(Quadrilateral quadrilateral)
            {
                base.copyFrom(quadrilateral);
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
                if (_gui == null) _gui = new QuadrilateralGui(this);
                return _gui;
            }




            public class QuadrilateralGui : MonoPolygonGui
            {
                private IM.Quadrilateral _imQuadrilateral;

                public QuadrilateralGui(IM.Quadrilateral quadrilateral)
                    : base(quadrilateral)
                {
                    _imQuadrilateral = quadrilateral;
                }

                ~QuadrilateralGui()
                {
                }

                public override void drawRight(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    header(gui, _imQuadrilateral.headerName());

                    drawVerticesSelectors(gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);
                }

                public override string buttonText()
                {
                    return "Quadrilateral";
                }
            }
        }
    }
}
