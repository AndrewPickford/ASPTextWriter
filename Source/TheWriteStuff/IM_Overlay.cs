using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class Overlay : ImageModifier
        {
            protected IntVector2 _position;
            protected IntVector2 _origin;
            protected bool _mirrorX = false;
            protected bool _mirrorY = false;
            protected byte _textureAlpha = 0;
            protected AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            protected double _normalScale = 2.0d;
            protected NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            protected bool _scaleNormalsByAlpha = false;
            protected bool _normalsFromDerivatives = false;
            protected BlendMethod _blendMethod = BlendMethod.SSR;
            protected int _rotation = 0;
            protected Image _image;
            protected bool _overlayRotates = true;
            private IntVector2 _overlayPosition;

            public abstract void drawImage();
            public abstract void drawImageSolid(); 

            protected Overlay() :
                base()
            {
                _position = new IntVector2();
                _position.x = 0;
                _position.y = 0;

                _origin = new IntVector2();
                _origin.x = 0;
                _origin.y = 0;

                _overlayPosition = new IntVector2(0, 0);
            }

            public void setPosition(IntVector2 position)
            {
                _position.x = position.x;
                _position.y = position.y;
            }

            public override void load(ConfigNode node)
            {
                _position = new IntVector2();
                _mirrorX = false;
                _mirrorY = false;
                _textureAlpha = 0;
                _alphaOption = AlphaOption.USE_TEXTURE;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _scaleNormalsByAlpha = false;
                _normalsFromDerivatives = false;
                _blendMethod = BlendMethod.RGB;
                _rotation = 0;

                if (node.HasValue("x")) _position.x = int.Parse(node.GetValue("x"));
                if (node.HasValue("y")) _position.y = int.Parse(node.GetValue("y"));
                if (node.HasValue("mirrorX")) _mirrorX = bool.Parse(node.GetValue("mirrorX"));
                if (node.HasValue("mirrorY")) _mirrorX = bool.Parse(node.GetValue("mirrorY"));
                if (node.HasValue("textureAlpha")) _textureAlpha = byte.Parse(node.GetValue("textureAlpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = double.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("scaleNormalsByAlpha")) _scaleNormalsByAlpha = bool.Parse(node.GetValue("scaleNormalsByAlpha"));
                if (node.HasValue("normalsFromDerivatives")) _normalsFromDerivatives = bool.Parse(node.GetValue("normalsFromDerivatives"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("rotation")) _rotation = int.Parse(node.GetValue("rotation"));
            }

            public override void save(ConfigNode node)
            {
                base.save(node);
                node.AddValue("x", _position.x);
                node.AddValue("y", _position.y);
                node.AddValue("mirrorX", _mirrorX);
                node.AddValue("mirrorY", _mirrorY);
                node.AddValue("textureAlpha", _textureAlpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale.ToString("F1"));
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("scaleNormalsByAlpha", _scaleNormalsByAlpha);
                node.AddValue("normalsFromDerivatives", _normalsFromDerivatives);
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("rotation", _rotation);
            }

            protected void copyFrom(Overlay overlay)
            {
                base.copyFrom(overlay);
                _position = new IntVector2(overlay._position);
                _mirrorX = overlay._mirrorX;
                _mirrorY = overlay._mirrorY;
                _textureAlpha = overlay._textureAlpha;
                _alphaOption = overlay._alphaOption;
                _normalScale = overlay._normalScale;
                _normalOption = overlay._normalOption;
                _scaleNormalsByAlpha = overlay._scaleNormalsByAlpha;
                _normalsFromDerivatives = overlay._normalsFromDerivatives;
                _blendMethod = overlay._blendMethod;
                _rotation = overlay._rotation;
            }

            public override void drawOnImage(ref Image image, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("start");

                drawImage();

                if (_overlayRotates)
                {
                    _image.rotate(_rotation, ref _origin);
                    if (_mirrorX) _image.flipHorizontally();
                    if (_mirrorY) _image.flipVertically();
                }

                _overlayPosition.x = _position.x - _origin.x;
                _overlayPosition.y = _position.y - _origin.y;

                image.blendImage(_image, _blendMethod, _overlayPosition, _alphaOption, _textureAlpha, boundingBox);
            }

            public override void drawOnImage(ref Image image, ref Image normalMap, BoundingBox boundingBox)
            {
                if (Global.Debug3) Utils.Log("start");

                drawOnImage(ref image, boundingBox);

                if (_normalOption == NormalOption.USE_BACKGROUND) return;

                Image overlayNormalMap = new Image(image.width, image.height);
                Color32 backgroudColor = new Color32(127, 127, 127, 0);
                overlayNormalMap.fill(backgroudColor);

                drawImageSolid();
                if (_overlayRotates)
                {
                    _image.rotate(_rotation, ref _origin);
                    if (_mirrorX) _image.flipHorizontally();
                    if (_mirrorY) _image.flipVertically();
                }

                switch (_normalOption)
                {
                    case NormalOption.FLAT:
                        _image.fill(Global.Gray32, false);
                        break;

                    case NormalOption.RAISE:
                        if (_scaleNormalsByAlpha) _image.recolorScaledByAlpha(Global.Gray32, Global.White32);
                        else _image.recolorScaledByGray(Global.Gray32, Global.White32);
                        if (_normalsFromDerivatives)
                        {
                            _image.edges(2);
                            _image.recolorScaledByGray(Global.Gray32, Global.White32);
                        }
                        break;

                    case NormalOption.LOWER:
                        if (_scaleNormalsByAlpha) _image.recolorScaledByAlpha(Global.Gray32, Global.Black32);
                        else _image.recolorScaledByGray(Global.Gray32, Global.Black32);
                        if (_normalsFromDerivatives)
                        {
                            _image.edges(2);
                            _image.recolorScaledByGray(Global.Gray32, Global.Black32);
                        }
                        break;

                    case NormalOption.USE_BACKGROUND:
                    default:
                        break;
                }

                _overlayPosition.x = _position.x - _origin.x;
                _overlayPosition.y = _position.y - _origin.y;
                overlayNormalMap.blendImage(_image, BlendMethod.RGB, _overlayPosition, AlphaOption.OVERWRITE, 255, boundingBox);

                BoundingBox bBox = new BoundingBox(boundingBox);
                if (image.width != normalMap.width || image.height != normalMap.height)
                {
                    overlayNormalMap.rescale(normalMap.width, normalMap.height);
                    bBox.x = (int)((double)bBox.x * (double)normalMap.width / (double)image.width);
                    bBox.w = (int)((double)bBox.w * (double)normalMap.width / (double)image.width);
                    bBox.y = (int)((double)bBox.y * (double)normalMap.height / (double)image.height);
                    bBox.h = (int)((double)bBox.h * (double)normalMap.height / (double)image.height);
                }

                Image normalMapImage = overlayNormalMap.createNormalMap(_normalScale);
                normalMap.overlay(normalMapImage, overlayNormalMap, 128, bBox);
            }




            public abstract class OverlayGui : ImageModifierGui
            {
                private IM.Overlay _overlay;
                protected ValueSelector<double, DoubleField> _normalScaleSelector;
                protected ValueSelector<byte, ByteField> _textureAlphaSelector;
                protected ValueSelector<int, IntField> _xPositionSelector;
                protected ValueSelector<int, IntField> _yPositionSelector;
                protected ValueSelector<int, IntField> _rotationSelector;

                public OverlayGui(IM.Overlay overlay)
                {
                    _overlay = overlay;
                }

                public override void drawBottom(TextureEditGUI gui)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    header(gui, _overlay.headerName());
                    drawExtras1(gui);
                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();
                    positionSelector(gui, ref _xPositionSelector, ref _yPositionSelector);
                    GUILayout.Space(5);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    drawExtras2(gui);
                    rotationSelector(gui, ref _rotationSelector, ref _overlay._mirrorX, ref _overlay._mirrorY);
                    GUILayout.Space(10f);
                    blendMethodSelector(gui, ref _overlay._blendMethod);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (gui.kspTextureInfo().isSpecular || gui.kspTextureInfo().isTransparent)
                    {
                        _textureAlphaSelector.draw();
                        GUILayout.Space(10f);
                        alphaOptionSelector(gui, ref _overlay._alphaOption);
                        GUILayout.Space(10f);
                    }

                    if (gui.kspTextureInfo().hasNormalMap)
                    {
                        normalMapOptionSelector(gui, ref _overlay._normalOption, ref _overlay._scaleNormalsByAlpha, ref _overlay._normalsFromDerivatives);
                        GUILayout.Space(10f);
                        _normalScaleSelector.draw();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    checkChanged(ref _overlay._position.x, _xPositionSelector.value(), gui);
                    checkChanged(ref _overlay._position.y, _yPositionSelector.value(), gui);
                    checkChanged(ref _overlay._rotation, _rotationSelector.value(), gui);
                    if (_overlay._normalScale != _normalScaleSelector.value()) _overlay._normalScale = _normalScaleSelector.value();
                    if (_overlay._textureAlpha != _textureAlphaSelector.value()) _overlay._textureAlpha = _textureAlphaSelector.value();
                }

                protected virtual void drawExtras1(TextureEditGUI gui)
                {
                }

                protected virtual void drawExtras2(TextureEditGUI gui)
                {
                }

                public override void initialise(TextureEditGUI gui)
                {
                    _xPositionSelector = new ValueSelector<int, IntField>(_overlay._position.x, -9999, 9999, 1, "X", Color.white, false);
                    _yPositionSelector = new ValueSelector<int, IntField>(_overlay._position.y, -9999, 9999, 1, "Y", Color.white, false);

                    _textureAlphaSelector = new ValueSelector<byte, ByteField>(_overlay._textureAlpha, 0, 255, 1, "Texture Alpha", Color.white);
                    _normalScaleSelector = new ValueSelector<double, DoubleField>(_overlay._normalScale, 0, 5.0, 0.1, "Normal Scale", Color.white);

                    _rotationSelector = new ValueSelector<int, IntField>(_overlay._rotation, 0, 359, 1, "", Color.white);

                    if (gui.kspTextureInfo().isSpecular) _textureAlphaSelector.setLabel("Specularity");
                    else if (gui.kspTextureInfo().isTransparent) _textureAlphaSelector.setLabel("Transparency");
                }
            }
        }
    }
}
