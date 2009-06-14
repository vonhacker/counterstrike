using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using doru.Vectors;

namespace VectorWorld
{
    public class Vector2D
    {
        public double A, B, X, Y;
        public Point dot1 { get { return new Point(X, Y); } }
        public Point dot2 { get { return new Point(X - B, Y + A); } }
        public Vector2D() { A = B = X = Y = 0.0f; }
        public Vector2D(double a, double b) { A = a; B = b; X = Y = 0.0f; }
        public Vector2D(double a, double b, double x, double y) { A = a; B = b; X = x; Y = y; }
        public Vector2D(Point a, Point b) { A = b.Y - a.Y; B = a.X - b.X; X = a.X; Y = a.Y; }
        public Vector2D(Point a, Vector2D v) { A = v.A; B = v.B; X = a.X; Y = a.Y; }
        public Vector2D(Vector2D v) { A = v.A; B = v.B; X = v.X; Y = v.Y; }
        public double distance(Point d) { return (A * d.X + B * d.Y - A * X - B * Y) / (double)Math.Sqrt((double)(A * A + B * B)); }
        public static Vector2D operator - (Vector2D v) { return new Vector2D(- v.A, - v.B, v.X, v.Y); }
        public bool cross(Vector2D l, out Point cross_dot) { return cross(l, out cross_dot, true); }
        public bool cross(Vector2D l, out Point cross_dot, bool f)
        {
            double v = A * l.B - l.A * B;
            if (v == 0.0) { cross_dot = new Point(); return false; }
            bool r = true;
            double c1 = A * X + B * Y;
            double c2 = l.A * l.X + l.B * l.Y;
            double x = (l.B * c1 - B * c2) / v;
            double y = (A * c2 - l.A * c1) / v;
            cross_dot = new Point(x, y);
            double min, max;
            min = Math.Min(X, X - B); max = Math.Max(X, X - B); if (x < min) r = false; if (x > max) r = false;
            min = Math.Min(Y, Y + A); max = Math.Max(Y, Y + A); if (y < min) r = false; if (y > max) r = false;
            if (f)
            {
                min = Math.Min(l.X, l.X - l.B); max = Math.Max(l.X, l.X - l.B); if (x < min) r = false; if (x > max) r = false;
                min = Math.Min(l.Y, l.Y + l.A); max = Math.Max(l.Y, l.Y + l.A); if (y < min) r = false; if (y > max) r = false;
            }
            return r;
        }
        public Vector2D normal() { return new Vector2D(-B, A); }
        public Vector2D rotate(double v)
        {
            double sin_v = (double)Math.Sin((double)v);
            double cos_v = (double)Math.Cos((double)v);
            return new Vector2D(A * cos_v - B * sin_v, A * sin_v + B * cos_v);
        }
        public double angle(Vector2D l) { return (double)Math.Atan2((double)(A * l.B - l.A * B), (double)(A * l.A + B * l.B)); }
        public double length
        {
            get { return (double)Math.Sqrt((double)(A * A + B * B)); }
            set
            {
                if (A == 0.0f) { B = value; return; }
                if (B == 0.0f) { A = value; return; }
                double l = (double)Math.Sqrt((double)(A * A + B * B));
                double kA = A / l;
                double kB = B / l;
                A = value * kA;
                B = value * kB;
            }
        }
        public static bool checkCrossWalls(Vector2D wall, Point lastDot, Point Dot, out Point newDot, double minDistance)
        {
            wall = new Vector2D(
                new Point(wall.dot1.X + .01, wall.dot1.Y + .01),
                new Point(wall.dot2.X, wall.dot2.Y));
            
            Vector2D way = new Vector2D(lastDot, Dot);
            Point cross;
            bool isCross = wall.cross(way, out cross);
            double distance = wall.distance(way.dot2);
            if (isCross || Math.Abs(distance) < minDistance)
            {
                Vector2D n;
                if (isCross)
                    n = new Vector2D(Dot, distance < 0 ? -wall.normal() : wall.normal());
                else
                    n = new Vector2D(Dot, distance < 0 ? wall.normal() : -wall.normal());
                if (wall.cross(n, out cross, false))
                {
                    n = new Vector2D(cross, n);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                } else if (Math.Sqrt(Math.Pow(wall.dot1.X - Dot.X, 2.0f) + Math.Pow(wall.dot1.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot1, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                } else if (Math.Sqrt(Math.Pow(wall.dot2.X - Dot.X, 2.0f) + Math.Pow(wall.dot2.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot2, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                }
            }
            newDot = Dot;
            return false;
        }
        public static Point Fazika(Point pos, Point oldpos, double dist, List<Vector2D> walls)
        {

            Vector2D way = new Vector2D(oldpos, pos);

            Point newDot;
            Point newDotResult = new Point();
            int countCross = 0;
            // Находим стены пересекающиеся с новым положением и предположительные точки


            foreach (Vector2D v in walls)
            {
                if (Vector2D.checkCrossWalls(v, oldpos, pos, out newDot, dist))
                {
                    countCross++;
                    newDotResult.X += newDot.X;
                    newDotResult.Y += newDot.Y;
                }
                if (doru.Vectors.Calculator.DistanceBetweenVectorAndLineSegment((Vector)pos, (Vector)v.dot1, (Vector)v.dot2) < dist/2)
                    return oldpos;
            }
            if (countCross == 1) return newDotResult;
            else if (countCross == 2)
            {
                newDotResult.X /= (double)countCross;
                newDotResult.Y /= (double)countCross;
                pos = newDotResult;

            }
            return pos;
        }
    }

}
