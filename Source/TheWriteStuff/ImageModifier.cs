using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public enum Rotation { R0, R90, R180, R270 };
    public enum BlendMethod { PIXEL, RGB, HSV };
    public enum TransformOption { USE_FIRST, USE_ALL };
    public enum NormalOption { FLAT, RAISE_TEXT, LOWER_TEXT, USE_BACKGROUND };
    public enum AlphaOption { USE_TEXTURE, OVERWRITE };

    public abstract class ImageModifier
    {
        public enum Type { INVALID, BASE_TEXTURE, TEXT, MONO_DECAL, COLOR_DECAL };

        public abstract void save(ConfigNode node);
        public abstract void load(ConfigNode node);
        public abstract void drawOnImage(ref Image image, BoundingBox boundingBox);
        public abstract void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox);
        public abstract ImageModifier clone();
        public abstract void cleanUp();
        public abstract string displayName();
        public abstract ImageModifierGui gui();

        protected Type _type = Type.INVALID;

        public static ImageModifier CreateFromConfig(ConfigNode node)
        {
            ImageModifier imageModifier = null;

            Type type = Type.INVALID;
            if (node.HasValue("type")) type = (Type)ConfigNode.ParseEnum(typeof(Type), node.GetValue("type"));

            switch (type)
            {
                case Type.BASE_TEXTURE:
                    imageModifier = new IM.BaseTexture();
                    break;

                case Type.TEXT:
                    imageModifier = new IM.Text();
                    break;

                case Type.MONO_DECAL:
                    imageModifier = new IM.MonoDecal();
                    break;

                case Type.COLOR_DECAL:
                    imageModifier = new IM.ColorDecal();
                    break;

                default:
                case Type.INVALID:
                    break;
            }

            if (imageModifier == null)  Utils.LogError("unknown image modifier");
            else
            {
                imageModifier._type = type;
                imageModifier.load(node);
            }

            return imageModifier;
        }

        protected void saveImageModifier(ConfigNode node)
        {
            node.AddValue("type", ConfigNode.WriteEnum(_type));
        }
    }

    public abstract class ImageModifierGui
    {
        private static string[] _posButtons = { "+", "-", "<", ">", "++", "--", "<<", ">>" };
        private static string[] _speedGrid = { "x 1", "x 10" };
        private static string[] _rotationGrid = { "0", "90", "180", "270" };
        private static string[] _blendMethodGrid = { "Pixel", "RGB", "HSV" };
        private static string[] _alphaOptionGrid = { "Use Texture", "Overwrite" };
        private static string[] _normalOptionGrid = { "Flat", "Raise", "Lower", "Background" };

        public abstract void drawBottom(TextureEditGUI gui);
        public abstract void drawRight(TextureEditGUI gui);
        public abstract string buttonText();
        public abstract void initialise(TextureEditGUI gui);

        public void positionSelector(TextureEditGUI gui, ref IntVector2 position)
        {
            bool repeatOK = false;
            bool buttonPressed = false;
            int button = 0;
            int delta = 1;

            if ((Time.time - Global.LastRepeat) > Global.AutoRepeatGap) repeatOK = true;
            if (gui.speedSelection < 0 || gui.speedSelection > 1) gui.speedSelection = 0;
            if (gui.speedSelection == 1)
            {
                button = 4;
                delta = 10;
            }

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(120));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Position");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton(_posButtons[button], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                Global.LastButtonPress = Time.time;
                if (repeatOK) position.y += delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton(_posButtons[button + 2], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                Global.LastButtonPress = Time.time;
                if (repeatOK) position.x -= delta;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("O", GUILayout.Width(25), GUILayout.Height(25)))
            {
                position = gui.centrePosition();
                gui.setRemakePreview();
            }

            GUILayout.Space(5);

            if (GUILayout.RepeatButton(_posButtons[button + 3], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                Global.LastButtonPress = Time.time;
                if (repeatOK) position.x += delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton(_posButtons[button + 1], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                Global.LastButtonPress = Time.time;
                if (repeatOK) position.y -= delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            gui.speedSelection = GUILayout.SelectionGrid(gui.speedSelection, _speedGrid, 2);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                Global.LastRepeat = Global.LastButtonPress;
                gui.setRemakePreview();

                Global.AutoRepeatGap = Global.AutoRepeatGap * 0.8f;
                if (Global.AutoRepeatGap < 0.04f) Global.AutoRepeatGap = 0.04f;
            }
        }

        public void colorSelector(TextureEditGUI gui, ref ValueSelector<byte, ByteField> redSelector, ref ValueSelector<byte, ByteField> greenSelector,
                                  ref ValueSelector<byte, ByteField> blueSelector, ref ValueSelector<byte, ByteField> alphaSelector)
        {
            if (redSelector.draw()) gui.setRemakePreview();
            GUILayout.Space(10f);
            if (greenSelector.draw()) gui.setRemakePreview();
            GUILayout.Space(10f);
            if (blueSelector.draw()) gui.setRemakePreview();
            GUILayout.Space(10f);
            if (alphaSelector.draw()) gui.setRemakePreview();     
        }

        public void header(TextureEditGUI gui, string text)
        {
            GUILayout.BeginHorizontal(gui.largeHeader);
            GUILayout.Space(20);
            GUILayout.Label(text, gui.largeHeader, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        public void rotationSelector(TextureEditGUI gui, ref Rotation rotation, ref bool mirror)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("Direction");

            int selection = (int) rotation;
            int oldSelection = selection;
            selection = GUILayout.SelectionGrid(selection, _rotationGrid, 2);

            bool oldMirror = mirror;
            mirror = GUILayout.Toggle(mirror, "Mirror");
            if (oldMirror != mirror) gui.setRemakePreview();

            if (oldSelection != selection)
            {
                rotation = (Rotation)selection;
                gui.setRemakePreview();
            }

            GUILayout.EndVertical();
        }

        public void blendMethodSelector(TextureEditGUI gui, ref BlendMethod blendMethod)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("Blend Method");

            int selection = (int)blendMethod;
            int oldSelection = selection;
            selection = GUILayout.SelectionGrid(selection, _blendMethodGrid, 2);

            if (oldSelection != selection)
            {
                blendMethod = (BlendMethod)selection;
                gui.setRemakePreview();
            }

            GUILayout.EndVertical();
        }

        public void alphaOptionSelector(TextureEditGUI gui, ref AlphaOption alphaOption)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("Alpha Option");

            int selection = (int) alphaOption;
            int oldSelection = selection;
            selection = GUILayout.SelectionGrid(selection, _alphaOptionGrid, 1);

            if (oldSelection != selection)
            {
                alphaOption = (AlphaOption) selection;
                gui.setRemakePreview();
            }

            GUILayout.EndVertical();
        }

        public void normalMapOptionSelector(TextureEditGUI gui, ref NormalOption normalOption)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("Normal Option");

            int selection = (int)normalOption;
            int oldSelection = selection;
            selection = GUILayout.SelectionGrid(selection, _normalOptionGrid, 2);

            if (oldSelection != selection)
            {
                normalOption = (NormalOption)selection;
                gui.setRemakePreview();
            }

            GUILayout.EndVertical();
        }
    }
}
