using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BitmapFontCache : MonoBehaviour
    {
        public struct BitmapFontInfo
        {
            public string name;
            public string displayName;
            public List<int> sizes;
        }

        public static BitmapFontCache Instance { get; private set; }

        public List<BitmapFont> fonts { get; private set; }
        public List<BitmapFontInfo> fontInfo { get; private set; }
        public bool hasFonts { get; private set; }

        private Dictionary<string, BitmapFont> _dictionary;

        public BitmapFont getFontByID(string id)
        {
            if (_dictionary.ContainsKey(id)) return _dictionary[id];
            else return null;
        }

        public BitmapFont getFontByNameSize(string name, int size)
        {
            string id = name + "-" + size.ToString();
            return getFontByID(id);
        }

        public int getFontIndexByName(string name)
        {
            for (int i = 0; i < fontInfo.Count; ++i)
            {
                if (fontInfo[i].name == name) return i;
            }

            return -1;
        }

        public int getFontSizeIndex(string name, int size)
        {
            for (int i = 0; i < fontInfo.Count; ++i)
            {
                if (fontInfo[i].name == name)
                {
                    for (int j = 0; j < fontInfo[i].sizes.Count; ++j)
                    {
                        if (fontInfo[i].sizes[j] == size) return j;
                    }
                    return -1;
                }
            }

            return -1;
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Utils.LogError("Another instance of the FontCache is running exiting.");
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);

            _dictionary = new Dictionary<string, BitmapFont>();
            hasFonts = false;
        }

        public void Start()
        {
            LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();
            if (loadingScreen == null)
            {
                Utils.LogError("FontCache: unable to find loading screen, exiting");
                return;
            }

            List<LoadingSystem> loadersList = LoadingScreen.Instance.loaders;
            if (loadersList == null)
            {
                Utils.LogError("FontCache: empty loading list, exiting.");
                return;
            }

            Utils.Log("FontCache: adding fontcache to loading list");
            BitmapFontLoader bitmapFontLoader = this.gameObject.AddComponent<BitmapFontLoader>();
            bitmapFontLoader.setFontCache(this);
            loadersList.Add(bitmapFontLoader);       
        }

        public void addFont(BitmapFont font)
        {
            _dictionary[font.id] = font;
        }

        public void updateCache()
        {
            fonts = new List<BitmapFont>();
            fontInfo = new List<BitmapFontInfo>();

            foreach (KeyValuePair<string, BitmapFont> entry in _dictionary)
            {
                fonts.Add(entry.Value);

                int index = fontInfo.FindIndex(x => x.name == entry.Value.name);
                if (index == -1)
                {
                    BitmapFontInfo info = new BitmapFontInfo();
                    info.sizes = new List<int>();

                    info.name = entry.Value.name;
                    info.displayName = entry.Value.displayName;

                    fontInfo.Add(info);
                    index = fontInfo.FindIndex(x => x.name == entry.Value.name);
                }
                
                fontInfo[index].sizes.Add(entry.Value.size);
            }

            for (int i = 0; i < fontInfo.Count; ++i)
            {
                fontInfo[i].sizes.Sort();
            }

            if (fonts.Count > 0 && fontInfo.Count > 0) hasFonts = true;
        }
    }
}
