using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VectorWorld
{
    class Vector2D
    {
        public float A, B, X, Y;
        public PointF dot1 { get { return new PointF(X, Y); } }
        public PointF dot2 { get { return new PointF(X - B, Y + A); } }
        public Vector2D() { A = B = X = Y = 0.0f; }
        public Vector2D(float a, float b) { A = a; B = b; X = Y = 0.0f; }
        public Vector2D(float a, float b, float x, float y) { A = a; B = b; X = x; Y = y; }
        public Vector2D(PointF a, PointF b) { A = b.Y - a.Y; B = a.X - b.X; X = a.X; Y = a.Y; }
        public Vector2D(PointF a, Vector2D v) { A = v.A; B = v.B; X = a.X; Y = a.Y; }
        public Vector2D(Vector2D v) { A = v.A; B = v.B; X = v.X; Y = v.Y; }
        public float distance(PointF d) { return (A * d.X + B * d.Y - A * X - B * Y) / (float)Math.Sqrt((double)(A * A + B * B)); }
        public static Vector2D operator - (Vector2D v) { return new Vector2D(- v.A, - v.B, v.X, v.Y); }
        public bool cross(Vector2D l, out PointF cross_dot) { return cross(l, out cross_dot, true); }
        public bool cross(Vector2D l, out PointF cross_dot, bool f)
        {
            float v = A * l.B - l.A * B;
            if (v == 0.0) { cross_dot = new PointF(); return false; }
            bool r = true;
            float c1 = A * X + B * Y;
            float c2 = l.A * l.X + l.B * l.Y;
            float x = (l.B * c1 - B * c2) / v;
            float y = (A * c2 - l.A * c1) / v;
            cross_dot = new PointF(x, y);
            float min, max;
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
        public Vector2D rotate(float v)
        {
            float sin_v = (float)Math.Sin((double)v);
            float cos_v = (float)Math.Cos((double)v);
            return new Vector2D(A * cos_v - B * sin_v, A * sin_v + B * cos_v);
        }
        public float angle(Vector2D l) { return (float)Math.Atan2((double)(A * l.B - l.A * B), (double)(A * l.A + B * l.B)); }
        public float length
        {
            get { return (float)Math.Sqrt((double)(A * A + B * B)); }
            set
            {
                if (A == 0.0f) { B = value; return; }
                if (B == 0.0f) { A = value; return; }
                float l = (float)Math.Sqrt((double)(A * A + B * B));
                float kA = A / l;
                float kB = B / l;
                A = value * kA;
                B = value * kB;
            }
        }
    }
}
