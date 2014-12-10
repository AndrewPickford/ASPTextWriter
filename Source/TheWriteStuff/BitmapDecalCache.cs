using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BitmapDecalCache : MonoBehaviour
    {
        public struct BitmapDecalSheet
        {
            public string url;
            public string displayName;
            public List<BitmapDecal> decals;
        }

        public static BitmapDecalCache Instance { get; private set; }

        public List<BitmapDecalSheet> sheets { get; private set; }
        public Dictionary<string, BitmapDecal> decals { get; private set; }

        public void Awake()
        {
            if (Instance != null)
            {
                Utils.LogError("Another instance of the DecalCache is running exiting.");
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);

            decals = new Dictionary<string, BitmapDecal>();
            sheets = new List<BitmapDecalSheet>();
        }

        public void Start()
        {
            LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();
            if (loadingScreen == null)
            {
                Utils.LogError("unable to find loading screen, exiting");
                return;
            }

            List<LoadingSystem> loadersList = LoadingScreen.Instance.loaders;
            if (loadersList == null)
            {
                Utils.LogError("empty loading list, exiting.");
                return;
            }

            Utils.Log("adding decalcache to loading list");
            BitmapDecalLoader bitmapDecalLoader = this.gameObject.AddComponent<BitmapDecalLoader>();
            bitmapDecalLoader.setDecalCache(this);
            loadersList.Add(bitmapDecalLoader);
        }

        public void addSheet(ConfigNode node, string nodeUrl)
        {
            BitmapDecalSheet sheet = new BitmapDecalSheet();

            sheet.url = nodeUrl + "/" + node.GetValue("id");
            sheet.displayName = node.GetValue("displayName");
            sheet.decals = new List<BitmapDecal>();

            Texture2D texture = Utils.LoadTexture("GameData/" + nodeUrl + ".pngmap", false);

            foreach (ConfigNode n in node.GetNodes("ASP_DECAL"))
            {
                BitmapDecal decal = new BitmapDecal(n, sheet.url, texture);

                if (decal.image != null && decal.image.width > 0 && decal.image.height > 0)
                {
                    decals[decal.url] = decal;
                    sheet.decals.Add(decal);
                }
            }

            Destroy(texture);
            sheets.Add(sheet);
        }
    }
}
