﻿using System;
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

        public bool load(ConfigNode node)
        {
            bool ok = true;
            int c = 0;
            modifiers = new List<ImageModifier>();
          
            ConfigNode[] nodes = node.GetNodes("ASP_IMAGEMODIFIER");

            foreach (ConfigNode n in nodes)
            {
                try
                {
                    ImageModifier im = ImageModifier.CreateFromConfig(n);
                    if (im != null) modifiers.Add(im);
                }
                catch
                {
                    Utils.LogError("Failed to parse imagemodifier {0}", c);
                    ok = false;
                }
                ++c;
            }

            return ok;
        }

        public void drawOnImage(ref Image image, BoundingBox boundingBox)
        {
            for (int i = 0; i < modifiers.Count; ++i)
            {
                try
                {
                    modifiers[i].drawOnImage(ref image, boundingBox);
                }
                catch
                {
                    Utils.LogError("Failed to draw image modifer {0}: {1}", i, modifiers[i].type());
                }
            }
        }

        public void drawOnImage(ref Image image, ref Image normalMapImage, BoundingBox boundingBox)
        {
            for (int i = 0; i < modifiers.Count; ++i)
            {
                try
                {
                    modifiers[i].drawOnImage(ref image, ref normalMapImage, boundingBox);
                }
                catch
                {
                    Utils.LogError("Failed to draw image modifer {0}: {1}", i, modifiers[i].type());
                }
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

        public void guiInit(TextureEditGUI gui)
        {
            foreach (ImageModifier im in modifiers)
            {
                im.gui().initialise(gui);
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
