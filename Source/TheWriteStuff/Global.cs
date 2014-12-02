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
        public static bool Debugging = true;
        public static int FileCacheSize = 40 * 1024 * 1024; // bytes

        public void Awake()
        {
            if (Instance != null) return;

            foreach (UrlDir.UrlConfig url in GameDatabase.Instance.GetConfigs("THE_WRITE_STUFF"))
            {
                if (url.config.HasValue("debug")) Debugging = bool.Parse(url.config.GetValue("debug"));
                if (url.config.HasValue("cacheSize")) FileCacheSize = int.Parse(url.config.GetValue("cacheSize"));
            }

            Utils.Log("Deugging set to {0}", Debugging);
        }

    }
}
