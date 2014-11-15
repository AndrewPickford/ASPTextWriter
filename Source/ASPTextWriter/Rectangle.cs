﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class Rectangle
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public int wx()
        {
            return x + w;
        }

        public int hy()
        {
            return h + y;
        }

        public Rectangle()
        {
            x = 0;
            y = 0;
            w = 0;
            h = 0;
        }

        public Rectangle(int xx, int yy, int ww, int hh)
        {
            x = xx;
            y = yy;
            w = ww;
            h = hh;
        }
    }
}
