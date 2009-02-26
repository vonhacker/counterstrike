using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using doru;
using System.Net.Sockets;
using System.Windows.Threading;

namespace SocketTest
{
    public partial class Page : UserControl
    {
        
        public Page()
        {
            
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
                        
            string ipAddress = "localhost";            
            Helper.Connect(ipAddress,Helper._DefaultSilverlightPort,Dispatcher,OnConnected);
        }
        Socket _Socket;
        public void OnConnected(SocketAsyncEventArgs sc)
        {
            Trace.Assert(sc.SocketError == SocketError.Success);
            _Socket = (Socket)sc.UserToken;
            
            BeginAccept();
            DispatcherTimer _DispatcherTimer = new DispatcherTimer();
            _DispatcherTimer.Interval = TimeSpan.FromMilliseconds(5);
            _DispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick);
            _DispatcherTimer.Start();
            BeginAccept();
        }

        void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        void BeginAccept()
        {
            SocketAsyncEventArgs s2 = new SocketAsyncEventArgs();
            
            s2.SetBuffer(new byte[1024], 0, 1024);
            s2.Completed += new EventHandler<SocketAsyncEventArgs>(Bytes_Accepted);
            _Socket.ReceiveAsync(s2);
        }
        void Bytes_Accepted(object sender, SocketAsyncEventArgs e)
        {
            BeginAccept();
            _receivedpacketscount++;
            _receivedtotalBytes += e.BytesTransferred;
            Dispatcher.BeginInvoke(new Action(delegate()
                {
                    _TextBox1.Text = "total bytes received" + _receivedtotalBytes + " Packet count" + _receivedpacketscount;
                }));
        }
        int _receivedpacketscount = 0;
        int _receivedtotalBytes = 0;

        int _sendedpacketscount = 0;
        int _sendedtotalBytes = 0;
        public void Update()
        {
            byte[] bts = new byte[] { 1 };
            _sendedtotalBytes += bts.Length;
            _sendedpacketscount++;
            _TextBox.Text = "total bytes sended" + _sendedtotalBytes + " Packet count" + _sendedpacketscount;
            _Socket.Send(bts);
            
            //Dispatcher.BeginInvoke(Update);
        }
    }
}
