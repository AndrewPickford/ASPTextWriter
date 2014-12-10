using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class BitmapChar
    {
        public enum Orientation { UPRIGHT, FLIPPED_XY, INVERTED };

        public char character { get; private set; }
        public float vx { get; private set; }
        public float vy { get; private set; }
        public float vw { get; private set; }
        public float vh { get; private set; }
        public float cw { get; private set; }
        public Image image { get; private set; }

        public BitmapChar(ConfigNode node, UnityEngine.Texture2D texture)
        {
            if (node.GetValue("character") == "_space_") character = ' ';
            else if (node.GetValue("character") == "_open_brace_") character = '{';
            else if (node.GetValue("character") == "_close_brace_") character = '}';
            else character = node.GetValue("character")[0];

            vx = float.Parse(node.GetValue("vx"));
            vy = float.Parse(node.GetValue("vy"));
            vw = float.Parse(node.GetValue("vw"));
            vh = float.Parse(node.GetValue("vh"));
            cw = float.Parse(node.GetValue("cw"));

            Rectangle rect = new Rectangle();
            rect.x = int.Parse(node.GetValue("x"));
            rect.y = int.Parse(node.GetValue("y"));
            rect.w = int.Parse(node.GetValue("w"));
            rect.h = int.Parse(node.GetValue("h"));

            Orientation orientation = Orientation.UPRIGHT;
            if (node.HasValue("orientation")) orientation = (Orientation)ConfigNode.ParseEnum(typeof(Orientation), node.GetValue("orientation"));

            image = texture.GetImage(rect);

            switch (orientation)
            {
                case Orientation.FLIPPED_XY:
                    image.flipXY(false);
                    break;

                case Orientation.INVERTED:
                    image.flipVertically();
                    break;

                case Orientation.UPRIGHT:
                default:
                    break;
            }
        }
    }
}
