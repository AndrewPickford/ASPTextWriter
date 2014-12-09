using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    namespace IM
    {
        public abstract class MonoOverlay : ImageModifier
        {
            protected IntVector2 _position;
            protected bool _mirror = false;
            protected Color32 _color = new Color32(0, 0, 0, 255);
            protected byte _textureAlpha = 0;
            protected AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            protected float _normalScale = 2.0f;
            protected NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            protected BlendMethod _blendMethod = BlendMethod.RGB;
            protected Rotation _rotation = Rotation.R0;

            public MonoOverlay()
            {
                _position = new IntVector2();
            }

            public void setPosition(IntVector2 position)
            {
                _position.x = position.x;
                _position.y = position.y;
            }

            protected void loadMonoOverlay(ConfigNode node)
            {
                _position = new IntVector2();
                _mirror = false;
                _color = new Color32(0, 0, 0, 255);
                _textureAlpha = 0;
                _alphaOption = AlphaOption.USE_TEXTURE;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _blendMethod = BlendMethod.RGB;
                _rotation = Rotation.R0;

                if (node.HasValue("x")) _position.x = int.Parse(node.GetValue("x"));
                if (node.HasValue("y")) _position.y = int.Parse(node.GetValue("y"));
                if (node.HasValue("mirror")) _mirror = bool.Parse(node.GetValue("mirror"));
                if (node.HasValue("red")) _color.r = byte.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _color.g = byte.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _color.b = byte.Parse(node.GetValue("blue"));
                if (node.HasValue("alpha")) _color.a = byte.Parse(node.GetValue("alpha"));
                if (node.HasValue("textureAlpha")) _textureAlpha = byte.Parse(node.GetValue("textureAlpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = int.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("rotation")) _rotation = (Rotation)ConfigNode.ParseEnum(typeof(Rotation), node.GetValue("rotation"));
            }

            protected void saveMonoOverlay(ConfigNode node)
            {
                node.AddValue("x", _position.x);
                node.AddValue("y", _position.y);
                node.AddValue("mirror", _mirror);
                node.AddValue("red", _color.r);
                node.AddValue("green", _color.g);
                node.AddValue("blue", _color.b);
                node.AddValue("alpha", _color.a);
                node.AddValue("textureAlpha", _textureAlpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("rotation", ConfigNode.WriteEnum(_rotation));
            }

            protected void copyFromMonoOverlay(MonoOverlay overlay)
            {
                _position = new IntVector2(overlay._position);
                _mirror = overlay._mirror;
                _color = new Color32(overlay._color.r, overlay._color.g, overlay._color.b, overlay._color.a);
                _textureAlpha = overlay._textureAlpha;
                _alphaOption = overlay._alphaOption;
                _normalScale = overlay._normalScale;
                _normalOption = overlay._normalOption;
                _blendMethod = overlay._blendMethod;
                _rotation = overlay._rotation;
            }




            public abstract class MonoOverlayGui : ImageModifierGui
            {
                private IM.MonoOverlay _overlay;
                private ValueSelector<byte, ByteField> _redSelector;
                private ValueSelector<byte, ByteField> _greenSelector;
                private ValueSelector<byte, ByteField> _blueSelector;
                private ValueSelector<byte, ByteField> _alphaSelector;
                private ValueSelector<float, FloatField> _normalScaleSelector;
                private ValueSelector<byte, ByteField> _textureAlphaSelector;

                public MonoOverlayGui(IM.MonoOverlay overlay)
                {
                    _overlay = overlay;
                }

                protected void drawBottomMonoOverlay(TextureEditGUI gui)
                {
                    GUILayout.BeginHorizontal();
                    positionSelector(gui, ref _overlay._position);
                    GUILayout.Space(5);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    colorSelector(gui, ref _redSelector, ref _greenSelector, ref _blueSelector, ref _alphaSelector);
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

                    setMonoOverlayValues(gui);
                }

                protected void setMonoOverlayValues(TextureEditGUI gui)
                {
                    if (_overlay._color.r != _redSelector.value())
                    {
                        _overlay._color.r = _redSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._color.g != _greenSelector.value())
                    {
                        _overlay._color.g = _greenSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._color.b != _blueSelector.value())
                    {
                        _overlay._color.b = _blueSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._color.a != _alphaSelector.value())
                    {
                        _overlay._color.a = _alphaSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._normalScale != _normalScaleSelector.value()) _overlay._normalScale = _normalScaleSelector.value();
                    if (_overlay._textureAlpha != _textureAlphaSelector.value()) _overlay._textureAlpha = _textureAlphaSelector.value();
                }

                protected void initialiseMonoOverlay(TextureEditGUI gui)
                {
                    _redSelector = new ValueSelector<byte, ByteField>(_overlay._color.r, 0, 255, 1, "Red", Color.red);
                    _greenSelector = new ValueSelector<byte, ByteField>(_overlay._color.g, 0, 255, 1, "Green", Color.green);
                    _blueSelector = new ValueSelector<byte, ByteField>(_overlay._color.b, 0, 255, 1, "Blue", Color.blue);
                    _alphaSelector = new ValueSelector<byte, ByteField>(_overlay._color.a, 0, 255, 1, "Alpha", Color.white);

                    _textureAlphaSelector = new ValueSelector<byte, ByteField>(_overlay._textureAlpha, 0, 255, 1, "Texture Alpha", Color.white);
                    _normalScaleSelector = new ValueSelector<float, FloatField>(_overlay._normalScale, 0, 5.0f, 0.1f, "Normal Scale", Color.white);

                    if (gui.kspTextureInfo().isSpecular) _textureAlphaSelector.setLabel("Specularity");
                    else if (gui.kspTextureInfo().isTransparent) _textureAlphaSelector.setLabel("Transparency");
                }
            }
        }
    }
}
