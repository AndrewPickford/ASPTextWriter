using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class BitmapDecal
    {
        public enum Orientation { UPRIGHT, FLIPPED_XY, INVERTED };

        public string url { get; private set; }
        public string name { get; private set; }
        public string type { get; private set; }
        public Image image { get; private set; }

        public BitmapDecal(ConfigNode node, string nodeUrl, Texture2D texture)
        {
            type = node.GetValue("type");
            name = node.GetValue("name");
            url = nodeUrl + "/" + name;

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

        public string textureUrl()
        {
            return System.IO.Path.GetDirectoryName(url);
        }
    }
}
