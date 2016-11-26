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
        public double minX { get; private set; }
        public double maxX { get; private set; }
        public double minY { get; private set; }
        public double maxY { get; private set; }

        public Polygon()
        {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            splines = new List<Spline>();
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
            calculateMinMaxes();
        }

        public void align()
        {
            calculateMinMaxes();
            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i].x = vertices[i].x - minX + 2d;
                vertices[i].y = vertices[i].y - minY + 2d;
            }
            calculateMinMaxes();
            if (vertices.Count >= 3) calculateEdges();
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
            minX = vertices[0].x;
            maxX = minX;
            minY = vertices[0].y;
            maxY = minY;

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (vertices[i].x < minX) minX = vertices[i].x;
                if (vertices[i].x > maxX) maxX = vertices[i].x;
                if (vertices[i].y < minY) minY = vertices[i].y;
                if (vertices[i].y > maxY) maxY = vertices[i].y;
            }
        }
    }
}