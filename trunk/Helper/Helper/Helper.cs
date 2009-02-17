using System;
using System.Collections.Generic;
using System.Text;
#if(SILVERLIGHT||WPF)
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
#endif
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;
#if(!SILVERLIGHT)
using System.IO.Compression;
#endif
#if(ZIPLIB)
using ICSharpCode.SharpZipLib.Zip;
#endif




namespace CounterStrikeLive
{
    //[DebuggerStepThrough]
    public static class Random
    {
        static System.Random _Random = new System.Random();
        public static int Next(int min, int max)
        {
            return _Random.Next(min, max);
        }
        public static float Next(float min, float max)
        {
            return min + ((float)_Random.NextDouble()) * (max - min);
        }
        public static int Next(int max)
        {
            return _Random.Next(max);
        }
    }
}

namespace doru
{

#if(WPF)
    namespace WPF
    {
        using System.Windows;
        using System.Collections.ObjectModel;
        //[DebuggerStepThrough]
        public class BindableList<T> : ObservableCollection<T>
        {

            public List<ICVS> _ArrayList = new List<ICVS>();
            public interface ICVS
            {
                void Add(object o);
                void Remove(object o);
            }
            //[DebuggerStepThrough]
            public class CVS<T2> : ICVS
            {
                public delegate T2 Converter(T t);
                public IList<T2> _list;
                public void Add(object o)
                {
                    T2 t2 = _Converter((T)o);
                    if (!_list.Contains(t2))
                        _list.Add(t2);
                }
                public void Remove(object o)
                {
                    T2 t2 = _Converter((T)o);
                    if (_list.Contains(t2))
                        _list.Remove(t2);
                }
                public CVS()
                {
                    if (_Converter == null) _Converter = DefaultConverter;
                }
                public Converter _Converter;
                public T2 DefaultConverter(T t)
                {
                    object o = (object)t;
                    return (T2)o;
                }
            }
            public void BindTo<T2>(IList<T2> list)
            {
                CVS<T2> _CVS = new CVS<T2> { _list = list };
                _ArrayList.Add(_CVS);
            }

            public void BindTo<T2>(IList<T2> list, CVS<T2>.Converter cv)
            {
                CVS<T2> _CVS = new CVS<T2> { _Converter = cv, _list = list };
                _ArrayList.Add(_CVS);
            }
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (T t in e.NewItems)
                            foreach (ICVS o in _ArrayList)
                                o.Add(t);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (T t in e.OldItems)
                            foreach (ICVS o in _ArrayList)
                                o.Remove(t);
                        break;
                }
                base.OnCollectionChanged(e);
            }
        }
        namespace Vectors
        {
            public static class Vector2
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
            public static class Extensions
            {

                public static double Length(this Vector p)
                {
                    double num = (p.X * p.X) + (p.Y * p.Y);
                    return Math.Sqrt((double)num);

                }
            }
            public static class Calculator
            {
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

                public static double DistanceBetweenVectorAndVector(Vector Vector1, Vector Vector2)
                {
                    Vector v = Vector.Subtract(Vector1, Vector2);
                    return v.Length();
                }

                public static double DistanceBetweenVectorAndLineSegment(Vector Vector, Vector lineEndVector1, Vector lineEndVector2, out Vector VectorOnLine)
                {
                    Vector v = Vector.Subtract(lineEndVector2, lineEndVector1);
                    Vector w = Vector.Subtract(Vector, lineEndVector1);

                    double c1 = Vector2.Dot(w, v);
                    if (c1 <= 0)
                    {

                        VectorOnLine = lineEndVector1;
                        return DistanceBetweenVectorAndVector(Vector, lineEndVector1);
                    }

                    double c2 = Vector2.Dot(v, v);

                    if (c2 <= c1)
                    {
                        VectorOnLine = lineEndVector2;
                        return DistanceBetweenVectorAndVector(Vector, lineEndVector2);
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
                    Vector2.Multiply(ref vector, length, out truncatedVector);
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
                    }
                    else
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
                /// <param name="t">Value between 0.0f and 1.0f.</param>
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
                /// <param name="t">Value between 0.0f and 1.0f.</param>
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
                    double dotProduct = Vector2.Dot(axis, Vectors[0]);
                    min = dotProduct;
                    max = dotProduct;

                    for (int i = 0; i < Vectors.Length; i++)
                    {
                        dotProduct = Vector2.Dot(Vectors[i], axis);
                        if (dotProduct < min)
                        {
                            min = dotProduct;
                        }
                        else
                        {
                            if (dotProduct > max)
                            {
                                max = dotProduct;
                            }
                        }
                    }
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
    }
#endif

    public abstract class Encoding : System.Text.Encoding
    {
#if (!SILVERLIGHT)
        public new static System.Text.Encoding Default = System.Text.Encoding.Default;
        public static System.Text.Encoding Default2 { get { return System.Text.Encoding.Default; } }
#else
        public static System.Text.Encoding Default = Encoding.UTF8;
        static System.Text.Encoding _DefaultEncoding = Encoding.UTF8;
#endif

    }
    //[DebuggerStepThrough]
    public class ExceptionC : Exception
    {
        public ExceptionC(string s) : base(s) { }
        public override string ToString()
        {
            return Message;
        }
    }
    //[DebuggerStepThrough]
    public class ExceptionB : Exception
    {
        public ExceptionB(string s) : base(s) { }
    }

    //[DebuggerStepThrough]
    public class ExceptionA : Exception { public ExceptionA(string s) : base(s) { } public ExceptionA() { } };

    //[DebuggerStepThrough]
    public static class Helper
    {
#if(ZIPLIB)
        public static void OpenZip(FileStream fs, Dictionary<string, Stream> _Resources)
        {
            ZipInputStream _ZipInputStream = new ZipInputStream(fs);
            while (true)
            {
                ZipEntry _ZipEntry = _ZipInputStream.GetNextEntry();
                if (_ZipEntry == null) break;
                MemoryStream ms = new MemoryStream(_ZipInputStream.Read());
                if (_ZipEntry.IsFile)
                    _Resources.Add(_ZipEntry.Name.Trace(), ms);
            }
        }
#endif
        public static void MergeList<T>(IList<T> a, IList<T> b)
        {
            for (int i = 0; i < Math.Max(a.Count, b.Count); i++)
            {

                if (a.Count <= i) { a.Add(b[i]); continue; }
                if (b.Count <= i) { a.RemoveAt(i); continue; }
                if (!a[i].Equals(b[i])) a[i] = b[i];
            }
        }
        public static class IO
        {
            public static void CreateDirectories(List<string> _directories, string _directoryb)
            {
                if (!Directory.Exists(_directoryb)) Directory.CreateDirectory(_directoryb);
                foreach (string _Direcotry in _directories)
                    try
                    {
                        if (!Directory.Exists(_directoryb + "/" + _Direcotry)) Directory.CreateDirectory(_directoryb + "/" + _Direcotry);
                    }
                    catch (PathTooLongException) { }
            }
            public static List<string> GetFiles(List<string> _directories)
            {
                List<string> _list = new List<string>();
                foreach (string _string in _directories)
                {
                    try
                    {
                        string[] _files = Directory.GetFiles(_string);
                        foreach (string file in _files)
                        {
                            _list.Add(file);
                        }
                    }
                    catch { }
                }
                return _list;
            }
            public static List<string> GetDirectories(params string[] _strings) { List<String> list = new List<string>(); GetDirectories(list, _strings); return list; }
            public static void GetDirectories(List<string> _strings2, string[] _strings)
            {
                foreach (string _string in _strings)
                {
                    _strings2.Add(_string);
                    try
                    {
                        string[] _directories = Directory.GetDirectories(_string);
                        GetDirectories(_strings2, _directories);
                    }
                    catch { }
                }
            }
        }
        public static byte[] JoinBytes(params object[] objects)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (object o in objects)
                    if (o is byte[])
                    {
                        if (((byte[])o).Length != 0)
                            ms.Write((byte[])o);
                    }
                    else
                        ms.Write(Convert.ToByte(o));

                return ms.ToArray();
            }
        }
        public static string ReplaceRandoms(string text, string[] _RandomTags)
        {
            text = Regex.Replace(text, @"_randomtext(\d+)_", delegate(Match m)
            {
                StringBuilder _StringBuilder = new StringBuilder();
                for (int i2 = 0; i2 < int.Parse(m.Groups[1].Value); i2++)
                {
                    _StringBuilder.Append(_RandomTags[_Random.Next(_RandomTags.Length)] + " ");
                }
                return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
            });

            foreach (Match m in Regex.Matches(text, @"_randomcode(\d+)_"))
            {
                text = text.Replace(m.Value, _Random.RandomString(int.Parse(m.Groups[1].Value)));
            }
            return text;
        }

        public static List<string> RemoveDuplicates(List<string> inputList)
        {
            Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
            List<string> finalList = new List<string>();

            foreach (string currValue in inputList)
            {
                if (!uniqueStore.ContainsKey(currValue))
                {
                    uniqueStore.Add(currValue, 0);
                    finalList.Add(currValue);
                }
            }
            return finalList;
        }

        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray)
        {
            Replace(ref _source, _oldarray, _newarray, 0, -1);
        }
        public static void Replace(ref string _source, string _oldarray, string _newarray, int count)
        {
            byte[] _bytes = _source.ToBytes();
            Replace(ref _bytes, _oldarray.ToBytes(), _newarray.ToBytes(), 0, count);
            _source = _bytes.ToStr();
        }
        public static void Replace(ref byte[] _source, string _oldarray, string _newarray, int count)
        {
            Replace(ref _source, _oldarray.ToBytes(), _newarray.ToBytes(), 0, count);
        }
        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int count)
        {
            Replace(ref _source, _oldarray, _newarray, 0, count);
        }
        public static void Replace<T>(ref T[] _source, T[] _oldarray, T[] _newarray, int startpos, int count)
        {
            for (int c = 0; c < count || count == -1; c++)
            {
                startpos = _source.IndexOf2(_oldarray, startpos);
                if (startpos != -1)
                {
                    int length = _source.Length - _oldarray.Length + _newarray.Length;
                    T[] dest = new T[length];
                    int i = 0;
                    for (; i < startpos; i++)
                        dest[i] = _source[i];
                    for (int j = 0; j < _newarray.Length; i++, j++)
                        dest[i] = _newarray[j];
                    for (int j = startpos + _oldarray.Length; i < length; i++, j++)
                        dest[i] = _source[j];
                    _source = dest;
                }
                else
                {
                    if (count == -1)
                    {
                        return;
                    }
                    throw new ExceptionA("Count Didnt Match");
                }
            }
            return;
        }

        public static string Randomstr(int size)
        {
            char[] chars = new char[size];
            string s = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
            for (int i = 0; i < size; i++)
            {
                chars[i] = s[_Random.Next(s.Length - 1)];
            }
            return new string(chars);
        }

        public static string convertToString<T>(IEnumerable<T> array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in array)
                sb.Append(item + ",");
            return sb.ToString();
        }
        public static Random _Random = new Random();
        public static int putToNextFreePlace<T>(T item, IList<T> items)
        {
            int id = items.IndexOf(default(T));
            if (id == -1)
            {
                items.Add(item);
                return items.Count - 1;
            }
            else
            {
                items[id] = item;
                return id;
            }
        }
        public static bool Compare<T>(T[] source, T[] pattern)
        {
            return Compare(source, pattern, 0);
        }
        public static bool Compare<T>(T[] source, T[] pattern, int startpos)
        {
            if (source.Length - startpos < pattern.Length) return false;
            for (int j = 0; j < pattern.Length; j++, startpos++)
            {
                if (startpos >= source.Length || !pattern[j].Equals(source[startpos])) return false;
            }
            return true;
        }

#if(SILVERLIGHT||WPF)
        
        public static DispatcherTimer StartRepeatMethod(this DispatcherTimer ds, double secconds, Action d)
        {
            ds.Interval = TimeSpan.FromSeconds(secconds);
            ds.Tick += delegate { d(); };
            ds.Start();
            return ds;
        }
        public static DispatcherTimer StartCallMethod(this DispatcherTimer ds, double secconds, Delegate d, params object[] os)
        {
            ds.Interval = TimeSpan.FromSeconds(secconds);
            ds.Tick += delegate { ds.Stop(); d.DynamicInvoke(os); };
            ds.Start();
            return ds;
        }
#endif
#if(SILVERLIGHT)        
        public static SocketAsyncEventArgs Connect(string ip, Dispatcher Dispatcher, OnConnected _OnConnected)
        {
            string[] ss = ip.Split(":");
            return Connect(ss[0], int.Parse(ss[1]), Dispatcher, _OnConnected);
        }
        public delegate void OnConnected(SocketAsyncEventArgs s);
        public static SocketAsyncEventArgs Connect(string ip, int port, Dispatcher Dispatcher, OnConnected _OnConnected)
        {
            Socket _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
            if (Application.Current.Host.Source.DnsSafeHost == "") "warning host not safe".Trace();
            _SocketAsyncEventArgs.RemoteEndPoint = new DnsEndPoint(ip, port);
            _SocketAsyncEventArgs.UserToken = _Socket;            
            _SocketAsyncEventArgs.Completed += delegate
            {
                "0".Trace();
                Dispatcher.BeginInvoke(
                    delegate
                    {
                        "1".Trace();
                        _OnConnected(_SocketAsyncEventArgs);
                    });
            };
            _Socket.ConnectAsync(_SocketAsyncEventArgs);
            return _SocketAsyncEventArgs;
        }
        

#else
        public delegate void OnConnected(Socket s);
        public static void Trace2(string t, string prefix)
        {
            string[] ss = t.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 3)
            {
                System.Diagnostics.Trace.Write(prefix + ":");
                t.ToString().Save();
            }
            else
            {
                foreach (string s in ss)
                    System.Diagnostics.Trace.WriteLine(prefix + ":" + s);
            }
        }
        public static XmlSerializer CreateSchema(string name, params Type[] types)
        {
            string SchemasPath = Environment.GetEnvironmentVariable("VS90COMNTOOLS");
            if (SchemasPath == null) SchemasPath = "./";
            else SchemasPath = Path.GetFullPath(SchemasPath + "../../Xml/Schemas");
            XmlReflectionImporter _XmlReflectionImporter = new XmlReflectionImporter(name);
            XmlSchemas _XmlSchemas = new XmlSchemas();

            XmlSchemaExporter _XmlSchemaExporter = new XmlSchemaExporter(_XmlSchemas);
            List<Type> xtratypes = new List<Type>();
            for (int i = 1; i < types.Length; i++)
            {
                _XmlReflectionImporter.IncludeType(types[i]);
                xtratypes.Add(types[i]);
            }
            XmlTypeMapping map = _XmlReflectionImporter.ImportTypeMapping(types[0]);
            _XmlSchemaExporter.ExportTypeMapping(map);


            using (StringWriter fs = new StringWriter())
            {
                _XmlSchemas[0].Write(fs);
                //FixSchema(_XmlSchemas[0]);
                string s = fs.ToString();
                s = Regex.Replace(s.Replace("xs:sequence", "xs:all"), @"minOccurs=""?"" maxOccurs=""?""", "minOccurs=\"0\"");
                s = s.Replace("\"utf-16\"", "\"utf-8\"");
                s = Regex.Replace(s, @"(ArrayOf.*\n.*xs\:)all(.*\n.*\n.*</xs:)all", "${1}sequence${2}sequence");
                s = Regex.Replace(s, @"xs:all(>\r?\n?.*<xs:element ref=""xs:schema"" />\r?\n?.*\r?\n?.*)xs:all", "xs:sequence${1}xs:sequence", RegexOptions.Multiline);
                File.WriteAllText(SchemasPath + "/" + name + ".xsd", s, Encoding.Default);
            }
            XmlSerializer _XmlSerializer = new XmlSerializer(types[0], new XmlAttributeOverrides(), xtratypes.ToArray(), new XmlRootAttribute(), name);
            return _XmlSerializer;
        }
        public static Process StartProcess(string s)
        {
            ProcessStartInfo _ProcessStartInfo = new ProcessStartInfo(Path.GetFullPath(s));
            _ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(s);
            return Process.Start(_ProcessStartInfo);
        }
        public static void GenerateXsd(Type _type, Type[] _types, string filename)
        {
            XmlReflectionImporter _XmlReflectionImporter = new XmlReflectionImporter();
            XmlSchemas _XmlSchemas = new XmlSchemas();

            XmlSchemaExporter _XmlSchemaExporter = new XmlSchemaExporter(_XmlSchemas);
            foreach (Type _Type in _types)
                _XmlReflectionImporter.IncludeType(_Type);

            _XmlSchemaExporter.ExportTypeMapping(_XmlReflectionImporter.ImportTypeMapping(_type));

            using (FileStream _FileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                _XmlSchemas[0].Write(_FileStream);
        }
        public static string getMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        public static Socket Connect(string ip, int port)
        {
            return new TcpClient(ip, port).Client;
        }
#endif


        
    }

    //[DebuggerStepThrough]
    public static class Extensions
    {
        public static string Replace(this string s,params string[] ss)
        {
            for (int i = 0; i < ss.Length; i++)
            {
                s = s.Replace(ss[i], ss[++i]);
            }
            return s;    
			
        }
#if(!SILVERLIGHT)
        static Extensions()
        {
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
        }

        public static string Save(this string s, string comment)
        {
            Encoding.Default.GetBytes(s).Save(comment);
            return s;
        }
        public static string Save(this string s)
        {
            Encoding.Default.GetBytes(s).Save();
            return s;
        }
        public static byte[] Save(this byte[] s)
        {
            return Save(s, "");
        }
        public static byte[] Save(this byte[] s, string comment)
        {
            string path = "./logs/" + DateTime.Now.ToString().Replace(":", "-") + _Random.RandomString(4) + ".html";
            File.WriteAllBytes(path, s);
            System.Diagnostics.Trace.WriteLine(Path.GetFullPath(path) + ":" + comment);
            return s;
        }
        public static void Send(this Socket _Socket, string s)
        {
            _Socket.Send(Encoding.Default.GetBytes(s));
        }
        public static string ReceiveText(this Socket _Socket)
        {
            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new ExceptionA();
            return Encoding.Default.GetString(_buffer.Substr(count));
        }
        public static byte[] Receive(this Socket _Socket)
        {

            byte[] _buffer = new byte[99999];
            int count = _Socket.Receive(_buffer);
            if (count == 0) throw new SocketException();
            return _buffer.Substr(count);

        }
        public static byte[] Receive(this Socket _Socket, int length)
        {
            byte[] _buffer = new byte[length];
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int count = _Socket.Receive(_buffer, length, 0);
                    if (count == 0) throw new ExceptionA("Read Socket failed");
                    _MemoryStream.Write(_buffer, 0, count);
                    length -= count;
                    if (length == 0) return _MemoryStream.ToArray();
                }
            }
        }
#else
        #region
        public static void Send(this Socket _Socket, byte[] buffer) { Send(_Socket, buffer, 0, buffer.Length); }
        public static void Send(this Socket _Socket, byte[] buffer, int offset, int count)
        {
            SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
            _SocketAsyncEventArgs.SetBuffer(buffer, offset, count);
            _Socket.SendAsync(_SocketAsyncEventArgs);
        }

        public static void Show(this Control _Control)        ///<sumary> create</sumary>
        {
            _Control.Visibility = Visibility.Visible;
            _Control.IsEnabled = true;
            _Control.Focus();
        }
        public static void Hide(this Control _Control)
        {
            _Control.Visibility = Visibility.Collapsed;
            _Control.IsEnabled = false;
        }
        public static void Toggle(this Control _Control)
        {
            if (_Control.Visibility == Visibility.Visible)
                _Control.Hide();
            else
                _Control.Show();
        }
        #endregion
#endif
        #region Stream
        public static byte[] Cut(this Stream source, string pattern)
        {
            return Cut(source, Encoding.Default.GetBytes(pattern));
        }
        public static byte[] Cut(this Stream source, byte[] pattern)
        {
            MemoryStream _MemoryStream = new MemoryStream();
            while (true)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    int b = source.ReadByte();
                    if (b == -1) throw new IOException("Cut: unable to cut");
                    _MemoryStream.WriteByte((byte)b);
                    if (pattern[i] != b) break;
                    if (i == pattern.Length - 1) return _MemoryStream.ToArray();
                }
            }
        }
        public static string WriteLine(this Stream _Stream, string s)
        {
            _Stream.Write(s + "\r\n");
            return s;
        }
        public static string ReadLine(this Stream _Stream)
        {
            return _Stream.Cut("\n").ToStr().TrimEnd('\r', '\n');
        }
        public static byte[] Read(this Stream _Stream)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            using (MemoryStream outputStream = new MemoryStream())
            {
                while ((bytesRead = _Stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }
                return outputStream.ToArray();
            }
        }
        public static void Write(this Stream _Stream, byte[] _bytes)
        {
            if (_bytes.Length == 0) throw new Exception();
            _Stream.Write(_bytes, 0, _bytes.Length);
        }
#if(BIGENDIAN)
        public static UInt32 ReadUInt32(this Stream _Stream)
        {
            return BitConverter.ToUInt32(_Stream.Read(4).ReverseA(), 0);
        }
        public static UInt16 ReadUInt16(this Stream _Stream)
        {
            return BitConverter.ToUInt16(_Stream.Read(2).ReverseA(), 0);
        }
        public static void WriteUint32(this Stream _Stream, UInt32 i)
        {
            _Stream.Write(BitConverter.GetBytes(i).ReverseA());
        }
        public static void WriteUint16(this Stream _Stream, UInt16 i)
        {
            _Stream.Write(BitConverter.GetBytes(i).ReverseA(2), 0, 2);
        }

#else

        public static Int16 ReadInt16(this Stream s)
        {
            return BitConverter.ToInt16(s.Read(2), 0);
        }
        public static UInt16 ReadUInt16(this Stream s)
        {
            return BitConverter.ToUInt16(s.Read(2), 0);
        }
        public static float ReadFloat(this Stream s)
        {
            return BitConverter.ToSingle(s.Read(4), 0);
        }
        public static void Write(this Stream s, Int16 _int)
        {
            s.Write(BitConverter.GetBytes(_int));
        }
        public static void Write(this Stream s, UInt16 _int)
        {
            s.Write(BitConverter.GetBytes(_int));
        }
        public static void Write(this Stream s, float str)
        {
            s.Write(BitConverter.GetBytes(str));
        }
#endif
        public static void Write(this Stream s, string _str)
        {
            s.Write(_str.ToBytes());
        }
        public static void Write(this Stream s, byte _int)
        {
            s.WriteByte(_int);
        }
        public static void Write(this Stream s, bool _int)
        {
            s.Write(BitConverter.GetBytes(_int));
        }
        public static bool ReadBoolean(this Stream s)
        {
            return BitConverter.ToBoolean(new[] { s.ReadB() }, 0);
        }
        public static byte ReadB(this Stream _Stream)
        {
            int i = _Stream.ReadByte();
            if (i == -1) throw new IOException();
            return (byte)i;
        }
        public static byte[] ReadBlock(this Stream _Stream)
        {
            byte[] _bytes = new byte[9999999];
            _Stream.Read(_bytes, 0, _bytes.Length);
            return _bytes.Cut(_bytes.Length);
            //_Stream.Read(_b
        }
        public static byte[] Read(this Stream _Stream, int length)
        {
            if (length == 0) throw new Exception("length == 0");
            byte[] _buffer = new byte[length];
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int count = _Stream.Read(_buffer, 0, length);
                    if (count == 0) throw new IOException("Read Stream failed");
                    _MemoryStream.Write(_buffer, 0, count);
                    length -= count;
                    if (length == 0) return _MemoryStream.ToArray();
                }
            }
        }

        public static string ReadString(this Stream s)
        {
            int c = s.ReadB();
            return s.Read(c).ToStr();
        }
        public static void WriteString(this Stream s, string _str)
        {
            byte[] bs = _str.ToBytes();
            s.Write(Helper.JoinBytes(new[] { (byte)bs.Length }, bs));
        }
        #endregion
        public static string[] Split2(this string s, string s2)
        {
            return s.Split(new[] { s2 }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static T DeserealizeOrCreate<T>(this XmlSerializer x, string path, T t)
        {
            if (t == null) throw new NullReferenceException("omg");

            try
            {
                using (FileStream fs1 = File.Open(path, FileMode.Open))
                    return (T)x.Deserialize(fs1);
            }
            catch
            {
                using (FileStream fs = File.Create(path)) x.Serialize(fs, t);
                return t;
            }
        }
        public static void Serialize<T>(this XmlSerializer x, string path, T t)
        {
            using (FileStream fs = File.Open(path, FileMode.Create))
                x.Serialize(fs, t);
        }
        public static byte[] Serialize<T>(this XmlSerializer x, T t)
        {
            MemoryStream ms = new MemoryStream();
            x.Serialize(ms, t);
            return ms.ToArray();
        }
        public static bool IsValid(this Enum e)
        {
            return Enum.IsDefined(e.GetType(), e);
        }
        public static int PutToNextFreePlace<T>(this IList<T> items, T item)
        {
            int id = items.IndexOf(default(T));
            if (id == -1)
            {
                items.Add(item);
                return items.Count - 1;
            }
            else
            {
                items[id] = item;
                return id;
            }
        }
        public static bool Contains<T>(this IList<T> list, params T[] ts)
        {
            foreach (T t in ts)
                if (!list.Contains(t)) return false;
            return true;
        }
        public static T Trace<T>(this T t)
        {
            return Trace(t, "");
        }
        public static T Trace<T>(this T t, string s)
        {
#if(SILVERLIGHT)
            doru.Trace.WriteLine(s + t);
#else
            System.Diagnostics.Trace.WriteLine(s + t);
#endif
            return t;
        }

        public static string Join<T>(this IEnumerable<T> list, string text)
        {
            string s = "";
            foreach (object o in list)
                s += o.ToString() + text;
            return s.Substring(0, s.Length - text.Length);
        }
        //public static string 
        public static IEnumerable<T> Last<T>(this IEnumerable<T> list, int c)
        {
            return list.Skip(Math.Max(0, list.Count() - c));
        }
        public static string ToString<T>(this IEnumerable<T> list, string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in list)
            {
                sb.Append(t.ToString() + s);
            }
            return sb.ToString();
        }
        public static string ToString(this IEnumerable list, string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object t in list)
            {
                sb.Append(t.ToString() + s);
            }
            return sb.ToString();
        }
        public static T2 TryGetValue<T, T2>(this Dictionary<T, T2> dict, T t)
        {
            T2 t2;
            dict.TryGetValue(t, out t2);
            return t2;
        }
        public static string[] Split(this string a, string b)
        {
            return a.Split(new string[] { b }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static void AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }
        public static bool Equals2(this byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        public static string TrimStart(this string s, string a)
        {
            if (s.StartsWith(a)) return s.Substring(a.Length, s.Length - a.Length);
            return s;
        }
        public static T Pop<T>(this List<T> list)
        {
            T t = list[0];
            list.Remove(t);
            list.Add(t);
            return t;
        }
        public static byte[] Hex2(this string s)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection ms = Regex.Matches(s, @"0x\d{4,4}   (.+?)   ");
            foreach (Match m in ms)
                sb.AppendLine(m.Groups[1].Value);
            return sb.ToString().Hex();

        }
        public static byte[] Hex(this string s)
        {
            MatchCollection ms = Regex.Matches(s, "[0-9a-fA-F]{2,2}");
            byte[] _bytes = new byte[ms.Count];
            for (int i = 0; i < ms.Count; i++)
            {
                _bytes[i] = byte.Parse(ms[i].Value, System.Globalization.NumberStyles.HexNumber);
            }
            return _bytes;
        }


        public static T Random<T>(this IList<T> list, T t2)
        {
            T t;
            while ((t = list[_Random.Next(list.Count - 1)]).Equals(t2)) ;
            return t;
        }
        public static T Random<T>(this IList<T> list)
        {
            return list[_Random.Next(list.Count - 1)];
        }
        public static string Random(this string[] _Tags)
        {
            return _Tags[_Random.Next(_Tags.Length)];
        }



        public static string GetString(this System.Text.Encoding e, byte[] str)
        {
            return e.GetString(str, 0, str.Length);
        }
        public static string[][] Split(this string[] stringArray, char spec)
        {
            string[][] mString = new string[stringArray.Length][];
            for (int i = 0; i < stringArray.Length; i++)
            {
                mString[i] = stringArray[i].Trim(spec).Split(spec);
            }
            return mString;
        }

        public static string Replace(this string s, string a, string b)
        {
            Debugger.Break();
            return s.Replace(a, b);
        }
        public static string RandomString(this Random _Random, int length)
        {
            const string r = "1234567890qwertyuiopasdfghjklzxcvbnm";
            StringBuilder _StringBuilder = new StringBuilder();
            for (int i2 = 0; i2 < length; i2++)
            {
                _StringBuilder.Append(r[_Random.Next(0, r.Length)]);
            }
            return _StringBuilder.ToString(0, _StringBuilder.Length - 1);
        }

        public static byte[] NextBytes(this Random _Random, int l)
        {
            byte[] _bytes = new byte[l];
            _Random.NextBytes(_bytes);
            return _bytes;
        }
        public static Thread StartBackground(this Thread t, string name)
        {
            t.Name = name;
            StartBackground(t);
            return t;
        }
        public static Thread StartBackground(this Thread t)
        {
            t.IsBackground = true;
            t.Start();
            return t;
        }

        public static void ReplaceOnce(this string text, string a, string b, out string text2)
        {
            int i = text.IndexOf(a);
            if (i == -1) throw new ExceptionA();
            text = text.Remove(i, a.Length);
            text2 = text.Insert(i, b);
        }
        public static T[] ReverseA<T>(this T[] a)
        {
            return a.ReverseA(a.Length);
        }
        public static T[] ReverseA<T>(this T[] a, int len)
        {
            T[] b = new T[len];
            for (int i = 0; i < len; i++)
            {
                b[a.Length - i - 1] = a[i];
            }
            return b;
        }

        public static string ToHex(this byte b)
        {
            return String.Format("0x{0:x}", b);
        }

        public static string ToHex(this byte[] _bytes)
        {
            StringBuilder sb = new StringBuilder();
            int x = 0;
            while (true)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; ; i++)
                    {
                        sb.Append(_bytes[x] > 15 ? String.Format("{0:X}", _bytes[x]) : String.Format("0{0:X}", _bytes[x]));
                        if (++x == _bytes.Length) return sb.ToString().Trim();
                        if (i == 8) break;
                        sb.Append(" ");
                    }
                    sb.Append("-");
                }
                sb.Append("\r\n");
            }
        }

        public static string ToDec(this byte[] _bytes)
        {
            string s = "";
            foreach (byte b in _bytes)
                s += b + " ";
            return s;
        }
        public static string ToStr(this byte[] _Bytes)
        {
            return Encoding.Default.GetString(_Bytes);
        }
        public static byte[] Trace(this byte[] t)
        {
            string s = "";
            foreach (byte b in t)
                s += b + ",";
            s.Trace();
            return t;
        }
        public static byte[] ToBytes(this string _String)
        {
            return Encoding.Default.GetBytes(_String);
        }


        public static string Crypter(this string s)
        {
            char[] _chars1 = new char[] { 'o', 'l', 'e', 's' };
            char[] _chars2 = new char[] { '0', 'I', '3', '5' };
            StringBuilder _StringBuilder = new StringBuilder(s);
            for (int i = 0; i < s.Length; i++)
            {
                if (_Random.Next(3) == 1)
                {
                    for (int j = 0; j < _chars1.Length; j++)
                    {
                        if (s[i] == _chars1[j])
                        {
                            _StringBuilder[i] = _chars2[j];
                            break;
                        }
                    }
                }
            }
            return _StringBuilder.ToString();
        }

        public static byte[] Join(this byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            int j = 0;
            for (int i = 0; i < a.Length; i++, j++)
            {
                c[j] = a[i];
            }
            for (int i = 0; i < b.Length; i++, j++)
            {
                c[j] = b[i];
            }
            return c;
        }
        public static byte[][] Split(this byte[] source, int pos)
        {
            byte[] _bytes = new byte[source.Length - pos];
            byte[] _bytes2 = new byte[pos];
            for (int i = 0; i < pos; i++)
            {
                _bytes2[i] = source[i];
            }
            for (int i = pos; i < source.Length; i++)
            {
                _bytes[i - pos] = source[i];
            }
            return new byte[2][] { _bytes, _bytes2 };
        }
        public static byte[] Cut(this byte[] source, int start, out byte[] _bytes2)
        {
            byte[][] bss = source.Split(start);
            _bytes2 = bss[1];
            return bss[0];
        }
        public static byte[] Substr(this byte[] source, int length)
        {
            byte[] _bytes2 = new byte[length];
            for (int i = 0; i < length; i++)
            {
                _bytes2[i] = source[i];
            }
            return _bytes2;
        }
        public static string Substr(this string s, string a)
        {
            return s.Substring(0, s.IndexOf(a));
        }
        public static byte[] Cut(this byte[] source, int start)
        {
            byte[] _bytes = new byte[source.Length - start];
            for (int i = start; i < source.Length; i++)
            {
                _bytes[i - start] = source[i];
            }
            return _bytes;
        }
        public static bool Contains(this byte[] _bytes, string s)
        {
            return _bytes.ToStr().Contains(s);
        }
        private static Random _Random = new Random();

        public static int IndexOf2(this byte[] source, string pattern)
        {
            return IndexOf2(source, Encoding.Default.GetBytes(pattern));
        }
        public static int IndexOf2<T>(this T[] source, T[] pattern)
        {
            return IndexOf2(source, pattern, 0);
        }


        public static int IndexOf2<T>(this T[] source, T[] pattern, int startpos)
        {
            for (int i = startpos; i < source.Length; i++)
            {
                if (source.Length - i < pattern.Length) return -1;
                if (Helper.Compare(source, pattern, i)) return i;
            }
            return -1;
        }
    }

#if(WINFORMS)
    //[DebuggerStepThrough] public static class Win32
    {
        using System.Windows.Forms;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;


        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using(Process curProcess = Process.GetCurrentProcess())
            using(ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        public delegate bool OnKeyDown(Keys key,bool down);
        private static OnKeyDown onKeyDown;        
        public static OnKeyDown _OnKeyDown
        {
            get { return onKeyDown; }
            set
            {
                if(value != null && _hookID != null) _hookID = SetHook(_proc);
                else UnhookWindowsHookEx(_hookID);
                onKeyDown = value;
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if(nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if(onKeyDown != null) onKeyDown((Keys)vkCode, wParam == (IntPtr)WM_KEYDOWN);                
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        public static String GetSelectedText(bool all)
        {
            IntPtr hWnd = GetForegroundWindow();
            uint processId;
            uint activeThreadId = GetWindowThreadProcessId(hWnd, out processId);
            uint currentThreadId = GetCurrentThreadId();
            AttachThreadInput(activeThreadId, currentThreadId, true);
            IntPtr focusedHandle = GetFocus();
            AttachThreadInput(activeThreadId, currentThreadId, false);
            int len = SendMessage(focusedHandle, WM_GETTEXTLENGTH, 0, null);
            StringBuilder sb = new StringBuilder(len);
            int numChars = SendMessage(focusedHandle, WM_GETTEXT, len + 1, sb);
            if(all)
                return sb.ToString();
            else
            {
                int start, next;
                SendMessage(focusedHandle, EM_GETSEL, out start, out next);
                return sb.ToString().Substring(start, next - start);
            }
            
        }        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo,
        bool fAttach);
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);
        // second overload of SendMessage
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, out int wParam, out int lParam);
        const uint WM_GETTEXT = 0x0D;
        const uint WM_GETTEXTLENGTH = 0x0E;
        const uint EM_GETSEL = 0xB0;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible, string title)
        {
            IntPtr hWnd = FindWindow(null, title);
            if (hWnd != IntPtr.Zero)
            {
                if (!visible)
                    ShowWindow(hWnd, 0);
                else
                    ShowWindow(hWnd, 1);
            }
        }
        public static void HideConsoleBar(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                style &= ~WS_EX_APPWINDOW;
                SetWindowLong(hWnd, GWL_EXSTYLE, style);
            }
            else Debugger.Break();
        }

        public static void HideConsoleBar(string title)
        {
            IntPtr hWnd = FindWindow(null, title);
            HideConsoleBar(hWnd);
        }
        public static int WS_EX_APPWINDOW = 0x40000;
        public static int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateWaitableTimer(IntPtr
        lpTimerAttributes,
        bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll")]
        private static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long
        pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr
        lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        private static extern Int32 WaitForSingleObject(IntPtr handle, uint
        milliseconds);

        static IntPtr handle;
        public static void SetWaitForWakeUpTime(int secconds)
        {
            long duetime = -10000000 * secconds;
            handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer");
            SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
            IntPtr.Zero, true);
            //duetime = -t;
            Console.WriteLine("{0:x}", duetime);
            handle = CreateWaitableTimer(IntPtr.Zero, true,
            "MyWaitabletimer");
            SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero,
            IntPtr.Zero, true);
            uint INFINITE = 0xFFFFFFFF;
            int ret = WaitForSingleObject(handle, INFINITE);
            //MessageBox.Show("Wake up call");
        }


        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static long GetTickCount()
        {
            return Environment.TickCount;
        }

        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }
    }
#endif
#if (!SILVERLIGHT)
    [XmlRoot("dictionary")]
    //[DebuggerStepThrough]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
    #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (TKey key in this.Keys)
            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion
    }
    //[DebuggerStepThrough]
    public class MemoryStreamA : MemoryStream
    {
        public SortedList<int, byte[]> _List = new SortedList<int, byte[]>();
        public int _i;
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                int a = base.Read(buffer, offset, count);
                if (a > 0) return a;
                Thread.Sleep(20);
            }
        }
        public override int ReadByte()
        {
            int b;
            while (true)
            {
                b = base.ReadByte();
                if (b != -1) break;
                Thread.Sleep(20);
            }
            return b;
        }
        public void Write(byte[] buffer, int index)
        {
            _List.Add(index, buffer);
            if (_i == 0) _i = index - 1;
            if (index <= _i) throw new Exception("Cannot Write Index Error");
            while (true)
            {
                if (_List.ContainsKey(_i + 1))
                {
                    _i++;
                    Write(_List[_i], 0, _List[_i].Length);
                }
                else
                    break;
            }

        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            long oldpos = Position;
            Seek(0, SeekOrigin.End);
            base.Write(buffer, offset, count);
            Seek(oldpos, SeekOrigin.Begin);
        }
    }
    //[DebuggerStepThrough]
    public class PolicyServer
    {
        class PolicyConnection
        {
            private Socket m_connection;
            private byte[] m_buffer;
            private int m_received;
            private byte[] m_policy;
            private static string s_policyRequestString = "<policy-file-request/>";
            public PolicyConnection(Socket client, byte[] policy)
            {
                m_connection = client;
                m_policy = policy;
                m_buffer = new byte[s_policyRequestString.Length];
                m_received = 0;
                try
                {
                    m_connection.BeginReceive(m_buffer, 0, s_policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                }
                catch (SocketException)
                {
                    m_connection.Close();
                }
            }
            private void OnReceive(IAsyncResult res)
            {
                try
                {
                    m_received += m_connection.EndReceive(res);
                    if (m_received < s_policyRequestString.Length)
                    {
                        m_connection.BeginReceive(m_buffer, m_received, s_policyRequestString.Length - m_received, SocketFlags.None, new AsyncCallback(OnReceive), null);
                        return;
                    }
                    string request = System.Text.Encoding.UTF8.GetString(m_buffer, 0, m_received);
                    if (StringComparer.InvariantCultureIgnoreCase.Compare(request, s_policyRequestString) != 0)
                    {
                        m_connection.Close();
                        return;
                    }
                    m_connection.BeginSend(m_policy, 0, m_policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                }
                catch (SocketException)
                {
                    m_connection.Close();
                }
            }
            public void OnSend(IAsyncResult res)
            {
                try
                {
                    m_connection.EndSend(res);
                }
                finally
                {
                    m_connection.Close();
                }
            }
        }
        private Socket m_listener;
        private byte[] m_policy;
        public string policyFile;
        public int _PolicyPort { get { return 943; } }
        public void StartAsync()
        {
            Console.WriteLine("PolicyServer Started");

            if (policyFile == null)
            {
                m_policy = @"<?xml version=""1.0"" encoding =""utf-8""?>
<access-policy>
  <cross-domain-access>
    <policy>
      <allow-from>
        <domain uri=""*"" />
      </allow-from>
      <grant-to>
        <socket-resource port=""4530"" protocol=""tcp"" />
        <socket-resource port=""4531"" protocol=""tcp"" />
        <socket-resource port=""4532"" protocol=""tcp"" />
        <socket-resource port=""4533"" protocol=""tcp"" />
        <socket-resource port=""4534"" protocol=""tcp"" />                
      </grant-to>
    </policy>
  </cross-domain-access>
</access-policy>".ToBytes();
            }
            else m_policy = File.ReadAllBytes(policyFile);
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listener.Bind(new IPEndPoint(IPAddress.Any, _PolicyPort));
            m_listener.Listen(10);
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }

        public void OnConnection(IAsyncResult res)
        {
            Socket client = null;
            try
            {
                client = m_listener.EndAccept(res);
            }
            catch (SocketException)
            {
                return;
            }
            Trace.WriteLine("Client Connected " + ((IPEndPoint)client.RemoteEndPoint).Address);
            PolicyConnection pc = new PolicyConnection(client, m_policy);
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }
        public void Close()
        {
            m_listener.Close();
        }
    }
    //[DebuggerStepThrough]
    public class intA
    {

        public override string ToString()
        {
            return i.ToString();
        }
        string file;
        public intA(string s)
        {
            file = s;
        }
        int? _i;

        public int i
        {
            get
            {
                lock ("intA")
                {
                    if (_i != null) return _i.Value;
                    else
                    {
                        if (File.Exists(file)) _i = int.Parse(File.ReadAllText(file));
                        else _i = 0;
                    }
                    return _i.Value;
                }
            }
            set
            {
                lock ("intA")
                {
                    _i = value; File.WriteAllText(file, _i.ToString());
                }
            }
        }
    }
    //[DebuggerStepThrough]
    public class ListA : List<string>
    {
        string file;
        public ListA(string file)
        {
            this.file = file;
            if (File.Exists(file))
                foreach (string s2 in File.ReadAllLines(file))
                {
                    base.Add(s2);
                }
        }
        public new bool Add(string s)
        {
            if (!Contains(s))
            {
                base.Add(s);
                return true;
            }
            else return false;
        }
        public void Flush()
        {
            File.WriteAllLines(file, this.ToArray(), Encoding.Default);
        }
    }

    //[DebuggerStepThrough]
    public class ListB<T> : List<T>
    {
        public ListB()
        {
        }
        string file;
        public ListB(string file)
        {
            this.file = file;
            _XmlSerializer = new XmlSerializer(this.GetType(), new Type[] { typeof(T) });
            if (File.Exists(file))
                using (FileStream _FileStream = File.Open(file, FileMode.Open))
                {
                    List<T> list = (List<T>)_XmlSerializer.Deserialize(_FileStream);
                    foreach (T t in list)
                    {
                        Add(t);
                    }
                }
        }
        XmlSerializer _XmlSerializer;

        public void Flush()
        {
            lock ("flush")
            {
                using (FileStream _FileStream = new FileStream("users.xml", FileMode.Create, FileAccess.Write))
                    _XmlSerializer.Serialize(_FileStream, this);
            }
        }
    }

    

    //[DebuggerStepThrough]
    public class TcpRedirect
    {
        public string _LocalhostReplace;
        public delegate Socket GetSocket();
        public event GetSocket _RemoteIpDelegate;
        public int _LocalPort;
        Thread _Thread;
        public void StartAsync()
        {
            _Thread = new Thread(Start);
            _Thread.IsBackground = true;
            _Thread.Start();
        }
        const string hosts = @"C:\WINDOWS\system32\drivers\etc\hosts";
        TcpListener _TcpListener;

        public void Start()
        {
            if (Directory.Exists("logs")) Directory.Delete("logs", true);
            if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
            Thread.Sleep(500);
            _TcpListener = new TcpListener(IPAddress.Any, _LocalPort);
            _TcpListener.Start();
            int i = 0;
            try
            {
                while (true)
                {
                    i++;
                    Socket _LocalSocket = _TcpListener.AcceptSocket();
                    File.WriteAllText(hosts, "");
                    Socket _RemoteSocket = _RemoteIpDelegate();
                    if (_LocalhostReplace != null) File.WriteAllText(hosts, "127.0.0.1 " + _LocalhostReplace);
                    Trace.Write("<<<<<Client Connected>>>>>>>>" + i + "\n");
                    Client _LocalListen = new Client() { _ListenSocket = _LocalSocket, _SendToSocket = _RemoteSocket, _name = i + "Sended" };
                    _LocalListen.StartAsync();
                    Client _RemoteListen = new Client() { _ListenSocket = _RemoteSocket, _SendToSocket = _LocalSocket, _name = i + "Received" };
                    _RemoteListen.StartAsync();
                    Thread.Sleep(2);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("TpcRedirect: " + e.Message);
            }
        }
        public void Stop()
        {
            _TcpListener.Stop();
            File.WriteAllText(hosts, "");
            Thread.Sleep(500);
        }
        //[DebuggerStepThrough]
        public class Client
        {
            public Socket _ListenSocket;
            public Socket _SendToSocket;
            public string _name;
            public void StartAsync()
            {
                Thread _Thread = new Thread(Start);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
            void Start()
            {
                Directory.CreateDirectory("logs");
                FileStream _FileStream = new FileStream("logs/" + _name + ".txt", FileMode.Create, FileAccess.Write);

                Byte[] data = new byte[99999];
                try
                {
                    while (true)
                    {
                        int count = _ListenSocket.Receive(data);
                        if (count == 0) break;
                        Trace.WriteLine(_name + "<<<<<<<<<<<<<<<<<<<<" + count + " Bytes>>>>>>>>>>>>>>>>>");
                        Trace.WriteLine(Encoding.Default.GetString(data, 0, count));
                        _FileStream.Write(data, 0, count);
                        _SendToSocket.Send(data, count, SocketFlags.None);
                    }
                }
                catch (SocketException e) { Trace.WriteLine(e.Message); }
                Trace.WriteLine(_name + " <<<<<<<<<<<<<<Disconnected>>>>>>>>>>>>");
                _ListenSocket.Close();
                _SendToSocket.Close();
                _FileStream.Close();
            }
        }
    }

    //[DebuggerStepThrough]
    public static class Proxy
    {
        public static Socket Socks5Connect(string _proxyAddress, int _proxyPort, string _DestAddress, int _DestPort)
        {
            TcpClient _TcpClient = new TcpClient(_proxyAddress, _proxyPort);
            Socket _Socket = _TcpClient.Client;

            _Socket.Send(new byte[] { 5, 1, 0 });
            byte[] _bytes = _Socket.Receive(2);
            Trace.WriteLine("<<<<<<<socks5 received1>>>>>>>>");
            if (_bytes[0] != 5 && _bytes[1] != 1) throw new ExceptionA();
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                _BinaryWriter.Write(new byte[] {
                    5, // version
                    1, // tcp stream
                    0, // reserved
                    3 //type - domainname 
                });
                _BinaryWriter.Write(_DestAddress);
                _BinaryWriter.Write(BitConverter.GetBytes((UInt16)_DestPort).ReverseA());
                _Socket.Send(_MemoryStream.ToArray());
            }
            Trace.WriteLine("<<<<<<<<socks5 received2>>>>>>>>");
            byte[] _response = _Socket.Receive(10);
            if (_response[1] != 0) throw new ExceptionA("socket Error: " + _response[1]);
            return _Socket;
        }
    }
    //[DebuggerStepThrough]
    public static class Http
    {

        public static string Length(string _bytes)
        {
            Helper.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
            return _bytes;
        }
        public static void Length(ref string _bytes)
        {
            Helper.Replace(ref _bytes, "_length_", (_bytes.Length - 4 - _bytes.IndexOf("\r\n\r\n")).ToString(), 1);
        }
        public static void Length(ref byte[] _bytes)
        {
            Helper.Replace(ref _bytes, "_length_".ToBytes(), (_bytes.Length - 4 - _bytes.IndexOf2("\r\n\r\n")).ToString().ToBytes(), 1);
        }
        public static byte[] ReadHttp(Socket _Socket)
        {
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.ReadTimeout = 10000;
            return ReadHttp(_NetworkStream);
        }
        public static byte[] ReadHttp(Stream _Stream)
        {
            byte[] _headerbytes;


            _headerbytes = _Stream.Cut("\r\n\r\n");
            byte[] _Content = null;
            string _header = Encoding.Default.GetString(_headerbytes);
            Match _Match = Regex.Match(_header, @"Content-Length: (\d+)");
            if (_Match.Success)
            {
                int length = int.Parse(_Match.Groups[1].Value);
                if (length == 0) return _headerbytes;
                _Content = _Stream.Read(length);
            }
            else if (Regex.IsMatch(_header, "Transfer-Encoding: chunked"))
            {
                _Content = ReadChunk(_Stream);
            }
            else //if (Regex.IsMatch(_header, @"Proxy-Connection\: close|Connection\: close",RegexOptions.IgnoreCase))
            {
                _Content = DownloadHttp(_Stream);
            }
            //else throw new ExceptionA("Header Error:"+_header);

            if (Regex.IsMatch(_header, "Content-Encoding: gzip"))
            {
                _Content = Unpack(_Content);
            }
            return _headerbytes.Join(_Content);

        }

        private static byte[] DownloadHttp(Stream _Stream)
        {
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                while (true)
                {
                    int i = _Stream.ReadByte();
                    if (i == -1) return _MemoryStream.ToArray();
                    _MemoryStream.WriteByte((byte)i);
                }
            }
        }
        private static byte[] Unpack(byte[] _bytes)
        {
            GZipStream _GZipStream = new GZipStream(new MemoryStream(_bytes), CompressionMode.Decompress, false);
            byte[] _buffer2 = new byte[99999];
            int count = _GZipStream.Read(_buffer2, 0, _buffer2.Length);
            return _buffer2.Substr(count);
        }

        static readonly byte[] _rn = new byte[] { 13, 10 };
        public static byte[] ReadChunk(Stream _Stream)
        {

            MemoryStream _MemoryStream = new MemoryStream();
            while (true)
            {
                byte[] _bytes = _Stream.Cut("\r\n");
                int length = int.Parse(Encoding.Default.GetString(_bytes), System.Globalization.NumberStyles.HexNumber);
                if (length == 0) break;
                _MemoryStream.Write(_Stream.Read(length), 0, length);
                if (!Helper.Compare(_Stream.Read(2), _rn)) throw new ExceptionA("ReadChunk: cant find Chunk");
            }
            return _MemoryStream.ToArray();
        }


    }
    //[DebuggerStepThrough]
    public class DMime
    {
        static Dictionary<String, string> _Mimes = new Dictionary<string, string>();
        static DMime()
        {
            foreach (Match _Match in Regex.Matches(mimetypes, @"([\w.*]+)\s+([\w/\-.]+)"))
            {
                string key = _Match.Groups[1].Value;
                string value = _Match.Groups[2].Value;
                _Mimes[key] = value;
            }
        }
        public static string GetMime(string _type)
        {
            if (_Mimes.ContainsKey(_type)) return _Mimes[_type];
            return _Mimes[".*"];
        }
        public const string mimetypes = @".* 	 application/octet-stream
.323 	 text/h323
.acx 	 application/internet-property-stream
.ai 	 application/postscript
.aif 	 audio/x-aiff
.aifc 	 audio/aiff
.aiff 	 audio/aiff
.application 	 application/x-ms-application
.asf 	 video/x-ms-asf
.asr 	 video/x-ms-asf
.asx 	 video/x-ms-asf
.au 	 audio/basic
.avi 	 video/x-msvideo
.axs 	 application/olescript
.bas 	 text/plain
.bcpio 	 application/x-bcpio
.bin 	 application/octet-stream
.bmp 	 image/bmp
.c 	 text/plain
.cat 	 application/vndms-pkiseccat
.cdf 	 application/x-cdf
.cer 	 application/x-x509-ca-cert
.clp 	 application/x-msclip
.cmx 	 image/x-cmx
.cod 	 image/cis-cod
.cpio 	 application/x-cpio
.crd 	 application/x-mscardfile
.crl 	 application/pkix-crl
.crt 	 application/x-x509-ca-cert
.csh 	 application/x-csh
.css 	 text/css
.dcr 	 application/x-director
.deploy 	 application/octet-stream
.der 	 application/x-x509-ca-cert
.dib 	 image/bmp
.dir 	 application/x-director
.disco 	 text/xml
.dll 	 application/x-msdownload
.doc 	 application/msword
.dot 	 application/msword
.dvi 	 application/x-dvi
.dxr 	 application/x-director
.eml 	 message/rfc822
.eps 	 application/postscript
.etx 	 text/x-setext
.evy 	 application/envoy
.exe 	 application/octet-stream
.fif 	 application/fractals
.flr 	 x-world/x-vrml
.gif 	 image/gif
.gtar 	 application/x-gtar
.gz 	 application/x-gzip
.h 	 text/plain
.hdf 	 application/x-hdf
.hlp 	 application/winhlp
.hqx 	 application/mac-binhex40
.hta 	 application/hta
.htc 	 text/x-component
.htm 	 text/html
.html 	 text/html
.htt 	 text/webviewhtml
.ico 	 image/x-icon
.ief 	 image/ief
.iii 	 application/x-iphone
.ins 	 application/x-internet-signup
.isp 	 application/x-internet-signup
.IVF 	 video/x-ivf
.jfif 	 image/pjpeg
.jpe 	 image/jpeg
.jpeg 	 image/jpeg
.jpg 	 image/jpeg
.js 	 application/x-javascript
.latex 	 application/x-latex
.lsf 	 video/x-la-asf
.lsx 	 video/x-la-asf
.m13 	 application/x-msmediaview
.m14 	 application/x-msmediaview
.m1v 	 video/mpeg
.m3u 	 audio/x-mpegurl
.man 	 application/x-troff-man
.manifest 	 application/x-ms-manifest
.mdb 	 application/x-msaccess
.me 	 application/x-troff-me
.mht 	 message/rfc822
.mhtml 	 message/rfc822
.mid 	 audio/mid
.mmf 	 application/x-smaf
.mny 	 application/x-msmoney
.mov 	 video/quicktime
.movie 	 video/x-sgi-movie
.mp2 	 video/mpeg
.mp3 	 audio/mpeg
.mpa 	 video/mpeg
.mpe 	 video/mpeg
.mpeg 	 video/mpeg
.mpg 	 video/mpeg
.mpp 	 application/vnd.ms-project
.mpv2 	 video/mpeg
.ms 	 application/x-troff-ms
.mvb 	 application/x-msmediaview
.nc 	 application/x-netcdf
.nws 	 message/rfc822
.oda 	 application/oda
.ods 	 application/oleobject
.p10 	 application/pkcs10
.p12 	 application/x-pkcs12
.p7b 	 application/x-pkcs7-certificates
.p7c 	 application/pkcs7-mime
.p7m 	 application/pkcs7-mime
.p7r 	 application/x-pkcs7-certreqresp
.p7s 	 application/pkcs7-signature
.pbm 	 image/x-portable-bitmap
.pdf 	 application/pdf
.pfx 	 application/x-pkcs12
.pgm 	 image/x-portable-graymap
.pko 	 application/vndms-pkipko
.pma 	 application/x-perfmon
.pmc 	 application/x-perfmon
.pml 	 application/x-perfmon
.pmr 	 application/x-perfmon
.pmw 	 application/x-perfmon
.png 	 image/png
.pnm 	 image/x-portable-anymap
.pnz 	 image/png
.pot 	 application/vnd.ms-powerpoint
.ppm 	 image/x-portable-pixmap
.pps 	 application/vnd.ms-powerpoint
.ppt 	 application/vnd.ms-powerpoint
.prf 	 application/pics-rules
.ps 	 application/postscript
.pub 	 application/x-mspublisher
.qt 	 video/quicktime
.ra 	 audio/x-pn-realaudio
.ram 	 audio/x-pn-realaudio
.ras 	 image/x-cmu-raster
.rgb 	 image/x-rgb
.rmi 	 audio/mid
.roff 	 application/x-troff
.rtf 	 application/rtf
.rtx 	 text/richtext
.scd 	 application/x-msschedule
.sct 	 text/scriptlet
.setpay 	 application/set-payment-initiation
.setreg 	 application/set-registration-initiation
.sh 	 application/x-sh
.shar 	 application/x-shar
.sit 	 application/x-stuffit
.smd 	 audio/x-smd
.smx 	 audio/x-smd
.smz 	 audio/x-smd
.snd 	 audio/basic
.spc 	 application/x-pkcs7-certificates
.spl 	 application/futuresplash
.src 	 application/x-wais-source
.sst 	 application/vndms-pkicertstore
.stl 	 application/vndms-pkistl
.stm 	 text/html
.sv4cpio 	 application/x-sv4cpio
.sv4crc 	 application/x-sv4crc
.t 	 application/x-troff
.tar 	 application/x-tar
.tcl 	 application/x-tcl
.tex 	 application/x-tex
.texi 	 application/x-texinfo
.texinfo 	 application/x-texinfo
.tgz 	 application/x-compressed
.tif 	 image/tiff
.tiff 	 image/tiff
.tr 	 application/x-troff
.trm 	 application/x-msterminal
.tsv 	 text/tab-separated-values
.txt 	 text/plain
.uls 	 text/iuls
.ustar 	 application/x-ustar
.vcf 	 text/x-vcard
.wav 	 audio/wav
.wbmp 	 image/vnd.wap.wbmp
.wcm 	 application/vnd.ms-works
.wdb 	 application/vnd.ms-works
.wks 	 application/vnd.ms-works
.wmf 	 application/x-msmetafile
.wps 	 application/vnd.ms-works
.wri 	 application/x-mswrite
.wrl 	 x-world/x-vrml
.wrz 	 x-world/x-vrml
.wsdl 	 text/xml
.xaf 	 x-world/x-vrml
.xbm 	 image/x-xbitmap
.xla 	 application/vnd.ms-excel
.xlc 	 application/vnd.ms-excel
.xlm 	 application/vnd.ms-excel
.xls 	 application/vnd.ms-excel
.xlt 	 application/vnd.ms-excel
.xlw 	 application/vnd.ms-excel
.xml 	 text/xml
.xof 	 x-world/x-vrml
.xpm 	 image/x-xpixmap
.xsd 	 text/xml
.xsl 	 text/xml
.xwd 	 image/x-xwindowdump
.z 	 application/x-compress
.zip 	 application/x-zip-compressed";

    }
    [Obsolete]
    //[DebuggerStepThrough]
    public class Spammer3 : Logging { }
    //[DebuggerStepThrough]
    public class Logging
    {
        public static string title;
        public static string _Title
        {
            set { if (title != value) title = Console.Title = value; }
        }
        public static Random _Random = new Random();

        public static bool done;
        public static bool Beep = true;

        public static void Setup() { Setup("../../"); }
        public static bool _supsend;
        public static IEnumerable<Process> FindProcess(string s)
        {
            IEnumerable<Process> ps = (from p in Process.GetProcesses() where p.ProcessName == s select p);
            return ps;
        }

        public static List<string> _console = new List<string>();

        public static void StartRemoteConsoleAsync(int port)
        {
            Thread _Thread = new Thread(StartRemoteConsole);
            _Thread.IsBackground = true;
            _Thread.Start(port);
        }
        private static void StartRemoteConsole(object o)
        {
            int port = (int)o;
            TcpListener _TcpListener = new TcpListener(IPAddress.Any, port);
            _TcpListener.Start();
            while (true)
            {
                Socket _Socket = _TcpListener.AcceptSocket();
                Thread _Thread = new Thread(StartListenClient);
                _Thread.IsBackground = true;
                _Thread.Start(_Socket);
            }
        }

        private static void StartListenClient(object o)
        {
            Socket _Socket = (Socket)o;
            "Console Connected".Trace();
            NetworkStream _NetworkStream = new NetworkStream(_Socket);
            TextWriterTraceListener _TextWriterTraceListener = new TextWriterTraceListener(_NetworkStream);
            Trace.Listeners.Add(_TextWriterTraceListener);
            try
            {
                while (true)
                {
                    string s = _NetworkStream.ReadLine();
                    _console.Add(s);
                }
            }
            catch (IOException) { }
            Trace.Listeners.Remove(_TextWriterTraceListener);
            "Console Disconnected".Trace();
        }
        private static void StartReadConsole()
        {
            while (true)
                _console.Add(Console.ReadLine());
        }
        public static void Setup(string s)
        {
            if (Directory.Exists("./logs/")) Directory.Delete("logs", true);
            if (done == true) return;
            done = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Process _Process = Process.GetCurrentProcess();
            if (!_AllowDuplicates && FindProcess(_Process.ProcessName).Count() > 1)
            {
                Console.Beep(100, 10);
                _Process.Kill();
            }
            Directory.SetCurrentDirectory(s);
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            if (Console.LargestWindowHeight != 0)
            {
                new Thread(StartReadConsole).StartBackground();
                Console.Title = Assembly.GetEntryAssembly().GetName().Name;
                Trace.Listeners.Add(new TextWriterTraceListener(Console.OpenStandardOutput()));
            }
            Trace.AutoFlush = true;
            Trace.WriteLine("Programm Started " + DateTime.Now);
        }
        public static bool _AllowDuplicates;
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
            if (Console.LargestWindowHeight != 0 && Beep)
                Console.Beep(100, 10);
        }

    }
#else
    //[DebuggerStepThrough]
    public class File
    {
        public static IsolatedStorageFile _IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForSite();
        public static System.IO.FileStream Create(string path)
        {
            return _IsolatedStorageFile.CreateFile(path);
        }
        public static bool Exists(string path)
        {
            return _IsolatedStorageFile.FileExists(path);
        }
        public static FileStream Open(string path, FileMode mode)
        {
            return _IsolatedStorageFile.OpenFile(path, mode);
        }

    }
    [DebuggerStepThrough()]
    public class Trace : Debug
    {
    }

    [DebuggerStepThrough()]
    public class Debug
    {
        public static void Assert(bool b, string s)
        {
            if (!b) Debugger.Break();
        }        
        public static void Assert(bool b)
        {
            if (!b) Debugger.Break();
        }
        public static void Fail(string s)
        {
            Debugger.Break();
        }
        public static void WriteLine<T>(T o)
        {
            System.Diagnostics.Debug.WriteLine(o);
        }
        public static void Write<T>(T o)
        {
            System.Diagnostics.Debug.WriteLine(o);
        }
    }
#endif

    //[DebuggerStepThrough]
    [Obsolete("use TimerA")]
    public static class STimer
    {
        public static void AddMethod(double _Time, Action _Action)
        {

            _Timer.AddMethod(_Time, _Action);
        }

        static TimerA _Timer = new TimerA();
        public static double _TimeElapsed { get { return _Timer._TimeElapsed; } }
        public static void Update()
        {
            _Timer.Update();
        }
        public static double _SecodsElapsed { get { return _Timer._TimeElapsed / 1000; } }
        public static bool TimeElapsed(int _Milisecconds)
        {
            return _Timer.TimeElapsed(_Milisecconds);
        }

        public static double? GetFps()
        {
            return _Timer.GetFps();
        }
    }
    [Obsolete("use TimerA")]
    //[DebuggerStepThrough]
    public class Timer2 : TimerA { }
    //[DebuggerStepThrough]
    public class TimerA
    {
        DateTime _DateTime = DateTime.Now;
        double oldtime;

        int fpstimes;
        double totalfps;
        public double GetFps()
        {
            if (fpstimes > 0)
            {
                double fps = (totalfps / fpstimes);
                fpstimes = 0;
                totalfps = 0;
                if (fps == double.PositiveInfinity) return 0;
                return fps;
            }
            else return 0;
        }
        double time;
        public void Update()
        {
            while ((DateTime.Now - _DateTime).TotalMilliseconds - oldtime == 0) Thread.Sleep(1);

            time = (DateTime.Now - _DateTime).TotalMilliseconds;
            _TimeElapsed = time - oldtime;
            oldtime = time;
            fpstimes++;
            totalfps += 1000 / _TimeElapsed;

            UpdateActions();
        }

        private void UpdateActions()
        {
            for (int i = _List.Count - 1; i >= 0; i--)
            {
                CA _CA = _List[i];
                _CA._Time -= _TimeElapsed;
                if (_CA._Time < 0)
                {
                    _List.Remove(_CA);
                    _CA._Action();
                }
            }
        }

        public double _TimeElapsed = 0;
        public double _SecodsElapsed { get { return _TimeElapsed / 1000; } }
        public double _oldTime { get { return time - _TimeElapsed; } }
        public bool TimeElapsed(double _Milisecconds)
        {
            if (_TimeElapsed > _Milisecconds) return true;
            if (time % _Milisecconds < _oldTime % _Milisecconds)
                return true;
            else
                return false;
        }

        public void AddMethod(double time, Action _Action)
        {
            if (_List.FirstOrDefault(a => a._Action == _Action) == null)
                _List.Add(new CA { _Action = _Action, _Time = time });
        }

        List<CA> _List = new List<CA>();
        class CA
        {
            public double _Time;
            public Action _Action;
        }
    }

    namespace OldTcp
    {
#if (SILVERLIGHT)
        //[DebuggerStepThrough]
        public class Sender
        {
            public Socket _Socket;
            public void Send(byte[] _Buffer2)
            {
                byte[] _Buffer = new byte[_Buffer2.Length + 1];
                _Buffer[0] = (byte)_Buffer2.Length;
                Buffer.BlockCopy(_Buffer2, 0, _Buffer, 1, _Buffer2.Length);
                if (_Buffer2.Length == 0) throw new Exception("Break");
                SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
                _SocketAsyncEventArgs.SetBuffer(_Buffer, 0, _Buffer.Length);
                _Socket.SendAsync(_SocketAsyncEventArgs);
            }
            public static byte Encode(float _V, float _min, float _max)
            {
                _V = Math.Min(Math.Max(_V, _min), _max);
                float _range = _max - _min;
                float _dopolnenie = _V - _min;
                float _Procent = _dopolnenie / _range;
                float _fullV = _Procent * byte.MaxValue;
                return (byte)Math.Max(Math.Min((byte)_fullV, byte.MaxValue), byte.MinValue);
            }
            public static UInt16 EncodeInt(float _V, float _min, float _max)
            {
                _V = Math.Min(Math.Max(_V, _min), _max);
                float _range = _max - _min;
                float _dopolnenie = _V - _min;
                float _Procent = _dopolnenie / _range;
                float _fullV = _Procent * UInt16.MaxValue;
                return (UInt16)Math.Max(Math.Min((UInt16)_fullV, UInt16.MaxValue), UInt16.MinValue);
            }
        }
        //[DebuggerStepThrough]
        public class Listener
        {
            public Socket _Socket;
            long _position;
            public bool _Connected
            {
                get { return _Socket.Connected; }
            }
            private List<byte[]> _Messages = new List<byte[]>();
            public List<byte[]> GetMessages()
            {
                lock ("Get")
                {
                    List<byte[]> _Return = _Messages;
                    _Messages = new List<byte[]>();
                    return _Return;
                }
            }
            void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs _SocketAsyncEventArgs)
            {
                _MemoryStream.Write(_SocketAsyncEventArgs.Buffer, 0, _SocketAsyncEventArgs.BytesTransferred);
                _MemoryStream.Seek(_position, SeekOrigin.Begin);
                while (true)
                {
                    int _length = _MemoryStream.ReadByte();
                    if (_length == -1 || _MemoryStream.Length <= _position + _length) break;
                    Byte[] _Buffer = new byte[_length];

                    _MemoryStream.Read(_Buffer, 0, _length);
                    _position = _MemoryStream.Position;
                    lock ("Get")
                        _Messages.Add(_Buffer);
                }
                _MemoryStream.Seek(0, SeekOrigin.End);
                StartReceive();
            }
            MemoryStream _MemoryStream = new MemoryStream();
            public void Start()
            {
                StartReceive();
            }
            private void StartReceive()
            {
                SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
                _SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAsyncEventArgs_Completed);
                //_Socket.ReceiveBufferSize = 100;
                //_Socket.SendBufferSize = 100;
                _SocketAsyncEventArgs.SetBuffer(new byte[100], 0, 100);
                _Socket.ReceiveAsync(_SocketAsyncEventArgs);
            }
            public static float Decode(byte _fullV, float _min, float _max)
            {
                float _range = _max - _min;
                float _V1 = ((float)_fullV) / byte.MaxValue;
                float _ranged = _V1 * _range;
                float _V = _ranged + _min;
                return _V;
            }
            public static float DecodeInt(UInt16 _fullV, float _min, float _max)
            {
                float _range = _max - _min;
                float _V1 = ((float)_fullV) / UInt16.MaxValue;
                float _ranged = _V1 * _range;
                float _V = _ranged + _min;
                return _V;
            }
        }
#else
        //[DebuggerStepThrough]
        public class ClientWait
        {
            public int _Port;
            private List<TcpClient> _TcpClients = new List<TcpClient>();

            public void Start()
            {
                TcpListener _TcpListener = new TcpListener(IPAddress.Any, _Port);
                _TcpListener.Start();
                while (true)
                {
                    TcpClient _TcpClient = _TcpListener.AcceptTcpClient();
                    lock ("clientwait")
                        _TcpClients.Add(_TcpClient);
                    Thread.Sleep(10);
                }
            }

            public List<TcpClient> GetClients()
            {
                lock ("clientwait")
                {
                    List<TcpClient> _Return = _TcpClients;
                    _TcpClients = new List<TcpClient>();
                    return _Return;
                }
            }
        }
        //[DebuggerStepThrough]
        public class Sender
        {
            public TcpClient _TcpClient;
            public Socket _Socket { get { return _TcpClient.Client; } }
            public void Send(byte[] _Buffer2)
            {
                if (_TcpClient.Connected)
                    try
                    {
                        byte[] _Buffer3 = new byte[_Buffer2.Length + 1];
                        _Buffer3[0] = (byte)_Buffer2.Length;
                        Buffer.BlockCopy(_Buffer2, 0, _Buffer3, 1, _Buffer2.Length);
                        if (_Buffer2.Length == 0) Debugger.Break();
                        _Socket.Send(_Buffer3);
                    }
                    catch (SocketException e) { Trace.WriteLine(e.Message); }
            }
        }
        //[DebuggerStepThrough]
        public class Listener
        {
            public TcpClient _TcpClient;
            private List<byte[]> _Messages = new List<byte[]>();

            public List<byte[]> GetMessages()
            {
                lock ("Get")
                {
                    List<byte[]> _Return = _Messages;
                    _Messages = new List<byte[]>();
                    return _Return;
                }
            }

            public bool _Connected
            {
                get { return _TcpClient.Connected; }
            }
            public void Start()
            {
                //_TcpClient.ReceiveBufferSize = 100;
                //_TcpClient.SendBufferSize = 100;            
                MemoryStream _MemoryStream = new MemoryStream();
                byte[] _Buffer = new byte[9999];
                long _position = 0;
                while (_TcpClient.Connected)
                {
                    try
                    {
                        int count = _TcpClient.Client.Receive(_Buffer);

                        _MemoryStream.Write(_Buffer, 0, count);
                        _MemoryStream.Seek(_position, SeekOrigin.Begin);
                        while (true)
                        {
                            int _length = _MemoryStream.ReadByte();
                            if (_length == -1 || _MemoryStream.Length <= _position + _length) break;
                            Byte[] _Buffer1 = new byte[_length];

                            _MemoryStream.Read(_Buffer1, 0, _length);
                            _position = _MemoryStream.Position;
                            lock ("Get")
                                _Messages.Add(_Buffer1);
                        }
                        _MemoryStream.Seek(0, SeekOrigin.End);
                    }
                    catch (SocketException) { }
                    catch (IOException) { }
                    Thread.Sleep(1);
                }
            }
        }
#endif
    }
    //first byte is length, if length is more than 254 then first byte is 255 second is uint16 packet length 

#if(!SILVERLIGHT)
    namespace Tcp 
    {
        //[DebuggerStepThrough]
        public class ClientWait
        {
            public int _Port;
            private List<Socket> _Sockets = new List<Socket>();

            public void StartAsync()
            {
                new Thread(Start).StartBackground("ClientWait");
            }
            private void Start()
            {
                TcpListener _TcpListener = new TcpListener(IPAddress.Any, _Port);
                _TcpListener.Start();
                while (true)
                {
                    Socket _Socket = _TcpListener.AcceptSocket();
                    lock ("clientwait")
                        _Sockets.Add(_Socket);
                    Thread.Sleep(10);
                }
            }

            public List<Socket> GetClients()
            {
                lock ("clientwait")
                {
                    List<Socket> _Return = _Sockets;
                    _Sockets = new List<Socket>();
                    return _Return;
                }
            }
        }
        //[DebuggerStepThrough]
        public class Listener
        {
            public Socket _Socket;
            List<byte[]> _Messages = new List<byte[]>();
            public void Start()
            {
                NetworkStream _NetworkStream = new NetworkStream(_Socket);
                try
                {
                    while (true)
                    {
                        byte[] split = _NetworkStream.Read(2); //every packet begins with "**" 42,42
                        if (!split.Equals2(new byte[] { 42, 42 })) throw new Exception("dammaged packet");
                        UInt16 length = _NetworkStream.ReadB(); //length
                        if (length == 255)
                            length = _NetworkStream.ReadUInt16();
                        byte[] bytes = _NetworkStream.Read(length); //bytes
                        _Messages.Add(bytes);//add to packets buffer
                    }
                }
                catch (IOException) { }
            }
            public bool _Connected { get { return _Socket.Connected; } }
            public List<byte[]> GetMessages()
            {
                lock ("Get")
                {
                    List<byte[]> _Return = _Messages;
                    //if (_Messages.Count > 0) Debugger.Break();
                    _Messages = new List<byte[]>();
                    return _Return;
                }
            }
            Thread _Thread;
            internal void StartAsync(string s)
            {
                 _Thread = new Thread(Start).StartBackground(s);
            }
        }
        //[DebuggerStepThrough]
        public class Sender
        {
            public Socket _Socket;
            public void Send(byte[] _bytes)
            {
                try
                {
                    Debug.Assert(_bytes.Length > 0);
                    Debug.Assert(_Socket.Connected);
                    byte[] _bytes2;
                    if (_bytes.Length > 254)
                    {
                        Debug.Assert(_bytes.Length < UInt16.MaxValue);
                        _bytes2 = Helper.JoinBytes(new byte[] { 42, 42, 255 }, BitConverter.GetBytes((UInt16)_bytes.Length), _bytes);
                    }
                    else
                        _bytes2 = Helper.JoinBytes(new byte[] { 42, 42, (byte)_bytes.Length }, _bytes);
                    _Socket.Send(_bytes2);
                }
                catch (SocketException) { }

            }
        }
    }
#else
    namespace TcpSilverlight
    {
        ////[DebuggerStepThrough]
        public class NetworkStream : MemoryStream
        {
            public bool Connected { get { return _Socket.Connected; } }
            public override string ToString()
            {
                return base.ToString() + " " + Length + " " + Connected;
            }
            public Socket _Socket;
            public NetworkStream(Socket s)
            {
                _Socket = s;
                StartReceive();
            }

            private void StartReceive()
            {
                SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
                _SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAsyncEventArgs_Completed);
                _SocketAsyncEventArgs.SetBuffer(new byte[1024], 0, 1024);
                _Socket.ReceiveAsync(_SocketAsyncEventArgs);
            }

            void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                long pos = Position;
                Seek(0, SeekOrigin.End);
                base.Write(e.Buffer, 0, e.BytesTransferred);
                Position = pos;
                StartReceive();
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                while (Position == Length) Thread.Sleep(2);
                if (Position == Length) return 0;
                else
                    return base.Read(buffer, offset, count);
            }
            public override int ReadByte()
            {
                while (Position == Length) Thread.Sleep(2);
                return base.ReadByte();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                _Socket.Send(buffer);
            }
        }
        //[DebuggerStepThrough]
        public class Sender
        {
            public NetworkStream _NetworkStream;
            public void Send(byte[] _Buffer)
            {                
                Trace.Assert(_Buffer.Length > 0);
                byte[] bytes;
                if (_Buffer.Length < 255)
                    bytes = Helper.JoinBytes(42, 42, (byte)_Buffer.Length, _Buffer);
                else
                {
                    bytes = Helper.JoinBytes(42, 42, (byte)255, BitConverter.GetBytes(_Buffer.Length), _Buffer);
                    Trace.Assert(_Buffer.Length < UInt16.MaxValue);
                }
                _NetworkStream.Write(bytes);
            }
            public static byte Encode(float _V, float _min, float _max)
            {
                _V = Math.Min(Math.Max(_V, _min), _max);
                float _range = _max - _min;
                float _dopolnenie = _V - _min;
                float _Procent = _dopolnenie / _range;
                float _fullV = _Procent * byte.MaxValue;
                return (byte)Math.Max(Math.Min((byte)_fullV, byte.MaxValue), byte.MinValue);
            } //converting float to byte
            public static UInt16 EncodeInt(float _V, float _min, float _max)
            {
                _V = Math.Min(Math.Max(_V, _min), _max);
                float _range = _max - _min;
                float _dopolnenie = _V - _min;
                float _Procent = _dopolnenie / _range;
                float _fullV = _Procent * UInt16.MaxValue;
                return (UInt16)Math.Max(Math.Min((UInt16)_fullV, UInt16.MaxValue), UInt16.MinValue);
            } //converting float to uint16
        }
        //[DebuggerStepThrough]
        public class Listener
        {
            public NetworkStream _NetworkStream;            
            public bool _Connected
            {
                get { return _NetworkStream.Connected; }
            }
            private List<byte[]> _Messages = new List<byte[]>();
            public List<byte[]> GetMessages()
            {
                Trace.Assert(_Connected);
                lock ("Get")
                {
                    List<byte[]> _Return = _Messages;
                    _Messages = new List<byte[]>();
                    return _Return;
                }
            }
            
            MemoryStream _MemoryStream = new MemoryStream();
            public void StartAsync()
            {
                new Thread(Start).StartBackground("listener");                
            }
            private void Start()
            {
                try
                {
                    while (true)
                    {
                        byte[] split = _NetworkStream.Read(2); //every packet begins with "**" 42,42
                        if (!split.Equals2(new byte[] { 42, 42 })) throw new Exception("dammaged packet");
                        UInt16 length = _NetworkStream.ReadB(); //length
                        if (length == 255)
                            length = _NetworkStream.ReadUInt16();
                        byte[] bytes = _NetworkStream.Read(length); //bytes
                        _Messages.Add(bytes);//add to packets buffer
                    }
                }
                catch (AccessViolationException) { }
            }
            public static float Decode(byte _fullV, float _min, float _max)
            {
                float _range = _max - _min;
                float _V1 = ((float)_fullV) / byte.MaxValue;
                float _ranged = _V1 * _range;
                float _V = _ranged + _min;
                return _V;
            }
            public static float DecodeInt(UInt16 _fullV, float _min, float _max)
            {
                float _range = _max - _min;
                float _V1 = ((float)_fullV) / UInt16.MaxValue;
                float _ranged = _V1 * _range;
                float _V = _ranged + _min;
                return _V;
            }
        }
        ////[DebuggerStepThrough]
        //public class Sender
        //{
        //    public Socket _Socket;
        //    public void Send(byte[] _Buffer)
        //    {
        //        SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
        //        Trace.Assert(_Buffer.Length > 0);
        //        byte[] bytes;
        //        if (_Buffer.Length < 255)
        //            bytes = Helper.JoinBytes(42, 42, (byte)_Buffer.Length, _Buffer);
        //        else
        //        {
        //            bytes = Helper.JoinBytes(42, 42, (byte)255, BitConverter.GetBytes(_Buffer.Length), _Buffer);
        //            Trace.Assert(_Buffer.Length < UInt16.MaxValue);
        //        }
        //        _SocketAsyncEventArgs.SetBuffer(bytes, 0, bytes.Length);
        //        bool b = _Socket.SendAsync(_SocketAsyncEventArgs);
        //    }
        //    public static byte Encode(float _V, float _min, float _max)
        //    {
        //        _V = Math.Min(Math.Max(_V, _min), _max);
        //        float _range = _max - _min;
        //        float _dopolnenie = _V - _min;
        //        float _Procent = _dopolnenie / _range;
        //        float _fullV = _Procent * byte.MaxValue;
        //        return (byte)Math.Max(Math.Min((byte)_fullV, byte.MaxValue), byte.MinValue);
        //    } //converting float to byte
        //    public static UInt16 EncodeInt(float _V, float _min, float _max)
        //    {
        //        _V = Math.Min(Math.Max(_V, _min), _max);
        //        float _range = _max - _min;
        //        float _dopolnenie = _V - _min;
        //        float _Procent = _dopolnenie / _range;
        //        float _fullV = _Procent * UInt16.MaxValue;
        //        return (UInt16)Math.Max(Math.Min((UInt16)_fullV, UInt16.MaxValue), UInt16.MinValue);
        //    } //converting float to uint16
        //}
        ////[DebuggerStepThrough]
        //public class Listener
        //{
        //    public Socket _Socket;
        //    long _position;
        //    public bool _Connected
        //    {
        //        get { return _Socket.Connected; }
        //    }
        //    private List<byte[]> _Messages = new List<byte[]>();
        //    public List<byte[]> GetMessages()
        //    {
        //        Trace.Assert(_Socket != null);
        //        lock ("Get")
        //        {
        //            List<byte[]> _Return = _Messages;
        //            _Messages = new List<byte[]>();
        //            return _Return;
        //        }
        //    }
        //    void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs _SocketAsyncEventArgs)
        //    {
        //        StartReceive();
        //        _MemoryStream.Seek(0, SeekOrigin.End); //write at the end
        //        _MemoryStream.Write(_SocketAsyncEventArgs.Buffer, 0, _SocketAsyncEventArgs.BytesTransferred);
        //        _MemoryStream.Seek(_position, SeekOrigin.Begin); // go to last position                
        //        while (true)
        //        {
        //            byte[] split = _MemoryStream.Read(2);
        //            if (!split.Equals2(new byte[] { 42, 42 })) throw new Exception("dammaged packet");
        //            int _length = _MemoryStream.ReadByte();
        //            if (_length == 255)
        //                _length = _MemoryStream.ReadUInt16();

        //            if (_length == -1 || _MemoryStream.Length <= _position + _length) break; //break if not success
        //            Byte[] _Buffer = new byte[_length];

        //            _MemoryStream.Read(_Buffer, 0, _length);
        //            _position = _MemoryStream.Position; // move position when read success
        //            lock ("Get")
        //                _Messages.Add(_Buffer);
        //        }
        //        //loop
        //    }
        //    MemoryStream _MemoryStream = new MemoryStream();
        //    public void Start()
        //    {
        //        StartReceive();
        //    }
        //    private void StartReceive()
        //    {
        //        SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
        //        _SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAsyncEventArgs_Completed);
        //        //_Socket.ReceiveBufferSize = 100;
        //        //_Socket.SendBufferSize = 100;
        //        _SocketAsyncEventArgs.SetBuffer(new byte[100], 0, 100);
        //        _Socket.ReceiveAsync(_SocketAsyncEventArgs);
        //    }
        //    public static float Decode(byte _fullV, float _min, float _max)
        //    {
        //        float _range = _max - _min;
        //        float _V1 = ((float)_fullV) / byte.MaxValue;
        //        float _ranged = _V1 * _range;
        //        float _V = _ranged + _min;
        //        return _V;
        //    }
        //    public static float DecodeInt(UInt16 _fullV, float _min, float _max)
        //    {
        //        float _range = _max - _min;
        //        float _V1 = ((float)_fullV) / UInt16.MaxValue;
        //        float _ranged = _V1 * _range;
        //        float _V = _ranged + _min;
        //        return _V;
        //    }
        //}
    }
#endif
}
