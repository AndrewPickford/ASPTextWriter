using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public class MonoDecal : MonoOverlay
        {
            static string _displayName = "Mono Decal";

            private string _url = string.Empty;

            private MonoDecalGui _gui;

            public override void load(ConfigNode node)
            {
                _url = string.Empty;

                loadMonoOverlay(node);

                if (node.HasValue("url")) _url = node.GetValue("url");
            }

            public override void save(ConfigNode node)
            {
                node.AddValue("type", "mono_decal");
                node.AddValue("url", _url);
                saveMonoOverlay(node);
            }


            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
            }

            public override ImageModifier clone()
            {
                IM.MonoDecal im = new IM.MonoDecal();

                im._url = _url;

                im.copyFromMonoOverlay(this);

                return im;
            }

            public override void cleanUp()
            {
                _gui = null;
            }

            public override string displayName()
            {
                return _displayName;
            }

            public override ImageModifierGui gui()
            {
                if (_gui == null) _gui = new MonoDecalGui(this);
                return _gui;
            }






            public class MonoDecalGui : MonoOverlayGui
            {
                private IM.MonoDecal _imMonoDecal;

                public MonoDecalGui(IM.MonoDecal monoDecal)
                    : base(monoDecal)
                {
                    _imMonoDecal = monoDecal;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    header(gui, "MONO DECAL");

                    GUILayout.Label("Url: " + _imMonoDecal._url, GUILayout.ExpandWidth(false));
                    GUILayout.Space(5);

                    drawBottomMonoOverlay(gui);

                    GUILayout.EndVertical();
                }

                public override void drawRight(TextureEditGUI gui)
                {
                }

                public override void initialise(TextureEditGUI gui)
                {
                    initialiseMonoOverlay(gui);
                }

                public override string buttonText()
                {
                    if (_imMonoDecal._url == string.Empty) return "Mono Decal";
                    else if (_imMonoDecal._url.Length < 8) return _imMonoDecal._url;
                    else return _imMonoDecal._url.Substring(_imMonoDecal._url.Length - 7, 7) + "..";
                }
            }
        }
    }
}
