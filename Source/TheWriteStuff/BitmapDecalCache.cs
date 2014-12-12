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
        public enum Origin { BOTTOM_LEFT, TOP_LEFT };

        public struct Sheet
        {
            public string url;
            public Origin origin;
            public string displayName;
            public List<BitmapDecal> decals;
        }

        public static BitmapDecalCache Instance { get; private set; }

        public List<Sheet> monoSheets { get; private set; }
        public List<Sheet> colorSheets { get; private set; }
        public Dictionary<string, BitmapDecal> decals { get; private set; }
        public bool hasDecals { get; private set; }

        private List<Sheet> _sheets;

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
            _sheets = new List<Sheet>();
            monoSheets = new List<Sheet>();
            colorSheets = new List<Sheet>();
            hasDecals = false;
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
            Sheet sheet = new Sheet();
            Sheet monoSheet = new Sheet();
            Sheet colorSheet = new Sheet();

            sheet.url = nodeUrl + "/" + node.GetValue("id");
            sheet.displayName = node.GetValue("displayName");
            sheet.decals = new List<BitmapDecal>();

            sheet.origin = Origin.BOTTOM_LEFT;
            if (node.HasValue("origin")) sheet.origin = (Origin)ConfigNode.ParseEnum(typeof(Origin), node.GetValue("origin"));

            Texture2D texture = Utils.LoadTexture("GameData/" + nodeUrl + ".pngmap", false);

            monoSheet.url = sheet.url;
            monoSheet.displayName = sheet.displayName;
            monoSheet.decals = new List<BitmapDecal>();
            monoSheet.origin = sheet.origin;

            colorSheet.url = sheet.url;
            colorSheet.displayName = sheet.displayName;
            colorSheet.decals = new List<BitmapDecal>();
            colorSheet.origin = sheet.origin;

            foreach (ConfigNode n in node.GetNodes("ASP_BITMAP_DECAL"))
            {
                BitmapDecal decal = new BitmapDecal(n, sheet, texture);

                if (decal.image != null && decal.image.width > 0 && decal.image.height > 0)
                {
                    decals[decal.url] = decal;
                    sheet.decals.Add(decal);

                    if (decal.type == BitmapDecal.Type.MONO) monoSheet.decals.Add(decal);
                    else if (decal.type == BitmapDecal.Type.COLOR) colorSheet.decals.Add(decal);
                }
            }

            Destroy(texture);

            _sheets.Add(sheet);
            if (monoSheet.decals.Count > 0) monoSheets.Add(monoSheet);
            if (colorSheet.decals.Count > 0) colorSheets.Add(colorSheet);

            if (decals.Count > 0) hasDecals = true;
        }
    }
}
