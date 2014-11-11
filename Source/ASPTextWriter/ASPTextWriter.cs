using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace ASP
{
    public enum AlphaOption { USE_TEXTURE, TEXT_ONLY, WHOLE_TEXTURE };

    public enum NormalOption { RAISE_TEXT, USE_BACKGROUND };

    public class ASPTextWriter : PartModule
    {
        [KSPField(isPersistant = true)]
        public string text = "";

        [KSPField(isPersistant = true)]
        public string fontName = "";

        [KSPField(isPersistant = true)]
        public int fontSize = 0;

        [KSPField(isPersistant = true)]
        public string transformName = "";

        [KSPField(isPersistant = true)]
        public int topLeftX = 0;

        [KSPField(isPersistant = true)]
        public int topLeftY = 0;

        [KSPField(isPersistant = true)]
        public int width = 100;

        [KSPField(isPersistant = true)]
        public int height = 100;

        [KSPField(isPersistant = true)]
        public int offsetX = 0;

        [KSPField(isPersistant = true)]
        public int offsetY = 0;

        [KSPField(isPersistant = true)]
        public int red = 0;

        [KSPField(isPersistant = true)]
        public int green = 0;

        [KSPField(isPersistant = true)]
        public int blue = 0;

        [KSPField(isPersistant = true)]
        public int alpha = 255;

        [KSPField(isPersistant = true)]
        public float normalScale = 2.0f;

        public AlphaOption alphaOption = AlphaOption.USE_TEXTURE;
        public NormalOption normalOption = NormalOption.RAISE_TEXT;
        public bool hasNormalMap = false;

        private TextEntryGUI gui;
        private Texture2D originalTexture;
        private Texture2D originalBumpMap;

        [KSPEvent(name = "Edit Text Event", guiName = "Edit text", guiActive = false, guiActiveEditor = true)]
        public void editTextEvent()
        {
            gui = gameObject.GetComponent<TextEntryGUI>();
            if (gui == null)
            {
                gui = gameObject.AddComponent<TextEntryGUI>();
                gui.initialise(this);
            }
        }

        public static Texture2D PaintText(Texture2D background, string text, MappedFont font, Color color, int x, int y, int alpha, AlphaOption alphaOption)
        {
            Texture2D texture = new Texture2D(background.width, background.height, TextureFormat.ARGB32, true);

            Color32[] pixels = background.GetPixels32();
            texture.name = background.name;
            texture.SetPixels32(pixels);
            texture.Apply();

            if (alphaOption == AlphaOption.TEXT_ONLY) texture.DrawText(text, font, color, x, y, true, alpha);
            else texture.DrawText(text, font, color, x, y);

            if (alphaOption == AlphaOption.WHOLE_TEXTURE)
            {
                pixels = texture.GetPixels32();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i].a = (byte) alpha;
                }
            }

            texture.Apply(true);

            if (background.format == TextureFormat.DXT1 || background.format == TextureFormat.DXT5) texture.Compress(true);

            return texture;
        }

        public static Texture2D PaintBumpMap(Texture2D background, string text, MappedFont font, Color color, int x, int y, float scale)
        {
            Texture2D normalMap = Utils.LoadNormalMapFromUrl(background.name);

            Texture2D textMap = new Texture2D(normalMap.width, normalMap.height, TextureFormat.ARGB32, false);
            textMap.Fill(Color.gray);
            textMap.DrawText(text, font, Color.white, x, y);
            textMap.Apply(false);

            Texture2D textNormalMap = NormalMap.Create(textMap, scale);

            Color transparent = new Color(0f, 0f, 0f, 0f);
            textMap.Fill(transparent);
            textMap.DrawText(text, font, Color.white, x, y);
            textMap.Apply();

            for (int i = 0; i < normalMap.width; ++i)
            {
                for (int j = 0; j < normalMap.height; ++j)
                {
                    Color textColor = textMap.GetPixel(i, j);
                    Color textNormalColor = textNormalMap.GetPixel(i, j);

                    if (textColor.a != 0f) normalMap.SetPixel(i, j, textNormalColor);
                }
            }
            normalMap.Apply();

            return normalMap;
        }

        public void writeText()
        {
            if (text == null || text == "") return;

            Transform transform = this.part.FindModelTransform(transformName);
            if (transform == null) return;

            string fontID = fontName + "-" + fontSize.ToString();
            MappedFont font = ASPFontCache.Instance.getFontByID(fontID);
            if (font == null)
            {
                font = ASPFontCache.Instance.list.First();
                if (font == null) return;
                fontName = font.name;
                fontSize = font.size;
            }

            Material material = Instantiate(transform.gameObject.renderer.material) as Material;

            Color color = new Color((float) red/255f, (float) green/255f, (float) blue/255f);

            Texture2D newTexture = PaintText(originalTexture, text, font, color, topLeftX + offsetX, topLeftY + offsetY, alpha, alphaOption);

            material.SetTexture("_MainTex", newTexture);
            
            if (originalBumpMap != null)
            {
                if (normalOption == NormalOption.RAISE_TEXT)
                {
                    Texture2D newBumpMap = PaintBumpMap(originalBumpMap, text, font, color, topLeftX + offsetX, topLeftY + offsetY, normalScale);
                    material.SetTexture("_BumpMap", newBumpMap);
                }
                else
                {
                    Texture2D bumpMap = material.GetTexture("_BumpMap") as Texture2D;
                    if (bumpMap != originalBumpMap) material.SetTexture("_BumpMap", originalBumpMap);
                }
            }
            transform.gameObject.renderer.material = material;
        }

        private void OnEditorDestroy()
        {
            if (gui != null) GameObject.Destroy(gui);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("alphaOption")) alphaOption = (AlphaOption) ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
            if (node.HasValue("normalOption")) normalOption = (NormalOption) ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.AddValue("alphaOption", ConfigNode.WriteEnum(alphaOption));
            node.AddValue("normalOption", ConfigNode.WriteEnum(normalOption));
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                this.part.OnEditorDestroy += OnEditorDestroy;
            }

            Transform transform = this.part.FindModelTransform(transformName);
            if (transform == null) return;

            originalTexture = transform.gameObject.renderer.material.mainTexture as Texture2D;
            originalBumpMap = transform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (originalBumpMap != null) hasNormalMap = true;

            writeText();
        }
    }
}