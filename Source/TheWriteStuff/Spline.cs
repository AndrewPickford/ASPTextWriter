using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASP
{
    public class Spline
    {
        private DoubleVector2 _start;
        private DoubleVector2 _end;
        private DoubleVector2 _corner;
        private double _minX;
        private double _maxX;
        private double _minY;
        private double _maxY;

        public Spline(DoubleVector2 start, DoubleVector2 end, DoubleVector2 corner)
        {
            _start = new DoubleVector2(start);
            _end = new DoubleVector2(end);
            _corner = new DoubleVector2(corner);
            calculateMinMaxes();
        }

        public Spline(Vertex start, Vertex end, Vertex corner)
        {
            _start = new DoubleVector2(start.x, start.y);
            _end = new DoubleVector2(end.x, end.y);
            _corner = new DoubleVector2(corner.x, corner.y);
            calculateMinMaxes();
        }

        public void addIntersectsAlongY(double y, ref List<double> intersects)
        {
            double bottomPart = 2d * (_start.y + _end.y - 2 * _corner.y);
            double sRoot = 2d * (_corner.y - _start.y);
            sRoot *= sRoot;
            sRoot -= 2d * bottomPart * (_start.y - y);

            if (sRoot >= 0d)
            {
                sRoot = Math.Sqrt(sRoot);
                double topPart = 2d * (_start.y - _corner.y);
                for (int i = -1; i <= 1; i += 2)
                {
                    double f = (topPart + (i * sRoot)) / bottomPart;
                    if (f >= 0d && f <= 1d)
                    {
                        double linearX = _start.x + f * (_corner.x - _start.x);
                        double x = linearX + f * (_corner.x + f * (_end.x - _corner.x) - linearX);
                        intersects.Add(x);
                    }
                }
            }
        }

        private void calculateMinMaxes()
        {
            calculateMaxX();
            calculateMinX();
            calculateMaxY();
            calculateMinY();
        }

        private void calculateMaxX()
        {
            double c = _start.x - _corner.x;
            double d = c + _end.x - _corner.x;
            _maxX = _start.x;

            if (_end.x > _start.x) _maxX = _end.x;

            if (d != 0d)
            {
                double f = c / d;
                if (f > 0d && f < 1d)
                {
                    double x = _start.x - c * f;
                    if (x > _maxX) _maxX = x;
                }
            }
        }

        private void calculateMinX()
        {
            double c = _start.x - _corner.x;
            double d = c + _end.x - _corner.x;
            _minX = _start.x;

            if (_end.x < _start.x) _minX = _end.x;

            if (d != 0d)
            {
                double f = c / d;
                if (f > 0d && f < 1d)
                {
                    double x = _start.x - c * f;
                    if (x < _minX) _minX = x;
                }
            }
        }

        private void calculateMaxY()
        {
            double c = _start.y - _corner.y;
            double d = c + _end.y - _corner.y;
            _maxY = _start.y;

            if (_end.y > _start.y) _maxY = _end.y;

            if (d != 0d)
            {
                double f = c / d;
                if (f > 0d && f < 1d)
                {
                    double y = _start.y - c * f;
                    if (y > _maxY) _maxY = y;
                }
            }
        }

        private void calculateMinY()
        {
            double c = _start.y - _corner.y;
            double d = c + _end.y - _corner.y;
            _minY = _start.y;

            if (_end.y < _start.y) _minY = _end.y;

            if (d != 0d)
            {
                double f = c / d;
                if (f > 0d && f < 1d)
                {
                    double y = _start.y - c * f;
                    if (y < _minY) _minY = y;
                }
            }
        }
    }
}
