using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class BitmapDecalLoader : LoadingSystem
    {
        private static BitmapDecalLoader Instance = null;

        private bool _ready = false;
        private BitmapDecalCache _decalCache = null;
        private int _totalSheets = 0;
        private int _loadedSheets = 0;
        private string _statusText;

        public void Awake()
        {
            if (Instance != null)
            {
                Utils.LogError("Another instance of the BitmappedDecalLoader is running exiting.");
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

            if (_totalSheets > 0)
                fraction = (float)_loadedSheets / (float)_totalSheets;

            return fraction;
        }

        public override string ProgressTitle()
        {
            return _statusText;
        }

        public override void StartLoad()
        {
            StartCoroutine(loadDecals());
        }

        public void setDecalCache(BitmapDecalCache decalCache)
        {
            _decalCache = decalCache;
        }

        private IEnumerator loadDecals()
        {
            UrlDir.UrlConfig[] decalListConfigs = GameDatabase.Instance.GetConfigs("ASP_BITMAP_DECAL_LIST");
            _totalSheets = decalListConfigs.Length;
            _loadedSheets = 0;

            foreach (UrlDir.UrlConfig url in decalListConfigs)
            {
                if (!url.config.HasValue("id"))
                {
                    Utils.Log("missing decal list id in {0}", url);
                }
                else
                {
                    _statusText = url.config.GetValue("displayName");
                    Utils.Log("DecalCache: Loading decal sheet {0}", _statusText);

                    try
                    {
                        _decalCache.addSheet(url.config, url.parent.url);
                        ++_loadedSheets;
                    }
                    catch
                    {
                        Utils.LogError("error loading decal sheet {0}", _statusText);
                    }
                }

                yield return null;
            }

            _ready = true;
        }
    }
}
