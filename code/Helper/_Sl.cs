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
    public static class Extensions23
    {
        [DebuggerStepThrough]
        public static void SetSource(this MediaElement mm, string s)
        {
            mm.SetSource(Application.GetResourceStream(new Uri(Helper._ContentFolder + s, UriKind.Relative)).Stream);
        }
        public static void Send(this Socket _Socket, byte[] buffer) { Send(_Socket, buffer, 0, buffer.Length); }
        public static void Send(this Socket _Socket, byte[] buffer, int offset, int count)
        {
            SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
            _SocketAsyncEventArgs.SetBuffer(buffer, offset, count);
            _Socket.SendAsync(_SocketAsyncEventArgs);
        }
    }
	public class NetworkStream : MemoryStream
	{
		public static bool Loaded;
		public override string ToString()
		{
			return base.ToString() + " " + Length + " " + _Socket.Connected;
		}
		public Socket _Socket;
		public NetworkStream(Socket s)
		{
			if (Loaded) throw new Exception("only one NetworkStream can be created");
			Loaded = true;
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
			//if (Position == Length) return 0;
			//else
			return base.Read(buffer, offset, count);
		}
		public override int ReadByte()
		{
			while (Position == Length) Thread.Sleep(2);
			return base.ReadByte();
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			_Socket.Send(buffer, offset, count);
		}
	}

	public abstract class Encoding : System.Text.Encoding
	{



		public static System.Text.Encoding Default = Encoding.UTF8;
		static System.Text.Encoding _DefaultEncoding = Encoding.UTF8;


	}
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

}
