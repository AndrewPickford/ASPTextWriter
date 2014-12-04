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
        private Rect _windowPosition;
        private string _lockText = "TextureEditGUILock";
        private bool _locked;
        private ASPTextureEdit _textureEdit = null;
        private bool _remakePreview = true;
        private Texture2D _previewTexture = null;
        private Image _previewImage = null;
        private Vector2 _previewScrollPos;
        private ImageModifiers _imageModifiers = null;


        public void initialise(ASPTextureEdit textureEdit)
        {
            _windowID = _nextID;
            ++_nextID;

            _textureEdit = textureEdit;
            _windowPosition = new Rect(700, 100, 400, 500);
            _locked = false;
            _remakePreview = true;
            _imageModifiers = _textureEdit.cloneImageModifiers();
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
            _windowPosition = GUILayout.Window(_windowID, _windowPosition, drawWindow, "Texture Editor");
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

            drawTexture();
            GUILayout.Space(5);
            //drawImageModifiersList();
            GUILayout.Space(5);
            //drawBackgroundSelector();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            drawCloseButton();

            GUILayout.EndVertical();

            GUI.DragWindow();
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

        private void remakePreview()
        {
            if (_previewImage == null) _previewImage = new Image();

            _imageModifiers.drawOnImage(ref _previewImage);

            if (_previewTexture == null) _previewTexture = new Texture2D(_previewImage.width, _previewImage.height, TextureFormat.ARGB32, false);
            
            if (_previewTexture.width != _previewImage.width || _previewTexture.height != _previewImage.height)
            {
                Destroy(_previewTexture);
                _previewTexture = new Texture2D(_previewImage.width, _previewImage.height, TextureFormat.ARGB32, false);
            }

            _previewTexture.SetPixels32(_previewImage.pixels);

            _remakePreview = false;
        }

        private void drawTexture()
        {
            if (_remakePreview) remakePreview();

            if (_previewTexture.width > 500 || _previewTexture.height > 500) _previewScrollPos = GUILayout.BeginScrollView(_previewScrollPos, GUI.skin.box, GUILayout.MinWidth(500), GUILayout.MinHeight(500));
            GUILayout.Box(_previewTexture, GUI.skin.box, GUILayout.Width(_previewTexture.width), GUILayout.Height(_previewTexture.height));
            if (_previewTexture.width > 500 || _previewTexture.height > 500) GUILayout.EndScrollView();
        }
    }
}
