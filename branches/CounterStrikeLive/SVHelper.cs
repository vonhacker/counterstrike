using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Threading;

namespace CounterStrikeLive
{
    public static class Extensions
    {
        public static void Show(this Control _Control)
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
    }

    public class Sender
    {
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
    }
    public class Listener
    {
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
        public Socket _Socket;
        void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs _SocketAsyncEventArgs)
        {
            lock ("Receive")
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
        long _position;
    }



    public class Line2
    {
        public Vector2 _p1; public Vector2 _p2; public Vector2 _cpoint;
    }
    public class Physics
    {
        public static Vector2 ConvertPoint(Point _Point)
        {
            return new Vector2((float)_Point.X, (float)_Point.Y);
        }
        public static Vector2 Reflect(Vector2 speed, Line2 wall)
        {
            Vector2 _vector = new Vector2(wall._p1.X - wall._p2.X, wall._p1.Y - wall._p2.Y);
            Calculator.RotateVector(ref _vector, 1.57f);
            _vector.Normalize();
            return Reflect(speed, _vector);
        }
        public static Vector2 Reflect(Vector2 _Vector2, Vector2 _Vector2Normal)
        {
            float mul = Vector2.Dot(_Vector2, _Vector2Normal);

            Vector2 vmul = new Vector2(_Vector2Normal.X * mul * 2, _Vector2Normal.Y * mul * 2);
            Vector2 reflect = new Vector2(_Vector2.X - vmul.X, _Vector2.Y - vmul.Y);
            return reflect;
        }
        public static Vector2? LineCollision(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22, bool Segment)
        {

            p12.Y += .08f;
            p12.X += .08f;
            // cheat to avoid handling of vertical lines 
            p22.Y += .08f;
            p22.X += .08f;

            // equations of the lines: y=ax+b  or  y-y0=k(x-x0), k=dy/dx
            var a1 = (p12.Y - p11.Y) / (p12.X - p11.X);
            var b1 = p11.Y - a1 * p11.X;

            var a2 = (p22.Y - p21.Y) / (p22.X - p21.X);
            var b2 = p21.Y - a2 * p21.X;

            // aligned lines do not intersect at all or they intersect in every point
            if (a1 == a2) return null;

            // calculate intersection point
            Vector2 ip = new Vector2();
            ip.X = (b2 - b1) / (a1 - a2);
            ip.Y = a1 * ip.X + b1;

            if (Segment)
            {
                // is the intersection outside any segment span

                if (ip.X < (float)Math.Min(p11.X, p12.X)) return null;
                if (ip.X > (float)Math.Max(p11.X, p12.X)) return null;

                if (ip.Y < (float)Math.Min(p11.Y, p12.Y)) return null;
                if (ip.Y > (float)Math.Max(p11.Y, p12.Y)) return null;

                if (ip.X < (float)Math.Min(p21.X, p22.X)) return null;
                if (ip.X > (float)Math.Max(p21.X, p22.X)) return null;

                if (ip.Y < (float)Math.Min(p21.Y, p22.Y)) return null;
                if (ip.Y > (float)Math.Max(p21.Y, p22.Y)) return null;
            }

            return ip;
        }
    }
    public static class STimer
    {
        public static void AddMethod(double _Time, Action _Action)
        {
            _Timer.AddMethod(_Time, _Action);
        }

        static Timer2 _Timer = new Timer2();
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

        internal static double? GetFps()
        {
            return _Timer.GetFps();
        }
    }
    public class Timer2
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
}

