using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace ASP
{
    public enum AlphaOption { USE_TEXTURE, TEXT_ONLY, WHOLE_TEXTURE };

    public enum NormalOption { FLAT, RAISE_TEXT, LOWER_TEXT, USE_BACKGROUND };

    public class ASPTextWriter : PartModule
    {
        [KSPField(isPersistant = true)]
        public string text = "text";

        [KSPField(isPersistant = true)]
        public string fontName = string.Empty;

        [KSPField(isPersistant = true)]
        public int fontSize = -1;

        [KSPField(isPersistant = true)]
        public string transformName = string.Empty;

        [KSPField(isPersistant = true)]
        public int topLeftX = -1;

        [KSPField(isPersistant = true)]
        public int topLeftY = -1;

        [KSPField(isPersistant = true)]
        public int width = -1;

        [KSPField(isPersistant = true)]
        public int height = -1;

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

        [KSPField(isPersistant = true)]
        public string textures = string.Empty;

        [KSPField(isPersistant = true)]
        public string normals = string.Empty;

        [KSPField(isPersistant = true)]
        public string displayNames = string.Empty;

        [KSPField(isPersistant = true)]
        public int selectedTexture = 0;

        public AlphaOption alphaOption = AlphaOption.USE_TEXTURE;
        public NormalOption normalOption = NormalOption.RAISE_TEXT;
        public bool hasNormalMap = false;
        public Rectangle boundingBox { get; private set; }
        public Texture2D backgroundTexture { get; private set; }
        public Texture2D backgroundNormalMap { get; private set; }
        public string[] textureArray { get; private set; }
        public string[] normalArray { get; private set; }
        public string[] displayNameArray { get; private set; }
        public string url { get; private set; }

        private TextEntryGUI _gui;

        [KSPEvent(name = "Edit Text Event", guiName = "Edit text", guiActive = false, guiActiveEditor = true)]
        public void editTextEvent()
        {
            _gui = gameObject.GetComponent<TextEntryGUI>();
            if (_gui == null)
            {
                _gui = gameObject.AddComponent<TextEntryGUI>();
                _gui.initialise(this);
            }
        }

        public static Texture2D PaintText(Texture2D background, string text, MappedFont font, Color color, int x, int y, Rectangle bBox, int alpha, AlphaOption alphaOption)
        {
            // load the texture in otherwise we can't read the pixels
            Texture2D backgroundReadable = Utils.LoadTextureFromUrl(background.name);
            Texture2D texture = new Texture2D(background.width, background.height, TextureFormat.ARGB32, true);

            Color32[] pixels = backgroundReadable.GetPixels32();
            texture.name = backgroundReadable.name;
            texture.SetPixels32(pixels);

            if (alphaOption == AlphaOption.TEXT_ONLY) texture.DrawText(text, font, color, x, y, bBox, true, alpha);
            else texture.DrawText(text, font, color, x, y, bBox);

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

        public static Texture2D PaintNormalMap(Texture2D background, string text, MappedFont font, Color color, int x, int y, Rectangle bBox, float scale, NormalOption normalOption)
        {
            // load the texture in otherwise we can't read the pixels
            Texture2D normalMap = Utils.LoadNormalMapFromUrl(background.name);

            Texture2D textMap = new Texture2D(normalMap.width, normalMap.height, TextureFormat.ARGB32, false);
            textMap.Fill(Color.gray);
            Color normalColor = Color.gray;
            if (normalOption == NormalOption.RAISE_TEXT) normalColor = Color.black;
            if (normalOption == NormalOption.LOWER_TEXT) normalColor = Color.white;
            textMap.DrawText(text, font, normalColor, x, y, bBox);

            if (normalOption == NormalOption.FLAT) textMap.Fill(Color.gray);

            Texture2D textNormalMap = NormalMap.Create(textMap, scale);

            Color transparent = new Color(0f, 0f, 0f, 0f);
            textMap.Fill(transparent);
            textMap.DrawText(text, font, Color.white, x, y, bBox);

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

            string textureURL = url + "/" + textureArray[selectedTexture];
            backgroundTexture = GameDatabase.Instance.GetTexture(textureURL, false);

            textureURL = url + "/" + normalArray[selectedTexture];
            backgroundNormalMap = GameDatabase.Instance.GetTexture(textureURL, true);

            Texture2D newTexture = PaintText(backgroundTexture, text, font, color, topLeftX + offsetX, topLeftY + offsetY, boundingBox, alpha, alphaOption);

            material.SetTexture("_MainTex", newTexture);
            
            if (backgroundNormalMap != null)
            {
                if (normalOption != NormalOption.USE_BACKGROUND)
                {
                    Texture2D newNormalMap = PaintNormalMap(backgroundNormalMap, text, font, color, topLeftX + offsetX, topLeftY + offsetY, boundingBox, normalScale, normalOption);
                    material.SetTexture("_BumpMap", newNormalMap);
                }
                else
                {
                    Texture2D normalMap = material.GetTexture("_BumpMap") as Texture2D;
                    if (normalMap != backgroundNormalMap)
                    {
                        material.SetTexture("_BumpMap", backgroundNormalMap);
                    }
                }
            }
            transform.gameObject.renderer.material = material;
        }

        private void OnEditorDestroy()
        {
            if (_gui != null) GameObject.Destroy(_gui);
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

            if (transformName == "__FIRST__" || transformName == string.Empty)
            {
                transformName = string.Empty;
                Transform[] children = part.gameObject.GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    if (child.gameObject.renderer != null && child.gameObject.renderer.material != null) transformName = child.name;
                }
            }
            if (transformName == string.Empty)
            {
                Debug.LogError("Unable to find transform with material");
                return;
            }

            Transform transform = this.part.FindModelTransform(transformName);
            if (transform == null) return;

            backgroundTexture = transform.gameObject.renderer.material.mainTexture as Texture2D;
            backgroundNormalMap = transform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (backgroundNormalMap != null) hasNormalMap = true;

            // all the textures need to be in the same directory as the first texture
            url = Path.GetDirectoryName(backgroundTexture.name);
            Debug.Log(String.Format("q22 {0}", url));

            if (textures == string.Empty)
            {
                textureArray = new string[1];
                normalArray = new string[1];
                displayNameArray = new string[1];

                textureArray[0] = Path.GetFileName(backgroundTexture.name);
                displayNameArray[0] = textureArray[0];

                if (backgroundNormalMap != null) normalArray[0] = Path.GetFileName(backgroundNormalMap.name);
            }
            else
            {
                textureArray = Utils.SplitString(textures);
                normalArray = Utils.SplitString(normals);
                displayNameArray = Utils.SplitString(displayNames);
            }

            if (topLeftX == -1)
            {
                // if no bounding box is given write on the whole texture
                // usually a bad idea, but very usefull for testing
                topLeftX = 0;
                topLeftY = 0;
                width = backgroundTexture.width;
                height = backgroundTexture.height;
            }

            // invert the y position, unity starts at the bottom left
            boundingBox = new Rectangle(topLeftX, backgroundTexture.height - (topLeftY + height), width, height);

            string fontID = fontName + "-" + fontSize.ToString();
            MappedFont font = ASPFontCache.Instance.getFontByID(fontID);
            if (font == null)
            {
                font = ASPFontCache.Instance.list.First();
                if (font == null) return;
                fontName = font.name;
                fontSize = font.size;
            }

            writeText();
        }
    }
}