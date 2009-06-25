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

namespace doru
{
    public static class Helper
    {
        public static DebugState _TraceState { get { return (DebugState)int.Parse(Resource1._TraceLevel); } }

        public static void ShowMessageBox(string s)
        {
            System.Windows.MessageBox.Show(s);
        }

        public static bool IsPrime(int x)
        {
            for (int i = 2; i <= Math.Sqrt(x); i++)
                if (x % i == 0)
                    return false;
            return true;
        }

#if(!SILVERLIGHT)
        public static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.GetType().InvokeMember(e.Property.Name, System.Reflection.BindingFlags.SetProperty, Type.DefaultBinder, d, new object[] { e.NewValue });
        }
#endif
        public static int _DefaultSilverlightPort = 4530;
        [Obsolete]
        public static void OpenZip(Stream fs, Dictionary<string, Stream> _Resources) { LoadResourcesFromZip(fs, _Resources); }

        public static void LoadResourcesFromZip(Stream fs, IDictionary<string, Stream> _Resources)
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
                    } catch (PathTooLongException) { }
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
                    } catch { }
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
                    } catch { }
                }
            }
        }
        [DebuggerStepThrough]
        public static byte[] JoinBytes(params object[] objects)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (object o in objects)
                    if (o is byte[])
                    {
                        if (((byte[])o).Length != 0)
                            ms.Write((byte[])o);
                    } else if (o is object[])
                    {
                        ms.Write(JoinBytes(o));
                    } else
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
                } else
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
        public static string Random(params string[] ss)
        {
            return ss[_Random.Next(ss.Length)];
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
            } else
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
            //if (Application.Current.Host.Source.DnsSafeHost == "") "warning host not safe".Trace();
            _SocketAsyncEventArgs.RemoteEndPoint = new DnsEndPoint(ip, port);
            _SocketAsyncEventArgs.UserToken = _Socket;
            _SocketAsyncEventArgs.Completed += delegate
            {
                "0".Trace();
                Dispatcher.BeginInvoke(//у меня места не хвататет на с другой он=) студию сп1 
                    new Action(
                    delegate()
                    {
                        "1".Trace();
                        _OnConnected(_SocketAsyncEventArgs);
                    }));
            };
            _Socket.ConnectAsync(_SocketAsyncEventArgs);
            return _SocketAsyncEventArgs;
        }



#if(SILVERLIGHT)        
                
#else

        public class DnsEndPoint : IPEndPoint
        {

            public DnsEndPoint(string ip, int port) : base(Dns.GetHostAddresses(ip)[0], port) { }
        }
        public static void Trace2(string t, string prefix)
        {
            string[] ss = t.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 3)
            {
                System.Diagnostics.Trace.Write(prefix + ":");
                t.ToString().Save();
            } else
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
        [Obsolete("use CreateSchema")]
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
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string getMD5HashFromFile(string sFilePath)
        {
            using (FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read))
            {
                Byte[] hashCode = new MD5CryptoServiceProvider().ComputeHash(fs);
                return BitConverter.ToString(hashCode);
            }

        }

        public static Socket Connect(string ip, int port)
        {
            return new TcpClient(ip, port).Client;
        }
        public class MyNetworkStream : System.Net.Sockets.NetworkStream
        {
            public DateTime _LastActivity = DateTime.Now;
            public MyNetworkStream(Socket _Socket) : base(_Socket) { }
            public override int Read(byte[] buffer, int offset, int size)
            {
                var a = base.Read(buffer, offset, size);
                _LastActivity = DateTime.Now;
                return a;
            }
            public override int ReadByte()
            {
                var a = base.ReadByte();
                _LastActivity = DateTime.Now;
                return a;
            }
            public override void Write(byte[] buffer, int offset, int size)
            {
                base.Write(buffer, offset, size);
                _LastActivity = DateTime.Now;
            }
            public override void WriteByte(byte value)
            {
                base.WriteByte(value);
                _LastActivity = DateTime.Now;
            }
        }
#endif






        public static string _ContentFolder = "";

        public static void Switch<T>(ref T t,ref T t2)
        {
            T temp = t2;
            t2 = t;
            t = temp;
        }
    }                
}
