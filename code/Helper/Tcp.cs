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
    namespace Tcp
    {
        

        //[DebuggerStepThrough]
        public class Sender
        {
            static bool _Dissabled { get { return Listener._Dissabled; } }
            public int PacketCheck;//= new Random().Next();
            public Socket _Socket;
            public NetworkStream _NetworkStream;
            
            public void Send(byte[] _Buffer)
            {
                if (_Dissabled) return;
                PacketCheck++;
                if (PacketCheck > 255) PacketCheck = 0;
                Trace.Assert(_Socket.Connected);
                Trace.Assert(_Buffer.Length > 0);
                byte[] bytes;
                if (_Buffer.Length < 255)
                    bytes = Helper.JoinBytes(42, 42, (byte)PacketCheck, (byte)_Buffer.Length, _Buffer);
                else
                {
                    bytes = Helper.JoinBytes(42, 42, (byte)PacketCheck, (byte)255, BitConverter.GetBytes((UInt16)_Buffer.Length), _Buffer);
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
            public static bool _Dissabled;

            public int PacketCheck;
            public NetworkStream _NetworkStream;
            
            public Socket _Socket;
            public bool _Connected
            {
                get
                {
                    if (_Dissabled) return true;
                    return _Socket.Connected;
                }
            }
            private List<byte[]> _Messages = new List<byte[]>();
            public List<byte[]> GetMessages()
            {
                if (_Dissabled) return _Messages;
                // Trace.Assert(_Connected);
                lock ("Get")
                {
                    List<byte[]> _Return = _Messages;
                    foreach (byte[] bts in _Return)
                        bts.Trace("Received", DebugState.Tcp);
                    _Messages = new List<byte[]>();
                    return _Return;
                }
            }

            MemoryStream _MemoryStream = new MemoryStream();
            public void StartAsync(string s)
            {
                if (_Dissabled) return;
                Trace.Assert(_Socket != null && _NetworkStream != null);
                new Thread(Start).StartBackground(s);
            }
            private void Start()
            {                
                try
                {
                    while (true)
                    {
                        byte[] split = _NetworkStream.Read(2); //every packet begins with "**" 42,42
                        if (!split.Equals2(new byte[] { 42, 42 })) throw new Exception("DammagedPacket");
                        PacketCheck++;
                        if (PacketCheck > 255) PacketCheck = 0;
                        byte pck=_NetworkStream.ReadB();                        
                        UInt16 length = _NetworkStream.ReadB(); //length
                        if (length == 255)
                            length = _NetworkStream.ReadUInt16();
                        byte[] bytes = _NetworkStream.Read(length); //bytes
                        lock ("Get")
                            _Messages.Add(bytes);//add to packets buffer
                        if (PacketCheck != pck) Debugger.Break();
                    }
                } catch (IOException) { "packet sending failed".Trace(); }
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

        public class LagStream : NetworkStream
        {
            public LagStream(Socket s)
                : base(s)
            {

                new Thread(Send).Start();
                
            }
            public int bytecount = 150 * 1024;
            Random _Random = new Random();
            public int interval = 300;
            MemoryStream ms = new MemoryStream();
            public override void Write(byte[] buffer, int offset, int size)
            {
                long pos = ms.Position;
                ms.Seek(0, SeekOrigin.End);
                ms.Write(buffer, offset, size);
                ms.Position = pos;
                
            }
            public override void WriteByte(byte value)
            {
                long pos = ms.Position;
                ms.Seek(0, SeekOrigin.End);
                ms.WriteByte(value);
                ms.Position = pos;
            }
            public void Send()
            {
                try
                {
                    while (true)
                    {

                        byte[] bts = new byte[bytecount];
                        int cnt = ms.Read(bts, 0, bts.Length);
                        if (cnt > 0)
                            base.Write(bts, 0, cnt);
                        Thread.Sleep(interval);
                    }
                } catch(IOException) { }
            }                                    

        }


    }
}
