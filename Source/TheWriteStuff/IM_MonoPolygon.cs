using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ASP
{
    namespace IM
    {
        public abstract class MonoPolygon : Overlay
        {
            protected Color32 _fillColor;
            protected Color32 _edgeColor;
            protected bool _fillShape;
            protected bool _drawEdge;
            protected float _edgeWidth;

            public MonoPolygon() :
                base()
            {
                _fillColor = new Color32(255, 255, 255, 255);
                _edgeColor = new Color32(255, 255, 255, 255);
                _fillShape = true;
                _drawEdge = false;
                _edgeWidth = 1.0f;
                longRight = false;
            }

            protected void loadMonoPolygon(ConfigNode node)
            {
                _fillColor.r = 255;
                _fillColor.g = 255;
                _fillColor.b = 255;
                _fillColor.a = 255;
                _edgeColor.r = 255;
                _fillShape = true;
                _drawEdge = false;
                _edgeWidth = 1.0f;

                if (node.HasValue("fill_shape")) _fillShape = bool.Parse(node.GetValue("fill_shape"));
                if (node.HasValue("fill_red")) _fillColor.r = byte.Parse(node.GetValue("fill_red"));
                if (node.HasValue("fill_green")) _fillColor.g = byte.Parse(node.GetValue("fill_green"));
                if (node.HasValue("fill_blue")) _fillColor.b = byte.Parse(node.GetValue("fill_blue"));
                if (node.HasValue("fill_alpha")) _fillColor.a = byte.Parse(node.GetValue("fill_alpha"));

                if (node.HasValue("draw_edge")) _drawEdge = bool.Parse(node.GetValue("draw_edge"));
                if (node.HasValue("edge_red")) _edgeColor.r = byte.Parse(node.GetValue("edge_red"));
                if (node.HasValue("edge_green")) _edgeColor.g = byte.Parse(node.GetValue("edge_green"));
                if (node.HasValue("edge_blue")) _edgeColor.b = byte.Parse(node.GetValue("edge_blue"));
                if (node.HasValue("edge_alpha")) _edgeColor.a = byte.Parse(node.GetValue("edge_alpha"));
                if (node.HasValue("edge_width")) _edgeWidth = float.Parse(node.GetValue("edge_width"));
            }

            protected void saveMonoPolygon(ConfigNode node)
            {
                saveOverlay(node);

                node.AddValue("fill_shape", _fillShape);
                node.AddValue("fill_red", _fillColor.r);
                node.AddValue("fill_green", _fillColor.g);
                node.AddValue("fill_blue", _fillColor.b);
                node.AddValue("fill_alpha", _fillColor.a);

                node.AddValue("draw_edge", _drawEdge);
                node.AddValue("edge_red", _edgeColor.r);
                node.AddValue("edge_green", _edgeColor.g);
                node.AddValue("edge_blue", _edgeColor.b);
                node.AddValue("edge_alpha", _edgeColor.a);
                node.AddValue("edge_width", _edgeWidth);
            }

            protected void copyFromMonoPolygon(MonoPolygon overlay)
            {
                copyFromOverlay(overlay);

                _fillShape = overlay._fillShape;
                _fillColor = new Color32(overlay._fillColor.r, overlay._fillColor.g, overlay._fillColor.b, overlay._fillColor.a);

                _drawEdge = overlay._drawEdge;
                _edgeColor = new Color32(overlay._edgeColor.r, overlay._edgeColor.g, overlay._edgeColor.b, overlay._edgeColor.a);
                _edgeWidth = overlay._edgeWidth;
            }




            public abstract class MonoPolygonGui : OverlayGui
            {
                private IM.MonoPolygon _overlay;
                private ValueSelector<byte, ByteField> _redFillSelector;
                private ValueSelector<byte, ByteField> _greenFillSelector;
                private ValueSelector<byte, ByteField> _blueFillSelector;
                private ValueSelector<byte, ByteField> _alphaFillSelector;
                private ValueSelector<byte, ByteField> _redEdgeSelector;
                private ValueSelector<byte, ByteField> _greenEdgeSelector;
                private ValueSelector<byte, ByteField> _blueEdgeSelector;
                private ValueSelector<byte, ByteField> _alphaEdgeSelector;


                public MonoPolygonGui(IM.MonoPolygon overlay) :
                    base(overlay)
                {
                    _overlay = overlay;
                }

                protected void drawBottomMonoPolygon(TextureEditGUI gui)
                {
                    bool oldFillShape = _overlay._fillShape;
                    bool oldDrawEdge = _overlay._drawEdge;

                    GUILayout.BeginHorizontal();
                    positionSelector(gui, ref _xPositionSelector, ref _yPositionSelector);
                    GUILayout.Space(5);

                    GUILayout.BeginVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();
                    _overlay._fillShape = GUILayout.Toggle(_overlay._fillShape, "");
                    GUILayout.Label("Fill", gui.smallHeader);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    colorSelector(gui, ref _redFillSelector, ref _greenFillSelector, ref _blueFillSelector, ref _alphaFillSelector);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.Space(15f);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();
                    _overlay._drawEdge = GUILayout.Toggle(_overlay._drawEdge, "");
                    GUILayout.Label("Edge", gui.smallHeader);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    colorSelector(gui, ref _redEdgeSelector, ref _greenEdgeSelector, ref _blueEdgeSelector, ref _alphaEdgeSelector);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.EndVertical();

                    GUILayout.Space(5);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    rotationSelector(gui, ref _overlay._rotation, ref _overlay._mirror);
                    GUILayout.Space(10f);
                    blendMethodSelector(gui, ref _overlay._blendMethod);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

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
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_overlay._position.x != _xPositionSelector.value())
                    {
                        _overlay._position.x = _xPositionSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._position.y != _yPositionSelector.value())
                    {
                        _overlay._position.y = _yPositionSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._normalScale != _normalScaleSelector.value()) _overlay._normalScale = _normalScaleSelector.value();
                    if (_overlay._textureAlpha != _textureAlphaSelector.value()) _overlay._textureAlpha = _textureAlphaSelector.value();

                    if (oldFillShape != _overlay._fillShape) gui.setRemakePreview();
                    if (_overlay._fillColor.r != _redFillSelector.value())
                    {
                        _overlay._fillColor.r = _redFillSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._fillColor.g != _greenFillSelector.value())
                    {
                        _overlay._fillColor.g = _greenFillSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._fillColor.b != _blueFillSelector.value())
                    {
                        _overlay._fillColor.b = _blueFillSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._fillColor.a != _alphaFillSelector.value())
                    {
                        _overlay._fillColor.a = _alphaFillSelector.value();
                        gui.setRemakePreview();
                    }

                    if (oldDrawEdge != _overlay._drawEdge) gui.setRemakePreview();
                    if (_overlay._edgeColor.r != _redEdgeSelector.value())
                    {
                        _overlay._edgeColor.r = _redEdgeSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._edgeColor.g != _greenEdgeSelector.value())
                    {
                        _overlay._edgeColor.g = _greenEdgeSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._edgeColor.b != _blueEdgeSelector.value())
                    {
                        _overlay._edgeColor.b = _blueEdgeSelector.value();
                        gui.setRemakePreview();
                    }

                    if (_overlay._edgeColor.a != _alphaEdgeSelector.value())
                    {
                        _overlay._edgeColor.a = _alphaEdgeSelector.value();
                        gui.setRemakePreview();
                    }
                }

                protected void initialiseMonoOverlay(TextureEditGUI gui)
                {
                    initialiseOverlay(gui);

                    _redFillSelector = new ValueSelector<byte, ByteField>(_overlay._fillColor.r, 0, 255, 1, "Red", Color.red);
                    _greenFillSelector = new ValueSelector<byte, ByteField>(_overlay._fillColor.g, 0, 255, 1, "Green", Color.green);
                    _blueFillSelector = new ValueSelector<byte, ByteField>(_overlay._fillColor.b, 0, 255, 1, "Blue", Color.blue);
                    _alphaFillSelector = new ValueSelector<byte, ByteField>(_overlay._fillColor.a, 0, 255, 1, "Alpha", Color.white);

                    _redEdgeSelector = new ValueSelector<byte, ByteField>(_overlay._edgeColor.r, 0, 255, 1, "Red", Color.red);
                    _greenEdgeSelector = new ValueSelector<byte, ByteField>(_overlay._edgeColor.g, 0, 255, 1, "Green", Color.green);
                    _blueEdgeSelector = new ValueSelector<byte, ByteField>(_overlay._edgeColor.b, 0, 255, 1, "Blue", Color.blue);
                    _alphaEdgeSelector = new ValueSelector<byte, ByteField>(_overlay._edgeColor.a, 0, 255, 1, "Alpha", Color.white);
                }
            }
        }
    }
}