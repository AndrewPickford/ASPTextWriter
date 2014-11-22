using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class CharacterMap
    {
        public char character { get; private set; }
        public float x { get; private set; }
        public float y { get; private set; }
        public float w { get; private set; }
        public float h { get; private set; }
        public float vx { get; private set; }
        public float vy { get; private set; }
        public float vw { get; private set; }
        public float vh { get; private set; }
        public float cw { get; private set; }
        public bool flipped { get; private set; }
        public Rectangle uv { get; private set; }

        public CharacterMap(ConfigNode node, UnityEngine.Texture2D texture)
        {
            if (node.GetValue("character") == "_space_") character = ' ';
            else if (node.GetValue("character") == "_open_brace_") character = '{';
            else if (node.GetValue("character") == "_close_brace_") character = '}';
            else character = node.GetValue("character")[0];
            x = float.Parse(node.GetValue("x"));
            y = float.Parse(node.GetValue("y"));
            w = float.Parse(node.GetValue("w"));
            h = float.Parse(node.GetValue("h"));
            vx = float.Parse(node.GetValue("vx"));
            vy = float.Parse(node.GetValue("vy"));
            vw = float.Parse(node.GetValue("vw"));
            vh = float.Parse(node.GetValue("vh"));
            cw = float.Parse(node.GetValue("cw"));
            flipped = bool.Parse(node.GetValue("flipped"));

            uv = new Rectangle();
            uv.x = (int) ((float) texture.width * x);
            uv.y = (int) ((float) texture.height * (y + h));
            uv.w = (int) ((float) texture.width * w);
            uv.h = (int) ((float) texture.height * -h);
        }
    }
}
