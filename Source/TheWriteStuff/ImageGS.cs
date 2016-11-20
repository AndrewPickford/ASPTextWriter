using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class ImageGS
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public int length { get; private set; }
        public byte[] pixels { get; private set; }

        public ImageGS()
        {
            width = 0;
            height = 0;
            length = 0;
        }

        public ImageGS(int w, int h)
        {
            resize(w, h);
        }

        public ImageGS(IntVector2 size)
        {
            resize(size);
        }

        public ImageGS(ImageGS image)
        {
            resize(image.width, image.height);
            for (int i = 0; i < image.length; ++i)
                pixels[i] = image.pixels[i];
        }

        public void resize(int w, int h)
        {
            width = w;
            height = h;
            length = width * height;

            pixels = new byte[length];
        }

        public void resize(IntVector2 size)
        {
            resize(size.x, size.y);
        }

        public void clear()
        {
            for (int i = 0; i < length; ++i)
                pixels[i] = 0;
        }

        public void drawCircleCentered(float radius, float thickness)
        {
            float x = width / 2f;
            float y = height / 2f;
            drawCircle(x, y, radius, thickness);
        }

        public void drawCircle(float x, float y, float radius, float thickness)
        {

        }
    }
}
