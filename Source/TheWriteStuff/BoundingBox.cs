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
            ConfigNode bbNode = node.AddNode("ASP_BOUNDINGBOX");

            bbNode.AddValue("x", x);
            bbNode.AddValue("y", y);
            bbNode.AddValue("w", w);
            bbNode.AddValue("h", h);
            bbNode.AddValue("valid", valid);
            bbNode.AddValue("use", use);
        }

        public void load(ConfigNode node)
        {
            x = int.Parse(node.GetValue("x"));
            y = int.Parse(node.GetValue("y"));
            w = int.Parse(node.GetValue("w"));
            h = int.Parse(node.GetValue("h"));
            valid = bool.Parse(node.GetValue("valid"));
            use = bool.Parse(node.GetValue("use"));
        }
    }
}
