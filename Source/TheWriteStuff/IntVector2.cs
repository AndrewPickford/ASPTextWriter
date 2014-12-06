using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class IntVector2
    {
        public int x;
        public int y;

        public IntVector2()
        {
            x = 0;
            y = 0;
        }

        public IntVector2(int X, int Y)
        {
            x = X;
            y = Y;
        }

        public IntVector2(IntVector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
    }
}
