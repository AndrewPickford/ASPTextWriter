using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class FontCache : MonoBehaviour
    {
        public struct FontInfo
        {
            public string name;
            public string displayName;
            public int[] sizes;
        }

        public static FontCache Instance { get; private set; }

        public List<MappedFont> mappedList { get; private set; }
        public FontInfo[] fontInfoArray { get; private set; }

        private Dictionary<string, MappedFont> _dictionary;

        public MappedFont getFontByID(string id)
        {
            if (_dictionary.ContainsKey(id)) return _dictionary[id];
            else return null;
        }

        public MappedFont getFontByNameSize(string name, int size)
        {
            string id = name + "-" + size.ToString();
            return getFontByID(id);
        }

        public int getFontIndexByName(string name)
        {
            for (int i = 0; i < fontInfoArray.Length; ++i)
            {
                if (fontInfoArray[i].name == name) return i;
            }

            return -1;
        }

        public int getFontSizeIndex(string name, int size)
        {
            for (int i = 0; i < fontInfoArray.Length; ++i)
            {
                if (fontInfoArray[i].name == name)
                {
                    for (int j = 0; j < fontInfoArray[i].sizes.Length; ++j)
                    {
                        if (fontInfoArray[i].sizes[j] == size) return j;
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

            _dictionary = new Dictionary<string, MappedFont>();
            mappedList = new List<MappedFont>(); 
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
            MappedFontLoader mappedFontLoader = this.gameObject.AddComponent<MappedFontLoader>();
            mappedFontLoader.setFontCache(this);
            loadersList.Add(mappedFontLoader);       
        }

        public void addFont(MappedFont font)
        {
            _dictionary[font.id] = font;
        }

        public void updateCache()
        {
            Dictionary<string, FontInfo> infoDictionary = new Dictionary<string, FontInfo>();
            Dictionary<string, List<int>> infoSizes = new Dictionary<string, List<int>>();
            foreach (KeyValuePair<string, MappedFont> entry in _dictionary)
            {
                mappedList.Add(entry.Value);

                if (!infoDictionary.ContainsKey(entry.Value.name))
                {
                    FontInfo fontInfo = new FontInfo();

                    fontInfo.name = entry.Value.name;
                    fontInfo.displayName = entry.Value.displayName;

                    infoDictionary.Add(entry.Value.name, fontInfo);

                    List<int> sizeList = new List<int>();
                    sizeList.Add(entry.Value.size);

                    infoSizes.Add(entry.Value.name, sizeList);
                }
                else
                {
                    infoSizes[entry.Value.name].Add(entry.Value.size);
                }
            }

            foreach (KeyValuePair<string, List<int>> entry in infoSizes)
            {
                entry.Value.Sort();
            }

            fontInfoArray = new FontInfo[infoDictionary.Count];
            int i = 0;
            foreach (KeyValuePair<string, FontInfo> entry in infoDictionary)
            {
                fontInfoArray[i] = entry.Value;
                fontInfoArray[i].sizes = new int[infoSizes[entry.Key].Count];
                for (int j = 0; j < infoSizes[entry.Key].Count; ++j)
                {
                    fontInfoArray[i].sizes[j] = infoSizes[entry.Key][j];
                }
                ++i;
            }
        }
    }
}
