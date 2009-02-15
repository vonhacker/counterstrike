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
    public partial class Irc : UserControl , IUpdate
    {
        public Irc()
        {            
            InitializeComponent();
            Loaded += new RoutedEventHandler(Irc_Loaded);
        }

        public void Update()
        {
            _ServerList.Update();
        }
        void Irc_Loaded(object sender, RoutedEventArgs e) //creating socket, connecting to _Config._Irc
        {
            _ServerList.Start();
            AddMessage("connecting to irc server");            
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
            _NetworkStream.Write(string.Format(Res._ircon, _Nick, "cslive", "localhost", "http://cslive.no-ip.org").Trace());            
            new Thread(Read).Start();            
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
            _Connected = false;
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

            if(Regex.Match(s,".+? 433 ").Success)            
                OnNickAlreadyInUse();
            
        }

        public void OnNickAlreadyInUse()
        {
            _LocalDatabase._Nick = null;
            AddMessage("Nick Already In Use");
        }
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
            _Connected = true;
            Send("CODEPAGE UTF-8");
            AddMessage("connected");
            Join(_Room);
        }
        public void Leave(string room)
        {
            Send(Trace2("part " + room));
        }
        public void Join(string room)
        {
            Send(Trace2("join " + room));
        }
        
        public void AddMessage(string msg) { AddMessage(null, msg); }
        public void AddMessage(string user, string msg)//adding message to textwindow
        {
            _TextOutput.Text += user + ":" + msg + "\r\n";

        }
        public string _Nick { get { return _LocalDatabase._Nick; } } 
        public string _Room { get { return _Config._IrcRoom; } }
        bool _Connected;
        public void SendMessage(string msg) //sending message to irc
        {
            AddMessage(_Nick, msg);
            foreach(string s in msg.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                Send(String.Format("PRIVMSG {0} :{1}", _Room, s));
            
        }
        public void Send(string s)
        {
            if(_Connected)
                _NetworkStream.WriteLine(s);
            else 
                AddMessage("error you are not connected");
        }
        public class IrcIm  //irc message
        {
            public string room;
            public string msg;
            public string user;
        }
        
        public static T Trace2<T>(T t)
        {
            t.Trace();
            return t;
        }
    }
}