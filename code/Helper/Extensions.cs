using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
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
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;

using System.Windows.Controls;
using System.ComponentModel;

using System.Windows.Media;
namespace doru
{


    public static class Extensions
    {
        public static double Length(this Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static T AddOrSelect<T>(this IList<T> ie, T t)
        {
            if (!ie.Contains(t)) ie.Add(t);
            return t;
        }
        public static T Trace<T>(this T t)
        {
            return Trace(t, "");
        }
        public static T Trace<T>(this T t, object s)
        {
            return Trace<T>(t, s, 0);
        }
        public static T Trace<T>(this T t, object s, DebugState _traceLevel)
        {
            StringBuilder sb;
            if (t == null) sb = new StringBuilder("null");
            else sb = new StringBuilder(t.ToString());
            if (_traceLevel != H._TraceState) return t;
            if (t is IEnumerable && !(t is string))
                foreach (object o in (IEnumerable)t)
                    sb.Append(o).Append(' ');

#if(SILVERLIGHT)
            doru.Trace.WriteLine("Silverlight: "+s.ToString() +": "+ sb);
#else
            System.Diagnostics.Trace.WriteLine(Assembly.GetEntryAssembly().GetName().Name + ": " + s.ToString() + ": " + sb);
#endif
            return t;
        }

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

        public static T Add<T>(this System.Windows.Controls.Panel _This, T _ui) where T: UIElement
        {
            _This.Children.Add(_ui);
            return _ui;
            
        }
        
		public static void Center(this UIElement ui, BitmapImage _Image)
		{
			ui.SetX(-_Image.PixelWidth / 2);
			ui.SetY(-_Image.PixelHeight / 2);
		}
        public static void SetSource(this Image img, string path)
        {
            img.SetSource(new BitmapImage(new Uri(path, UriKind.Relative)));
        }
        public static void SetSource(this Image img, BitmapImage _Image)
        {
            img.Source = _Image;
            var a= _Image.PixelWidth;
        }
        public static void MoveTo<T>(this List<T> a, List<T> b, T t)
        {
            a.Remove(t);
            b.Add(t);
        }
        
        public static double Pow(this double a)
        {
            return Math.Pow(a, 2);
        }
        public static string[] SplitString(this string a)
        {
            return a.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
		public static void SetPos(this UIElement fw, double x, double y)
		{
			Canvas.SetLeft(fw, x);
			Canvas.SetTop(fw, y);
		}
        public static T Center<T>(this T ui) where T : FrameworkElement
        {
            TransformGroup t = new TransformGroup();
            t.Children.Add(new TranslateTransform { X = -ui.Width / 2, Y = -ui.Height / 2 });
            ui.RenderTransform = t;
            return ui;
        }
        public static T AddTo<T>(this T fw, Panel p) where T : UIElement
        {
            p.Children.Add(fw);
            return fw;
        }
        public static T SetX<T>(this T fw,double x) where T: UIElement
        {
            Canvas.SetLeft(fw,x);
            return fw;
        }
        public static T SetY<T>(this T fw, double y)where T: UIElement
        {
            Canvas.SetTop(fw,y);
            return fw;
        }
        public static Point GetPos(this UIElement fw)
        {
            return new Point(Canvas.GetLeft(fw), Canvas.GetTop(fw));
        }
        public static double GetX(this UIElement fw)
        {
            double a = Canvas.GetLeft(fw);
            if (double.IsNaN(a))
            {
                "nan".Trace();
                return 0;
            }
            return a;
        }
        public static double GetY(this UIElement fw)
        {
            return Canvas.GetTop(fw);
        }
        public static string ReplaceAndSkip(this string s,string s2, string s3)
        {
            return s.Split(new[] { s2 }, StringSplitOptions.RemoveEmptyEntries).Join(s3);
        }
        public static string Replace(this string s, params string[] ss)
        {            
            for (int i = 0; i < ss.Length; i++)
            {
                if (!s.Contains(ss[i])) throw new Exception(ss[i] + " cannot be replaced");
                s = s.Replace(ss[i], ss[++i]);
            }
            return s;
        }


        
        public static string[] Split2(this string s, string s2)
        {
            return s.Split(new[] { s2 }, StringSplitOptions.RemoveEmptyEntries);
        }
        
        
        [Obsolete("DeserealizeOrCreate")]
        public static T DeserealizeOrSerialize<T>(this XmlSerializer x, string path, T t)
        {
            if (t == null)
            {
                try
                {
                    using (FileStream fs1 = File.Open(path, FileMode.Open))
                    {   
                        t =(T)x.Deserialize(fs1);
                        if (t == null) throw new Exception();
                        return t;
                    }
                } catch
                {
                    using (FileStream fs = File.Create(path)) x.Serialize(fs, t);
                    return t;
                }
            } else
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


        public static T Random<T>(this IList<T> list, T except)
        {
            if (list.All(a => a.Equals(except))) return except;
            if (list.Count == 1) return list.First();
            T t;
            while ((t = list[_Random.Next(list.Count)]).Equals(except)) ;
            return t;
        }
        public static T Random<T>(this IList<T> list)
        {
            return list[_Random.Next(list.Count)];
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
        public static byte[] ToHexTrace(this byte[] _bytes)
        {
            _bytes.ToHex().Trace();
            return _bytes;
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

        public static uint ToUInt16(this byte[] bts)
        {
            return BitConverter.ToUInt16(bts, 0);
        }
        public static int ToInt(this byte[] bts)
        {
            return BitConverter.ToInt32(bts, 0);
        }
        public static int ToInt16(this byte[] bts)
        {
            return BitConverter.ToInt16(bts, 0);
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
        [DebuggerStepThrough]
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
            //byte[] _bytes = new byte[source.Length - pos];
            //byte[] _bytes2 = new byte[pos];
            //for (int i = 0; i < pos; i++)
            //{
            //    _bytes2[i] = source[i];
            //}
            //for (int i = pos; i < source.Length; i++)
            //{
            //    _bytes[i - pos] = source[i];
            //}
            return new byte[2][] { Read(source, pos), Cut(source, pos) };
        }
        public static byte[] Read(this byte[] source, int end)
        {
            byte[] _bytes2 = new byte[end];
            for (int i = 0; i < end; i++)
            {
                _bytes2[i] = source[i];
            }
            return _bytes2;
        
        } // read from 0 to position

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

        //public static string Substr(this string s, string a)
        //{
        //    return s.Substring(0, s.IndexOf(a));
        //}
        public static string Strstr(this string s, string a)
        {
            return s=s.Substring(s.IndexOf(a)+a.Length,s.Length- a.Length);
        }
        public static byte[] Cut(this byte[] source, int start) //read from position To End
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

        public static T2 AddOrSelect<T, T2>(this Dictionary<T, T2> _Vars, T key, T2 o)
        {
            if (!_Vars.ContainsKey(key)) _Vars.Add(key, o);
            return _Vars[key];
        }

        public static void AddOrCreate<T,T2>(this Dictionary<T,T2> _Vars, T key, T2 o)
        {
            if (!_Vars.ContainsKey(key)) _Vars.Add(key, o);
            else _Vars[key] = o;
        }
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
                if (H.Compare(source, pattern, i)) return i;
            }
            return -1;
        }
    }

    [Obsolete("use NotifyPropertyChangedExt")]
    public class NotifyPropertyChanged : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string s)
        {

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(s));
        }
        private Dictionary<string, DependencyProperty> _Vars = new Dictionary<string, DependencyProperty>();
        public void Set<T>(string s, T o)
        {
            if (!_Vars.ContainsKey(s)) Create(s, o);
            SetValue(_Vars[s], o);
            OnPropertyChanged(s);
        }

        private void Create<T>(string s, T o)
        {
            DependencyProperty dp = DependencyProperty.Register(s, typeof(T), this.GetType(), new PropertyMetadata(o));
            _Vars.Add(s, dp);
        }
        public T Get<T>(string s)
        {
            if (_Vars.ContainsKey(s))
                return (T)GetValue(_Vars[s]);
            else
            {
                T t;
                try
                {
                    t = Activator.CreateInstance<T>();
                } catch { t = default(T); }
                Create(s, t);
                return t;
            }
        }
    }
}
