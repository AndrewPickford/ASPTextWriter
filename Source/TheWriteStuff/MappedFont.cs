using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class MappedFont
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public string displayName { get; private set; }
        public int size { get; private set; }
        public UnityEngine.Texture2D texture { get; private set; }
        public Dictionary<char, CharacterMap> characterMap { get; private set; }
        public int height { get; private set; }

        public MappedFont(ConfigNode node, string nodeUrl)
        {
            id = node.GetValue("id");
            name = node.GetValue("name");
            displayName = node.GetValue("displayName");
            size = int.Parse(node.GetValue("size"));

            GameDatabase.TextureInfo textInfo = GameDatabase.Instance.databaseTexture.Find(x => x.name == nodeUrl);
            texture = Utils.GetReadableTexture(textInfo.texture, textInfo.texture.name, false);

            if (Global.Debugging)
            {
                if (!System.Object.ReferenceEquals(textInfo.texture, texture)) Utils.Log("MappedFont: texture not readable - reloaded from file");
            }

            characterMap = new Dictionary<char, CharacterMap>();
            float h = 0;
            foreach(ConfigNode n in node.GetNodes("ASPCHAR"))
            {
                CharacterMap cMap = new CharacterMap(n, texture);

                characterMap[cMap.character] = cMap;

                // vh is -ve
                if (cMap.vh < height) h = cMap.vh;
            }

            height = (int) Math.Abs(h);

        }
    }
}
