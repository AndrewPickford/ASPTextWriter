using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ASP
{
    namespace IM
    {
        public abstract class MonoPolygon : Shape
        {
            protected int _bevel;

            public MonoPolygon() :
                base()
            {
                _bevel = 0;
            }

            public override void load(ConfigNode node)
            {
                _bevel = 0;

                base.load(node);
                if (node.HasValue("bevel")) _bevel = int.Parse(node.GetValue("bevel"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("bevel", _bevel);
            }

            protected void copyFrom(MonoPolygon polygon)
            {
                base.copyFrom(polygon);
                _bevel = polygon._bevel;
            }



            public abstract class MonoPolygonGui : ShapeGui
            {
                private IM.MonoPolygon _overlay;
                private ValueSelector<int, IntField> _bevelSelector;

                public MonoPolygonGui(IM.MonoPolygon overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);

                    GUILayout.Space(10);
                    _bevelSelector.draw();
                    checkChanged(ref _overlay._bevel, _bevelSelector.value(), gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _bevelSelector = new ValueSelector<int, IntField>(_overlay._bevel, 0, 9999, 1, "Bevel", Color.white);
                }
            }
        }
    }
}