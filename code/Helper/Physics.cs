using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using doru.Vectors;
namespace doru
{
	namespace Vectors
	{
		public static class VectorExtensions
		{
			public static void Multiply(ref Vector value1, double scaleFactor, out Vector result)
			{
				result = new Vector();
				result.X = value1.X * scaleFactor;
				result.Y = value1.Y * scaleFactor;
			}
			public static Vector Multiply(Vector value1, Vector value2)
			{
				Vector vector = new Vector();
				vector.X = value1.X * value2.X;
				vector.Y = value1.Y * value2.Y;
				return vector;
			}
			public static Vector Multiply(Vector value1, double scaleFactor)
			{
				Vector vector = new Vector();
				vector.X = value1.X * scaleFactor;
				vector.Y = value1.Y * scaleFactor;
				return vector;
			}
			public static void Multiply(ref Vector value1, ref Vector value2, out Vector result)
			{
				result = new Vector();
				result.X = value1.X * value2.X;
				result.Y = value1.Y * value2.Y;
			}
			public static double Dot(Vector value1, Vector value2)
			{
				return ((value1.X * value2.X) + (value1.Y * value2.Y));
			}

			public static void Dot(ref Vector value1, ref Vector value2, out double result)
			{
				result = (value1.X * value2.X) + (value1.Y * value2.Y);
			}
		}

		//public class Calculator
		//{

		//    public static float DistanceBetweenPointAndLineSegment(Vector point, Vector lineEndPoint1, Vector lineEndPoint2, out Vector pointOnLine)
		//    {
		//        Vector v = Vector.Subtract(lineEndPoint2, lineEndPoint1);
		//        Vector w = Vector.Subtract(point, lineEndPoint1);

		//        float c1 = Vector.Dot(w, v);
		//        if (c1 <= 0)
		//        {
		//            pointOnLine = lineEndPoint1;
		//            return DistanceBetweenPointAndPoint(point, lineEndPoint1);
		//        }

		//        float c2 = Vector.Dot(v, v);

		//        if (c2 <= c1)
		//        {
		//            pointOnLine = lineEndPoint2;
		//            return DistanceBetweenPointAndPoint(point, lineEndPoint2);
		//        }

		//        float b = c1 / c2;
		//        pointOnLine = Vector.Add(lineEndPoint1, Vector.Multiply(v, b));
		//        return DistanceBetweenPointAndPoint(point, pointOnLine);
		//    }
		//}

		public static class Calculator
		{

			public static double Length(this Vector p)
			{
				double num = (p.X * p.X) + (p.Y * p.Y);
				return Math.Sqrt((double)num);

			}
			public const double TwoPi = 6.28318531f;
			public const double DegreesToRadiansRatio = 57.29577957855f;
			public const double RadiansToDegreesRatio = 1f / 57.29577957855f;
			private static Random random = new Random();

			public static double Sin(double angle)
			{
				return (double)Math.Sin((double)angle);
			}

			public static double Cos(double angle)
			{
				return (double)Math.Cos((double)angle);
			}

			public static double ACos(double value)
			{
				return (double)Math.Acos((double)value);
			}

			public static double ATan2(double y, double x)
			{
				return (double)Math.Atan2((double)y, (double)x);
			}

			//performs bilinear interpolation of a Vector
			public static double BiLerp(Vector Vector, Vector min, Vector max, double value1, double value2, double value3, double value4, double minValue, double maxValue)
			{
				double x = Vector.X;
				double y = Vector.Y;
				double value;

				x = MathHelper.Clamp(x, min.X, max.X);
				y = MathHelper.Clamp(y, min.Y, max.Y);

				double xRatio = (x - min.X) / (max.X - min.X);
				double yRatio = (y - min.Y) / (max.Y - min.Y);

				double top = MathHelper.Lerp(value1, value4, xRatio);
				double bottom = MathHelper.Lerp(value2, value3, xRatio);

				value = MathHelper.Lerp(top, bottom, yRatio);
				value = MathHelper.Clamp(value, minValue, maxValue);
				return value;
			}

			public static double Clamp(double value, double low, double high)
			{
				return Math.Max(low, Math.Min(value, high));
			}

			public static double DistanceBetweenVectorAndVector(Vector Vector1, Vector Vector)
			{
				Vector v = Vector.Subtract(Vector1, Vector);
				return v.Length();
			}
			public static double DistanceBetweenVectorAndLineSegment(Vector Vector, Vector lineEndVector1, Vector lineEndVector)
			{
				Vector VectorOnLine;
				return DistanceBetweenVectorAndLineSegment(Vector, lineEndVector1, lineEndVector, out VectorOnLine);
			}
			public static double DistanceBetweenVectorAndLineSegment(Vector Vector, Vector lineEndVector1, Vector lineEndVector, out Vector VectorOnLine)
			{
				Vector v = Vector.Subtract(lineEndVector, lineEndVector1);
				Vector w = Vector.Subtract(Vector, lineEndVector1);

				double c1 = VectorExtensions.Dot(w, v);
				if (c1 <= 0)
				{

					VectorOnLine = lineEndVector1;
					return DistanceBetweenVectorAndVector(Vector, lineEndVector1);
				}

				double c2 = VectorExtensions.Dot(v, v);

				if (c2 <= c1)
				{
					VectorOnLine = lineEndVector;
					return DistanceBetweenVectorAndVector(Vector, lineEndVector);
				}

				double b = c1 / c2;
				VectorOnLine = Vector.Add(lineEndVector1, Vector.Multiply(v, b));
				return DistanceBetweenVectorAndVector(Vector, VectorOnLine);
			}

			public static double Cross(Vector value1, Vector value2)
			{
				return value1.X * value2.Y - value1.Y * value2.X;
			}

			public static Vector Cross(Vector value1, double value2)
			{
				return new Vector(value2 * value1.Y, -value2 * value1.X);
			}

			public static Vector Cross(double value2, Vector value1)
			{
				return new Vector(-value2 * value1.Y, value2 * value1.X);
			}

			public static void Cross(ref Vector value1, ref Vector value2, out double ret)
			{
				ret = value1.X * value2.Y - value1.Y * value2.X;
			}

			public static void Cross(ref Vector value1, ref double value2, out Vector ret)
			{
				ret = value1; //necassary to get past a compile error on 360
				ret.X = value2 * value1.Y;
				ret.Y = -value2 * value1.X;
			}

			public static void Cross(ref double value2, ref Vector value1, out Vector ret)
			{
				ret = value1;//necassary to get past a compile error on 360
				ret.X = -value2 * value1.Y;
				ret.Y = value2 * value1.X;
			}

			public static Vector Project(Vector projectVector, Vector onToVector)
			{
				double multiplier = 0;
				double numerator = (onToVector.X * projectVector.X + onToVector.Y * projectVector.Y);
				double denominator = (onToVector.X * onToVector.X + onToVector.Y * onToVector.Y);

				if (denominator != 0)
				{
					multiplier = numerator / denominator;
				}

				return Vector.Multiply(onToVector, multiplier);
			}

			public static void Truncate(ref Vector vector, double maxLength, out Vector truncatedVector)
			{
				double length = vector.Length();
				length = Math.Min(length, maxLength);
				if (length > 0)
				{
					vector.Normalize();
				}
				VectorExtensions.Multiply(ref vector, length, out truncatedVector);
			}

			public static double DegreesToRadians(double degrees)
			{
				return degrees * RadiansToDegreesRatio;
			}

			public static double RandomNumber(double min, double max)
			{
				return (double)((max - min) * random.NextDouble() + min);
			}

			public static bool IsBetweenNonInclusive(double number, double min, double max)
			{
				if (number > min && number < max)
				{
					return true;
				} else
				{
					return false;
				}

			}

			/// Temp variables to speed up the following code.
			private static double tPow2;
			private static double wayToGo;
			private static double wayToGoPow2;

			private static Vector startCurve;
			private static Vector curveEnd;
			private static Vector _temp;

			public static double VectorToRadians(Vector vector)
			{
				return (double)Math.Atan2((double)vector.X, -(double)vector.Y);
			}

			public static Vector RadiansToVector(double radians)
			{
				return new Vector((double)Math.Sin((double)radians), -(double)Math.Cos((double)radians));
			}

			public static void RadiansToVector(double radians, ref Vector vector)
			{
				vector.X = (double)Math.Sin((double)radians);
				vector.Y = -(double)Math.Cos((double)radians);
			}

			public static void RotateVector(ref Vector vector, double radians)
			{
				double length = vector.Length();
				double newRadians = (double)Math.Atan2((double)vector.X, -(double)vector.Y) + radians;

				vector.X = (double)Math.Sin((double)newRadians) * length;
				vector.Y = -(double)Math.Cos((double)newRadians) * length;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="start"></param>
			/// <param name="end"></param>
			/// <param name="key">Value between 0.0f and 1.0f.</param>
			/// <returns></returns>
			public static Vector LinearBezierCurve(Vector start, Vector end, double t)
			{
				return start + (end - start) * t;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="start"></param>
			/// <param name="curve"></param>
			/// <param name="end"></param>
			/// <param name="key">Value between 0.0f and 1.0f.</param>
			/// <returns></returns>
			public static Vector QuadraticBezierCurve(Vector start, Vector curve, Vector end, double t)
			{
				wayToGo = 1.0f - t;

				return wayToGo * wayToGo * start
					   + 2.0f * t * wayToGo * curve
					   + t * t * end;
			}

			public static Vector QuadraticBezierCurve(Vector start, Vector curve, Vector end, double t, ref double radians)
			{
				startCurve = start + (curve - start) * t;
				curveEnd = curve + (end - curve) * t;
				_temp = curveEnd - startCurve;

				radians = (double)Math.Atan2((double)_temp.X, -(double)_temp.Y);
				return startCurve + _temp * t;
			}

			public static Vector CubicBezierCurve2(Vector start, Vector startVectorsTo, Vector end, Vector endVectorsTo, double t)
			{
				return CubicBezierCurve(start, start + startVectorsTo, end + endVectorsTo, end, t);
			}

			public static Vector CubicBezierCurve2(Vector start, Vector startVectorsTo, Vector end, Vector endVectorsTo, double t, ref double radians)
			{
				return CubicBezierCurve(start, start + startVectorsTo, end + endVectorsTo, end, t, ref radians);
			}

			public static Vector CubicBezierCurve2(Vector start, double startVectorDirection, double startVectorLength,
													Vector end, double endVectorDirection, double endVectorLength,
													double t, ref double radians)
			{
				return CubicBezierCurve(start,
										Calculator.RadiansToVector(startVectorDirection) * startVectorLength,
										Calculator.RadiansToVector(endVectorDirection) * endVectorLength,
										end,
										t,
										ref radians);
			}

			public static Vector CubicBezierCurve(Vector start, Vector curve1, Vector curve2, Vector end, double t)
			{
				tPow2 = t * t;
				wayToGo = 1.0f - t;
				wayToGoPow2 = wayToGo * wayToGo;

				return wayToGo * wayToGoPow2 * start
					   + 3.0f * t * wayToGoPow2 * curve1
					   + 3.0f * tPow2 * wayToGo * curve2
					   + t * tPow2 * end;
			}

			public static Vector CubicBezierCurve(Vector start, Vector curve1, Vector curve2, Vector end, double t, ref double radians)
			{
				return QuadraticBezierCurve(start + (curve1 - start) * t,
											curve1 + (curve2 - curve1) * t,
											curve2 + (end - curve2) * t,
											t,
											ref radians);
			}

			//Interpolate normal vectors ...
			public static Vector InterpolateNormal(Vector vector1, Vector Vector, double t)
			{
				vector1 += (Vector - vector1) * t;
				vector1.Normalize();

				return vector1;
			}

			public static void InterpolateNormal(Vector vector1, Vector Vector, double t, out Vector vector)
			{
				vector = vector1 + (Vector - vector1) * t;
				vector.Normalize();
			}

			public static void InterpolateNormal(ref Vector vector1, Vector Vector, double t)
			{
				vector1 += (Vector - vector1) * t;
				vector1.Normalize();
			}

			public static double InterpolateRotation(double radians1, double radians2, double t)
			{
				Vector vector1 = new Vector((double)Math.Sin((double)radians1), -(double)Math.Cos((double)radians1));
				Vector Vector = new Vector((double)Math.Sin((double)radians2), -(double)Math.Cos((double)radians2));

				vector1 += (Vector - vector1) * t;
				vector1.Normalize();

				return (double)Math.Atan2((double)vector1.X, -(double)vector1.Y);
			}

			public static void ProjectToAxis(ref Vector[] Vectors, ref Vector axis, out double min, out double max)
			{
				// To project a Vector on an axis use the dot product
				double dotProduct = VectorExtensions.Dot(axis, Vectors[0]);
				min = dotProduct;
				max = dotProduct;

				for (int i = 0; i < Vectors.Length; i++)
				{
					dotProduct = VectorExtensions.Dot(Vectors[i], axis);
					if (dotProduct < min)
					{
						min = dotProduct;
					} else
					{
						if (dotProduct > max)
						{
							max = dotProduct;
						}
					}
				}
			}



			public static double Distance(Vector vector, Vector vector_2)
			{
				Vector v = Vector.Subtract(vector, vector_2);
				return v.Length();
			}
		}
		public class MathHelper
		{
			public const double DegreesToRadiansRatio = 57.29577957855f;
			public const double RadiansToDegreesRatio = 1f / 57.29577957855f;

			public static double Lerp(double value1, double value2, double amount)
			{
				return value1 + (value2 - value1) * amount;
			}

			public static double Min(double value1, double value2)
			{
				return Math.Min(value1, value2);
			}

			public static double Max(double value1, double value2)
			{
				return Math.Max(value1, value2);
			}

			public static double Clamp(double value, double min, double max)
			{
				return Math.Max(min, Math.Min(value, max));
			}



			public static double Distance(double value1, double value2)
			{
				return Math.Abs((double)(value1 - value2));
			}

			public static double ToRadians(double degrees)
			{
				return degrees * RadiansToDegreesRatio;
			}

			public static double TwoPi = (double)(Math.PI * 2.0);
			public static double Pi = (double)(Math.PI);
			public static double PiOver2 = (double)(Math.PI / 2.0);
			public static double PiOver4 = (double)(Math.PI / 4.0);

		}
	}
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
			public static Vector2D operator -(Vector2D v) { return new Vector2D(-v.A, -v.B, v.X, v.Y); }
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
					if (doru.Vectors.Calculator.DistanceBetweenVectorAndLineSegment((Vector)pos, (Vector)v.dot1, (Vector)v.dot2) < dist / 2)
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
}
