using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class DoubleVector2
    {
        public double x;
        public double y;

        public DoubleVector2()
        {
            x = 0;
            y = 0;
        }

        public DoubleVector2(double X, double Y)
        {
            x = X;
            y = Y;
        }

        public DoubleVector2(DoubleVector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
    }
}
