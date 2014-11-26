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
        static private int _nextID = 63851;

        private int _windowID = 0;
        private ASPTextWriter _textWriter;
        private Rect _windowPosition;
        private Texture2D _previewTexture = null;
        private string _lockText = "TextEntryGUILock";
        private bool _remakePreview;
        private Vector2 _fontScrollPos;
        private int _selectedFont = 0;
        private string[] _fontSizeGrid = null;
        private int _fontSizeSelection = 0;
        private int _lastFontSize = 0;
        private Color _notSelectedColor;
        private Color _selectedColor;
        private Color _backgroundColor;
        private int _offsetX;
        private int _offsetY;
        private bool _useBoundingBox;
        private bool _mirrorText;
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
            _windowID = _nextID;
            ++_nextID;

            _textWriter = tw;
            _windowPosition = new Rect(700, 100, 400, 500);
            _remakePreview = true;
            _notSelectedColor = new Color(0.7f, 0.7f, 0.7f);
            _selectedColor = new Color(1.0f, 1.0f, 1.0f);
            _backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            _text = _textWriter.text;
            _lastButtonPress = 0;
            _autoRepeatGap = 0.4f;
            _redSelector = new ValueSelector<int, IntField>(_textWriter.red, 0, 255, 1, "Red", Color.red);
            _greenSelector = new ValueSelector<int, IntField>(_textWriter.green, 0, 255, 1, "Green", Color.green);
            _blueSelector = new ValueSelector<int, IntField>(_textWriter.blue, 0, 255, 1, "Blue", Color.blue);
            _alphaSelector = new ValueSelector<int, IntField>(_textWriter.alpha, 0, 255, 1, "Alpha", Color.white);
            _scaleSelector = new ValueSelector<float, FloatField>(_textWriter.normalScale, 0.01f, 4f, 0.1f, "Scale", Color.white);
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

            _selectedFont = FontCache.Instance.getFontIndexByName(_textWriter.fontName);
            if (_selectedFont < 0) _selectedFont = 0;
            _fontSizeSelection = FontCache.Instance.getFontSizeIndex(_textWriter.fontName, _textWriter.fontSize);
            if (_fontSizeSelection < 0) _fontSizeSelection = 0;
            _lastFontSize = FontCache.Instance.fontInfoArray[_selectedFont].sizes[_fontSizeSelection];

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

            _offsetX = _textWriter.offsetX;
            _offsetY = _textWriter.offsetY;
            _useBoundingBox = _textWriter.useBoundingBox;
            _mirrorText = _textWriter.mirrorText;

            getPreviewTexture();

            if (_text == string.Empty) centreOffset();
        }

        private void centreOffset()
        {
            _offsetX = _previewTexture.width / 2;
            _offsetY = _previewTexture.height / 2;

            if (_textWriter.width > 0 && _useBoundingBox == true)
            {
                _offsetX += _textWriter.bottomLeftX;
                _offsetY += _textWriter.bottomLeftY;
            }
        }

        private void OnVesselChange(Vessel vesselChange)
        {
            GameObject.Destroy(this);
        }

        public void Awake()
        {
            InputLockManager.RemoveControlLock(_lockText);
            _remakePreview = true;
            GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        public void OnDestroy()
        {
            EditorLogic.fetch.Unlock(_lockText);

            GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(this.OnVesselChange));

            if (_cachedBackground != null) GameObject.Destroy(_cachedBackground);
            if (_previewTexture != null) GameObject.Destroy(_previewTexture);
        }

        public void OnGUI()
        {
            GUI.backgroundColor = _backgroundColor;

            _windowPosition = GUILayout.Window(_windowID, _windowPosition, drawWindow, "Text Editor");
            checkGUILock();
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

                GUI.FocusWindow(_windowID);
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

            GUILayout.BeginVertical();
            drawBackgroundList();
            GUILayout.Space(3);
            drawFontList();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawCloseButton();

            GUILayout.EndVertical();

            if ((Time.time - _lastButtonPress) > 0.2f) _autoRepeatGap = 0.4f;

            GUI.DragWindow();
        }

        private void getPreviewTexture()
        {
            string textureUrl = _textWriter.url + "/" + _textWriter.textureArray[_selectedBackground];

            if (_cachedBackground != null) GameObject.Destroy(_cachedBackground);
            _cachedBackground = Utils.GetReadableTexture(GameDatabase.Instance.GetTexture(textureUrl, false), false);

            if (_cachedBackground == null)
            {
                Debug.LogError(String.Format("TWS: No such texture: {0}", _cachedBackground));
                _remakePreview = false;
            }

            if (_textWriter.width > 0 && _useBoundingBox) _cachedPixels = _cachedBackground.GetPixels(_textWriter.boundingBox);
            else _cachedPixels = _cachedBackground.GetPixels();
            _cachedBackgroundUrl = textureUrl;

            for (int i = 0; i < _cachedPixels.Length; ++i)
            {
                _cachedPixels[i].a = 1.0f;
            }

            if (_previewTexture == null)
            {
                if (_textWriter.width > 0 && _useBoundingBox) _previewTexture = new Texture2D(_textWriter.width, _textWriter.height, TextureFormat.ARGB32, true);
                else _previewTexture = new Texture2D(_cachedBackground.width, _cachedBackground.height, TextureFormat.ARGB32, true);
            }

            if (System.Object.ReferenceEquals(GameDatabase.Instance.GetTexture(textureUrl, false), _cachedBackground)) _cachedBackground = null;
        }

        private void drawTexture()
        {
            if (_remakePreview)
            {
                string textureUrl = _textWriter.url + "/" + _textWriter.textureArray[_selectedBackground];
                if (_cachedBackgroundUrl != textureUrl) getPreviewTexture();

                MappedFont font = FontCache.Instance.getFontByNameSize(FontCache.Instance.fontInfoArray[_selectedFont].name,
                                                                       FontCache.Instance.fontInfoArray[_selectedFont].sizes[_fontSizeSelection]);

                if (font == null) font = FontCache.Instance.mappedList.First();

                if (font != null)
                {
                    float r = (float)(_redSelector.value() / 255f);
                    float g = (float)(_greenSelector.value() / 255f);
                    float b = (float)(_blueSelector.value() / 255f);

                    Color color = new Color(r, g, b);

                    int offX = _offsetX;
                    int offY = _offsetY;
                    if (_textWriter.width > 0 && _useBoundingBox)
                    {
                        offX -= _textWriter.bottomLeftX;
                        offY -= _textWriter.bottomLeftY;
                    }

                    _previewTexture.SetPixels(_cachedPixels);
                    _previewTexture.DrawText(_text, font, color, offX, offY, _mirrorText, (TextDirection) _directionSelection);
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

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Size");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_fontSizeGrid == null)
            {
                _fontSizeGrid = new string[FontCache.Instance.fontInfoArray[_selectedFont].sizes.Length];
                for (int i = 0; i < _fontSizeGrid.Length; ++i)
                {
                    _fontSizeGrid[i] = FontCache.Instance.fontInfoArray[_selectedFont].sizes[i].ToString();
                }
            }

            int oldFontSizeSelection = _fontSizeSelection;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _fontSizeSelection = GUILayout.SelectionGrid(_fontSizeSelection, _fontSizeGrid, 6);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (oldFontSizeSelection != _fontSizeSelection) _remakePreview = true;

            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Font");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _fontScrollPos = GUILayout.BeginScrollView(_fontScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(500));

            int oldSelectedFont = _selectedFont;
            for (int i = 0; i < FontCache.Instance.fontInfoArray.Length; ++i)
            {
                GUILayout.BeginHorizontal();

                if (i == _selectedFont) GUI.contentColor = _selectedColor;
                else GUI.contentColor = _notSelectedColor;

                if (GUILayout.Button(FontCache.Instance.fontInfoArray[i].displayName, GUILayout.ExpandWidth(true)))
                {
                    _selectedFont = i;
                    _fontSizeGrid = null;
                    _lastFontSize = FontCache.Instance.fontInfoArray[oldSelectedFont].sizes[_fontSizeSelection];
                    _fontSizeSelection = FontCache.Instance.getFontSizeIndex(FontCache.Instance.fontInfoArray[_selectedFont].name, _lastFontSize);
                    if (_fontSizeSelection < 0) _fontSizeSelection = 0;
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

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Background Texture");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _backgroundScrollPos = GUILayout.BeginScrollView(_backgroundScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(100));

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
            _text = System.Text.RegularExpressions.Regex.Replace(_text, @"[\r\n]", "");

            if (oldText != _text) _remakePreview = true;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            drawColorSelector();

            GUILayout.Space(5);

            if (_textWriter.width > 0)
            {
                bool oldValue = _useBoundingBox;
                _useBoundingBox = !GUILayout.Toggle(!_useBoundingBox, "Use Full Texture");

                if (oldValue != _useBoundingBox)
                {
                    _cachedBackgroundUrl = string.Empty;
                    _remakePreview = true;
                    _windowPosition = new Rect(_windowPosition.x, _windowPosition.y, 400, 500);
                    if (_previewTexture != null) GameObject.Destroy(_previewTexture);
                }
            }

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
                _textWriter.fontName = FontCache.Instance.fontInfoArray[_selectedFont].name;
                _textWriter.fontSize = FontCache.Instance.fontInfoArray[_selectedFont].sizes[_fontSizeSelection];
                _textWriter.text = _text;
                _textWriter.red = _redSelector.value();
                _textWriter.green = _greenSelector.value();
                _textWriter.blue = _blueSelector.value();
                _textWriter.alpha = _alphaSelector.value();
                _textWriter.offsetX = _offsetX;
                _textWriter.offsetY = _offsetY;
                _textWriter.useBoundingBox = _useBoundingBox;
                _textWriter.mirrorText = _mirrorText;
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

            if (GUILayout.Button("O", GUILayout.Width(25), GUILayout.Height(25)))
            {
                centreOffset();
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

            GUILayout.BeginHorizontal();

            GUILayout.Label("Text Direction");
            GUILayout.FlexibleSpace();

            bool oldValue = _mirrorText;
            _mirrorText = GUILayout.Toggle(_mirrorText, "Mirror Text");
            if (oldValue != _mirrorText) _remakePreview = true;

            GUILayout.EndHorizontal();

            int oldDirection = _directionSelection;
            _directionSelection = GUILayout.SelectionGrid(_directionSelection, _directionGrid, 4);
            if (oldDirection != _directionSelection) _remakePreview = true;

            GUILayout.EndVertical();
        }
    }
}
