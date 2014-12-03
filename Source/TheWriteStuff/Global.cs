using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Global : MonoBehaviour
    {
        public static Global Instance { get; private set; }
        public static bool Debug1 = true;
        public static bool Debug2 = true;
        public static bool Debug3 = true;
        public static int FileCacheSize = 20 * 1024 * 1024; // bytes

        public void Awake()
        {
            if (Instance != null) return;

            Instance = this;
            DontDestroyOnLoad(this);

            foreach (UrlDir.UrlConfig url in GameDatabase.Instance.GetConfigs("THE_WRITE_STUFF"))
            {
                int debugLevel = 0;
                if (url.config.HasValue("debugLevel")) debugLevel = int.Parse(url.config.GetValue("debugLevel"));
                if (debugLevel >= 1) Debug1 = true;
                if (debugLevel >= 2) Debug2 = true;
                if (debugLevel >= 3) Debug3 = true;
                Utils.Log("Debug level {0}", debugLevel);

                if (url.config.HasValue("cacheSize")) FileCacheSize = int.Parse(url.config.GetValue("cacheSize"));
            }
        }

    }
}
