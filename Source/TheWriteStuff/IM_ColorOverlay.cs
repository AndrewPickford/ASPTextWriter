using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class ColorOverlay : Overlay
        {
            protected byte _alpha;

            protected ColorOverlay() :
                base()
            {
                _alpha = 255;
            }

            public override void load(ConfigNode node)
            {
                _alpha = 255;
                base.load(node);
                if (node.HasValue("alpha")) _alpha = byte.Parse(node.GetValue("alpha"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("alpha", _alpha);
            }

            protected void copyFrom(ColorOverlay overlay)
            {
                base.copyFrom(overlay);
                _alpha = overlay._alpha;
            }




            public abstract class ColorOverlayGui : OverlayGui
            {
                private IM.ColorOverlay _overlay;
                private ValueSelector<byte, ByteField> _alphaSelector;

                public ColorOverlayGui(IM.ColorOverlay overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);

                    _alphaSelector.draw();
                    GUILayout.Space(10f);

                    checkChanged(ref _overlay._alpha, _alphaSelector.value(), gui);
                }

                protected virtual void extras2ColorOverlay(TextureEditGUI gui)
                {
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _alphaSelector = new ValueSelector<byte, ByteField>(_overlay._alpha, 0, 255, 1, "Alpha", Color.white);
                }
            }
        }
    }
}