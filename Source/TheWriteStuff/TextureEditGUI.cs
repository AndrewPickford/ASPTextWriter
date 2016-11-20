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

        public GUIStyle largeHeader;
        public GUIStyle smallHeader;
        public GUIStyle windowTitle;

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
        private IM.BaseTexture _baseTexture = null;
        private ImageModifiers _imageModifiers = null;
        private Vector2 _modifiersScrollPos;
        private int _selectedModifier;
        private Vector2 _availableModifiersScrollPos;
        private BoundingBox _boundingBox = null;
        private ValueSelector<int, IntField> _bbXselector;
        private ValueSelector<int, IntField> _bbYselector;
        private ValueSelector<int, IntField> _bbWselector;
        private ValueSelector<int, IntField> _bbHselector;

        internal int speedSelection;

        ~TextureEditGUI()
        {
            if (_previewTexture != null) Destroy(_previewTexture);
        }

        public ASPTextureEdit textureEdit()
        {
            return _textureEdit;
        }

        public IntVector2 centrePosition()
        {
            IntVector2 position = new IntVector2();

            if (_boundingBox.valid && _boundingBox.use)
            {
                position.x = _boundingBox.w / 2 + _boundingBox.x;
                position.y = _boundingBox.h / 2 + _boundingBox.y;
            }
            else
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

            return position;
        }

        public void setRemakePreview()
        {
            _remakePreview = true;
        }

        public void initialise(ASPTextureEdit textureEdit, int count)
        {
            _windowID = _nextID;
            ++_nextID;

            _textureEdit = textureEdit;
            _windowPosition = new Rect(700, 200 -((count) * 100), 400, 400);
            _locked = false;
            _remakePreview = true;

            _imageModifiers = _textureEdit.cloneImageModifiers();
            _baseTexture = _textureEdit.cloneBaseTexture();
            _baseTexture.gui().initialise(this);
            _imageModifiers.guiInit(this);

            _boundingBox = _textureEdit.cloneBoundingBox();
            _bbXselector = new ValueSelector<int, IntField>(_boundingBox.x, 0, 999999, 1, "Bottom Left X", Color.white);
            _bbYselector = new ValueSelector<int, IntField>(_boundingBox.y, 0, 999999, 1, "Bottom Left Y", Color.white);
            _bbWselector = new ValueSelector<int, IntField>(_boundingBox.w, 0, 999999, 1, "Width", Color.white);
            _bbHselector = new ValueSelector<int, IntField>(_boundingBox.h, 0, 999999, 1, "Height", Color.white);

            _selectedModifier = -2;

            Global.LastButtonPress = 0f;
            Global.AutoRepeatGap = 0.4f;
        }

        public KSPTextureInfo kspTextureInfo()
        {
            return _textureEdit.kspTextureInfo;
        }

        private void OnVesselChange(Vessel vesselChange)
        {
            GameObject.Destroy(this);
        }

        private void OnGameSceneLoadRequested(GameScenes scene)
        {
            GameObject.Destroy(this);
        }

        public void Awake()
        {
            InputLockManager.RemoveControlLock(_lockText);
            _remakePreview = true;
            GameEvents.onVesselChange.Add(this.OnVesselChange);
            GameEvents.onGameSceneLoadRequested.Add(this.OnGameSceneLoadRequested);
        }

        public void OnDestroy()
        {
            EditorLogic.fetch.Unlock(_lockText);

            if (_textureEdit != null) _textureEdit.finalisePainting();
            if (_previewTexture != null) Destroy(_previewTexture);

            GameEvents.onVesselChange.Remove(this.OnVesselChange);
            GameEvents.onGameSceneLoadRequested.Remove(this.OnGameSceneLoadRequested);
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

                smallHeader = new GUIStyle(GUI.skin.label);
                smallHeader.fontSize = 16;
                smallHeader.fontStyle = FontStyle.Bold;
                smallHeader.normal.textColor = Color.white;
            }

            GUI.backgroundColor = Global.BackgroundColor;

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
                    //EditorTooltip.Instance.HideToolTip();
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

            if (_selectedModifier >= 0 && _selectedModifier < _imageModifiers.modifiers.Count && _imageModifiers.modifiers[_selectedModifier].longRight) drawWindowLongRight();
            else drawWindowLongBottom();

            GUILayout.Space(10);

            drawMainButtons();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void drawWindowLongRight()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            drawWindowMiddlePart();

            GUILayout.Space(5);

            if (_selectedModifier == -2) _baseTexture.gui().drawBottom(this);
            if (_selectedModifier == -1) drawGlobalBoundingBoxSelector();
            if (_selectedModifier >= 0 && _selectedModifier < _imageModifiers.modifiers.Count) _imageModifiers.modifiers[_selectedModifier].gui().drawBottom(this);

            GUILayout.EndVertical();

            GUILayout.Space(5);

            if (_selectedModifier == -2) _baseTexture.gui().drawRight(this);
            if (_selectedModifier >= 0 && _selectedModifier < _imageModifiers.modifiers.Count) _imageModifiers.modifiers[_selectedModifier].gui().drawRight(this);

            GUILayout.EndHorizontal();
        }

        private void drawWindowLongBottom()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            drawWindowMiddlePart();

            GUILayout.Space(5);

            if (_selectedModifier == -2) _baseTexture.gui().drawRight(this);
            if (_selectedModifier >= 0 && _selectedModifier < _imageModifiers.modifiers.Count) _imageModifiers.modifiers[_selectedModifier].gui().drawRight(this);

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (_selectedModifier == -2) _baseTexture.gui().drawBottom(this);
            if (_selectedModifier == -1) drawGlobalBoundingBoxSelector();
            if (_selectedModifier >= 0 && _selectedModifier < _imageModifiers.modifiers.Count) _imageModifiers.modifiers[_selectedModifier].gui().drawBottom(this);

            GUILayout.EndVertical();
        }

        private void drawWindowMiddlePart()
        {
            GUILayout.BeginHorizontal();
            drawTexture();
            GUILayout.FlexibleSpace();
            drawImageModifiersList();
            GUILayout.FlexibleSpace();
            drawAvailableModifiers();
            GUILayout.FlexibleSpace();
            GUILayout.Label("", largeHeader, GUILayout.Width(20), GUILayout.ExpandHeight(true));
            GUILayout.BeginVertical();
            GUILayout.Space(300);
            GUILayout.EndVertical();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }


        private void drawMainButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply", GUILayout.MinWidth(250)))
            {
                _textureEdit.setBaseTexture(_baseTexture);
                _textureEdit.setImageModifiers(_imageModifiers);
                _textureEdit.setBoundingBox(_boundingBox);
                _textureEdit.writeTexture();
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

            _baseTexture.drawOnImage(ref _previewImage, _boundingBox);
            _imageModifiers.drawOnImage(ref _previewImage, _boundingBox);
            _previewImage.fillAlpha((byte) 255);
            _previewImage.clip(_boundingBox);

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
                _previewScrollPos = GUILayout.BeginScrollView(_previewScrollPos, GUI.skin.box, GUILayout.MinWidth(550), GUILayout.MinHeight(550));
                GUILayout.Box(_previewTexture, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
                GUILayout.EndScrollView();
            }
            else GUILayout.Box(_previewTexture, GUI.skin.box, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
        }

        private void drawImageModifiersList()
        {
            Color contentColor = GUI.contentColor;
           
            _modifiersScrollPos = GUILayout.BeginScrollView(_modifiersScrollPos, GUI.skin.box, GUILayout.MinWidth(200), GUILayout.ExpandHeight(true));

            GUILayout.Label("Layers", smallHeader, GUILayout.ExpandWidth(true));

            GUILayout.Space(3);

            int oldSelectedModifier = _selectedModifier;
            int raiseModifier = -1;
            int lowerModifier = -1;
            int deleteModifier = -1;
            int lowLimit = -1;

            for (int i = _imageModifiers.modifiers.Count - 1; i >= 0; --i)
            {
                GUILayout.BeginHorizontal();

                if (i == _selectedModifier) GUI.contentColor = Global.SelectedColor;
                else GUI.contentColor = Global.NotSelectedColor;

                if (GUILayout.Button(_imageModifiers.modifiers[i].gui().buttonText(), GUILayout.ExpandWidth(true), GUILayout.MinWidth(80)))
                {
                    _selectedModifier = i;
                }
               
                GUI.contentColor = Global.SelectedColor;
                GUILayout.Space(5);
                if (GUILayout.Button("+")) raiseModifier = i;
                if (GUILayout.Button("-")) lowerModifier = i;
                GUILayout.Space(5);
                if (GUILayout.Button("X")) deleteModifier = i;

                GUILayout.EndHorizontal();
            }

            GUI.contentColor = (_selectedModifier == -1) ? Global.SelectedColor : Global.NotSelectedColor;
            if (GUILayout.Button("Bounding Box", GUILayout.ExpandWidth(true), GUILayout.MinWidth(80))) _selectedModifier = -1;

            GUI.contentColor = (_selectedModifier == -2) ? Global.SelectedColor : Global.NotSelectedColor;
            if (GUILayout.Button(_baseTexture.gui().buttonText(), GUILayout.ExpandWidth(true), GUILayout.MinWidth(80))) _selectedModifier = -2;

            GUILayout.EndScrollView();

            GUI.contentColor = contentColor;

            if (deleteModifier >= 0)
            {
                _imageModifiers.removeAt(deleteModifier);
                _remakePreview = true;
            }

            if (raiseModifier >= 0)
            {
                if (raiseModifier <= (_imageModifiers.modifiers.Count - 1))
                {
                    _imageModifiers.swap(raiseModifier, raiseModifier + 1);
                    _remakePreview = true;
                }
            }

            if (lowerModifier >= 1)
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
            _availableModifiersScrollPos = GUILayout.BeginScrollView(_availableModifiersScrollPos, GUI.skin.box, GUILayout.MinWidth(150), GUILayout.ExpandHeight(true));

            GUILayout.Label("Add Layer", smallHeader, GUILayout.ExpandWidth(true));

            GUILayout.Space(3);

            if (GUILayout.Button("Color Decal", GUILayout.ExpandWidth(true)))
            {
                IM.BitmapColorDecal im = new IM.BitmapColorDecal();
                im.setPosition(centrePosition());
                im.gui().initialise(this);
                _imageModifiers.add(im);
                _remakePreview = true;
                _selectedModifier = _imageModifiers.modifiers.Count - 1;
            }

            if (GUILayout.Button("Mono Decal", GUILayout.ExpandWidth(true)))
            {
                IM.BitmapMonoDecal im = new IM.BitmapMonoDecal();
                im.setPosition(centrePosition());
                im.gui().initialise(this);
                _imageModifiers.add(im);
                _remakePreview = true;
                _selectedModifier = _imageModifiers.modifiers.Count - 1;
            }

            if (GUILayout.Button("Bitmap Text", GUILayout.ExpandWidth(true)))
            {
                IM.Text im = new IM.BitmapText();
                im.setPosition(centrePosition());
                im.gui().initialise(this);
                _imageModifiers.add(im);
                _remakePreview = true;
                _selectedModifier = _imageModifiers.modifiers.Count - 1;
            }

            if (GUILayout.Button("Rectangle", GUILayout.ExpandWidth(true)))
            {
                IM.Rectangle im = new IM.Rectangle();
                im.setPosition(centrePosition());
                im.gui().initialise(this);
                _imageModifiers.add(im);
                _remakePreview = true;
                _selectedModifier = _imageModifiers.modifiers.Count - 1;
            }

            if (GUILayout.Button("Circle", GUILayout.ExpandWidth(true)))
            {
                IM.Circle im = new IM.Circle();
                im.setPosition(centrePosition());
                im.gui().initialise(this);
                _imageModifiers.add(im);
                _remakePreview = true;
                _selectedModifier = _imageModifiers.modifiers.Count - 1;
            }

            GUILayout.EndScrollView();
        }

        private void drawGlobalBoundingBoxSelector()
        {
            bool bbChanged = false;

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal(largeHeader);
            GUILayout.Space(20);
            GUILayout.Label("GLOBAL BOUNDING BOX", largeHeader, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (_boundingBox.valid) GUILayout.Label("Status: Valid", GUILayout.MinWidth(100));
            else GUILayout.Label("Status: Invalid", GUILayout.MinWidth(100));
            GUILayout.Space(5);
            bool oldUse = _boundingBox.use;
            _boundingBox.use = GUILayout.Toggle(_boundingBox.use, "Use bounding box");
            if (oldUse != _boundingBox.use) _remakePreview = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (_bbXselector.draw())
            {
                _boundingBox.x = _bbXselector.value();
                bbChanged = true;
            }
            GUILayout.Space(10f);

            if (_bbYselector.draw())
            {
                _boundingBox.y = _bbYselector.value();
                bbChanged = true;
            }
            GUILayout.Space(10f);

            if (_bbWselector.draw())
            {
                _boundingBox.w = _bbWselector.value();
                bbChanged = true;
            }
            if (_bbWselector.value() > _baseTexture.width())
            {
                _bbWselector.set(_baseTexture.width());
                _boundingBox.w = _bbWselector.value();
                bbChanged = true;
            }

            GUILayout.Space(10f);

            if (_bbHselector.draw())
            {
                if (_bbHselector.value() > _baseTexture.height()) _bbHselector.set(_baseTexture.height());
                _boundingBox.h = _bbHselector.value();
                bbChanged = true;
            }
            if (_bbHselector.value() > _baseTexture.height())
            {
                _bbHselector.set(_baseTexture.height());
                _boundingBox.w = _bbWselector.value();
                bbChanged = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (bbChanged)
            {
                _boundingBox.valid = false;

                if (_boundingBox.x >= 0 && _boundingBox.y >= 0 && _boundingBox.w > 0 && _boundingBox.h > 0)
                {
                    if (_baseTexture == null) _boundingBox.valid = true;
                    else
                    {
                        if (_boundingBox.x < _baseTexture.width() && _boundingBox.y < _baseTexture.height()) _boundingBox.valid = true;
                    }
                }
                _remakePreview = true;
            }
        }
    }
}
