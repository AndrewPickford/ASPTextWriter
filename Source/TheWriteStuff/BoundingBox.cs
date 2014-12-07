using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class BoundingBox
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public bool valid { get; set; }
        public bool use { get; set; }

        public BoundingBox()
        {
            x = 0;
            y = 0;
            w = 0;
            h = 0;
            valid = false;
            use = false;
        }

        public BoundingBox(int bottomLeftX, int bottomLeftY, int width, int height, bool validBB = true, bool useBB = true)
        {
            x = bottomLeftX;
            y = bottomLeftY;
            w = width;
            h = height;
            valid = validBB;
            use = useBB;
        }

        public void save(ConfigNode node)
        {
            node.AddValue("x", x);
            node.AddValue("y", y);
            node.AddValue("w", w);
            node.AddValue("h", h);
            node.AddValue("valid", valid);
            node.AddValue("use", use);
        }

        public void load(ConfigNode node)
        {
            x = 0;
            y = 0;
            w = 0;
            h = 0;
            valid = false;
            use = false;

            if (node.HasValue("x")) x = int.Parse(node.GetValue("x"));
            if (node.HasValue("y")) y = int.Parse(node.GetValue("y"));
            if (node.HasValue("w")) w = int.Parse(node.GetValue("w"));
            if (node.HasValue("h")) h = int.Parse(node.GetValue("h"));
            if (node.HasValue("valid")) valid = bool.Parse(node.GetValue("valid"));
            if (node.HasValue("use")) use = bool.Parse(node.GetValue("use"));
        }

        public BoundingBox clone()
        {
            BoundingBox bb = new BoundingBox();
            bb.x = x;
            bb.y = y;
            bb.w = w;
            bb.h = h;
            bb.valid = valid;
            bb.use = use;

            return bb;
        }

        public bool inBox(int i, int j)
        {
            if (i >= x && i < (x + w) && j >= y && j < (y + h)) return true;
            else return false;
        }

        public void fillLimits(ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            minX = x;
            maxX = x + w - 1;
            minY = y;
            maxY = y + h - 1;
        }
    }
}
