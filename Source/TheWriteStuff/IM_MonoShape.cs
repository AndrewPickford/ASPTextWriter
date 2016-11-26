using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class MonoShape : MonoOverlay
        {
            protected bool _fillShape;
            protected double _edgeWidth;

            public MonoShape() :
                base()
            {
                _fillShape = true;
                _edgeWidth = 1.0f;
            }

            public override void load(ConfigNode node)
            {
                _fillShape = true;
                _edgeWidth = 1.0f;

                base.load(node);
                if (node.HasValue("fill_shape")) _fillShape = bool.Parse(node.GetValue("fill_shape"));
                if (node.HasValue("edge_width")) _edgeWidth = double.Parse(node.GetValue("edge_width"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("fill_shape", _fillShape);
                node.AddValue("edge_width", _edgeWidth.ToString("F1"));
            }

            protected void copyFrom(MonoShape shape)
            {
                base.copyFrom(shape);
                _fillShape = shape._fillShape;
                _edgeWidth = shape._edgeWidth;
            }




            public abstract class MonoShapeGui : MonoOverlayGui
            {
                private IM.MonoShape _overlay;
                private ValueSelector<double, DoubleField> _edgeWidthSelector;
             

                public MonoShapeGui(IM.MonoShape overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);

                    // not yet implemented
                    /* 
                    bool oldFillShape = _overlay._fillShape;

                    GUILayout.Space(10f);

                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                    _overlay._fillShape = GUILayout.Toggle(_overlay._fillShape, "Fill");
                    _edgeWidthSelector.draw();
                    GUILayout.EndVertical();                    

                    if (oldFillShape != _overlay._fillShape) gui.setRemakePreview();
                    checkChanged(ref _overlay._edgeWidth, _edgeWidthSelector.value(), gui); */
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _edgeWidthSelector = new ValueSelector<double, DoubleField>(_overlay._edgeWidth, 0.1, 9999, 0.1, "Width", Color.white);
                }
            }
        }
    }
}