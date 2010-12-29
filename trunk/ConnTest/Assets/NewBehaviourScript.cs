using UnityEngine;
using System.Collections;
using Starksoft.Net.Proxy;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using System.IO;



public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
    void Start()
    {

    }

    //void Start()
    //{
    //    Thread t = new Thread(delegate()
    //    {
    //        print("th strt");
    //        var p = new Socks5ProxyClient("127.0.0.1", 1080);
    //        print("1");

    //        TcpClient tcp = p.CreateConnection("google.com", 80);
    //        print("success");
    //    });
    //    t.Start();

    //}

    //void Start () {
    //    new Thread(delegate()
    //    {
    //        print("1");
    //        Security.PrefetchSocketPolicy("127.0.0.1", 80);
    //        ProxyClientFactory factory = new ProxyClientFactory();
    //        IProxyClient proxy = null;
    //        proxy = factory.CreateProxyClient(ProxyType.Socks5, "127.0.0.1", 1080);
    //        proxy.CreateConnectionAsyncCompleted += new EventHandler<CreateConnectionAsyncCompletedEventArgs>(proxy_CreateConnectionAsyncCompleted);
    //        proxy.CreateConnectionAsync(host, 80);
    //        print("2");
    //    }).Start();
    //}
    //string host = "google.com";
    //private void proxy_CreateConnectionAsyncCompleted(object sender, CreateConnectionAsyncCompletedEventArgs e)
    //{
    //    // test to see if an error occurred
    //    if (e.Error != null)
    //    {
    //        Debug.Log(e.Error.Message+": Connection Error");
    //        return;
    //    }
    //    print("success");
    //}
    //// Update is called once per frame
    //void Update () {
    //}
}

public static class Ext
{
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
    public static byte[] Receive(this Socket _Socket, int length)
    {
        byte[] _buffer = new byte[length];
        using (MemoryStream _MemoryStream = new MemoryStream())
        {
            while (true)
            {
                int count = _Socket.Receive(_buffer, length, 0);
                if (count == 0) throw new Exception("Read Socket failed");
                _MemoryStream.Write(_buffer, 0, count);
                length -= count;
                if (length == 0) return _MemoryStream.ToArray();
            }
        }
    }

}
public static class Proxy
{
    public static Socket Socks5Connect(string _proxyAddress, int _proxyPort, string _DestAddress, int _DestPort)
    {
        TcpClient _TcpClient = new TcpClient(_proxyAddress, _proxyPort);
        Socket _Socket = _TcpClient.Client;

        _Socket.Send(new byte[] { 5, 1, 0 });
        byte[] _bytes = _Socket.Receive(2);
        Debug.Log("<<<<<<<socks5 received1>>>>>>>>");
        if (_bytes[0] != 5 && _bytes[1] != 1) throw new Exception();
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
        Debug.Log("<<<<<<<<socks5 received2>>>>>>>>");
        byte[] _response = _Socket.Receive(10);
        if (_response[1] != 0) throw new Exception("socket Error: " + _response[1]);
        return _Socket;
    }
}