using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class ImageModifiers
    {
        public List<ImageModifier> modifiers {get; private set; }

        public ImageModifiers()
        {
            modifiers = new List<ImageModifier>();
        }

        public ImageModifier findFirst<T>()
        {
            foreach (ImageModifier im in modifiers)
            {
                if (im is T) return im;
            }

            return null;
        }

        public void add(ImageModifier im)
        {
            modifiers.Add(im);
        }

        public void removeAt(int i)
        {
            modifiers.RemoveAt(i);
        }

        public void insert(int index, ImageModifier im)
        {
            modifiers.Insert(index, im);
        }

        public void move(int from, int to)
        {
            ImageModifier im = modifiers[from];
            modifiers.Insert(to, im);
            modifiers.RemoveAt(from);
        }

        public void save(ConfigNode node)
        {
            foreach (ImageModifier im in modifiers)
            {
                ConfigNode n = new ConfigNode("ASP_IMAGEMODIFIER");
                im.save(n);
                node.AddNode(n);
            }
        }

        public void load(ConfigNode node)
        {
            modifiers = new List<ImageModifier>();
          
            ConfigNode[] nodes = node.GetNodes("ASP_IMAGEMODIFIER");

            foreach (ConfigNode n in nodes)
            {
                ImageModifier im = ImageModifier.CreateFromConfig(n);
                modifiers.Add(im);
            }
        }

        public void drawOnImage(ref Image image)
        {
            foreach (ImageModifier im in modifiers)
            {
                im.drawOnImage(ref image);
            }
        }

        public void drawOnImage(ref Image image, BoundingBox boundingBox)
        {
            foreach (ImageModifier im in modifiers)
            {
                im.drawOnImage(ref image, boundingBox);
            }
        }

        public ImageModifiers clone()
        {
            ImageModifiers newIMs = new ImageModifiers();

            foreach (ImageModifier im in modifiers)
            {
                ImageModifier newIM = im.clone();
                newIMs.add(newIM);
            }

            return newIMs;
        }

        public void cleanUp()
        {
            foreach (ImageModifier im in modifiers)
                im.cleanUp();
        }

        public void guiInit()
        {
            foreach (ImageModifier im in modifiers)
            {
                im.gui().initialise();
            }
        }

        public void swap(int first, int second)
        {
            if (first < 0 || second < 0 || first >= modifiers.Count || second >= modifiers.Count)
            {
                Utils.LogError("swap on of range first={0}, second={1}", first, second);
                throw new ArgumentOutOfRangeException("swap out of range");
            }

            ImageModifier im = modifiers[first];
            modifiers[first] = modifiers[second];
            modifiers[second] = im;
        }
    }
}
