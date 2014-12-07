using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
   namespace IM
   {
       public class BaseTexture : ImageModifier
       {
           static string _displayName = "Base Texture";

           internal string _url = string.Empty;
           internal string _name = string.Empty;
           internal Texture2D _texture = null;
           internal Color32[] _pixels = null;
           internal int _width = 0;
           internal int _height = 0;
           internal bool _normalMap = false;

           private BaseTextureGui _gui;

           ~BaseTexture()
           {
               cleanUp();
           }

           public IM.BaseTexture cloneBaseTexture()
           {
               IM.BaseTexture im = new IM.BaseTexture();
               im._url = _url;
               im._name = _name;
               im._texture = null;
               im._pixels = null;
               im._width = 0;
               im._height = 0;

               return im;
           }

           public void set(string url, bool normalMap)
           {
               _url = url;
               _name = System.IO.Path.GetFileNameWithoutExtension(_url);
               _pixels = null;
               _normalMap = normalMap;

               if (_texture != null)
               {
                   UnityEngine.Object.Destroy(_texture);
                   _texture = null;
               }
           }

           public void setupImage()
           {
               if (_texture == null)
               {
                   _texture = Utils.LoadTextureFromUrl(_url, _normalMap);
                   _width = _texture.width;
                   _height = _texture.height;
                   _pixels = null;
               }

               if (_pixels == null) _pixels = _texture.GetPixels32();
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
               if (Global.Debug3) Utils.Log("draw base texture");

               setupImage();

               image.resizeAndFill(_width, _height, _pixels);
           }

           public override void drawOnImage(ref Image image, BoundingBox boundingBox)
           {
               if (Global.Debug3) Utils.Log("draw base texture");

               setupImage();

               image.resizeAndFill(_width, _height, _pixels, boundingBox);
           }

           public override ImageModifier clone()
           {
               IM.BaseTexture im = new IM.BaseTexture();
               im._url = _url;
               im._texture = null;
               im._pixels = null;
               im._width = 0;
               im._height = 0;

               return im;
           }

           public override void cleanUp()
           {
               if (_texture != null) UnityEngine.Object.Destroy(_texture);

               _pixels = null;
               _texture = null;
               _gui = null;
           }

           public override string displayName()
           {
               return _displayName;
           }

           public override bool locked()
           {
               return true;
           }

           public override ImageModifierGui gui()
           {
               if (_gui == null) _gui = new BaseTextureGui(this);
               return _gui;
           }
       }

       public class BaseTextureGui : ImageModifierGui
       {
           private BaseTexture _baseTexture;
           private Vector2 _scrollPos;

           public BaseTextureGui(BaseTexture baseTexture)
           {
               _baseTexture = baseTexture;
           }

           public override void drawBottom(TextureEditGUI gui)
           {
               GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

               header(gui, "BASE TEXTURE");

               GUILayout.Label("Texture: " + _baseTexture._name);
               GUILayout.Label("URL: " + _baseTexture._url);

               GUILayout.Space(5);

               if (_baseTexture._texture.width > 520 || _baseTexture._texture.height > 520)
               {
                   _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUI.skin.box, GUILayout.Width(520), GUILayout.MaxHeight(520), GUILayout.ExpandWidth(true));
                   GUILayout.Box(_baseTexture._texture, GUI.skin.box, GUILayout.Width(_baseTexture._texture.width), GUILayout.Height(_baseTexture._texture.height));
                   GUILayout.EndScrollView();
               }
               else GUILayout.Box(_baseTexture._texture, GUILayout.Width(_baseTexture._texture.width), GUILayout.Height(_baseTexture._texture.height));

               GUILayout.EndVertical();
           }

           public override void drawRight(TextureEditGUI gui)
           {
           }

           public override void initialise()
           {
               _baseTexture.setupImage();
           }

           public override string buttonText()
           {
               return "Base Texture";
           }
       }
   }
}
