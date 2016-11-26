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
        public static bool Debug1 = false;
        public static bool Debug2 = false;
        public static bool Debug3 = false;
        public static bool Debug4 = false;
        public static int FileCacheSize = 20 * 1024 * 1024; // bytes
        public static double LastButtonPress;
        public static double LastRepeat;
        public static double AutoRepeatGap;
        public static Texture2D WhiteBackground;
        public static Color32 Black32;
        public static Color32 White32;
        public static Color32 Gray32;
        public static Color SelectedColor;
        public static Color NotSelectedColor;
        public static Color BackgroundColor;

        public void Awake()
        {
            if (Instance != null) return;

            Instance = this;
            DontDestroyOnLoad(this);

            foreach (UrlDir.UrlConfig url in GameDatabase.Instance.GetConfigs("THE_WRITE_STUFF"))
            {
                int debugLevel = 0;
                if (url.config.HasValue("debugLevel"))
                {
                    debugLevel = int.Parse(url.config.GetValue("debugLevel"));
                    if (debugLevel >= 1) Debug1 = true;
                    if (debugLevel >= 2) Debug2 = true;
                    if (debugLevel >= 3) Debug3 = true;
                    if (debugLevel >= 4) Debug3 = true;
                    Utils.Log("Debug level {0}", debugLevel);
                }

                if (url.config.HasValue("cacheSize")) FileCacheSize = int.Parse(url.config.GetValue("cacheSize"));
            }

            WhiteBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            WhiteBackground.SetPixel(1, 1, Color.white);
            WhiteBackground.Apply();

            Black32 = new Color32(0, 0, 0, 255);
            White32 = new Color32(255, 255, 255, 255);
            Gray32 = new Color32(127, 127, 127, 255);

            SelectedColor = new Color(1.0f, 1.0f, 1.0f);
            NotSelectedColor = new Color(0.7f, 0.7f, 0.7f);
            BackgroundColor = new Color(0.5f, 0.5f, 0.5f);
        }
    }
}
