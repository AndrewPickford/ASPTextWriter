using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class TextureEditGUI : MonoBehaviour
    {
        static private int _nextID = 23851;

        private int _windowID = 0;
        private bool _first = true;

        private Rect _windowPosition;
        private string _lockText = "TextureEditGUILock";
        private bool _locked;
        private ASPTextureEdit _textureEdit = null;
        private bool _remakePreview = true;
        private Texture2D _previewTexture = null;
        private Image _previewImage = null;
        private Vector2 _previewScrollPos;
        private ImageModifiers _imageModifiers = null;
        private Vector2 _modifiersScrollPos;
        private int _selectedModifier;
        private Vector2 _availableModifiersScrollPos;

        internal Color _selectedColor;
        internal Color _notSelectedColor;
        internal Color _backgroundColor;
        internal int speedSelection;
        internal GUIStyle largeHeader;

        public void centrePosition(ref IntVector2 position)
        {
            if (_previewImage == null)
            {
                position.x = 0;
                position.y = 0;
            }
            else
            {
                position.x = _previewImage.width / 2;
                position.y = _previewImage.height / 2;
            }
        }

        public void setRemakePreview()
        {
            _remakePreview = true;
        }

        public void initialise(ASPTextureEdit textureEdit)
        {
            _windowID = _nextID;
            ++_nextID;

            _selectedColor = new Color(1.0f, 1.0f, 1.0f);
            _notSelectedColor = new Color(0.7f, 0.7f, 0.7f);
            _backgroundColor = new Color(0.5f, 0.5f, 0.5f);

            _textureEdit = textureEdit;
            _windowPosition = new Rect(700, 100, 400, 400);
            _locked = false;
            _remakePreview = true;
            _imageModifiers = _textureEdit.cloneImageModifiers();
            _imageModifiers.insert(0, _textureEdit.cloneBaseTexture());
            _imageModifiers.guiInit();
            _selectedModifier = 0;

            Global.LastButtonPress = 0f;
            Global.AutoRepeatGap = 0.4f;
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

            if (_previewTexture != null) Destroy(_previewTexture);

            GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(this.OnVesselChange));
        }

        public void OnGUI()
        {
            if (_first)
            {
                _first = false;
                largeHeader = new GUIStyle(GUI.skin.label);
                largeHeader.fontSize = 24;
                largeHeader.fontStyle = FontStyle.Bold;
                largeHeader.normal.background = Global.WhiteBackground;
                largeHeader.normal.textColor = Color.black;
            }

            GUI.backgroundColor = _backgroundColor;

            if(Event.current.type == EventType.Layout)
            {
                _windowPosition.width = 10;
                _windowPosition.height = 10;
            }

            _windowPosition = GUILayout.Window(_windowID, _windowPosition, drawWindow, "Texture Editor");

            if (Event.current.type != EventType.Layout) checkGUILock();

            if ((Time.time - Global.LastButtonPress) > 0.2f) Global.AutoRepeatGap = 0.4f;
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

            GUILayout.BeginHorizontal();
            drawTexture();
            GUILayout.Space(5);
            drawImageModifiersList();
            GUILayout.Space(5);
            drawAvailableModifiers();
            GUILayout.Space(5);

            if (_selectedModifier < _imageModifiers.modifiers.Count)
            {
                if (_imageModifiers.modifiers[_selectedModifier].gui().drawRightBar())
                {
                    GUILayout.Label("", largeHeader, GUILayout.Width(20), GUILayout.ExpandHeight(true));
                }
                GUILayout.Space(5);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_selectedModifier < _imageModifiers.modifiers.Count) _imageModifiers.modifiers[_selectedModifier].gui().drawBottom(this);
         
            GUILayout.EndVertical();

            if (_selectedModifier < _imageModifiers.modifiers.Count)_imageModifiers.modifiers[_selectedModifier].gui().drawRight(this);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawMainButtons();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void drawMainButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply", GUILayout.MinWidth(250)))
            {

            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                GameObject.Destroy(this);
            }

            GUILayout.EndHorizontal();
        }

        private void remakePreview()
        {
            if (_previewImage == null) _previewImage = new Image();

            _imageModifiers.drawOnImage(ref _previewImage);
            _previewImage.fillAlpha((byte) 255);

            if (_previewTexture == null) _previewTexture = new Texture2D(_previewImage.width, _previewImage.height, TextureFormat.ARGB32, false);
            
            if (_previewTexture.width != _previewImage.width || _previewTexture.height != _previewImage.height)
            {
                Destroy(_previewTexture);
                _previewTexture = new Texture2D(_previewImage.width, _previewImage.height, TextureFormat.ARGB32, false);
            }

            _previewTexture.SetPixels32(_previewImage.pixels);
            _previewTexture.Apply();

            _remakePreview = false;
        }

        private void drawTexture()
        {
            if (_remakePreview) remakePreview();

            if (_previewTexture.width > 520 || _previewTexture.height > 520)
            {
                _previewScrollPos = GUILayout.BeginScrollView(_previewScrollPos, GUI.skin.box, GUILayout.MinWidth(520), GUILayout.MinHeight(520));
                GUILayout.Box(_previewTexture, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
                GUILayout.EndScrollView();
            }
            else GUILayout.Box(_previewTexture, GUI.skin.box, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
        }

        private void drawImageModifiersList()
        {
            Color contentColor = GUI.contentColor;

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Layers", GUILayout.ExpandWidth(true));

            GUILayout.Space(3);

            _modifiersScrollPos = GUILayout.BeginScrollView(_modifiersScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.MinHeight(450));

            int oldSelectedModifier = _selectedModifier;
            int raiseModifier = -1;
            int lowerModifier = -1;
            int deleteModifier = -1;
            int lowLimit = 0;
            for (int i = _imageModifiers.modifiers.Count - 1; i >= 0; --i)
            {
                GUILayout.BeginHorizontal();

                if (i == _selectedModifier) GUI.contentColor = _selectedColor;
                else GUI.contentColor = _notSelectedColor;

                if (GUILayout.Button(_imageModifiers.modifiers[i].gui().buttonText(), GUILayout.ExpandWidth(true)))
                {
                    _selectedModifier = i;
                }

                if (!_imageModifiers.modifiers[i].locked())
                {
                    GUI.contentColor = _selectedColor;
                    GUILayout.Space(5);
                    if (GUILayout.Button("+")) raiseModifier = i;
                    if (GUILayout.Button("-")) lowerModifier = i;
                    GUILayout.Space(5);
                    if (GUILayout.Button("X")) deleteModifier = i;
                }
                else
                {
                    if (i > lowLimit) lowLimit = i;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.contentColor = contentColor;

            if (deleteModifier >= 0 && !_imageModifiers.modifiers[deleteModifier].locked())
            {
                _imageModifiers.removeAt(deleteModifier);
                _remakePreview = true;
            }

            if (raiseModifier >= 0 && !_imageModifiers.modifiers[raiseModifier].locked())
            {
                if (raiseModifier <= (_imageModifiers.modifiers.Count - 1))
                {
                    _imageModifiers.swap(raiseModifier, raiseModifier + 1);
                    _remakePreview = true;
                }
            }

            if (lowerModifier >= 2 && !_imageModifiers.modifiers[lowerModifier].locked())
            {
                if (lowerModifier >= (lowLimit + 2))
                {
                    _imageModifiers.swap(lowerModifier, lowerModifier - 1);
                    _remakePreview = true;
                }
            }

            if (_selectedModifier > (_imageModifiers.modifiers.Count - 1)) _selectedModifier = _imageModifiers.modifiers.Count - 1;
        }

        private void drawAvailableModifiers()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Add Layer", GUILayout.ExpandWidth(true));

            GUILayout.Space(3);

            _availableModifiersScrollPos = GUILayout.BeginScrollView(_availableModifiersScrollPos, GUI.skin.box, GUILayout.MinWidth(150), GUILayout.MinHeight(450));

            if (GUILayout.Button("Text", GUILayout.ExpandWidth(true)))
            {
                IM.Text im = new IM.Text();
                im.gui().initialise();
                _imageModifiers.add(im);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
