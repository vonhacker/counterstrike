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
#if(SILVERLIGHT)
using System.Windows.Controls;
using System.ComponentModel;
#else
using System.Windows.Forms;
using System.IO.Compression;
using System.Windows.Controls;
using System.ComponentModel;
using System.Web;
#endif

namespace doru
{
    namespace Tcp
    {
        

        //[DebuggerStepThrough]
        public class Sender
        {
            public int PacketCheck;//= new Random().Next();
            public Socket _Socket;
            public NetworkStream _NetworkStream;
            
            public void Send(byte[] _Buffer)
            {
                
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
            public int PacketCheck;
            public NetworkStream _NetworkStream;
            
            public Socket _Socket;
            public bool _Connected
            {
                get { return _Socket.Connected; }
            }
            private List<byte[]> _Messages = new List<byte[]>();
            public List<byte[]> GetMessages()
            {
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

#if(SILVERLIGHT)
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
                        _Socket.Send(buffer,offset,count);
                    }
                }

#else

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
#endif
    }
}
