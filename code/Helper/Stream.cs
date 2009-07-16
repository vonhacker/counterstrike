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

    public static class StreamExtensions
    {
        

        public static bool BigEndian;
        private static byte[] CV(this Byte[] bts)
        {
            if (BigEndian) return bts.ReverseA();
            else return bts;
        }
        public static int ReadInt32(this Stream s)
        {
            return BitConverter.ToInt32(s.Read(4).CV(), 0);
        }
        public static Int16 ReadInt16(this Stream s)
        {
            return BitConverter.ToInt16(s.Read(2).CV(), 0);
        }
        public static UInt32 ReadUInt32(this Stream s)
        {
            return BitConverter.ToUInt32(s.Read(4).CV(), 0);
        }
        public static UInt16 ReadUInt16(this Stream s)
        {
            return BitConverter.ToUInt16(s.Read(2).CV(), 0);
        }
        public static double ReadDouble(this Stream s)
        {
            return BitConverter.ToDouble(s.Read(8).CV(), 0);
        }
        public static float ReadFloat(this Stream s)
        {
            return BitConverter.ToSingle(s.Read(4).CV(), 0);
        }
        public static void Write(this Stream s, Int16 _int)
        {
            s.Write(BitConverter.GetBytes(_int).CV());
        }
        public static void WriteUint16(this Stream s, UInt16 _int) { Write(s, _int); }
        public static void Write(this Stream s, UInt16 _int)
        {
            s.Write(BitConverter.GetBytes(_int).CV());
        }
        public static void WriteUint32(this Stream s, UInt32 _int) { Write(s, _int); }
        public static void Write(this Stream s, UInt32 str)
        {
            s.Write(BitConverter.GetBytes(str).CV());
        }
        public static void Write(this Stream s, int str)
        {
            s.Write(BitConverter.GetBytes(str).CV());
        }
        public static void Write(this Stream s, float str)
        {
            s.Write(BitConverter.GetBytes(str).CV());
        }
        public static void Write(this Stream s, double str)
        {
            s.Write(BitConverter.GetBytes(str).CV());
        }
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
        [DebuggerStepThrough]
        public static void Write(this Stream _Stream, byte[] _bytes)
        {
            if (_bytes.Length == 0) throw new Exception();
            _Stream.Write(_bytes, 0, _bytes.Length);
        }
        [DebuggerStepThrough]
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
            return ReadBlock(_Stream, 1024 * 10);
        }
        public static byte[] ReadBlock(this Stream _Stream, int length)
        {
            byte[] _bytes = new byte[length];
            int cnt = _Stream.Read(_bytes, 0, _bytes.Length);
            if (cnt == 0) return new byte[] { };
            return _bytes.Read(cnt);
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
        public static string ReadStringToEnd(this Stream s)
        {
            return s.Read().ToStr();
        }
        public static string ReadString(this Stream s)
        {
            int c = s.ReadB();
            return s.Read(c).ToStr();
        }
        public static void WriteString(this Stream s, string _str)
        {
            byte[] bs = _str.ToBytes();
            s.Write(H.JoinBytes(new[] { (byte)bs.Length }, bs));
        }
        
    }
    public class LinkMemoryStream : MemoryStream
    {
        public LinkMemoryStream() : base() { }
        public LinkMemoryStream(byte[] bts) : base(bts) { }
        public class CV
        {
            public int pos;
            public Stream _Stream;
        }
        public List<CV> cvs = new List<CV>();
        public void SetPointer(Stream _Stream, int pos)
        {
            cvs.Add(new CV { _Stream = _Stream, pos = pos });
        }

        Stream curstream;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (curstream != null)
            {
                int c = curstream.Read(buffer, offset, count);
                if (c == 0) curstream = null;
                else
                    return c;
            }
            foreach (CV cv in cvs)
            {
                if (Position < cv.pos && Position + count > cv.pos)
                {
                    cv._Stream.Seek(0, SeekOrigin.Begin);
                    curstream = cv._Stream;
                    return base.Read(buffer, 0, (int)(cv.pos - Position));
                }
            }
            return base.Read(buffer, offset, count);
        }
    }
}
