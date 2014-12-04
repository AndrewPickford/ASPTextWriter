using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public abstract class ImageModifier
    {
        public static ImageModifier CreateFromConfig(ConfigNode node)
        {
            ImageModifier imageModifier = null;
            string type = node.GetValue("type");


            if (type == "text") imageModifier = new IM.Text();
            if (type == "base_texture") imageModifier = new IM.BaseTexture();

            if (imageModifier == null) throw new ArgumentException("unknown image modifier");
            else
            {
                imageModifier.load(node);
                return imageModifier;
            }
        }

        public abstract void save(ConfigNode node);
        public abstract void load(ConfigNode node);
        public abstract void drawOnImage(ref Image image);
        public abstract ImageModifier clone();
        public abstract void cleanUp();
    }

    namespace IM
    {
        public class BaseTexture : ImageModifier
        {
            private string _url = string.Empty;
            private Texture2D _texture = null;
            private Color32[] _pixels = null;
            private int _width = 0;
            private int _height = 0;

            public void set(string url)
            {
                _url = url;
                _pixels = null;
                _width = _texture.width;
                _height = _texture.height;

                if (_texture != null)
                {
                    UnityEngine.Object.Destroy(_texture);
                    _texture = null;
                }
            }

            public override void save(ConfigNode node)
            {
                node.AddValue("type", "base_texture");
            }

            public override void load(ConfigNode node)
            {
            }

            public override void drawOnImage(ref Image image)
            {
                if (_texture == null)
                {
                    _texture = Utils.LoadTextureFromUrl(_url, false);
                    _pixels = null;
                }

                if (_pixels == null) _pixels = _texture.GetPixels32();


                image.resizeAndFill(_width, _height, _pixels);
            }

            public override ImageModifier clone()
            {
                IM.BaseTexture im = new IM.BaseTexture();
                im._url = _url;
                im._texture = null;
                im._pixels = null;
                im._width = _width;
                im._height = _height;

                return im;
            }

            public override void cleanUp()
            {
                if (_texture != null) UnityEngine.Object.Destroy(_texture);

                _pixels = null;
                _texture = null;
            }
        }


        public class Text : ImageModifier
        {
            private string _text = string.Empty;
            private string _fontName = "CAPSMALL_CLEAN";
            private int _fontSize = 32;
            private int _x = 0;
            private int _y = 0;
            private bool _mirror = false;
            private int _red = 0;
            private int _green = 0;
            private int _blue = 0;
            private int _alpha = 255;
            private AlphaOption _alphaOption = AlphaOption.USE_TEXTURE;
            private float _normalScale = 2.0f;
            private NormalOption _normalOption = NormalOption.USE_BACKGROUND;
            private BlendMethod _blendMethod = BlendMethod.RGB;
            private TextDirection _textDirection = TextDirection.LEFT_RIGHT;

            public Text()
            {
            }

            public override void load(ConfigNode node)
            {
                _text = string.Empty;
                _fontName = "CAPSMALL_CLEAN";
                _fontSize = 32;
                _x = 0;
                _y = 0;
                _mirror = false;
                _red = 0;
                _green = 0;
                _blue = 0;
                _alpha = 255;
                _alphaOption = AlphaOption.USE_TEXTURE;
                _normalScale = 2.0f;
                _normalOption = NormalOption.USE_BACKGROUND;
                _blendMethod = BlendMethod.RGB;
                _textDirection = TextDirection.LEFT_RIGHT;

                if (node.HasValue("text")) _text = node.GetValue("text");
                if (node.HasValue("fontName")) _fontName = node.GetValue("fontName");
                if (node.HasValue("fontSize")) _fontSize = int.Parse(node.GetValue("fontSize"));
                if (node.HasValue("x")) _x = int.Parse(node.GetValue("x"));
                if (node.HasValue("y")) _y = int.Parse(node.GetValue("y"));
                if (node.HasValue("mirror")) _mirror = bool.Parse(node.GetValue("mirror"));
                if (node.HasValue("red")) _red = int.Parse(node.GetValue("red"));
                if (node.HasValue("green")) _green = int.Parse(node.GetValue("green"));
                if (node.HasValue("blue")) _blue = int.Parse(node.GetValue("blue"));
                if (node.HasValue("alpha")) _alpha = int.Parse(node.GetValue("alpha"));
                if (node.HasValue("alphaOption")) _alphaOption = (AlphaOption)ConfigNode.ParseEnum(typeof(AlphaOption), node.GetValue("alphaOption"));
                if (node.HasValue("normalScale")) _normalScale = int.Parse(node.GetValue("normalScale"));
                if (node.HasValue("normalOption")) _normalOption = (NormalOption)ConfigNode.ParseEnum(typeof(NormalOption), node.GetValue("normalOption"));
                if (node.HasValue("blendMethod")) _blendMethod = (BlendMethod)ConfigNode.ParseEnum(typeof(BlendMethod), node.GetValue("blendMethod"));
                if (node.HasValue("textDirection")) _textDirection = (TextDirection)ConfigNode.ParseEnum(typeof(TextDirection), node.GetValue("textDirection"));
            }

            public override void save(ConfigNode node)
            {
                node.AddValue("type", "text");
                node.AddValue("text", _text);
                node.AddValue("x", _x);
                node.AddValue("y", _y);
                node.AddValue("mirror", _mirror);
                node.AddValue("red", _red);
                node.AddValue("green", _green);
                node.AddValue("blue", _blue);
                node.AddValue("alpha", _alpha);
                node.AddValue("alphaOption", ConfigNode.WriteEnum(_alphaOption));
                node.AddValue("normalScale", _normalScale);
                node.AddValue("normalOption", ConfigNode.WriteEnum(_normalOption));
                node.AddValue("blendMethod", ConfigNode.WriteEnum(_blendMethod));
                node.AddValue("textDirection", ConfigNode.WriteEnum(_textDirection));
            }

            public override void drawOnImage(ref Image image)
            {
            }

            public override ImageModifier clone()
            {
                IM.Text im = new IM.Text();

                im._text = _text;
                im._fontName = _fontName;
                im._fontSize = _fontSize;
                im._x = _x;
                im._y = _y;
                im._mirror = _mirror;
                im._red = _red;
                im._green = _green;
                im._blue = _blue;
                im._alpha = _alpha;
                im._alphaOption = _alphaOption;
                im._normalScale = _normalScale;
                im._normalOption = _normalOption;
                im._blendMethod = _blendMethod;
                im._textDirection = _textDirection;

                return im;
            }

            public override void cleanUp()
            {
            }
        }
    }
}
