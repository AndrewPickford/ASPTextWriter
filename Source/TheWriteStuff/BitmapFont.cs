using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class BitmapFont
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }
        public int size { get; private set; }
        public Dictionary<char, BitmapChar> characterMap { get; private set; }
        public int height { get; private set; }

        public BitmapFont(ConfigNode node, string nodeUrl)
        {
            id = node.GetValue("id");
            name = node.GetValue("name");
            displayName = node.GetValue("displayName");
            size = int.Parse(node.GetValue("size"));

            UnityEngine.Texture2D texture = Utils.LoadTexture("GameData/" + nodeUrl + ".pngmap", false);

            characterMap = new Dictionary<char, BitmapChar>();
            double h = 0d;
            foreach(ConfigNode n in node.GetNodes("ASP_BITMAP_CHAR"))
            {
                BitmapChar cMap = new BitmapChar(n, texture);

                characterMap[cMap.character] = cMap;

                // vh is -ve
                if (cMap.vh < height) h = cMap.vh;
            }

            height = (int) Math.Abs(h);

            UnityEngine.Object.Destroy(texture);
        }
    }
}
