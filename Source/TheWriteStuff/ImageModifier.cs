using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    //public enum Rotation { R0, R90, R180, R270 };
    public enum BlendMethod { PIXEL, RGB, HSV, SSR };
    public enum TransformOption { USE_FIRST, USE_ALL };
    public enum NormalOption { FLAT, RAISE, LOWER, USE_BACKGROUND };
    public enum AlphaOption { USE_TEXTURE, OVERWRITE };

    public abstract class ImageModifier
    {
        public enum Type { INVALID, BASE_TEXTURE, BITMAP_TEXT, BITMAP_MONO_DECAL, BITMAP_COLOR_DECAL, RECTANGLE, CIRCLE, QUADRILATERAL, TRIANGLE };

        public abstract void load(ConfigNode node);
        public abstract void drawOnImage(ref Image image, BoundingBox boundingBox);
        public abstract void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox);
        public abstract void cleanUp();
        public abstract ImageModifier clone();
        public abstract string displayName();
        public abstract string headerName();
        public abstract ImageModifierGui gui();

        public bool longRight { get; protected set; }
        protected Type _type = Type.INVALID;

        protected ImageModifier()
        {
            longRight = true;
            _type = Type.INVALID;
        }

        public Type type()
        {
            return _type;
        }

        public static ImageModifier CreateFromConfig(ConfigNode node)
        {
            ImageModifier imageModifier = null;

            Type type = Type.INVALID;
            if (node.HasValue("type")) type = (Type)ConfigNode.ParseEnum(typeof(Type), node.GetValue("type"));

            switch (type)
            {
                case Type.BASE_TEXTURE:
                    imageModifier = IM.BaseTexture.CreateBaseTexture(node);
                    break;

                case Type.BITMAP_TEXT:
                    imageModifier = new IM.BitmapText();
                    break;

                case Type.BITMAP_MONO_DECAL:
                    imageModifier = new IM.BitmapMonoDecal();
                    break;

                case Type.BITMAP_COLOR_DECAL:
                    imageModifier = new IM.BitmapColorDecal();
                    break;

                case Type.RECTANGLE:
                    imageModifier = new IM.Rectangle();
                    break;

                case Type.CIRCLE:
                    imageModifier = new IM.Circle();
                    break;

                case Type.QUADRILATERAL:
                    imageModifier = new IM.Quadrilateral();
                    break;

                case Type.TRIANGLE:
                    imageModifier = new IM.Triangle();
                    break;

                default:
                case Type.INVALID:
                    break;
            }

            if (imageModifier == null)  Utils.LogError("unknown image modifier");
            else imageModifier.load(node);

            return imageModifier;
        }

        public virtual void save(ConfigNode node)
        {
            node.AddValue("type", ConfigNode.WriteEnum(_type));
        }

        protected void copyFrom(ImageModifier imageModifier)
        {
        }
    }

    public abstract class ImageModifierGui
    {
        private static string[] _posButtons = { "+", "-", "<", ">", "++", "--", "<<", ">>" };
        private static string[] _speedGrid = { "x 1", "x 10" };
        private static string[] _rotationGrid = { "0", "90", "180", "270" };
        private static string[] _blendMethodGrid = { "Pixel", "RGB", "HSV", "SSR" };
        private static string[] _alphaOptionGrid = { "Use Texture", "Overwrite" };
        private static string[] _normalOptionGrid = { "Flat", "Raise", "Lower", "Background" };

        public abstract void drawBottom(TextureEditGUI gui);
        public abstract void drawRight(TextureEditGUI gui);
        public abstract string buttonText();
        public abstract void initialise(TextureEditGUI gui);

        public void checkChanged(ref byte old, byte value, TextureEditGUI gui)
        {
            if (old != value)
            {
                old = value;
                gui.setRemakePreview();
            }
        }

        public void checkChanged(ref int old, int value, TextureEditGUI gui)
        {
            if (old != value)
            {
                old = value;
                gui.setRemakePreview();
            }
        }

        public void checkChanged(ref double old, double value, TextureEditGUI gui)
        {
            if (old != value)
            {
                old = value;
                gui.setRemakePreview();
            }
        }

        public void positionSelector(TextureEditGUI gui, ref ValueSelector<int, IntField> xSelector, ref ValueSelector<int, IntField> ySelector)
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
                if (repeatOK) ySelector.add(delta);
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
                if (repeatOK) xSelector.add(-delta);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("O", GUILayout.Width(25), GUILayout.Height(25)))
            {
                xSelector.set(gui.centrePosition().x);
                ySelector.set(gui.centrePosition().y);
                gui.setRemakePreview();
            }

            GUILayout.Space(5);

            if (GUILayout.RepeatButton(_posButtons[button + 3], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                Global.LastButtonPress = Time.time;
                if (repeatOK) xSelector.add(delta);
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
                if (repeatOK) ySelector.add(-delta);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            gui.speedSelection = GUILayout.SelectionGrid(gui.speedSelection, _speedGrid, 2);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            xSelector.draw();
            GUILayout.FlexibleSpace();
            ySelector.draw();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                Global.LastRepeat = Global.LastButtonPress;

                Global.AutoRepeatGap = Global.AutoRepeatGap * 0.8f;
                if (Global.AutoRepeatGap < 0.04f) Global.AutoRepeatGap = 0.04f;
            }
        }

        public void colorSelector(TextureEditGUI gui, ref ValueSelector<byte, ByteField> redSelector, ref ValueSelector<byte, ByteField> greenSelector,
                                  ref ValueSelector<byte, ByteField> blueSelector, ref ValueSelector<byte, ByteField> alphaSelector)
        {
            redSelector.draw();
            GUILayout.Space(10);
            greenSelector.draw();
            GUILayout.Space(10);
            blueSelector.draw();
            GUILayout.Space(10);
            alphaSelector.draw();
            GUILayout.Space(10);
        }

        public void header(TextureEditGUI gui, string text, float endSpace = 0f)
        {
            if (endSpace > 0f) GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal(gui.largeHeader);
            GUILayout.Space(20f);
            GUILayout.Label(text, gui.largeHeader, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            if (endSpace > 0f)
            {
                GUILayout.Space(endSpace + 10f);
                GUILayout.EndHorizontal();
            }
        }

        public void rotationSelector(TextureEditGUI gui, ref ValueSelector<int, IntField> rotation, ref bool mirrorX, ref bool mirrorY)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.BeginVertical();
            GUILayout.Label("              Rotation");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0", GUILayout.Width(35))) rotation.set(0);
            if (GUILayout.Button("90", GUILayout.Width(35))) rotation.set(90);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("180", GUILayout.Width(35))) rotation.set(180);
            if (GUILayout.Button("270", GUILayout.Width(35))) rotation.set(270);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(3);
            rotation.draw();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical();
            GUILayout.Label("Mirror", GUILayout.MinWidth(35));
            bool oldMirror = mirrorX;
            mirrorX = GUILayout.Toggle(mirrorX, "X");
            if (oldMirror != mirrorX) gui.setRemakePreview();
            oldMirror = mirrorY;
            mirrorY = GUILayout.Toggle(mirrorY, "Y");
            if (oldMirror != mirrorY) gui.setRemakePreview();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
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

        public void normalMapOptionSelector(TextureEditGUI gui, ref NormalOption normalOption, ref bool scaleNormalsByAlpha, ref bool normalsFromDerivatives)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("Normal Option");

            normalOption = (NormalOption)GUILayout.SelectionGrid((int)normalOption, _normalOptionGrid, 2);
            scaleNormalsByAlpha = GUILayout.Toggle(scaleNormalsByAlpha, "Alpha Scaling");
            normalsFromDerivatives = GUILayout.Toggle(normalsFromDerivatives, "Edges");

            GUILayout.EndVertical();
        }
    }
}
