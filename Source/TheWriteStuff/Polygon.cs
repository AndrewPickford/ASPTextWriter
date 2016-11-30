using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class Polygon
    {
        public List<Vertex> vertices { get; private set; }
        public List<Edge> edges { get; private set; }
        public List<Spline> splines { get; private set; }
        public DoubleVector2 min { get; private set; }
        public DoubleVector2 max { get; private set; }

        public Polygon()
        {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            splines = new List<Spline>();
            min = new DoubleVector2(0, 0);
            max = new DoubleVector2(0, 0);
        }

        public void addVertex(double x, double y, int rounding)
        {
            addVertex(new Vertex(x, y, rounding));
        }

        public void addVertex(Vertex v)
        {
            vertices.Add(new Vertex(v));
        }

        public void addVertices(List<Vertex> newVertices)
        {
            for (int i = 0; i < newVertices.Count; ++i)
                addVertex(newVertices[i]);
        }

        public void addVertices(Vertex[] newVertices)
        {
            for (int i = 0; i < newVertices.Length; ++i)
                addVertex(newVertices[i]);
        }

        public void scale(double scale)
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i].x *= scale;
                vertices[i].y *= scale;
            }
        }

        public void align()
        {
            calculateMinMaxes();
            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i].x = vertices[i].x - (int) Math.Floor(min.x) + 2d;
                vertices[i].y = vertices[i].y - (int) Math.Floor(min.y) + 2d;
            }
            calculateMinMaxes();
            if (vertices.Count >= 3) calculateEdges();
        }

        public void rotate(double rotation)
        {
            double cr = Math.Cos(2 * Math.PI * rotation / 360);
            double sr = Math.Sin(2 * Math.PI * rotation / 360);

            for (int i = 0; i < vertices.Count; ++i)
            {
                double x = vertices[i].x * cr - vertices[i].y * sr;
                double y = vertices[i].x * sr + vertices[i].y * cr;
                vertices[i].x = x;
                vertices[i].y = y;
            }
        }

        public void mirrorX()
        {
            for (int i = 0; i < vertices.Count; ++i)
                vertices[i].x = -vertices[i].x;
        }

        public void mirrorY()
        {
            for (int i = 0; i < vertices.Count; ++i)
                vertices[i].y = -vertices[i].y;
        }

        public void close()
        {
            if (vertices.Count >= 3) calculateEdges();
            calculateMinMaxes();
        }

        public List<double> intersectsAlongY(double y)
        {
            List<double> intersects = new List<double>(10);

            for (int i = 0; i < edges.Count; ++i)
                edges[i].addIntersectsAlongY(y, ref intersects);

            for (int i = 0; i < splines.Count; ++i)
                splines[i].addIntersectsAlongY(y, ref intersects);

            intersects.Sort();
            return intersects;
        }

        private void calculateEdges()
        {
            edges.Clear();
            splines.Clear();

            for (int i = 0; i < vertices.Count; ++i)
            {
                int j = i + 1;
                if (j == vertices.Count) j = 0;

                edges.Add(new Edge(vertices[i], vertices[j]));
            }

            for (int i = 0; i < edges.Count; ++i)
                edges[i].round();

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (vertices[i].rounding > 0)
                {
                    int j = vertices.Count - 1;
                    if (i > 0) j = i - 1;
                    splines.Add(new Spline(edges[i].start, edges[j].end, vertices[i]));
                }
            }
        }

        private void calculateMinMaxes()
        {
            min.x = vertices[0].x;
            max.x = min.x;
            min.y = vertices[0].y;
            max.y = min.y;

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (vertices[i].x < min.x) min.x = vertices[i].x;
                if (vertices[i].x > max.x) max.x = vertices[i].x;
                if (vertices[i].y < min.y) min.y = vertices[i].y;
                if (vertices[i].y > max.y) max.y = vertices[i].y;
            }
        }
    }
}