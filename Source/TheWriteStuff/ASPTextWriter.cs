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
    public enum BlendMethod {  PIXEL, RGB, HSV };
    public enum TextDirection { LEFT_RIGHT, RIGHT_LEFT, UP_DOWN, DOWN_UP };

    public class ASPTextWriter : PartModule
    {
        [KSPField(isPersistant = true)]
        public string text = string.Empty;

        [KSPField(isPersistant = true)]
        public string fontName = "CAPSMALL_CLEAN";

        [KSPField(isPersistant = true)]
        public int fontSize = 32;

        [KSPField(isPersistant = true)]
        public string transformName = string.Empty;

        [KSPField(isPersistant = true)]
        public int bottomLeftX = -1;

        [KSPField(isPersistant = true)]
        public int bottomLeftY = -1;

        [KSPField(isPersistant = true)]
        public bool useBoundingBox = false;

        [KSPField(isPersistant = true)]
        public int width = -1;

        [KSPField(isPersistant = true)]
        public int height = -1;

        [KSPField(isPersistant = true)]
        public int offsetX = 0;

        [KSPField(isPersistant = true)]
        public int offsetY = 0;

        [KSPField(isPersistant = true)]
        public bool mirrorText = false;

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
        public NormalOption normalOption = NormalOption.USE_BACKGROUND;
        public BlendMethod blendMethod = BlendMethod.RGB;
        public TextDirection textDirection = TextDirection.LEFT_RIGHT;
        public bool hasNormalMap = false;
        public Rectangle boundingBox { get; private set; }
        public Texture2D backgroundTexture { get; private set; }
        public Texture2D backgroundNormalMap { get; private set; }
        public string[] textureArray { get; private set; }
        public string[] normalArray { get; private set; }
        public string[] displayNameArray { get; private set; }
        public string url { get; private set; }

        private TextEntryGUI _gui;
        private Material _currentMaterial = null;
        private Texture2D _currentTexture = null;
        private Texture2D _currentNormalMap = null;
        private Transform _textTransform = null;
        private bool _ok = false;

        [KSPEvent(name = "Edit Text Event", guiName = "Edit Text", guiActive = false, guiActiveEditor = true)]
        public void editTextEvent()
        {
            if (_ok == false)
            {
                // something has gone wrong in OnStart
                Debug.LogError("TWS - incorrect start up, cannot display gui");
                ScreenMessages.PostScreenMessage("Error unable to start text writer gui", 5, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _gui = gameObject.GetComponent<TextEntryGUI>();
            if (_gui == null)
            {
                _gui = gameObject.AddComponent<TextEntryGUI>();
                _gui.initialise(this);
            }
        }

        public static Texture2D PaintText(Texture2D background, string textureURL, string text, MappedFont font, Color color, int x, int y, bool mirrorText,
                                          TextDirection direction, bool useBoundingBox, Rectangle boundingBox, BlendMethod blendMethod, int alpha, AlphaOption alphaOption)
        {
            Texture2D backgroundReadable = Utils.GetReadable32Texture(background, textureURL, false);
            Texture2D texture = new Texture2D(backgroundReadable.width, backgroundReadable.height, TextureFormat.ARGB32, true);

            Color32[] pixels = backgroundReadable.GetPixels32();
            texture.name = background.name + "(Copy)";
            texture.SetPixels32(pixels);

            Rectangle bBox = new Rectangle(boundingBox);
            if (bBox.w == -1 || useBoundingBox == false)
            {
                bBox.x = 0;
                bBox.y = 0;
                bBox.w = backgroundReadable.width;
                bBox.h = backgroundReadable.height;
            }

            if (alphaOption == AlphaOption.TEXT_ONLY) texture.DrawText(text, font, color, x, y, mirrorText, direction, bBox, blendMethod, true, (float) alpha / 255f);
            else texture.DrawText(text, font, color, x, y, mirrorText, direction, bBox, blendMethod, false, 255);

            if (alphaOption == AlphaOption.WHOLE_TEXTURE)
            {
                pixels = texture.GetPixels32();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i].a = (byte) alpha;
                }
                texture.SetPixels32(pixels);
            }

            texture.Apply(true);

            if (background.format == TextureFormat.DXT1 || background.format == TextureFormat.DXT5) texture.Compress(true);

            if (!System.Object.ReferenceEquals(background, backgroundReadable)) Destroy(backgroundReadable);

            return texture;
        }

        public static Texture2D PaintNormalMap(Texture2D background, string normalMapURL, Texture2D mainTexture, string text, MappedFont font, Color color, int x, int y,
                                               bool mirrorText, TextDirection direction, bool useBoundingBox, Rectangle boundingBox, float scale,
                                               NormalOption normalOption)
        {
            Texture2D backgroundReadable = Utils.GetReadable32Texture(background, normalMapURL, true);
            Texture2D normalMap = new Texture2D(backgroundReadable.width, backgroundReadable.height, TextureFormat.ARGB32, true);
            Color32[] pixels = backgroundReadable.GetPixels32();
            normalMap.name = background.name + "(Copy)";
            normalMap.SetPixels32(pixels);

            Rectangle bBox = new Rectangle(boundingBox);
            if (bBox.w == -1 || useBoundingBox)
            {
                bBox.x = 0;
                bBox.y = 0;
                bBox.w = backgroundReadable.width;
                bBox.h = backgroundReadable.height;
            }

            Texture2D textMap = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.ARGB32, false);
            Color transparentGray = new Color(0.5f, 0.5f, 0.5f, 0f);
            textMap.Fill(transparentGray);

            Color normalColor = Color.gray;
            if (normalOption == NormalOption.RAISE_TEXT) normalColor = Color.black;
            if (normalOption == NormalOption.LOWER_TEXT) normalColor = Color.white;
            textMap.DrawText(text, font, normalColor, x, y, mirrorText, direction, bBox);

            // scale if the main texture is a different size from the normal map
            if (mainTexture.width != background.width || mainTexture.height != background.height) textMap.Rescale(background.width, background.height);

            Texture2D textNormalMap = NormalMap.Create(textMap, scale);

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
            
            if (!System.Object.ReferenceEquals(background, backgroundReadable)) Destroy(backgroundReadable);
            Destroy(textMap);
            Destroy(textNormalMap);
            
            return normalMap;
        }

        public void writeText()
        {
            if (text == null || text == string.Empty) return;

            MappedFont font = findFont();
            if (font == null) return;

            Color color = new Color((float) red/255f, (float) green/255f, (float) blue/255f);

            string textureURL = url + "/" + textureArray[selectedTexture];
            backgroundTexture = GameDatabase.Instance.GetTexture(textureURL, false);

            string normalMapURL = string.Empty;
            if (hasNormalMap)
            {
                normalMapURL = url + "/" + normalArray[selectedTexture];
                backgroundNormalMap = GameDatabase.Instance.GetTexture(normalMapURL, true);
            }
            else backgroundNormalMap = null;

            Texture2D newTexture = PaintText(backgroundTexture, textureURL, text, font, color, offsetX, offsetY, mirrorText, textDirection, useBoundingBox, boundingBox, blendMethod, alpha, alphaOption);

            // have to make a new material 
            Material material = Instantiate(_textTransform.gameObject.renderer.material) as Material;
            material.CopyPropertiesFromMaterial(_textTransform.gameObject.renderer.material);
            material.SetTexture("_MainTex", newTexture);

            if (hasNormalMap && backgroundNormalMap != null)
            {
                if (normalOption == NormalOption.USE_BACKGROUND)
                {
                    material.SetTexture("_BumpMap", backgroundNormalMap);
                }
                else
                {
                    Texture2D newNormalMap = PaintNormalMap(backgroundNormalMap, normalMapURL, backgroundTexture, text, font, color, offsetX, offsetY, mirrorText, textDirection, useBoundingBox, boundingBox, normalScale, normalOption);
                    material.SetTexture("_BumpMap", newNormalMap);
                }
            }

            _textTransform.gameObject.renderer.material = material;

            if (_currentMaterial != null) Destroy(_currentMaterial);
            _currentMaterial = material;

            if (_currentTexture != null) Destroy(_currentTexture);
            _currentTexture = newTexture;

            if (_currentNormalMap != null) Destroy(_currentNormalMap);
            _currentNormalMap = null;
            if (backgroundNormalMap != null && normalOption != NormalOption.USE_BACKGROUND) _currentNormalMap = material.GetTexture("_BumpMap") as Texture2D;
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
            if (node.HasValue("blendMethod")) blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
            if (node.HasValue("textDirection")) textDirection = (TextDirection)ConfigNode.ParseEnum(typeof(TextDirection), node.GetValue("textDirection"));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.AddValue("alphaOption", ConfigNode.WriteEnum(alphaOption));
            node.AddValue("normalOption", ConfigNode.WriteEnum(normalOption));
            node.AddValue("blendMethod", ConfigNode.WriteEnum(blendMethod));
            node.AddValue("textDirection", ConfigNode.WriteEnum(textDirection));
        }

        private void findUsableTransform()
        {
            transformName = string.Empty;
            Transform[] children = this.part.partInfo.partPrefab.GetComponentsInChildren<Transform>(true);

            // use the first object with both a texture and a normal map with non empty names
            bool found = false;
            foreach (Transform child in children)
            {
                if (child.gameObject.renderer != null && child.gameObject.renderer.material != null)
                {
                    Texture2D main = child.gameObject.renderer.material.mainTexture as Texture2D;
                    Texture2D normal = child.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
                    
                    if (main != null && normal != null && main.name != string.Empty && normal.name != string.Empty)
                    {
                        transformName = child.name;
                        found = true;
                        break;
                    }
                }
            }

            if (found) return;

            // fall back to the first object with a material
            foreach (Transform child in children)
            {
                if (child.gameObject.renderer != null && child.gameObject.renderer.material != null)
                {
                    transformName = child.name;
                    found = true;
                    break;
                }
            }
        }

        private void findTextures()
        {
            backgroundTexture = _textTransform.gameObject.renderer.material.mainTexture as Texture2D;
            backgroundNormalMap = _textTransform.gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
            if (backgroundNormalMap != null && backgroundNormalMap.name != string.Empty) hasNormalMap = true;

            // all the textures need to be in the same directory as the first texture
            url = Path.GetDirectoryName(backgroundTexture.name);

            if (textures == string.Empty)
            {
                textureArray = new string[1];
                normalArray = new string[1];
                displayNameArray = new string[1];

                textureArray[0] = Path.GetFileName(backgroundTexture.name);
                displayNameArray[0] = textureArray[0];

                if (hasNormalMap) normalArray[0] = Path.GetFileName(backgroundNormalMap.name);
            }
            else
            {
                textureArray = Utils.SplitString(textures);

                if (normals != string.Empty)
                {
                    hasNormalMap = true;
                    normalArray = Utils.SplitString(normals);
                }
                else hasNormalMap = false;
                
                if (displayNames != string.Empty) displayNameArray = Utils.SplitString(displayNames);
                else displayNameArray = textureArray;
            }
        }

        private MappedFont findFont()
        {
            string fontID = fontName + "-" + fontSize.ToString();
            MappedFont font = FontCache.Instance.getFontByID(fontID);
            if (font == null)
            {
                font = FontCache.Instance.mappedList.First();
                if (font == null) fontName = string.Empty;
                else
                {
                    fontName = font.name;
                    fontSize = font.size;
                }
            }

            return font;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                this.part.OnEditorDestroy += OnEditorDestroy;
            }

            if (transformName == "__FIRST__" || transformName == string.Empty) findUsableTransform();
            if (transformName == string.Empty)
            {
                Debug.LogError("TWS: Unable to find transform with material");
                return;
            }

            _textTransform = this.part.FindModelTransform(transformName);
            if (_textTransform == null)
            {
                Debug.LogError(String.Format("TWS: Unable to find transform {0}", transformName));
                return;
            }

            findTextures();

            boundingBox = new Rectangle(bottomLeftX, bottomLeftY, width, height);

            MappedFont font = findFont();
            if (font == null) return;

            _ok = true;
            if (text != string.Empty) writeText();
        }
    }
}