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
            protected Color32 _color;

            public MonoOverlay() :
                base()
            {
                _color = new Color32(0, 0, 0, 255);
            }

            public override void load(ConfigNode node)
            {
                _color = new Color32(0, 0, 0, 255);

                base.load(node);
                if (node.HasValue("red")) _color.r = byte.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _color.g = byte.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _color.b = byte.Parse(node.GetValue("blue"));
                if (node.HasValue("alpha")) _color.a = byte.Parse(node.GetValue("alpha"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("red", _color.r);
                node.AddValue("green", _color.g);
                node.AddValue("blue", _color.b);
                node.AddValue("alpha", _color.a);
            }

            protected void copyFrom(MonoOverlay overlay)
            {
                base.copyFrom(overlay);
                _color = new Color32(overlay._color.r, overlay._color.g, overlay._color.b, overlay._color.a);
            }




            public abstract class MonoOverlayGui : OverlayGui
            {
                private IM.MonoOverlay _overlay;
                private ValueSelector<byte, ByteField> _redSelector;
                private ValueSelector<byte, ByteField> _greenSelector;
                private ValueSelector<byte, ByteField> _blueSelector;
                private ValueSelector<byte, ByteField> _alphaSelector;

                public MonoOverlayGui(IM.MonoOverlay overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected override void drawExtras2(TextureEditGUI gui)
                {
                    base.drawExtras2(gui);

                    colorSelector(gui, ref _redSelector, ref _greenSelector, ref _blueSelector, ref _alphaSelector);

                    checkChanged(ref _overlay._color.r, _redSelector.value(), gui);
                    checkChanged(ref _overlay._color.g, _greenSelector.value(), gui);
                    checkChanged(ref _overlay._color.b, _blueSelector.value(), gui);
                    checkChanged(ref _overlay._color.a, _alphaSelector.value(), gui);
                }

                public override void initialise(TextureEditGUI gui)
                {
                    base.initialise(gui);

                    _redSelector = new ValueSelector<byte, ByteField>(_overlay._color.r, 0, 255, 1, "Red", Color.red);
                    _greenSelector = new ValueSelector<byte, ByteField>(_overlay._color.g, 0, 255, 1, "Green", Color.green);
                    _blueSelector = new ValueSelector<byte, ByteField>(_overlay._color.b, 0, 255, 1, "Blue", Color.blue);
                    _alphaSelector = new ValueSelector<byte, ByteField>(_overlay._color.a, 0, 255, 1, "Alpha", Color.white);
                }
            }
        }
    }
}
