using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class Shape : MonoOverlay
        {
            protected bool _fillShape;
            protected float _edgeWidth;

            public Shape() :
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
                if (node.HasValue("edge_width")) _edgeWidth = float.Parse(node.GetValue("edge_width"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("fill_shape", _fillShape);
                node.AddValue("edge_width", _edgeWidth.ToString("F1"));
            }

            protected void copyFrom(Shape shape)
            {
                base.copyFrom(shape);
                _fillShape = shape._fillShape;
                _edgeWidth = shape._edgeWidth;
            }




            public abstract class ShapeGui : MonoOverlayGui
            {
                private IM.Shape _overlay;
                private ValueSelector<float, FloatField> _edgeWidthSelector;
             

                public ShapeGui(IM.Shape overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);

                    bool oldFillShape = _overlay._fillShape;

                    GUILayout.Space(10f);
                    _edgeWidthSelector.draw();
                    GUILayout.Space(10f);
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                    _overlay._fillShape = GUILayout.Toggle(_overlay._fillShape, "Fill");
                    GUILayout.EndVertical();                    

                    if (oldFillShape != _overlay._fillShape) gui.setRemakePreview();
                    checkChanged(ref _overlay._edgeWidth, _edgeWidthSelector.value(), gui);
                    GUILayout.Space(10f);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _edgeWidthSelector = new ValueSelector<float, FloatField>(_overlay._edgeWidth, 0.1f, 9999f, 0.1f, "Width", Color.white);
                }
            }
        }
    }
}