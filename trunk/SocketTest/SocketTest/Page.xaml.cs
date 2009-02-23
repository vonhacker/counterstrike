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

namespace SocketTest
{
    public partial class Page : UserControl
    {
        TextBox _TextBox;
        public Page()
        {
            
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _TextBox = new TextBox();
            this.Content = _TextBox;
            string ipAddress = "localhost";            
            Helper.Connect(ipAddress,Helper._DefaultSilverlightPort,Dispatcher,OnConnected);
        }
        Socket _Socket;
        public void OnConnected(SocketAsyncEventArgs sc)
        {
            Trace.Assert(sc.SocketError == SocketError.Success);
            _Socket = (Socket)sc.UserToken;
            Update();
        }

        int _packetscount = 0;
        int _totalBytes = 0;
        public void Update()
        {
            byte[] bts = new byte[] { 1, 2, 3, 4, 5, 6 };
            _totalBytes += bts.Length;
            _packetscount++;
            _TextBox.Text = "bytessended" + _totalBytes + " count" + _packetscount;
            _Socket.Send(bts);
            Dispatcher.BeginInvoke(Update);
        }
    }
}
