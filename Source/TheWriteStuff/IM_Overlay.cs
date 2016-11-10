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
            protected bool _mirror = false;
            protected byte _alpha = 255;
            protected byte _textureAlpha = 0;
            protected AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            protected float _normalScale = 2.0f;
            protected NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            protected BlendMethod _blendMethod = BlendMethod.RGB;
            protected Rotation _rotation = Rotation.R0;

            public Overlay()
            {
                _position = new IntVector2();
            }

            public void setPosition(IntVector2 position)
            {
                _position.x = position.x;
                _position.y = position.y;
            }

            protected void loadOverlay(ConfigNode node)
            {
                _position = new IntVector2();
                _mirror = false;
                _alpha = 255;
                _textureAlpha = 0;
                _alphaOption = AlphaOption.USE_TEXTURE;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _blendMethod = BlendMethod.RGB;
                _rotation = Rotation.R0;

                if (node.HasValue("x")) _position.x = int.Parse(node.GetValue("x"));
                if (node.HasValue("y")) _position.y = int.Parse(node.GetValue("y"));
                if (node.HasValue("mirror")) _mirror = bool.Parse(node.GetValue("mirror"));
                if (node.HasValue("alpha")) _alpha = byte.Parse(node.GetValue("alpha"));
                if (node.HasValue("textureAlpha")) _textureAlpha = byte.Parse(node.GetValue("textureAlpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = float.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("rotation")) _rotation = (Rotation)ConfigNode.ParseEnum(typeof(Rotation), node.GetValue("rotation"));
            }

            protected void saveOverlay(ConfigNode node)
            {
                saveImageModifier(node);
                node.AddValue("x", _position.x);
                node.AddValue("y", _position.y);
                node.AddValue("mirror", _mirror);
                node.AddValue("alpha", _alpha);
                node.AddValue("textureAlpha", _textureAlpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("rotation", ConfigNode.WriteEnum(_rotation));
            }

            protected void copyFromOverlay(Overlay overlay)
            {
                copyFromImageModifer(overlay);
                _position = new IntVector2(overlay._position);
                _mirror = overlay._mirror;
                _alpha = overlay._alpha;
                _textureAlpha = overlay._textureAlpha;
                _alphaOption = overlay._alphaOption;
                _normalScale = overlay._normalScale;
                _normalOption = overlay._normalOption;
                _blendMethod = overlay._blendMethod;
                _rotation = overlay._rotation;
            }

            protected void drawDecalOnImage(ref Image image, ref Image normalMap, string _url, BoundingBox boundingBox)
            {
                BitmapDecal decal;
                if (!BitmapDecalCache.Instance.decals.TryGetValue(_url, out decal)) return;

                drawOnImage(ref image, boundingBox);

                if (_normalOption == NormalOption.USE_BACKGROUND) return;

                Image backgroundImage = new Image(normalMap.width, normalMap.height);
                Color32 backgroudColor = new Color32(127, 127, 127, 0);
                backgroundImage.fill(backgroudColor);

                Color32 color = Global.Gray32;
                if (_normalOption == NormalOption.RAISE_TEXT) color = Global.White32;
                if (_normalOption == NormalOption.LOWER_TEXT) color = Global.Black32;

                Image decalImage = new Image(decal.image);
                decalImage.recolor(Global.Black32, color, false, true);
                decalImage.rotateImage(_rotation);
                if (_mirror) decalImage.flipHorizontally();

                backgroundImage.blendImage(decalImage, BlendMethod.PIXEL, _position, AlphaOption.OVERWRITE, 255, boundingBox);

                Image normalMapImage = backgroundImage.createNormalMap(_normalScale);

                if (image.width == normalMap.width && image.height == normalMap.height) normalMap.rescale(image.width, image.height);
                normalMap.overlay(normalMapImage, backgroundImage, 128, boundingBox);
            }




            public abstract class OverlayGui : ImageModifierGui
            {
                private IM.Overlay _overlay;
                private ValueSelector<byte, ByteField> _alphaSelector;
                private ValueSelector<float, FloatField> _normalScaleSelector;
                private ValueSelector<byte, ByteField> _textureAlphaSelector;

                protected virtual void drawBottomOverlayExtras1(TextureEditGUI gui)
                {
                }

                public OverlayGui(IM.Overlay overlay)
                {
                    _overlay = overlay;
                }

                protected void drawBottomOverlay(TextureEditGUI gui)
                {
                    GUILayout.BeginHorizontal();
                    positionSelector(gui, ref _overlay._position);
                    GUILayout.Space(5);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    drawBottomOverlayExtras1(gui);
                    _alphaSelector.draw();
                    GUILayout.Space(10f);
                    rotationSelector(gui, ref _overlay._rotation, ref _overlay._mirror);
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
                        normalMapOptionSelector(gui, ref _overlay._normalOption);
                        GUILayout.Space(10f);
                        _normalScaleSelector.draw();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    if (_overlay._alpha != _alphaSelector.value())
                    {
                        _overlay._alpha = _alphaSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._normalScale != _normalScaleSelector.value()) _overlay._normalScale = _normalScaleSelector.value();
                    if (_overlay._textureAlpha != _textureAlphaSelector.value()) _overlay._textureAlpha = _textureAlphaSelector.value();
                }

                protected void initialiseOverlay(TextureEditGUI gui)
                {
                    _alphaSelector = new ValueSelector<byte, ByteField>(_overlay._alpha, 0, 255, 1, "Alpha", Color.white);

                    _textureAlphaSelector = new ValueSelector<byte, ByteField>(_overlay._textureAlpha, 0, 255, 1, "Texture Alpha", Color.white);
                    _normalScaleSelector = new ValueSelector<float, FloatField>(_overlay._normalScale, 0, 5.0f, 0.1f, "Normal Scale", Color.white);

                    if (gui.kspTextureInfo().isSpecular) _textureAlphaSelector.setLabel("Specularity");
                    else if (gui.kspTextureInfo().isTransparent) _textureAlphaSelector.setLabel("Transparency");
                }
            }
        }
    }
}
