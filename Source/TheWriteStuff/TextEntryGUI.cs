using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class TextEntryGUI : MonoBehaviour
    {
        static private string[] _posButtons = { "+", "-", "<", ">", "++", "--", "<<", ">>" };

        private ASPTextWriter _textWriter;
        private Rect _windowPosition;
        private Texture2D _previewTexture = null;
        private string _lockText = "TextEntryGUILock";
        private bool _remakePreview;
        private Vector2 _fontScrollPos;
        private int _selectedFont = 0;
        private Color _notSelectedColor;
        private Color _selectedColor;
        private Color _backgroundColor;
        private int _offsetX;
        private int _offsetY;
        private string _text;
        private float _lastButtonPress;
        private float _lastRepeat;
        private float _autoRepeatGap;
        private ValueSelector<int, IntField> _redSelector;
        private ValueSelector<int, IntField> _greenSelector;
        private ValueSelector<int, IntField> _blueSelector;
        private ValueSelector<int, IntField> _alphaSelector;
        private bool _locked;
        private string[] _alphaSelectionGrid;
        private int _alphaSelection;
        private ValueSelector<float, FloatField> _scaleSelector;
        private string[] _normalSelectionGrid;
        private int _normalSelection;
        private Vector2 _backgroundScrollPos;
        private int _selectedBackground = 0;
        private Texture2D _cachedBackground = null;
        private Color[] _cachedPixels;
        private string _cachedBackgroundUrl = string.Empty;
        private Vector2 _previewScrollPos;
        private string[] _speedGrid;
        private int _speedSelection;
        private string[] _blendMethodGrid;
        private int _blendMethodSelection;
        private string[] _directionGrid;
        private int _directionSelection;

        public void initialise(ASPTextWriter tw)
        {
            _textWriter = tw;
            _windowPosition = new Rect(700, 100, 400, 500);
            _remakePreview = true;
            _selectedFont = 0;
            _notSelectedColor = new Color(0.7f, 0.7f, 0.7f);
            _selectedColor = new Color(1.0f, 1.0f, 1.0f);
            _backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            _offsetX = _textWriter.offsetX;
            _offsetY = _textWriter.offsetY;
            _text = _textWriter.text;
            _lastButtonPress = 0;
            _autoRepeatGap = 0.4f;
            _redSelector = new ValueSelector<int, IntField>(_textWriter.red, 0, 255, 1, "Red", Color.red);
            _greenSelector = new ValueSelector<int, IntField>(_textWriter.green, 0, 255, 1, "Green", Color.green);
            _blueSelector = new ValueSelector<int, IntField>(_textWriter.blue, 0, 255, 1, "Blue", Color.blue);
            _alphaSelector = new ValueSelector<int, IntField>(_textWriter.alpha, 0, 255, 1, "Alpha", Color.white);
            _scaleSelector = new ValueSelector<float, FloatField>(_textWriter.normalScale, 0f, 4f, 0.1f, "Scale", Color.white);
            _locked = false;

            _alphaSelectionGrid = new string[3];
            _alphaSelectionGrid[0] = "Use background";
            _alphaSelectionGrid[1] = "Text Only";
            _alphaSelectionGrid[2] = "Whole Texture";
            _alphaSelection = (int) _textWriter.alphaOption;

            _normalSelectionGrid = new string[4];
            _normalSelectionGrid[0] = "Flat";
            _normalSelectionGrid[1] = "Raise Text";
            _normalSelectionGrid[2] = "Lower Text";
            _normalSelectionGrid[3] = "Use background";
            _normalSelection = (int) _textWriter.normalOption;

            string fontID = _textWriter.fontName + "-" + _textWriter.fontSize.ToString();
            _selectedFont = ASPFontCache.Instance.getFontIndexByID(fontID);
            if (_selectedFont < 0) _selectedFont = 0;

            _selectedBackground = _textWriter.selectedTexture;

            _speedGrid = new string[2];
            _speedGrid[0] = "x 1";
            _speedGrid[1] = "x 10";
            _speedSelection = 0;

            _blendMethodGrid = new string[3];
            _blendMethodGrid[0] = "Pixel";
            _blendMethodGrid[1] = "RGB";
            _blendMethodGrid[2] = "HSV";
            _blendMethodSelection = (int) _textWriter.blendMethod;

            _directionGrid = new string[4];
            _directionGrid[0] = "Left to Right";
            _directionGrid[1] = "Right to Left";
            _directionGrid[2] = "Up to Down";
            _directionGrid[3] = "Down to Up";
            _directionSelection = (int) _textWriter.textDirection;
        }

        public void Awake()
        {
            InputLockManager.RemoveControlLock(_lockText);
            _remakePreview = true;
        }

        public void OnDestroy()
        {
            EditorLogic.fetch.Unlock(_lockText);

            if (_cachedBackground != null) Destroy(_cachedBackground);
            if (_previewTexture != null) Destroy(_previewTexture);
        }

        public void OnGUI()
        {
            GUI.backgroundColor = _backgroundColor;

            checkGUILock();
            _windowPosition = GUILayout.Window(0, _windowPosition, drawWindow, "Text Editor");
        }

        // fix for editor click through: http://forum.kerbalspaceprogram.com/threads/83660-Fixed-Stopping-click-through-a-GUI-window-%28Part-menu-in-flight-and-Editor%29
        // with mouse pos inversion fix from Kerbal Engineer Redux
        private void checkGUILock()
        {
            Vector2 mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            if (_windowPosition.Contains(mouse) && !_locked)
            {
                _locked = true;
                if (HighLogic.LoadedSceneIsEditor)
                {
                    EditorTooltip.Instance.HideToolTip();
                    EditorLogic.fetch.Lock(false, false, false, _lockText);
                }
            }
            
            if (!_windowPosition.Contains(mouse) && _locked)
            {
                _locked = false;
                if (HighLogic.LoadedSceneIsEditor)
                {
                    EditorLogic.fetch.Unlock(_lockText);
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

            drawBackgroundList();

            GUILayout.Space(5);

            drawFontList();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawCloseButton();

            GUILayout.EndVertical();

            if ((Time.time - _lastButtonPress) > 0.2f) _autoRepeatGap = 0.4f;

            GUI.DragWindow();
        }

        private void drawTexture()
        {
            if (_remakePreview)
            {
                string textureUrl = _textWriter.url + "/" + _textWriter.textureArray[_selectedBackground];

                if (_cachedBackgroundUrl != textureUrl)
                {
                    if (_cachedBackground != null) Destroy(_cachedBackground);
                    _cachedBackground = Utils.GetReadableTexture(GameDatabase.Instance.GetTexture(textureUrl, false), false);

                    if (_cachedBackground == null)
                    {
                        Debug.LogError(String.Format("No such texture: {0}", _cachedBackground));
                        _remakePreview = false;
                    }

                    if (_textWriter.width > 0) _cachedPixels = _cachedBackground.GetPixels(_textWriter.boundingBox);
                    else _cachedPixels = _cachedBackground.GetPixels();
                    _cachedBackgroundUrl = textureUrl;

                    for (int i = 0; i < _cachedPixels.Length; ++i)
                    {
                        _cachedPixels[i].a = 1.0f;
                    }

                    if (_previewTexture == null)
                    {
                        if (_textWriter.width > 0) _previewTexture = new Texture2D(_textWriter.width, _textWriter.height, TextureFormat.ARGB32, true);
                        else _previewTexture = new Texture2D(_cachedBackground.width, _cachedBackground.height, TextureFormat.ARGB32, true);
                    }

                    if (System.Object.ReferenceEquals(GameDatabase.Instance.GetTexture(textureUrl, false), _cachedBackground)) _cachedBackground = null;
                }

                MappedFont font = ASPFontCache.Instance.list[_selectedFont];

                if (font != null)
                {
                    float r = (float)(_redSelector.value() / 255f);
                    float g = (float)(_greenSelector.value() / 255f);
                    float b = (float)(_blueSelector.value() / 255f);

                    Color color = new Color(r, g, b);

                    _previewTexture.SetPixels(_cachedPixels);
                    _previewTexture.DrawText(_text, font, color, _offsetX, _offsetY, (TextDirection) _directionSelection);
                    _previewTexture.Apply();
                }

                _remakePreview = false;
            }

            if (_previewTexture.width > 500 || _previewTexture.height > 500) _previewScrollPos = GUILayout.BeginScrollView(_previewScrollPos, GUI.skin.box, GUILayout.MinWidth(500), GUILayout.MinHeight(500));
            GUILayout.Box(_previewTexture, GUI.skin.box, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
            if (_previewTexture.width > 500 || _previewTexture.height > 500) GUILayout.EndScrollView();
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

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Fonts");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _fontScrollPos = GUILayout.BeginScrollView(_fontScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(500));

            for (int i = 0; i < ASPFontCache.Instance.list.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                if (i == _selectedFont) GUI.contentColor = _selectedColor;
                else GUI.contentColor = _notSelectedColor;

                if (GUILayout.Button(ASPFontCache.Instance.list[i].displayName + "-" + ASPFontCache.Instance.list[i].size.ToString(), GUILayout.ExpandWidth(true)))
                {
                    _selectedFont = i;
                    _remakePreview = true;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.contentColor = contentColor;
        }

        private void drawBackgroundList()
        {
            Color contentColor = GUI.contentColor;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Background Texture");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _backgroundScrollPos = GUILayout.BeginScrollView(_backgroundScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(500));

            for (int i = 0; i < _textWriter.displayNameArray.Length; ++i)
            {
                GUILayout.BeginHorizontal();

                if (i == _selectedBackground) GUI.contentColor = _selectedColor;
                else GUI.contentColor = _notSelectedColor;

                if (GUILayout.Button(_textWriter.displayNameArray[i], GUILayout.ExpandWidth(true)))
                {
                    _selectedBackground = i;
                    _remakePreview = true;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

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
            string oldText = _text;

            GUI.SetNextControlName("TextField");
            _text = GUILayout.TextField(_text, GUILayout.ExpandWidth(true));

            if (oldText != _text) _remakePreview = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawColorSelector();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            drawAlphaSelector();
            GUILayout.Space(5);
            if (_textWriter.hasNormalMap) drawNormalSelector();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            drawBlendSelector();
            GUILayout.Space(3);
            drawDirectionSelector();

            GUILayout.Space(10);
          

            if (GUILayout.Button("  Apply  ", GUILayout.Height(20)))
            {
                _textWriter.offsetX = _offsetX;
                _textWriter.offsetY = _offsetY;
                _textWriter.fontName = ASPFontCache.Instance.list[_selectedFont].name;
                _textWriter.fontSize = ASPFontCache.Instance.list[_selectedFont].size;
                _textWriter.text = _text;
                _textWriter.red = _redSelector.value();
                _textWriter.green = _greenSelector.value();
                _textWriter.blue = _blueSelector.value();
                _textWriter.alpha = _alphaSelector.value();
                _textWriter.alphaOption = (AlphaOption) _alphaSelection;
                _textWriter.normalScale = _scaleSelector.value();
                _textWriter.normalOption = (NormalOption) _normalSelection;
                _textWriter.selectedTexture = _selectedBackground;
                _textWriter.blendMethod = (BlendMethod) _blendMethodSelection;
                _textWriter.textDirection = (TextDirection) _directionSelection;
                _textWriter.writeText();
            }

            GUILayout.EndVertical();
        }

        private void drawOffsetButtons()
        {
            bool repeatOK = false;
            bool buttonPressed = false;
            int button = 0;
            int delta = 1;

            if ((Time.time - _lastRepeat) > _autoRepeatGap) repeatOK = true;
            if (_speedSelection == 1)
            {
                button = 4;
                delta = 10;
            }

            GUILayout.BeginVertical(GUI.skin.box,GUILayout.Width(120), GUILayout.Height(120));

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
                _lastButtonPress = Time.time;
                if (repeatOK) _offsetY += delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton(_posButtons[button+2], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                _lastButtonPress = Time.time;
                if (repeatOK) _offsetX -= delta;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
            {
                _offsetX = 0;
                _offsetY = 0;
                _remakePreview = true;
            }

            GUILayout.Space(5);

            if (GUILayout.RepeatButton(_posButtons[button+3], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                _lastButtonPress = Time.time;
                if (repeatOK) _offsetX += delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.RepeatButton(_posButtons[button+1], GUILayout.Width(25), GUILayout.Height(25)))
            {
                buttonPressed = true;
                _lastButtonPress = Time.time;
                if (repeatOK) _offsetY -= delta;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _speedSelection = GUILayout.SelectionGrid(_speedSelection, _speedGrid, 2);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            if (buttonPressed && repeatOK)
            {
                _lastRepeat = _lastButtonPress;
                _remakePreview = true;

                _autoRepeatGap = _autoRepeatGap * 0.8f;
                if (_autoRepeatGap < 0.04f) _autoRepeatGap = 0.04f;
            }
        }

        private void drawColorSelector()
        {
            GUILayout.BeginHorizontal();

            if (_redSelector.draw()) _remakePreview = true;
            GUILayout.Space(10f);
            if (_greenSelector.draw()) _remakePreview = true;
            GUILayout.Space(10f);
            if (_blueSelector.draw()) _remakePreview = true;
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void drawAlphaSelector()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            _alphaSelector.draw();
            _alphaSelection = GUILayout.SelectionGrid(_alphaSelection, _alphaSelectionGrid, 1);

            GUILayout.EndHorizontal();
        }

        private void drawNormalSelector()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            _scaleSelector.draw();
            _normalSelection = GUILayout.SelectionGrid(_normalSelection, _normalSelectionGrid, 1); 

            GUILayout.EndHorizontal();
        }

        private void drawBlendSelector()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Text Blending Method");
            _blendMethodSelection = GUILayout.SelectionGrid(_blendMethodSelection, _blendMethodGrid, 3);

            GUILayout.EndVertical();
        }

        private void drawDirectionSelector()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Text Direction");

            int oldDirection = _directionSelection;
            _directionSelection = GUILayout.SelectionGrid(_directionSelection, _directionGrid, 4);
            if (oldDirection != _directionSelection) _remakePreview = true;

            GUILayout.EndVertical();
        }
    }
}
