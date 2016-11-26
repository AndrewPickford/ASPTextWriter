using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class Vertex
    {
        public double x;
        public double y;
        public int rounding;

        public Vertex()
        {
            x = 0;
            y = 0;
            rounding = 0;
        }

        public Vertex(double pos_x, double pos_y, int rounding_amount)
        {
            x = pos_x;
            y = pos_y;
            rounding = rounding_amount;
        }

        public Vertex(Vertex v)
        {
            x = v.x;
            y = v.y;
            rounding = v.rounding;
        }
    }
}
