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

            protected void loadColorOverlay(ConfigNode node)
            {
                _alpha = 255;

                loadOverlay(node);
                if (node.HasValue("alpha")) _alpha = byte.Parse(node.GetValue("alpha"));
            }

            protected void saveColorOverlay(ConfigNode node)
            {
                saveOverlay(node);
                node.AddValue("alpha", _alpha);
            }

            protected void copyFromColorOverlay(ColorOverlay overlay)
            {
                copyFromOverlay(overlay);
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

                protected void drawBottomColorOverlay(TextureEditGUI gui)
                {
                    drawBottomOverlay(gui);
                }

                protected override void drawBottomOverlayExtras1(TextureEditGUI gui)
                {
                    _alphaSelector.draw();
                    GUILayout.Space(10f);

                    if (_overlay._alpha != _alphaSelector.value())
                    {
                        _overlay._alpha = _alphaSelector.value();
                        gui.setRemakePreview();
                    }
                }

                protected void initialiseColorOverlay(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);
                    _alphaSelector = new ValueSelector<byte, ByteField>(_overlay._alpha, 0, 255, 1, "Alpha", Color.white);
                }
            }
        }
    }
}