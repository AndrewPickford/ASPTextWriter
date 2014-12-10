using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class BitmapFontLoader : LoadingSystem
    {
        private static BitmapFontLoader Instance = null;

        private bool _ready = false;
        private BitmapFontCache _fontCache = null;
        private int _totalFonts = 0;
        private int _loadedFonts = 0;
        private string _statusText;

        public void Awake()
        {
            if (Instance != null)
            {
                Utils.LogError("Another instance of the BitmapFontLoader is running exiting.");
                Destroy(this.gameObject);
                return;
            }
        }

        public override bool IsReady()
        {
            return _ready;
        }

        public override float ProgressFraction()
        {
            float fraction = 0f;

            if (_totalFonts > 0)
                fraction = (float) _loadedFonts / (float) _totalFonts;  

            return fraction;
        }

        public override string ProgressTitle()
        {
            return _statusText;
        }

        public override void StartLoad()
        {
            StartCoroutine(loadFonts());
        }

        public void setFontCache(BitmapFontCache fontCache)
        {
            _fontCache = fontCache;
        }

        private IEnumerator loadFonts()
        {
            UrlDir.UrlConfig[] fontConfigs = GameDatabase.Instance.GetConfigs("ASP_FONT");
            _totalFonts = fontConfigs.Length;
            _loadedFonts = 0;

            foreach (UrlDir.UrlConfig url in fontConfigs)
            {
                if (!url.config.HasValue("name"))
                {
                    Utils.Log("missing font name in {0}", url);
                }

                _statusText = url.config.GetValue("displayName") + "-" + url.config.GetValue("size");
                Utils.Log("FontCache: Loading font {0}", _statusText);
                BitmapFont font = new BitmapFont(url.config, url.parent.url);
                _fontCache.addFont(font);
                ++_loadedFonts;
                
                yield return null;
            }

            _fontCache.updateCache();

            _ready = true;
        }
    }
}
