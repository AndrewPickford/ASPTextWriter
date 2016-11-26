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
        public enum Type { MONO, COLOR };

        public string url { get; private set; }
        public string name { get; private set; }
        public Type type { get; private set; }
        public Image image { get; private set; }
        public ImageGS gsImage { get; private set; }

        public BitmapDecal(ConfigNode node, BitmapDecalCache.Sheet sheet, Texture2D texture)
        {
            type = Type.MONO;
            if (node.HasValue("type")) type = (Type)ConfigNode.ParseEnum(typeof(Type), node.GetValue("type"));

            name = node.GetValue("name");
            url = sheet.url + "/" + name;

            Rectangle rect = new Rectangle();
            rect.x = int.Parse(node.GetValue("x"));
            rect.y = int.Parse(node.GetValue("y"));
            rect.w = int.Parse(node.GetValue("w"));
            rect.h = int.Parse(node.GetValue("h"));

            if (rect.w == 0 || rect.h == 0) return;

            if (sheet.origin == BitmapDecalCache.Origin.TOP_LEFT)
            {
                rect.y = texture.height - rect.y - rect.h;
            }

            Orientation orientation = Orientation.UPRIGHT;
            if (node.HasValue("orientation")) orientation = (Orientation)ConfigNode.ParseEnum(typeof(Orientation), node.GetValue("orientation"));

            if (type == Type.MONO)
            {
                gsImage = texture.GetImageMono(rect);
                image = texture.GetImage(rect);
                image.recolor(Color.black, Color.white, false, false);
            }
            else image = texture.GetImage(rect);
           

            switch (orientation)
            {
                case Orientation.FLIPPED_XY:
                    if (gsImage != null) gsImage.flipXY(false);
                    image.flipXY(false);
                    break;

                case Orientation.INVERTED:
                    if (gsImage != null) gsImage.flipVertically();
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
