using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class MonoOverlay : Overlay
        {
            protected byte _red;
            protected byte _green;
            protected byte _blue; 

            protected void loadMonoOverlay(ConfigNode node)
            {
                _red = 0;
                _green = 0;
                _blue = 0;

                loadOverlay(node);
                if (node.HasValue("red")) _red = byte.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _green = byte.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _blue = byte.Parse(node.GetValue("blue"));
            }

            protected void saveMonoOverlay(ConfigNode node)
            {
                saveOverlay(node);
                node.AddValue("red", _red);
                node.AddValue("green", _green);
                node.AddValue("blue", _blue);
            }

            protected void copyFromMonoOverlay(MonoOverlay overlay)
            {
                copyFromOverlay(overlay);
                _red = overlay._red;
                _green = overlay._green;
                _blue = overlay._blue;
            }




            public abstract class MonoOverlayGui : OverlayGui
            {
                private IM.MonoOverlay _overlay;
                private ValueSelector<byte, ByteField> _redSelector;
                private ValueSelector<byte, ByteField> _greenSelector;
                private ValueSelector<byte, ByteField> _blueSelector;

                public MonoOverlayGui(IM.MonoOverlay overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected void drawBottomMonoOverlay(TextureEditGUI gui)
                {
                    drawBottomOverlay(gui);
                }

                protected override void drawBottomOverlayExtras1(TextureEditGUI gui)
                {
                    _redSelector.draw();
                    GUILayout.Space(10f);
                    _greenSelector.draw();
                    GUILayout.Space(10f);
                    _blueSelector.draw();
                    GUILayout.Space(10f);

                    if (_overlay._red != _redSelector.value())
                    {
                        _overlay._red = _redSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._green != _greenSelector.value())
                    {
                        _overlay._green = _greenSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._blue != _blueSelector.value())
                    {
                        _overlay._blue = _blueSelector.value();
                        gui.setRemakePreview();
                    }
                }

                protected void initialiseMonoOverlay(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);
                    _redSelector = new ValueSelector<byte, ByteField>(_overlay._red, 0, 255, 1, "Red", Color.red);
                    _greenSelector = new ValueSelector<byte, ByteField>(_overlay._green, 0, 255, 1, "Green", Color.green);
                    _blueSelector = new ValueSelector<byte, ByteField>(_overlay._blue, 0, 255, 1, "Blue", Color.blue);
                }
            }
        }
    }
}
