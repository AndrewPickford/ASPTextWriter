using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class TextEntryGUI : MonoBehaviour
    {
        private ASPTextWriter textWriter;
        private Rect windowPosition;
        private Texture2D previewTexture;
        private string lockText = "TextEntryGUILock";
        private bool remakePreview;
        private Color32[] pixels;
        private Vector2 fontScrollPos;
        private int selectedFont = 0;
        private Color notSelectedColor;
        private Color selectedColor;
        private Color backgroundColor;
        private int offsetX;
        private int offsetY;
        private string text;
        private float lastButtonPress;
        private float lastRepeat;
        private float autoRepeatGap;
        private string red;
        private string green;
        private string blue;
        private string alpha;
        private bool locked;
        private string[] alphaSelectionGrid;
        private int alphaSelection;
        private string normalScale;
        private string[] normalSelectionGrid;
        private int normalSelection;

        public void initialise(ASPTextWriter tw)
        {
            textWriter = tw;
            previewTexture = new Texture2D(textWriter.width, textWriter.height, TextureFormat.ARGB32, true);
            pixels = previewTexture.GetPixels32();
            windowPosition = new Rect(700, 100, 400, 500);
            remakePreview = true;
            selectedFont = 0;
            notSelectedColor = new Color(0.7f, 0.7f, 0.7f);
            selectedColor = new Color(1.0f, 1.0f, 1.0f);
            backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            offsetX = 0;
            offsetY = 0;
            text = tw.text;
            lastButtonPress = 0;
            autoRepeatGap = 0.4f;
            red = textWriter.red.ToString();
            green = textWriter.green.ToString();
            blue = textWriter.blue.ToString();
            alpha = textWriter.alpha.ToString();
            normalScale = textWriter.normalScale.ToString();
            locked = false;

            alphaSelectionGrid = new string[3];
            alphaSelectionGrid[0] = "Use background";
            alphaSelectionGrid[1] = "Text Only";
            alphaSelectionGrid[2] = "Whole Texture";
            alphaSelection = (int) textWriter.alphaOption;

            normalSelectionGrid = new String[2];
            normalSelectionGrid[0] = "Raise Text";
            normalSelectionGrid[1] = "Use background";
            normalSelection = (int) textWriter.normalOption;

            string fontID = textWriter.fontName + "-" + textWriter.fontSize.ToString();
            selectedFont = ASPFontCache.Instance.getFontIndexByID(fontID);
            if (selectedFont < 0) selectedFont = 0;
        }

        public void Awake()
        {
            InputLockManager.RemoveControlLock(lockText);
            remakePreview = true;
        }

        public void OnDestroy()
        {
            EditorLogic.fetch.Unlock(lockText);
        }

        public void OnGUI()
        {
            GUI.backgroundColor = backgroundColor;

            checkGUILock();
            windowPosition = GUILayout.Window(0, windowPosition, drawWindow, "Text Editor");
        }

        // From Kerbal Engineer Redux and modified slightly
        private void checkGUILock()
        {
            Vector2 mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            if (windowPosition.Contains(mouse) && !locked)
            {
                locked = true;
                if (HighLogic.LoadedSceneIsEditor)
                {
                    EditorTooltip.Instance.HideToolTip();
                    EditorLogic.fetch.Lock(false, false, false, lockText);
                }
            }
            
            if (!windowPosition.Contains(mouse) && locked)
            {
                locked = false;
                if (HighLogic.LoadedSceneIsEditor)
                {
                    EditorLogic.fetch.Unlock(lockText);
                }
            }
        }

        private void drawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            drawTexture();
            drawButtons();
            GUILayout.EndVertical();

            GUILayout.Space(5);

            drawFontList();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawCloseButton();

            GUILayout.EndVertical();

            if ((Time.time - lastButtonPress) > 0.2f) autoRepeatGap = 0.4f;

            GUI.DragWindow();
        }

        private void drawTexture()
        {
            if (previewTexture != null)
            {
                if (remakePreview)
                {
                    MappedFont font = ASPFontCache.Instance.list[selectedFont];

                    if (font != null)
                    {
                        float r = (float)(int.Parse(red) / 255f);
                        float g = (float)(int.Parse(green) / 255f);
                        float b = (float)(int.Parse(blue) / 255f);

                        Color color = new Color(r, g, b);

                        previewTexture.SetPixels32(pixels);
                        previewTexture.DrawText(text, font, color, offsetX, offsetY);
                        previewTexture.Apply();
                    }

                    remakePreview = false;
                }

                GUILayout.Box(previewTexture, GUI.skin.box, GUILayout.Width(previewTexture.width), GUILayout.Height(previewTexture.height));
            }
        }

        private void drawCloseButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                GameObject.Destroy(this);
            }
            GUILayout.EndHorizontal();
        }

        private void drawFontList()
        {
            Color contentColor = GUI.contentColor;

            fontScrollPos = GUILayout.BeginScrollView(fontScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(500));

            for (int i = 0; i < ASPFontCache.Instance.list.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                if (i == selectedFont) GUI.contentColor = selectedColor;
                else GUI.contentColor = notSelectedColor;

                if (GUILayout.Button(ASPFontCache.Instance.list[i].displayName + "-" + ASPFontCache.Instance.list[i].size.ToString(), GUILayout.ExpandWidth(true)))
                {
                    selectedFont = i;
                    remakePreview = true;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUI.contentColor = contentColor;
        }

        private void drawButtons()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            drawOffsetButtons();

            GUILayout.Space(50);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Text", GUILayout.ExpandWidth(false));
            GUILayout.Space(5);
            string oldText = text;
            text = GUILayout.TextField(text, GUILayout.ExpandWidth(true));
            if (oldText != text)
            {
                remakePreview = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawColorSelector();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            drawAlphaSelector();
            GUILayout.Space(5);
            if (textWriter.hasNormalMap) drawNormalSelector();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("  Apply  ", GUILayout.Height(20)))
            {
                int r = Utils.ParseIntWithLimits(red, 0, 255);
                int g = Utils.ParseIntWithLimits(green, 0, 255);
                int b = Utils.ParseIntWithLimits(blue, 0, 255);
                int a = Utils.ParseIntWithLimits(alpha, 0, 255);
                float ns = Utils.ParseFloatWithLimits(normalScale, 0f, 10f);

                textWriter.offsetX = offsetX;
                textWriter.offsetY = offsetY;
                textWriter.fontName = ASPFontCache.Instance.list[selectedFont].name;
                textWriter.fontSize = ASPFontCache.Instance.list[selectedFont].size;
                textWriter.text = text;
                textWriter.red = r;
                textWriter.green = g;
                textWriter.blue = b;
                textWriter.alpha = a;
                textWriter.alphaOption = (AlphaOption) alphaSelection;
                textWriter.normalScale = ns;
                textWriter.normalOption = (NormalOption) normalSelection;
                textWriter.writeText();
            }

            GUILayout.EndVertical();
        }

        private void drawOffsetButtons()
        {
            bool repeatOK = false;
            if ((Time.time - lastRepeat) > autoRepeatGap) repeatOK = true;
            bool buttonPressed = false;

            GUILayout.BeginVertical(GUI.skin.box,GUILayout.Width(120), GUILayout.Height(120));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Position");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton("+", GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) offsetY--;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton("<", GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) offsetX--;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
            {
                offsetX = 0;
                offsetY = 0;
                remakePreview = true;
            }

            GUILayout.Space(5);

            if (GUILayout.RepeatButton(">", GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) offsetX++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton("-", GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) offsetY++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                lastRepeat = lastButtonPress;
                remakePreview = true;

                autoRepeatGap = autoRepeatGap * 0.8f;
                if (autoRepeatGap < 0.04f) autoRepeatGap = 0.04f;
            }
        }

        private void drawColorSelector()
        {
            GUILayout.BeginHorizontal();

            drawIntField(ref red, 0, 255, "Red", Color.red);
            GUILayout.Space(10f);
            drawIntField(ref green, 0, 255, "Green", Color.green);
            GUILayout.Space(10f);
            drawIntField(ref blue, 0, 255, "Blue", Color.blue);
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void drawAlphaSelector()
        {
            GUILayout.BeginHorizontal();

            drawIntField(ref alpha, 0, 255, "Alpha", Color.white);

            alphaSelection = GUILayout.SelectionGrid(alphaSelection, alphaSelectionGrid, 1);

            GUILayout.EndHorizontal();
        }

        private void drawNormalSelector()
        {
            GUILayout.BeginHorizontal();

            drawFloatField(ref normalScale, 0f, 20f, 0.1f, "Normal Scale", Color.white);

            normalSelection = GUILayout.SelectionGrid(normalSelection, normalSelectionGrid, 1); 

            GUILayout.EndHorizontal();
        }

        private void drawIntField(ref string value, int min, int max, string label, Color color)
        {
            Color contentColor = GUI.contentColor;

            bool repeatOK = false;
            if ((Time.time - lastRepeat) > autoRepeatGap) repeatOK = true;
            bool buttonPressed = false;

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUI.contentColor = color;
            GUILayout.Label(label);
            GUI.contentColor = contentColor;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            value = GUILayout.TextField(value, GUILayout.Height(50), GUILayout.Width(50));
            value = System.Text.RegularExpressions.Regex.Replace(value, @"[^0-9]", "");
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Return)
            {
                remakePreview = true;

                int v = 0;
                int.TryParse(value, out v);
                value = v.ToString();
            }

            GUILayout.BeginVertical();
            if (GUILayout.RepeatButton("+", GUILayout.Height(25), GUILayout.Width(20)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) value = Utils.StringAdd(value, 1);
            }

            if (GUILayout.RepeatButton("-", GUILayout.Height(25), GUILayout.Width(20)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) value = Utils.StringAdd(value, -1);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                lastRepeat = lastButtonPress;
                remakePreview = true;

                autoRepeatGap = autoRepeatGap * 0.8f;
                if (autoRepeatGap < 0.04f) autoRepeatGap = 0.04f;

                value = Utils.LimitIntString(value, min, max);
            }
        }

        private void drawFloatField(ref string value, float min, float max, float step, string label, Color color)
        {
            Color contentColor = GUI.contentColor;

            bool repeatOK = false;
            if ((Time.time - lastRepeat) > autoRepeatGap) repeatOK = true;
            bool buttonPressed = false;

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUI.contentColor = color;
            GUILayout.Label(label);
            GUI.contentColor = contentColor;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            value = GUILayout.TextField(value, GUILayout.Height(50), GUILayout.Width(50));
            value = System.Text.RegularExpressions.Regex.Replace(value, @"[^0-9.]", "");
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.Return)
            {
                remakePreview = true;

                float v = 0f;
                float.TryParse(value, out v);

                value = v.ToString();
            }

            GUILayout.BeginVertical();
            if (GUILayout.RepeatButton("+", GUILayout.Height(25), GUILayout.Width(20)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) value = Utils.StringAdd(value, step);
            }

            if (GUILayout.RepeatButton("-", GUILayout.Height(25), GUILayout.Width(20)))
            {
                buttonPressed = true;
                lastButtonPress = Time.time;
                if (repeatOK) value = Utils.StringAdd(value, -step);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                lastRepeat = lastButtonPress;
                remakePreview = true;

                autoRepeatGap = autoRepeatGap * 0.8f;
                if (autoRepeatGap < 0.04f) autoRepeatGap = 0.04f;

                value = Utils.LimitFloatString(value, min, max);
            }
        }
    }
}
