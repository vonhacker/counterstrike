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

        void Irc_Loaded(object sender, RoutedEventArgs e) //creating socket, connecting to _Config._Irc
        {
            AddMessage("connecting to irc server");
            _GameServerList.ItemsSource = _ServerList;
            string[] ss = _Config._Irc.Split(":");
            Helper.Connect(ss[0], int.Parse(ss[1])).Completed += delegate(object o, SocketAsyncEventArgs e2)
            {
                Dispatcher.BeginInvoke(new Action<SocketAsyncEventArgs>(Irc_Completed), e2);
            };
        }

        Socket _Socket;
        NetworkStream _NetworkStream;
        void Irc_Completed(SocketAsyncEventArgs e) //on socket connection completed
        {
            if(e.SocketError != System.Net.Sockets.SocketError.Success) { throw new Exception("cannot connect to irc"); }
            AddMessage("socket connection established, connecting to irc");
            _Socket = (Socket)e.UserToken;
            _NetworkStream = new NetworkStream(_Socket);
            _NetworkStream.WriteLine(Trace2(string.Format("NICK {0}", _Nick)));
            _NetworkStream.WriteLine(Trace2("USER " + "CsLiveClient" + " " + "CsLiveClient" + " server :" + "CsLiveClient"));            
            new Thread(Read).Start();

            _Storyboard.Completed += new EventHandler(_Storyboard_Completed);
            Update();

            _TextInput.KeyDown += new KeyEventHandler(TextInput_KeyDown);
        }

        void TextInput_KeyDown(object sender, KeyEventArgs e) //checking if Enter Pressed
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
        public void Read() //reading irc from networkstream 
        {
            try
            {
                while(true)
                {
                    
                    string s = _NetworkStream.ReadLine().Trace();
                    Dispatcher.BeginInvoke(new Action<string>(OnIrcMessage),s);                    
                }
            }
            catch(IOException) { }
            "disconnected".Trace();

        }

        private void OnIrcMessage(string s)//parsing irc messages
        {
            
            Match m;
            if((m = Regex.Match(s, @"PING \:(.+)", RegexOptions.IgnoreCase)).Success) //onping
                _NetworkStream.WriteLine(("PONG :" + m.Groups[1].Trace()));

            if((m = Regex.Match(s, @":(.+?)!.+? PRIVMSG #(.+?) :(.+)")).Success) //onmessage
                Dispatcher.BeginInvoke(new Action<string, string>(AddMessage), m.Groups[1].Value, m.Groups[3].Value);

            if(Regex.Match(s, @":.+? 002").Success) OnConnected(); //onconnected

            if((m = Regex.Match(s, @"^.+? 353 .+?\:(.+?)$")).Success) //onuserlist            
                foreach(string s2 in m.Groups[1].Value.Split2(" "))
                    OnUserJoin(s2);

            if((m = Regex.Match(s, @"^:(.+?)!.+? JOIN")).Success) //on user join
                if(m.Groups[1].Value != _Nick) OnUserJoin(m.Groups[1].Value);

            if((m = Regex.Match(s, @"^:(.+?)!.+? PART")).Success) //on user left 
                if(m.Groups[1].Value != _Nick) OnUserLeave(m.Groups[1].Value);

            if((m = Regex.Match(s, "^.+? 451 .+?:(.+?)$")).Success)
                OnConnectionFailed(m.Groups[1].Value);

        }
        [Obsolete]
        public new string Name;//use _Nick
        public void OnUserJoin(string s) //user joined to irc
        {            
            _UserList.Items.Add(s);
        }
        public void OnConnectionFailed(string s)
        {
            AddMessage("Connection Failed: " + s);
        }
        public void OnUserLeave(string s) //user leaved from irc
        {
            _UserList.Items.Remove(s);
        }
        public void OnConnected() //irc Connected
        {
            AddMessage("connected");
            Join(_Room);
        }
        public void Leave(string room)
        {
            _NetworkStream.WriteLine(Trace2("part " + room));
        }
        public void Join(string room)
        {
            _NetworkStream.WriteLine(Trace2("join " + room));
        }
        ObservableCollection<Item> _ServerList = new ObservableCollection<Item>(); //serverlist for datagrid
        public void AddMessage(string msg) { AddMessage(null, msg); }
        public void AddMessage(string user, string msg)//adding message to textwindow
        {
            _TextOutput.Text += user + ":" + msg + "\r\n";

        }
        public string _Nick { get { return _LocalDatabase._Nick; } } 
        public string _Room { get { return _Config._IrcRoom; } }
        public void SendMessage(string msg) //sending message to irc
        {
            AddMessage(_Nick, msg);
            
            foreach(string s in msg.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                _NetworkStream.WriteLine(String.Format("PRIVMSG {0} :{1}", _Room, s));
        }
        public class IrcIm  //irc message
        {
            public string room;
            public string msg;
            public string user;
        }
        public class Item //ServerListView
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