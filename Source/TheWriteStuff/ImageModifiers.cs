using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ImageModifiers
    {
        private List<ImageModifier> _modifiers;

        public ImageModifiers()
        {
            _modifiers = new List<ImageModifier>();
        }

        public ImageModifier findFirst<T>()
        {
            foreach (ImageModifier im in _modifiers)
            {
                if (im is T) return im;
            }

            return null;
        }

        public void add(ImageModifier im)
        {
            _modifiers.Add(im);
        }

        public void insert(int index, ImageModifier im)
        {
            _modifiers.Insert(index, im);
        }

        public void save(ConfigNode node)
        {
            foreach (ImageModifier im in _modifiers)
            {
                ConfigNode n = new ConfigNode("ASP_IMAGEMODIFIER");
                im.save(n);
                node.AddNode(n);
            }
        }

        public void load(ConfigNode node)
        {
            _modifiers = new List<ImageModifier>();
          
            ConfigNode[] nodes = node.GetNodes("ASP_IMAGEMODIFIER");

            foreach (ConfigNode n in nodes)
            {
                ImageModifier im = ImageModifier.CreateFromConfig(n);
                _modifiers.Add(im);
            }
        }

        public void drawOnImage(ref Image image)
        {
            foreach (ImageModifier im in _modifiers)
            {
                im.drawOnImage(ref image);
            }
        }

        public ImageModifiers clone()
        {
            ImageModifiers newIMs = new ImageModifiers();

            foreach (ImageModifier im in _modifiers)
            {
                ImageModifier newIM = im.clone();
                newIMs.add(im);
            }

            return newIMs;
        }

        public void cleanUp()
        {
            foreach (ImageModifier im in _modifiers)
                im.cleanUp();
        }
    }
}
