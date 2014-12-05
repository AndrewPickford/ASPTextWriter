using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public abstract class ImageModifier
    {
        public static ImageModifier CreateFromConfig(ConfigNode node)
        {
            ImageModifier imageModifier = null;
            string type = node.GetValue("type");


            if (type == "text") imageModifier = new IM.Text();
            if (type == "base_texture") imageModifier = new IM.BaseTexture();

            if (imageModifier == null) throw new ArgumentException("unknown image modifier");
            else
            {
                imageModifier.load(node);
                return imageModifier;
            }
        }

        public abstract void save(ConfigNode node);
        public abstract void load(ConfigNode node);
        public abstract void drawOnImage(ref Image image);
        public abstract ImageModifier clone();
        public abstract void cleanUp();
        public abstract string displayName();
        public abstract bool locked();
        public abstract ImageModifierGui gui();
    }

    public abstract class ImageModifierGui
    {
        static private string[] _posButtons = { "+", "-", "<", ">", "++", "--", "<<", ">>" };
        static private string[] _speedGrid = { "x 1", "x 10" };

        public static void Position(TextureEditGUI gui, ref IntVector2 position)
        {
            bool repeatOK = false;
            bool buttonPressed = false;
            int button = 0;
            int delta = 1;

            if ((Time.time - Global.LastRepeat) > Global.AutoRepeatGap) repeatOK = true;
            if (gui._speedSelection < 0 || gui._speedSelection > 1) gui._speedSelection = 0;
            if (gui._speedSelection == 1)
            {
                button = 4;
                delta = 10;
            }

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(120), GUILayout.Height(120));

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
                gui.centrePosition(ref position);
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

            gui._speedSelection = GUILayout.SelectionGrid(gui._speedSelection, _speedGrid, 2);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                Global.LastRepeat = Global.LastButtonPress;
                gui.setRemakePreview();

                Global.AutoRepeatGap = Global.AutoRepeatGap * 0.8f;
                if (Global.AutoRepeatGap < 0.04f) Global.AutoRepeatGap = 0.04f;
            }
        }

        public abstract void draw(TextureEditGUI gui);
        public abstract void initialise();
    }
}
