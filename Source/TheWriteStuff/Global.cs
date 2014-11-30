using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Global : MonoBehaviour
    {
        public static bool Debugging = false;
        public static Global Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null) return;

            foreach (UrlDir.UrlConfig url in GameDatabase.Instance.GetConfigs("THE_WRITE_STUFF"))
            {
                if (url.config.HasValue("debug")) Debugging = bool.Parse(url.config.GetValue("debug"));
            }
        }
    }
}
