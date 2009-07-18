using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace doru
{
    namespace Vectors
    {
        public class DLine2
        {
            public Vector _p1; public Vector _p2; public Vector _cpoint;
        }

        public struct Vector
        {
            public static implicit operator Vector(Point _Vector2)
            {
                return new Vector(_Vector2.X, _Vector2.Y);
            }
            public static implicit operator Point(Vector _Vector2)
            {
                return new Point(_Vector2.X, _Vector2.Y);
            }
            public double X;
            public double Y;
            public Vector(double x, double y) { X = x; Y = y; }

            public override bool Equals(object obj)
            {
                Vector other = (Vector)obj;
                if (this.X == other.X)
                {
                    return (this.Y == other.Y);
                }
                return false;
            }


            public override int GetHashCode()
            {
                return (this.X.GetHashCode() + this.Y.GetHashCode());
            }

            public void Normalize()
            {
                double num2 = (this.X * this.X) + (this.Y * this.Y);
                double num = 1f / ((double)Math.Sqrt((double)num2));
                this.X *= num;
                this.Y *= num;
            }

            public static Vector Normalize(Vector value)
            {
                Vector vector;
                double num2 = (value.X * value.X) + (value.Y * value.Y);
                double num = 1f / ((double)Math.Sqrt((double)num2));
                vector.X = value.X * num;
                vector.Y = value.Y * num;
                return vector;
            }

            public static void Normalize(ref Vector value, out Vector result)
            {
                double num2 = (value.X * value.X) + (value.Y * value.Y);
                double num = 1f / ((double)Math.Sqrt((double)num2));
                result.X = value.X * num;
                result.Y = value.Y * num;
            }

            public static Vector Add(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X + value2.X;
                vector.Y = value1.Y + value2.Y;
                return vector;
            }

            public static void Add(ref Vector value1, ref Vector value2, out Vector result)
            {
                result.X = value1.X + value2.X;
                result.Y = value1.Y + value2.Y;
            }

            public static Vector Subtract(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X - value2.X;
                vector.Y = value1.Y - value2.Y;
                return vector;
            }

            public static void Subtract(ref Vector value1, ref Vector value2, out Vector result)
            {
                result.X = value1.X - value2.X;
                result.Y = value1.Y - value2.Y;
            }

            public static Vector Multiply(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X * value2.X;
                vector.Y = value1.Y * value2.Y;
                return vector;
            }

            public static void Multiply(ref Vector value1, ref Vector value2, out Vector result)
            {
                result.X = value1.X * value2.X;
                result.Y = value1.Y * value2.Y;
            }

            public static Vector Multiply(Vector value1, double scaleFactor)
            {
                Vector vector;
                vector.X = value1.X * scaleFactor;
                vector.Y = value1.Y * scaleFactor;
                return vector;
            }

            public static void Multiply(ref Vector value1, double scaleFactor, out Vector result)
            {
                result.X = value1.X * scaleFactor;
                result.Y = value1.Y * scaleFactor;
            }

            public static Vector Divide(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X / value2.X;
                vector.Y = value1.Y / value2.Y;
                return vector;
            }

            public static void Divide(ref Vector value1, ref Vector value2, out Vector result)
            {
                result.X = value1.X / value2.X;
                result.Y = value1.Y / value2.Y;
            }

            public static Vector Divide(Vector value1, double divider)
            {
                Vector vector;
                double num = 1f / divider;
                vector.X = value1.X * num;
                vector.Y = value1.Y * num;
                return vector;
            }

            public static void Divide(ref Vector value1, double divider, out Vector result)
            {
                double num = 1f / divider;
                result.X = value1.X * num;
                result.Y = value1.Y * num;
            }

            public static Vector operator -(Vector value)
            {
                Vector vector;
                vector.X = -value.X;
                vector.Y = -value.Y;
                return vector;
            }

            public static bool operator ==(Vector value1, Vector value2)
            {
                if (value1.X == value2.X)
                {
                    return (value1.Y == value2.Y);
                }
                return false;
            }

            public static bool operator !=(Vector value1, Vector value2)
            {
                if (value1.X == value2.X)
                {
                    return (value1.Y != value2.Y);
                }
                return true;
            }

            public static Vector operator +(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X + value2.X;
                vector.Y = value1.Y + value2.Y;
                return vector;
            }

            public static Vector operator -(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X - value2.X;
                vector.Y = value1.Y - value2.Y;
                return vector;
            }

            public static Vector operator *(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X * value2.X;
                vector.Y = value1.Y * value2.Y;
                return vector;
            }

            public static Vector operator *(Vector value, double scaleFactor)
            {
                Vector vector;
                vector.X = value.X * scaleFactor;
                vector.Y = value.Y * scaleFactor;
                return vector;
            }

            public static Vector operator *(double scaleFactor, Vector value)
            {
                Vector vector;
                vector.X = value.X * scaleFactor;
                vector.Y = value.Y * scaleFactor;
                return vector;
            }

            public static Vector operator /(Vector value1, Vector value2)
            {
                Vector vector;
                vector.X = value1.X / value2.X;
                vector.Y = value1.Y / value2.Y;
                return vector;
            }

            public static Vector operator /(Vector value1, double divider)
            {
                Vector vector;
                double num = 1f / divider;
                vector.X = value1.X * num;
                vector.Y = value1.Y * num;
                return vector;
            }

            public static double Distance(Vector v1, Vector v2)
            {
                return Calculator.Distance(v1, v2);
            }
        }
    }
}
