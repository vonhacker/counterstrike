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
using System.Collections.Generic;

namespace CounterStrikeLive
{
    public class Listener
    {
        public Socket _Socket;

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


        //public bool _Connected // TODO: remove
        //{
        //    get { return _Socket.Connected; }
        //}

        //private List<byte[]> _Messages = new List<byte[]>(); // TODO: remove

        //public List<byte[]> GetMessages() // TODO: remove
        //{
        //    lock ("Get")
        //    {
        //        List<byte[]> _Return = _Messages;
        //        _Messages = new List<byte[]>();
        //        return _Return;
        //    }
        //}
        //void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs _SocketAsyncEventArgs) // TODO: remove
        //{
        //    lock ("Receive")
        //    {
        //        _MemoryStream.Write(_SocketAsyncEventArgs.Buffer, 0, _SocketAsyncEventArgs.BytesTransferred);
        //        _MemoryStream.Seek(_position, SeekOrigin.Begin);
        //        while (true)
        //        {
        //            int _length = _MemoryStream.ReadByte();
        //            if (_length == -1 || _MemoryStream.Length <= _position + _length) break;
        //            Byte[] _Buffer = new byte[_length];

        //            _MemoryStream.Read(_Buffer, 0, _length);
        //            _position = _MemoryStream.Position;
        //            lock ("Get")
        //                _Messages.Add(_Buffer);
        //        }
        //        _MemoryStream.Seek(0, SeekOrigin.End);
        //        StartReceive();
        //    }
        //}
        //MemoryStream _MemoryStream = new MemoryStream(); // TODO: remove
        //public void Start()
        //{
        //    StartReceive();
        //}

        //private void StartReceive()
        //{
        //    SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
        //    _SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAsyncEventArgs_Completed);
        //    //_Socket.ReceiveBufferSize = 100;
        //    //_Socket.SendBufferSize = 100;
        //    _SocketAsyncEventArgs.SetBuffer(new byte[100], 0, 100);
        //    _Socket.ReceiveAsync(_SocketAsyncEventArgs);
        //}
        long _position;
    }
}
