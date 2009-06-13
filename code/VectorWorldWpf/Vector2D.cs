using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace VectorWorld
{
    class Vector2D
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
    }
}
