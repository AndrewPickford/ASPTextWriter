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
    public enum TransformOption { USE_FIRST, USE_ALL };

    public class ASPTextWriter : PartModule
    {
        [KSPField(isPersistant = true)]
        public string text = string.Empty;

        [KSPField(isPersistant = true)]
        public string fontName = "CAPSMALL_CLEAN";

        [KSPField(isPersistant = true)]
        public int fontSize = 32;

        [KSPField(isPersistant = true)]
        public string transformNames = string.Empty;

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
        public string textureDirUrl = string.Empty;

        [KSPField(isPersistant = true)]
        public string textures = string.Empty;

        [KSPField(isPersistant = true)]
        public string normals = string.Empty;

        [KSPField(isPersistant = true)]
        public string displayNames = string.Empty;

        [KSPField(isPersistant = true)]
        public int selectedTexture = 0;

        [KSPField(isPersistant = true)]
        public bool debugging = true;

        public AlphaOption alphaOption = AlphaOption.USE_TEXTURE;
        public NormalOption normalOption = NormalOption.USE_BACKGROUND;
        public BlendMethod blendMethod = BlendMethod.RGB;
        public TextDirection textDirection = TextDirection.LEFT_RIGHT;
        public TransformOption transformOption = TransformOption.USE_FIRST;
        public bool hasNormalMap = false;
        public Rectangle boundingBox { get; private set; }
        public string[] textureArray { get; private set; }
        public string[] normalArray { get; private set; }
        public string[] displayNameArray { get; private set; }
        public string[] transformNameArray { get; private set; }

        private TextEntryGUI _gui;
        private Material _currentMaterial = null;
        private Texture2D _currentTexture = null;
        private Texture2D _currentNormalMap = null;
        private Transform[] _textTransforms = null;
        private bool _ok = false;

        [KSPEvent(name = "Edit Text Event", guiName = "Edit Text", guiActive = false, guiActiveEditor = true)]
        public void editTextEvent()
        {
            if (_ok == false)
            {
                // something has gone wrong in OnStart
                Utils.LogError("Incorrect start up, not displaying gui");
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

        public static Texture2D PaintText(string textureURL, string text, MappedFont font, Color color, int x, int y, bool mirrorText,
                                          TextDirection direction, bool useBoundingBox, Rectangle boundingBox, BlendMethod blendMethod,
                                          int alpha, AlphaOption alphaOption)
        {
            Texture2D background = GameDatabase.Instance.GetTexture(textureURL, false);
            if (background == null)
            {
                Utils.LogError("PaintText: bad texture [{0}]", textureURL);
                throw new ArgumentNullException("texture not found");
            }

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

        public static Texture2D PaintNormalMap(string normalMapURL, string mainTextureURL, string text, MappedFont font, Color color, int x, int y,
                                               bool mirrorText, TextDirection direction, bool useBoundingBox, Rectangle boundingBox, float scale,
                                               NormalOption normalOption)
        {
            Texture2D background = GameDatabase.Instance.GetTexture(normalMapURL, true);
            if (background == null)
            {
                Utils.LogError("PaintNormalMap: bad normal map [{0}]", normalMapURL);
                throw new ArgumentNullException("normal map not found");
            }

            Texture2D mainTexture = GameDatabase.Instance.GetTexture(mainTextureURL, false);
            if (mainTexture == null)
            {
                Utils.LogError("PaintNormalMap: bad main texture [{0}]", mainTextureURL);
                throw new ArgumentNullException("main texture not found");
            }

            Texture2D mainTextureReadable = Utils.GetReadable32Texture(mainTexture, mainTextureURL, false);
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

            Texture2D textMap = new Texture2D(mainTextureReadable.width, mainTextureReadable.height, TextureFormat.ARGB32, false);
            Color transparentGray = new Color(0.5f, 0.5f, 0.5f, 0f);
            textMap.Fill(transparentGray);

            Color normalColor = Color.gray;
            if (normalOption == NormalOption.RAISE_TEXT) normalColor = Color.black;
            if (normalOption == NormalOption.LOWER_TEXT) normalColor = Color.white;
            textMap.DrawText(text, font, normalColor, x, y, mirrorText, direction, bBox);

            // scale if the main texture is a different size from the normal map
            if (mainTextureReadable.width != normalMap.width || mainTextureReadable.height != normalMap.height) textMap.Rescale(normalMap.width, normalMap.height);

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

            Texture2D newTexture = PaintText(textureArray[selectedTexture], text, font, color, offsetX, offsetY, mirrorText, textDirection, useBoundingBox, boundingBox, blendMethod, alpha, alphaOption);

            // have to make a new material 
            Material material = Instantiate(_textTransforms[0].gameObject.renderer.material) as Material;
            material.CopyPropertiesFromMaterial(_textTransforms[0].gameObject.renderer.material);
            material.SetTexture("_MainTex", newTexture);

            Texture2D newNormalMap = null;
            if (hasNormalMap)
            {
                if (normalOption == NormalOption.USE_BACKGROUND)
                {
                    Texture2D texture = GameDatabase.Instance.GetTexture(normalArray[selectedTexture], true);
                    material.SetTexture("_BumpMap", texture);
                }
                else
                {
                    newNormalMap = PaintNormalMap(normalArray[selectedTexture], textureArray[selectedTexture], text, font, color, offsetX, offsetY, mirrorText, textDirection, useBoundingBox, boundingBox, normalScale, normalOption);
                    material.SetTexture("_BumpMap", newNormalMap);
                }
            }

            for (int i = 0; i < _textTransforms.Length; ++i)
            {
                _textTransforms[i].gameObject.renderer.material = material;
            }

            if (_currentMaterial != null) Destroy(_currentMaterial);
            _currentMaterial = material;

            if (_currentTexture != null) Destroy(_currentTexture);
            _currentTexture = newTexture;

            if (_currentNormalMap != null) Destroy(_currentNormalMap);
            _currentNormalMap = newNormalMap;
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
            if (node.HasValue("transformOption")) transformOption = (TransformOption)ConfigNode.ParseEnum(typeof(TransformOption), node.GetValue("transformOption"));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.AddValue("alphaOption", ConfigNode.WriteEnum(alphaOption));
            node.AddValue("normalOption", ConfigNode.WriteEnum(normalOption));
            node.AddValue("blendMethod", ConfigNode.WriteEnum(blendMethod));
            node.AddValue("textDirection", ConfigNode.WriteEnum(textDirection));
            node.AddValue("transformOption", ConfigNode.WriteEnum(transformOption));
        }

        private string findFirstUseableTransform()
        {
            string transformName = string.Empty;
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

            if (found) return transformName;

            // if not use the first with a texture and non empty name
            foreach (Transform child in children)
            {
                if (child.gameObject.renderer != null && child.gameObject.renderer.material != null)
                {
                    Texture2D main = child.gameObject.renderer.material.mainTexture as Texture2D;

                    if (main != null && main.name != string.Empty)
                    {
                        transformName = child.name;
                        found = true;
                        break;
                    }
                }
            }

            if (found) return transformName;

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

            return transformName;
        }

        private void findTextures()
        {
            if (textures == string.Empty)
            {
                textureArray = new string[1];
                normalArray = new string[1];
                displayNameArray = new string[1];

                Texture2D texture = _textTransforms[0].gameObject.renderer.material.mainTexture as Texture2D;
                textureArray[0] = texture.name;

                texture = _textTransforms[0].gameObject.renderer.material.GetTexture("_BumpMap") as Texture2D;
                if (texture != null && texture.name != string.Empty)
                {
                    hasNormalMap = true;
                    normalArray[0] = texture.name;
                }
                else hasNormalMap = false;

                displayNameArray[0] = Path.GetFileName(textureArray[0]);
            }
            else
            {
                string url = textureDirUrl;
                if (url == string.Empty)
                {
                    Texture2D texture = _textTransforms[0].gameObject.renderer.material.mainTexture as Texture2D;
                    url = Path.GetDirectoryName(texture.name);
                }

                textureArray = Utils.AddPrefix(url + "/", Utils.SplitString(textures));

                if (normals != string.Empty)
                {
                    hasNormalMap = true;
                    normalArray = Utils.AddPrefix(url + "/", Utils.SplitString(normals));
                }
                else hasNormalMap = false;

                if (displayNames != string.Empty) displayNameArray = Utils.SplitString(displayNames);
                else
                {
                    displayNameArray = new string[textureArray.Length];
                    for (int i = 0; i < textureArray.Length; ++i)
                        displayNameArray[i] = Path.GetFileName(textureArray[i]);
                }
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
        
        private void findTransforms()
        {
            if (transformNames == string.Empty) transformNames = findFirstUseableTransform();
            if (transformNames == string.Empty)
            {
                Utils.LogError("findTransforms: unable to find transform with material");
                return;
            }

            transformNameArray = Utils.SplitString(transformNames);
            if (transformNameArray == null || transformNameArray.Length == 0)
            {
                Utils.LogError("findTransforms: TransformNames empty");
                return;
            }

            int count = 0;
            for (int i = 0; i < transformNameArray.Length; ++i)
            {
                Transform[] transforms = this.part.FindModelTransforms(transformNameArray[i]);
                if (transforms.Length == 0 || transforms[0] == null) Utils.LogError("findTransforms: unable to find transform {0}", transformNameArray[i]);
                else
                {
                    int c = 0;
                    for (int j = 0; j < transforms.Length; ++j)
                    {
                        if (transforms[j] != null)
                        {
                            ++count;
                            ++c;
                            if (transformOption == TransformOption.USE_FIRST) break;
                        }
                    }
                    if (Global.Debugging) Utils.Log("Found transform {0}, {1} times", transformNameArray[i], c);
                }
            }
            if (Global.Debugging) Utils.Log("Found {0} usable transforms", count);

            _textTransforms = new Transform[count];
            count = 0;
            for (int i = 0; i < transformNameArray.Length; ++i)
            {
                Transform[] transforms = this.part.FindModelTransforms(transformNameArray[i]);
                for (int j = 0; j < transforms.Length; ++j)
                {
                    if (transforms[j] != null)
                    {
                        _textTransforms[count] = transforms[j];
                        ++count;
                        if (transformOption == TransformOption.USE_FIRST) break;
                    }
                }
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Global.Debugging = debugging;

            if (state == StartState.Editor)
            {
                this.part.OnEditorDestroy += OnEditorDestroy;
            }

            _ok = false;
            try
            {
                if (Global.Debugging) Utils.Log("OnStart, part {0}", this.part.name);

                findTransforms();

                if (_textTransforms == null || _textTransforms[0] == null)
                {
                    Utils.LogError("No useable transforms, disabling plugin");
                    return;
                }

                findTextures();

                boundingBox = new Rectangle(bottomLeftX, bottomLeftY, width, height);

                MappedFont font = findFont();
                if (font == null)
                {
                    Utils.LogError("No useable fonts, disabling plugin");
                    return;
                }

                if (text != string.Empty) writeText();
                _ok = true;
            }
            catch
            {
                Utils.LogError("Something went wrong in OnStart disabling plugin");
            }
        }
    }
}