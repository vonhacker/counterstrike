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
using System.Net.Sockets;
using System.IO;

namespace CounterStrikeLive
{
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

        //public Socket _Socket;// TODO: remove
        //public void Send(byte[] _Buffer2) // TODO: remove
        //{
        //    byte[] _Buffer = new byte[_Buffer2.Length + 1];
        //    _Buffer[0] = (byte)_Buffer2.Length;
        //    Buffer.BlockCopy(_Buffer2, 0, _Buffer, 1, _Buffer2.Length);
        //    if (_Buffer2.Length == 0) throw new Exception("Break");
        //    SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
        //    _SocketAsyncEventArgs.SetBuffer(_Buffer, 0, _Buffer.Length);
        //    _Socket.SendAsync(_SocketAsyncEventArgs);
        //}
    }
}
