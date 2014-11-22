using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class FontCache : MonoBehaviour
    {
        public static FontCache Instance { get; private set; }

        public List<MappedFont> list { get; private set; }

        private Dictionary<string, MappedFont> _dictionary;

        public MappedFont getFontByID(string id)
        {
            if (_dictionary.ContainsKey(id)) return _dictionary[id];
            else return null;
        }

        public int getFontIndexByID(string id)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].id == id) return i;
            }

            return -1;
        }

        public void Awake()
        {
            if (Instance != null) return;

            Instance = this;
            DontDestroyOnLoad(this);

            _dictionary = new Dictionary<string, MappedFont>();

            list = new List<MappedFont>();
            foreach (UrlDir.UrlConfig url in GameDatabase.Instance.GetConfigs("ASPFONT"))
            {
                if (!url.config.HasValue("name"))
                {
                    Debug.Log("TWS: ASPFontLoader: missing font name");
                }

                Debug.Log(String.Format("TWS: ASPFontLoader: Loading font {0}", url.config.GetValue("name")));
                MappedFont font = new MappedFont(url.config, url.parent.url);
                _dictionary[font.id] = font;
            }

            foreach(KeyValuePair<string, MappedFont> entry in _dictionary)
            {
                list.Add(entry.Value);
            }
        }
    }
}
