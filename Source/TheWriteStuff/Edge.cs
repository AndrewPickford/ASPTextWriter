using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class Edge
    {
        public Vertex start { get; private set; }
        public Vertex end { get; private set; }
        private double _my;
        private double _cy;

        public Edge(Vertex startVertex, Vertex endVertex)
        {
            start = new Vertex(startVertex);
            end = new Vertex(endVertex);
            preCalculate();
        }

        public void addIntersectsAlongY(double y, ref List<double> intersects)
        {
            if (betweenY(y)) intersects.Add(y * _my + _cy);
        }

        public bool betweenY(double y)
        {
            if ((y > start.y && y <= end.y) || (y > end.y && y <= start.y)) return true;
            else return false;
        }

        public void round()
        {
            if (start.rounding == 0 && end.rounding == 0) return;

            Vertex newStart = new Vertex(start);
            Vertex newEnd = new Vertex(end);

            if (start.rounding > 0) newStart = shorten(start, end, Math.Min(0.495, start.rounding / 200d));
            if (end.rounding > 0) newEnd = shorten(end, start, Math.Min(0.495, end.rounding / 200d));

            start = newStart;
            end = newEnd;
            preCalculate();
        }

        private void preCalculate()
        {
            if (start.y == end.y) _my = 0;
            else _my = (start.x - end.x) / (start.y - end.y);

            _cy = start.x - start.y * _my;
        }

        static Vertex shorten(Vertex a, Vertex b, double f)
        {
            Vertex v = new Vertex(a);
            double dx = a.x - b.x;
            double dy = a.y - b.y;

            v.x = a.x - f * dx;
            v.y = a.y - f * dy;

            return v;
        }
    }
}
