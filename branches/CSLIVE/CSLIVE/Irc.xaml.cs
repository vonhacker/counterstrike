using doru;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CSLIVE
{


    public partial class Irc : UserControl
    {
        public Irc()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Irc_Loaded);
        }

        void Irc_Loaded(object sender, RoutedEventArgs e)
        {
            _GameServerList.ItemsSource = _List;
            string[] ss = _Config._Irc.Split(":");
            Helper.Connect(ss[0], int.Parse(ss[1])).Completed += delegate(object o, SocketAsyncEventArgs e2)
            {
                Dispatcher.BeginInvoke(new Action<SocketAsyncEventArgs>(Irc_Completed), e2);
            };
        }
        
        Socket _Socket;
        NetworkStream _NetworkStream;
        void Irc_Completed(SocketAsyncEventArgs e)
        {
            if(e.SocketError != System.Net.Sockets.SocketError.Success) { }
            _Socket = (Socket)e.UserToken;
            _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.WriteLine(Trace2(string.Format("NICK {0}", "ChatBox2")));
            _NetworkStream.WriteLine(Trace2("USER " + "ChatBox2" + " " + "ChatBox2" + " server :" + "cslive"));
            _NetworkStream.WriteLine(Trace2("codepage cp1251"));
            new Thread(Read).Start();
            
            _Storyboard.Completed += new EventHandler(_Storyboard_Completed);
            Update();

            _TextInput.KeyDown += new KeyEventHandler(_TextInput_KeyDown);
        }

        void _TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && _TextInput.Text.Length > 0)
            {                
                SendMessage(_TextInput.Text);
                _TextInput.Text = "";
            }
        }

        void _Storyboard_Completed(object sender, EventArgs e)
        {
            Update();
            _Storyboard.Begin();
             
        }
        public void Update()
        {
            Trace.WriteLine("Irc Update");
        }
        Storyboard _Storyboard = new Storyboard();
        List<IrcIm> messages = new List<IrcIm>();
        public void Read()
        {
            try
            {
                while(true)
                {
                    string s = Trace2(_NetworkStream.ReadLine());
                    Match _Match = Regex.Match(s, @"PING \:(\w+)", RegexOptions.IgnoreCase);
                    if(_Match.Success)
                        _NetworkStream.WriteLine(("PONG :" + Trace2(_Match.Groups[1])));
                    _Match = Regex.Match(s, @":(.+?)!.+? PRIVMSG #(.+?) :(.+)");
                    if(_Match.Success)                    
                        Dispatcher.BeginInvoke(new Action<string,string>(AddMessage),_Match.Groups[1].Value,_Match.Groups[3].Value);                    
                    if(Regex.Match(s, @":.+? 005").Success) "connected".Trace();
                }
            }
            catch(IOException) { }
            "disconnected".Trace();
            
        }
        ObservableCollection<Item> _List = new ObservableCollection<Item>();
        public void AddMessage(string user, string msg)
        {
            _TextOutput.Text += user + ":" + msg + "\r\n";
        }
        public string _Nick;
        public string _Room;
        public void SendMessage(string msg)
        {
            AddMessage(_Nick, msg);
            foreach(string s in msg.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                _NetworkStream.WriteLine(String.Format("PRIVMSG {0} :{1}", _Room, s));
        }
        public class IrcIm
        {
            public string room;
            public string msg;
            public string user;
        }
        public class Item
        {
            public string _Name { get; set; }
            public string _Players { get; set; }
            public string _Port { get; set; }
            public string _Version { get; set; }
            public string _Ip { get; set; }
            public string _Map { get; set; }
        }
        public static T Trace2<T>(T t)
        {
            t.Trace();
            return t;
        }
    }
}