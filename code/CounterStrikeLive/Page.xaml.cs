#define SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Windows.Media;
using System.Net.Sockets;
using System.Windows.Browser;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Collections;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Threading;

using ICSharpCode.SharpZipLib.Zip;
using doru;
using doru.Tcp;
using CounterStrikeLive.Service;

[assembly: AssemblyVersionAttribute("3.0.*")]
namespace CounterStrikeLive
{
    public static class Extensions
    {

        public static void Send(this Sender _Sender,  PacketType _PacketType, params object[] data)
        {
            _Sender.Send(Helper.JoinBytes((byte)_PacketType, Helper.JoinBytes(data)));
        }

        public static void Send(this Sender _Sender, PacketType _PacketType)
        {
            _Sender.Send(new byte[] { (byte)_PacketType });
        }
        public static void Send(this Sender _Sender, PacketType _PacketType, byte[] data)
        {
            _Sender.Send(Helper.JoinBytes((byte)_PacketType, data));
        }

    }
    public static class Random {


        static System.Random _Random = new System.Random();
        public static int Next(int p)
        {
            return _Random.Next(p);
        }

        internal static int Next(int p, int _dist)
        {
            return _Random.Next(p,_dist);
        }

        internal static float Next(float p, float _d)
        {
            return (float)(p+ _Random.NextDouble()*_d);
        }
    }
    
    public class LocalDatabase
    {
        public static LocalDatabase _This;
        public LocalDatabase()
        {
            _This = this;
        }
        public double Volume = .5;
        public int _Points;
        public int _Deaths;
        public string _Nick;
    }
    public class SharedClient : INotifyPropertyChanged, ClientListItem
    {
        public SharedClient()
        {
            _Properties = (from p in GetType().GetProperties() where (p.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) != null) select p).ToList();
            if (_Properties.Count == 0) throw new Exception("Break");
            PropertyChanged += new PropertyChangedEventHandler(RemoteClient_PropertyChanged);
        }

        bool isSLowDown;
        [SharedObject(4)]
        public bool _IsSlowDown
        {
            get { return isSLowDown; }
            set
            {
                if (isSLowDown != value)
                {
                    isSLowDown = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("_IsSlowDown"));
                }

            }
        }

        float life;
        [SharedObject(3)]
        public float _Life
        {
            get { return life; }
            set
            {
                float lifechange = value - life;
                life = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Life"));
                if (_Player != null)
                    _Game.ShowDamage(_Player._Position, lifechange);
            }
        }        
        [SharedObject(3)]
        public int _Deaths
        {
            get { return _LocalDatabase._Deaths; }
            set
            {
                _LocalDatabase._Deaths = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Deaths"));
            }
        }
        
        public Sender _Sender { get { return _Menu._Sender; } }
        public static MemberInfo GetMember(int id, Type type)
        {
            MemberInfo[] _MemberInfos = type.GetMembers();
            return _MemberInfos[id];
        }
        public static int? GetMemberId(string name, Type type)
        {
            MemberInfo[] _MemberInfos = type.GetMembers();
            for (int i = _MemberInfos.Count() - 1; i >= 0; i--)
            {
                MemberInfo _MemberInfo = _MemberInfos[i];
                if (_MemberInfo.Name == name) return i;
            }
            return null;
        }
        [SharedObjectAttribute(1)]
        public float? _StartPosX
        {
            get
            {
                if (_Player == null) return null;
                return _Player._x;
            }
            set
            {
                _Player._x = value.Value;
                PropertyChanged(this, new PropertyChangedEventArgs("_StartPosX"));
            }
        }
        [SharedObjectAttribute(1)]
        public float? _StartPosY
        {
            get
            {
                if (_Player == null) return null;
                return _Player._y;
            }
            set
            {
                _Player._y = value.Value;
                PropertyChanged(this, new PropertyChangedEventArgs("_StartPosY"));
            }
        }
        public class SharedObjectAttribute : Attribute
        {
            public int _Priority;
            public SharedObjectAttribute(int _Priority)
            {
                this._Priority = _Priority;
            }
        }
        Player.Team team = Player.Team.spectator;
        [SharedObjectAttribute(-1)]
        public Player.Team _Team
        {
            get { return team; }
            set
            {
                if (team == value) return;
                team = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Team"));
            }
        }
        public enum PlayerState { dead, alive, removed };

        private Database.PlayerType playerType = Database.PlayerType.unknown;
        [SharedObjectAttribute(-3)]
        public Database.PlayerType _PlayerType
        {
            get { return playerType; }
            set
            {
                if (playerType == value) return;
                playerType = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_PlayerType"));
            }
        }
        PlayerState playerState;
        [SharedObjectAttribute(0)]
        public PlayerState _PlayerState
        {
            get
            {
                return playerState;
            }
            set
            {
                if (value == playerState) return;
                playerState = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_PlayerState"));
                if (playerState == PlayerState.alive)
                {
                    CreatePlayer();
                }
                else if (playerState == PlayerState.dead)
                {
                    _Game.Die(_Player);
                    _Player.Remove();
                }
                else if (playerState == PlayerState.removed)
                {
                    if (_Player != null)
                        _Player.Remove();
                }
            }
        }

        private void CreatePlayer()
        {
            if (_Local)
                _Player = new LocalPlayer();
            else
                _Player = new Player();
            if (_PlayerType != Database.PlayerType.TPlayer && _PlayerType != Database.PlayerType.CPlayer) throw new Exception("Break");
            _Player._dbPlayer = Menu._Database.GetPlayer(_PlayerType);
            _Player._Client = this;
            _Player._Game = _Game;
            _Player.Load();
        }
        public Game _Game { get { return _Menu._Game; } }
		protected float ping;
		//[SharedObjectAttribute(1)]
		public float _ping
		{
			get { return ping; }
			set
			{
				ping = value;
				if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_ping"));
			}
		}


        public MyObs<SharedClient> _RemoteClients { get { return _Menu._Clients; } }
        private int? id;
        public int? _id
        {
            get { return id; }
            set
            {
                id = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_id"));
            }
        }
        public LocalDatabase _LocalDatabase = new LocalDatabase();

        [SharedObjectAttribute(-2)]
        public string _Nick
        {
            get { return _LocalDatabase._Nick; }
            set
            {
                _LocalDatabase._Nick = value;
                _Menu.WriteKillText(value + " Joined");
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_Nick"));
            }
        }
        [SharedObjectAttribute(1)]
        public int _Points
        {
            get { return _LocalDatabase._Points; }
            set
            {
                _LocalDatabase._Points = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_Points"));
            }
        }


        public void Add()
        {
            _RemoteClients[_id.Value] = this;
        }
        public void Remove()
        {
            _RemoteClients[_id.Value] = null;
            if (_Player != null) _Player.Remove();
        }
        public Player _Player { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Menu _Menu;
        void RemoteClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_Local)
            {
                Send(null, e.PropertyName);
            }
        }
        List<PropertyInfo> _Properties;
        public void Send(int? _SendTo, string _PropertyName)
        {
            PropertyInfo _PropertyInfo = GetType().GetProperty(_PropertyName);
            int i = _Properties.IndexOf(_PropertyInfo);
            if (i != -1)
            {
                object value = _PropertyInfo.GetValue(this, null);
                if (value != null)
                {
                    using (BinaryWriter _BinaryWriter = new BinaryWriter(new MemoryStream()))
                    {
                        if (_SendTo != null)
                        {
                            _BinaryWriter.Write((byte)PacketType.sendTo);
                            _BinaryWriter.Write((byte)_SendTo);
                        }
                        _BinaryWriter.Write((byte)PacketType.sharedObject);
                        _BinaryWriter.Write((byte)i);
                        if (value is int) _BinaryWriter.Write((Int16)(int)value);
                        else if (value is string) _BinaryWriter.Write((string)value);
                        else if (value is Enum) _BinaryWriter.Write(value.ToString());
                        else if (value is float) _BinaryWriter.Write((float)value);
                        else if (value is byte) _BinaryWriter.Write((byte)value);
                        //else if (value is float?) _BinaryWriter.Write(((float?)value).Value);
                        else if (value is bool) _BinaryWriter.Write((bool)value);
                        else throw new Exception("Shared Send Unkown value");
                        _Menu._Sender.Send(((MemoryStream)_BinaryWriter.BaseStream).ToArray());
                        //_This.Provider.SendMessage(((MemoryStream)_BinaryWriter.BaseStream).ToArray());
                    }
                }
            }
        }
        public void SendAll(int? _SendTo)
        {
            if (!_Local) throw new Exception("Break");

            Trace.WriteLine("Sending All Sharedobjects To " + _SendTo ?? "all");
            List<PropertyInfo> _PropertyInfos = (from p in GetType().GetProperties()
                                                 from a in p.GetCustomAttributes(true)
                                                 where a is SharedObjectAttribute
                                                 orderby ((SharedObjectAttribute)a)._Priority
                                                 select p).ToList();
            foreach (PropertyInfo _PropertyInfo in _PropertyInfos)
                Send(_SendTo, _PropertyInfo.Name);

        }
        public void onReceive(BinaryReader _BinaryReader)
        {
            if (_Local) throw new Exception("Break");

            int id = _BinaryReader.ReadByte();
            PropertyInfo _PropertyInfo = _Properties[id];
            if (_PropertyInfo.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) == null)
                throw new Exception("Break");
            Type type = _PropertyInfo.PropertyType;
            if (type.IsAssignableFrom(typeof(int)))
                _PropertyInfo.SetValue(this, _BinaryReader.ReadInt16(), null);
            else if (type.IsAssignableFrom(typeof(string)))
                _PropertyInfo.SetValue(this, _BinaryReader.ReadString(), null);
            else if (type.BaseType == typeof(Enum))
                _PropertyInfo.SetValue(this, Enum.Parse(type, _BinaryReader.ReadString(), false), null);
            else if (type.IsAssignableFrom(typeof(float)))
                _PropertyInfo.SetValue(this, _BinaryReader.ReadSingle(), null);
            else if (type.IsAssignableFrom(typeof(bool)))
                _PropertyInfo.SetValue(this, _BinaryReader.ReadBoolean(), null);
            else throw new Exception("Break");

            //if (_PropertyInfo.Name != "_ping")
            Trace.WriteLine("shared value received " + _PropertyInfo.Name + ": " + _PropertyInfo.GetValue(this, null).ToString());

        }
        public bool _Local = false;
    }
    public class MyObs<T> : IEnumerable<T>, INotifyCollectionChanged
    {
        ObservableCollection<T> _List = new ObservableCollection<T>();
        T[] a;
        public MyObs(int count)
        {
            _List.CollectionChanged += new NotifyCollectionChangedEventHandler(List_CollectionChanged);
            a = new T[count];
        }


        void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(sender, e);
        }
        public T this[int i]
        {
            get { return a[i]; }
            set
            {
                T oldValue = a[i];
                a[i] = value;

                if (oldValue == null)
                {
                    _List.Add(value);
                }
                else
                {
                    _List.Remove(oldValue);
                    if (value != null) _List.Add(value);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return _List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _List.GetEnumerator();
        }
    }
    public partial class Menu : UserControl
    {
        public static TimerA _TimerA = new TimerA();
        public static bool isnotBlend = Environment.Version.Major == 3;
        public MyObs<SharedClient> _Clients { get; set; }

        public static Menu _This;

        
        
        public Menu()
        {
            
            _This = this;
            InitializeComponent();
            _Clients = new MyObs<SharedClient>(50);
            _ScoreBoard.LayoutRoot.DataContext = this;            
            Loaded += new RoutedEventHandler(PageLoaded);
        }

        

        public void WriteLine(object str)
        {
            str.Trace("WriteLine");
            //_Console.Text = str + "\n" + _This._Console.Text;
        }
        public double _Width { get { return _RootLayout.Width; } }
        public double _Height { get { return _RootLayout.Height; } }


        IsolatedStorageFile _IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForSite();

        string _Datafile = "data.xml";
        XmlSerializer _XmlSerializer2 = new XmlSerializer(typeof(LocalDatabase));

        public LocalDatabase _LocalDatabase = new LocalDatabase();
        public string _host;
        public int _port = 4530;
        Config _Config;
        void PageLoaded(object sender, RoutedEventArgs e)
        {
            

            Loading1.Text = "Loading Config";
            
            version.Text = Assembly.GetExecutingAssembly().FullName;
            ((Storyboard)Resources["DamageStoryboard"]).Begin();
            App.Current.Host.Content.Resized += new EventHandler(Content_Resized);
            App.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
            if (App.Current.Host.Source.DnsSafeHost.Length == 0) throw new Exception("Break");
            _ScoreBoard.Hide();
            _host = App.Current.Host.Source.DnsSafeHost;
            new Downloader().Download("Config.xml", delegate(Stream s)
            {                                
                _Config= (Config)Config._XmlSerializer.Deserialize(s);                
                _port = _Config._GamePort;
                EnterNick();
            });
        }
        public void EnterNick()
        {
            Loading1.Text = "Loading isolated storage";
            this.Loading1.Hide();
            try
            {
                if (_IsolatedStorageFile.FileExists(_Datafile))
                    using (FileStream _FileStream = _IsolatedStorageFile.OpenFile(_Datafile, FileMode.Open, FileAccess.Read))
                    {
                        _LocalDatabase = (LocalDatabase)_XmlSerializer2.Deserialize(_FileStream);
                    }
            } catch (InvalidOperationException e) { e.Trace("cannot load isolated storage database"); }
            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();
                _EnterNick.Success+= delegate
                {
                    LoadDb();
                };
            }
            else LoadDb();
        }
        

        

        public void WriteCenterText(string text)
        {
            _CenterText.Text += text + "\n";
        }
        public void LoadDb()
        {
            Loading1.Text = "Loading Content";
            Loading1.Value = 10;
            FolderList _FolderList =
                (FolderList)FolderList._XmlSerializer.Deserialize(
                App.GetResourceStream(new Uri(_Config._ContentFolder + "Content.xml", UriKind.Relative)).Stream);
            _FolderList.Load();
            Loading1.Text = "Loading Database";
            Loading1.Value = 20;
            App.Current.Exit += new EventHandler(Current_Exit);
            
            
            Content_Resized(null, null);

            Stream _Stream = App.GetResourceStream(new Uri("Content/db.xml", UriKind.Relative)).Stream;
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database));
            _Database = (Database)_XmlSerializer.Deserialize(_Stream);


            foreach (Database.ILoad _ILoad in Database._ILoads)
            {
                _ILoad.Load();
            }
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
            _SocketAsyncEventArgs.RemoteEndPoint = new DnsEndPoint(_host, _port); //port
            _SocketAsyncEventArgs.UserToken = _Socket;
            _Socket.ConnectAsync(_SocketAsyncEventArgs);

            Trace.WriteLine("Connecting");
            Loading1.Text = "Connecting";
            _SocketAsyncEventArgs.Completed += delegate(object o, SocketAsyncEventArgs e)
            {
                Dispatcher.BeginInvoke(new Action(OnConnected));
            };

            
        }
        
        public void OnConnected()
        {           
            
            if (_Socket.Connected == false) throw new Exception("Cannot Connect!");
            Trace.WriteLine("Connected");
            Loading1.Text = "Connected";
            Loading1.Value = 30;
            this.Cursor = Cursors.None;

            _Sender._Socket = _Listener._Socket = _Socket;

            _Sender._NetworkStream = _Listener._NetworkStream = _Config.GenerateClientLag ? new LagStream(_Socket) {  interval= 2100 } : new NetworkStream(_Socket);
            
            _Listener.StartAsync("listener");
            _Storyboard.Begin();
            _Storyboard.Completed += new EventHandler(Update);

            KeyDown += new KeyEventHandler(Page_KeyDown);
            KeyUp += new KeyEventHandler(PageKeyUp);
            
            MouseMove += new MouseEventHandler(Menu_MouseMove);

            

            MouseLeftButtonDown += new MouseButtonEventHandler(Menu_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(Menu_MouseLeftButtonUp);

        }
        
        void OnMessage(byte[] Data)
        {
            
            using (var _MemoryStream = new MemoryStream(Data))
            {
                int SenderID = _MemoryStream.ReadB();
                PacketType _PacketType = (PacketType)_MemoryStream.ReadB();
                var _BinaryReader = new BinaryReader(_MemoryStream);
                if (SenderID == (int)PacketType.serverid)
                {
                    switch (_PacketType)
                    {
                        case PacketType.playerid:
                            {
                                int id = _BinaryReader.ReadByte();
                                //Trace.id = id;
                                _LocalClient = new SharedClient();
                                _LocalClient._LocalDatabase = _LocalDatabase;
                                _LocalClient._id = id;
                                _LocalClient._Menu = this;
                                _LocalClient._Local = true;
                                _LocalClient.Add();
                                _LocalClient.SendAll(null);
                                Trace.WriteLine("ID Received:" + id);                                
                            }                        
                            break;
                        case PacketType.SelectMap:
                            {

                                MapSelect _MapSelect = new MapSelect();
                                
                                _MapSelect.Success+= delegate
                                {
                                    _MapSelect.Trace("Closed");
                                    _Sender.Send(PacketType.MapSelected, _MapSelect.MapName.ToBytes());
                                };
                            }
                            break;
                        case PacketType.ServerIsFull:
                            Loading1.Text = "Server Is Full";
                            new ChildWindow().Content = "Server Is Full";
                            break;
                        case PacketType.map:
                            {
                                if (MapSelect._This != null) MapSelect._This.Close();
                                _GameState = GameState.mapdownload;
                                string _Map = _MemoryStream.ReadStringToEnd();
                                Trace.WriteLine("MapInfo name Received:" + _Map);
                                LoadResources(_Map);
                                //LoadGame(App.GetResourceStream(new Uri(_Map, UriKind.Relative)).Stream);
                            }
                            break;
                        case PacketType.ping:
                            {
                              _Sender.Send(new byte[] { (byte)PacketType.pong });
                                //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.pong });
                            }
                            break;
                        default: throw new Exception("Break");
                    }
                }
                else
                {
                    if (_LocalClient._id == null) throw new Exception();
                    if (SenderID == _LocalClient._id && _PacketType != PacketType.pinginfo) throw new Exception();
                    SharedClient _SharedClient = _Clients[SenderID];
                    //Player _Player = _SharedClient!= null ? _SharedClient._Player : null; 
                    if (_PacketType != PacketType.PlayerJoined && _Clients[SenderID] == null) return;
                    switch (_PacketType)
                    {
                        case PacketType.rotation:
                            {
                                byte angle = _BinaryReader.ReadByte();
                                if (_Clients[SenderID] != null)
                                {
                                    SharedClient _RemoteClient = _Clients[SenderID];
                                    if (_RemoteClient._PlayerState == SharedClient.PlayerState.alive)
                                    {
                                        Player _RemotePlayer = _RemoteClient._Player;
                                        _RemotePlayer._Angle = Listener.Decode(angle, 0, 360);
                                    }
                                }
                            }
                            break;
                        case PacketType.voteMap:
                            {
                                string s =_MemoryStream.ReadStringToEnd();
                                _Chat.Text += _SharedClient._Nick + " Voted MapInfo " + s;
                            }
                            break;
                        case PacketType.addPoint:
                            {
                                SharedClient _KillerClient = _Clients[_BinaryReader.Read()];
                                _KillerClient._Points++;
                                ShowKilledMessage(_KillerClient, _SharedClient);
                            }
                            break;
                        case PacketType.Reloading:
                            _SharedClient._Player.ReloadSound();
                            break;
                        case PacketType.shoot: goto case PacketType.firstshoot;
                        case PacketType.firstshoot:
                            {

                                Player _RemotePlayer = _SharedClient._Player;
                                float _Angle = Listener.DecodeInt(_BinaryReader.ReadUInt16(), 0, 360);
                                _Game.Shoot(_RemotePlayer._x, _RemotePlayer._y, _Angle, _RemotePlayer,_PacketType == PacketType.firstshoot);
                            }
                            break;
                        case PacketType.PlayerJoined:
                            {
                                Trace.WriteLine("Player Joiend " +SenderID);
                                SharedClient _RemoteClient = new SharedClient();
                                _RemoteClient._Menu = this;
                                _RemoteClient._id =SenderID;
                                _RemoteClient.Add();
                                _LocalClient.SendAll(SenderID);
                            }
                            break;
                        case PacketType.pinginfo:
                            {
                                _SharedClient._ping = _BinaryReader.ReadInt16();
                            }
                            break;
                        case PacketType.PlayerLeaved:
                            WriteKillText(_SharedClient._Nick + " Player Leaved");
                            _SharedClient.Remove();
                            Trace.WriteLine("Player Leaved " +SenderID);
                            if (_Game != null) _Game.CheckWins();
                            break;
                        case PacketType.keyDown:
                            {
                                Key k = (Key)_BinaryReader.ReadByte();
                                if (_SharedClient._Player != null)
                                {
                                    _SharedClient._Player.OnKeyDown(k);
                                }
                            }
                            break;
                        case PacketType.checkWins:
                            Debug.WriteLine("Check Wins Received");
                            if (_Game != null) _Game.CheckWins();
                            break;
                        case PacketType.chat:
                            {
                                _Chat.Text += _MemoryStream.ReadStringToEnd()+"\r\n";
                            }
                            break;
                        case PacketType.keyUp:
                            {
                                Key k = (Key)_BinaryReader.ReadByte();
                                int x = _BinaryReader.ReadInt16();
                                int y = _BinaryReader.ReadInt16();
                                Player _Player = _SharedClient._Player;
                                if (_Player != null)
                                {
                                    _Player.OnKeyUp(k);
                                    _Player._x = x;
                                    _Player._y = y;
                                }
                            }
                            break;
                        case PacketType.sharedObject:
                            {
                                _Clients[SenderID].onReceive(_BinaryReader);
                            }
                            break;
                        default: throw new Exception();
                    }
                    if (_BinaryReader.BaseStream.Position != _BinaryReader.BaseStream.Length)
                        throw new Exception("Break");
                }
            }
        }

        public static Dictionary<string, Stream> _Resources = new Dictionary<string, Stream>();
        bool resloaded;
        private void LoadResources(string _Map)        
        {
            Trace.Assert(!resloaded);
            resloaded = true;
            Loading1.Text = "MapInfo Name Received";
            WebClient _WebClient = new WebClient();
            _WebClient.OpenReadAsync(new Uri(_Map, UriKind.Relative));
            _WebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(WebClient_OpenReadCompleted);
            _WebClient.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e)
            {
                Loading1.Value = e.ProgressPercentage;
                Loading1.Text = "Loaded " + (int)e.BytesReceived / 1024 + "/" + (int)e.TotalBytesToReceive / 1024; 
            };
        }
        void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Loading1.Text = "Executing MapInfo";
            "loading map".Trace();
            ZipInputStream _ZipInputStream = new ZipInputStream(e.Result);
            while (true)
            {
                ZipEntry _ZipEntry = _ZipInputStream.GetNextEntry();
                if (_ZipEntry == null) break;                                
                MemoryStream ms = new MemoryStream(_ZipInputStream.Read());
                if (_ZipEntry.IsFile)                
                    _Resources.Add(_ZipEntry.Name.Trace(), ms);                                    
            }
            LoadGame(_Resources["map.xml"]);

            _Sender.Send(new byte[] { (byte)PacketType.Join });
        }        
        

        void provider_ServerFailed(object sender, EventArgs e)
        {

        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            ScriptObject _ScriptObject = (ScriptObject)HtmlPage.Window.GetProperty("screen");
            _RootLayout.Width = (double)_ScriptObject.GetProperty("width");
            _RootLayout.Height = (double)_ScriptObject.GetProperty("height");
        }

        void Content_Resized(object sender, EventArgs e)
        {
            _RootLayout.Width = App.Current.Host.Content.ActualWidth;
            _RootLayout.Height = App.Current.Host.Content.ActualHeight;
        }
        //void AutoSelectButton_Click(object sender, RoutedEventArgs e)
        //{
        //    int cterr = (from pl in _Clients where pl != null && pl._Team == Player.Team.cterr select pl).Count();
        //    int terr = (from pl in _Clients where pl != null && pl._Team == Player.Team.terr select pl).Count();
        //    if (cterr > terr)
        //        _Game.TerroristsButtonClick();
        //    else
        //        _Game.CTerroritsButtonClick();
        //}
        //void TerroristsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _Game.TerroristsButtonClick();
        //}
        //void CTerroritsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _Game.CTerroritsButtonClick();
        //}
        //void SpectatorButton_Click(object sender, RoutedEventArgs e)
        //{
        //    _Game.SpectatorButtonClick();
        //}

        public static Database _Database;

        

        public void LoadBitmaps(List<Database.AnimatedBitmap> imgs)
        {
            foreach (var a in imgs)
            {
                a.Load();
            }
        }
        Socket _Socket;
        
        public Sender _Sender = new Sender();
        Listener _Listener = new Listener();

        double _SendSecondsElapsed;
        const int _RotationSendInterval = 500;
        protected void SendRotation()
        {
            if (Menu._TimerA.TimeElapsed(_RotationSendInterval))
            {
                _SendSecondsElapsed = _SendSecondsElapsed % _RotationSendInterval;
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write((byte)PacketType.rotation);
                    _BinaryWriter.Write(Sender.Encode(_Game._LocalPlayer._Angle, 0, 360));
                    _Sender.Send(_MemoryStream.ToArray());
                    //_This.Provider.SendMessage(_MemoryStream.ToArray());
                }
            }
        }

        

        void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_Game != null)
            {
                _Game.OnKeyDown(Key.Left);
                _Game.OnKeyUp(Key.Left);
            }
        }

        void Menu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _MouseLeftButtonDown = false;
        }

        void Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            _MouseLeftButtonDown = true;
        }
        public static bool _MouseLeftButtonDown;
        public static Vector2 _Mouse;
        public static Vector2 _MouseMove;
        public static MouseEventArgs _MouseEventArgs;
        void Menu_MouseMove(object sender, MouseEventArgs e)
        {
            _MouseEventArgs = e;
            Point _GetPosition = e.GetPosition(this);
            if (_Mouse != default(Vector2)) _MouseMove = new Vector2((float)_GetPosition.X, (float)_GetPosition.Y) - _Mouse;
            _Mouse = new Vector2((float)_GetPosition.X, (float)_GetPosition.Y);
            if (_Game != null) _Game.MouseMove();


        }
        void PageKeyUp(object sender, KeyEventArgs e)
        {
            if (_Game != null)
                _Game.OnKeyUp(e.Key);
            if (_Keyboard.Contains(e.Key)) _Keyboard.Remove(e.Key);
        }
        public static List<Key> _Keyboard = new List<Key>();

        void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (_Keyboard.Contains(e.Key)) return;
            _Keyboard.Add(e.Key);


            if (Key.T == e.Key) new ChatBox();            
            if (Key.Tab == e.Key || Key.Back == e.Key)
                _ScoreBoard.Toggle();                        

            if (_Game != null) _Game.OnKeyDown(e.Key);

            if (e.Key == Key.Escape)
            {                
                if (EscMenu._This == null)
                    new EscMenu();
            }
            
        }


        double _CenterTextTimeElapsed;
        double _KillTextElapsed;
        double _ChatTextElapsed;
        void ClearTextBlock(ref double _Elapsed, TextBlock _TextBlock)
        {
            ClearTextBlock(ref _Elapsed, _TextBlock, 5000);
        }
        void ClearTextBlock(ref double _Elapsed, TextBlock _TextBlock,int delay)
        {
            if (_Elapsed > delay)
            {
                _Elapsed = 0;
                int i = _TextBlock.Text.IndexOf('\n') + 1;
                if (i > 0) _TextBlock.Text = _TextBlock.Text.Remove(0, i);
                else _TextBlock.Text = "";
            }
            if (_TextBlock.Text.Length > 0)
                _Elapsed += Menu._TimerA._TimeElapsed;

        }

        void Update(object sender, EventArgs e)
        {
            
            ClearTextBlock(ref _KillTextElapsed, _KillText);
            ClearTextBlock(ref _ChatTextElapsed, _Chat);

            ClearTextBlock(ref _CenterTextTimeElapsed, _CenterText,2000);

            _Storyboard.Begin();

            List<Byte[]> _Messages = _Listener.GetMessages();
            foreach (byte[] _buffer in _Messages)
            {
                OnMessage(_buffer);
            }

            if (_Listener._Connected == false)
            {
                throw new Exception("Connection with server Have Been Lost!");
            }

            if (_GameState != GameState.mapdownload)
            {
                if (_GameState == GameState.alive)
                {
                    SendRotation();
                    _TextBlockLife.Text = _Game._LocalPlayer._Life.ToString();
                }
                _Game.Update();
            }
            _MouseMove = new Vector2();

            if (Menu._TimerA.TimeElapsed(200))
            {
                double? fps = Menu._TimerA.GetFps();
                if (fps != null)
                {
                    _FPS.Text = "FPS: " + ((int)fps).ToString() + "'";
                }
            }

            Menu._TimerA.Update();
        }                

        public enum GameState
        {
            mapdownload, teamSelect, spectator, alive
        }
        public GameState _GameState = GameState.mapdownload;

        void Current_Exit(object sender, EventArgs e)
        {
            if (_LocalClient != null)
                using (FileStream _FileStream = _IsolatedStorageFile.OpenFile(_Datafile, FileMode.Create, FileAccess.ReadWrite))
                {
                    _XmlSerializer2.Serialize(_FileStream, _LocalClient._LocalDatabase);
                }
            Trace.WriteLine("Exit");
        }



        public Game _Game;
        
        public SharedClient _LocalClient;

        protected void LoadGame(Stream _Map)
        {
            Loading1.Text = "Loading Game";            
            if (_Game != null) _Game.Dispose();
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(MapDatabase));
            MapDatabase _MapDatabase = (MapDatabase)_XmlSerializer.Deserialize(_Map);
            _Game = new Game();
            _Game._Menu = this;
            _Game._Sender = _Sender;
            _Game.Load(_MapDatabase);
            _GameCanvas.Children.Add(_Game._Canvas);
            Loading1.Hide();
        }
        Storyboard _Storyboard = new Storyboard();



        internal void ShowKilledMessage(SharedClient _Killer, SharedClient _Player)
        {
            _KillText.Text += _Killer._Nick + " killed " + _Player._Nick + "\n";
        }
        public void WriteKillText(string text)
        {
            _KillText.Text += "\n" + text;
        }
    }
    //public static class Arrays1
    //{
    //    public static TSource Random<TSource>(this List<TSource> source)
    //    {
    //        Random
    //        return source[Random.Next(source.Count)];
    //    }        
    //}
    public interface ClientListItem
    {
        Player.Team _Team { get; }
        float _ping { get; }
        int _Deaths { get; }
        int _Points { get; }
        int? _id { get; }
        string _Nick { get; }
    }
    public class Cursor
    {
        public float _x;
        public float _y;
        public Menu _Menu;
        public Canvas _Canvas = new Canvas();
        public void Load()
        {

            MakeLine(0);
            MakeLine(90);
            MakeLine(180);
            MakeLine(270);
            _Menu._CursorCanvas.Children.Add(_Canvas);
        }
        protected void MakeLine(int angle)
        {
            Line _Line = new Line();
            _Line.X1 = 5;
            _Line.X2 = 10;
            _Line.Y1 = 0;
            _Line.Y2 = 0;
            _Line.StrokeThickness = 3;
            _Line.Stroke = new SolidColorBrush(Colors.Red);
            RotateTransform _RotateTransform = new RotateTransform();
            _RotateTransform.Angle = angle;
            _Line.RenderTransform = _RotateTransform;
            _Canvas.Children.Add(_Line);
            _Canvas.RenderTransform = _ScaleTransform;
        }
        public float _Scale = 1;
        public float Scale { get { return _Scale - 1; } }
        public void Update()
        {
            _Scale -= (float)Menu._TimerA._SecodsElapsed * 3;
            if (_Scale < 1) _Scale = 1;
            _ScaleTransform.ScaleX = _Scale;
            _ScaleTransform.ScaleY = _Scale;
            Canvas.SetLeft(_Canvas, _x);
            Canvas.SetTop(_Canvas, _y);
        }
        ScaleTransform _ScaleTransform = new ScaleTransform();
    }
    public class Game
    {
        public Game()
        {
            _This = this;
        }
        public static Game _This;
        public bool _IsEverthingVisible;
        public List<AnimDamage> _AnimDamages = new List<AnimDamage>();        
        internal void ShowDamage(Vector2 pos, float change)
        {
            AnimDamage _AnimDamage = new AnimDamage { _Game = this, _Position = pos };
            _AnimDamage._Game = this;
            _AnimDamage.Load(change.ToString());
        }        
        public Cursor _Cursor;
        public Menu.GameState _GameState { get { return _Menu._GameState; } set { _Menu._GameState = value; } }        
        public void OnKeyDown(Key _Key)
        {
            if (Key.Add == _Key)
            {
                _ScaleTransform.ScaleX = _ScaleTransform.ScaleY += .2;
            }
            if (Key.Subtract == _Key)
            {
                _ScaleTransform.ScaleX = _ScaleTransform.ScaleY -= .2;
                if (_Scale < .2) _Scale = .2;
            }

            if (_Key == Key.M || _Key == Key.End)
            {
                new TeamSelect();
            }

            if ((_Key == Key.PageUp || _Key == Key.R) && _TotalPatrons != 0 && _LocalPlayer != null)
            {
                _Sender.Send(PacketType.Reloading);
                _LocalPlayer.ReloadSound();
                _LocalPlayer._isReloading = true;
                _TotalPatrons -= 30;
                WriteCenterText("Reloading");

                Menu._TimerA.AddMethod(1500, delegate { _Patrons = 30; if (_LocalPlayer != null) _LocalPlayer._isReloading = false; });
            }

            if ((_Key == Key.PageDown || _Key == Key.Space) && _GameState != Menu.GameState.alive)
            {
                _SpeciateId++;
                if (_SpeciateId < _Players.Count) WriteCenterText("Player: " + _Players[_SpeciateId]._Client._Nick);
            }
            if (Menu.GameState.alive == _Menu._GameState)
            {
                _Sender.Send(new byte[] { (byte)PacketType.keyDown, (byte)_Key });
                _Key.Trace("sending Key");
                //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.keyDown, (byte)_Key });
                _LocalPlayer.OnKeyDown(_Key);
            }
        }
        public void OnKeyUp(Key _Key)
        {
            if (Menu.GameState.alive == _Menu._GameState)
            {
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write((byte)PacketType.keyUp);
                    _BinaryWriter.Write((byte)_Key);
                    _BinaryWriter.Write((Int16)_LocalPlayer._x);
                    _BinaryWriter.Write((Int16)_LocalPlayer._y);
                    _Sender.Send(_MemoryStream.ToArray());
                    //_This.Provider.SendMessage(_MemoryStream.ToArray());
                }
                _LocalPlayer.OnKeyUp(_Key);
            }
        }
        public List<Player> _Players = new List<Player>();
        public Map _Map;
        public Menu _Menu;
        public Canvas _Canvas = new Canvas();
        TranslateTransform _TranslateTransform = new TranslateTransform();
        
        public Point _FreeViewPos;

        public void Load(MapDatabase _MapDatabase)
        {
            _Scale = .5;
            _FreeViewPos = _MapDatabase._CStartPos;
            //_This._Console.Hide(); ;

            new TeamSelect();

            _Menu._GameState = Menu.GameState.teamSelect;

            _Cursor = new Cursor();
            _Cursor._Menu = _Menu;
            _Cursor.Load();

            _Map = new Map();
            _Canvas.Children.Add(_Map._Canvas);
            TransformGroup _TransformGroup = new TransformGroup();
            _TransformGroup.Children.Add(_ScaleTransform);
            _TransformGroup.Children.Add(_TranslateTransform);
            _Canvas.RenderTransform = _TransformGroup;
            _Map.LoadMap(_MapDatabase);            
        }
        public ScaleTransform _ScaleTransform = new ScaleTransform();
        public void SpectatorButtonClick()
        {
            _LocalClient._Team = Player.Team.spectator;
            OnTeamSelect();
        }

        private void OnTeamSelect()
        {
            _LocalClient._PlayerState = SharedClient.PlayerState.dead;
            _GameState = Menu.GameState.spectator;
            
            SendCheckWins();
        }

        public void TerroristsButtonClick()
        {
            OnTeamSelect();
            _LocalClient._PlayerType = Database.PlayerType.TPlayer;
            _LocalClient._Team = Player.Team.terr;
            Debug.WriteLine("Terrorsits team selected");
        }
        public void AutoSelectClick()
        {
            int cterr = (from pl in _Clients where pl != null && pl._Team == Player.Team.cterr select pl).Count();
            int terr = (from pl in _Clients where pl != null && pl._Team == Player.Team.terr select pl).Count();
            if (cterr > terr)
            {
                TerroristsButtonClick();
            } else
            {
                CTerroritsButtonClick();
            }
        }
        public void CTerroritsButtonClick()
        {
            OnTeamSelect();
            _LocalClient._PlayerType = Database.PlayerType.CPlayer;
            _LocalClient._Team = Player.Team.cterr;
            Debug.WriteLine("Counter terrorsits team selected");
        }

        private void SendCheckWins()
        {
            Debug.WriteLine("check Wins Sended");
            _Sender.Send(new byte[] { (byte)PacketType.checkWins });
            //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.checkWins });
            CheckWins();
        }
        public LocalPlayer _LocalPlayer { get { return (LocalPlayer)_LocalClient._Player; } }
        public SharedClient _LocalClient { get { return _Menu._LocalClient; } }

        public MyObs<SharedClient> _Clients { get { return _Menu._Clients; } }

        public static void PlaySound(string s)
        {
            MediaElement m = new MediaElement();
            Menu._This._GameCanvas.Children.Add(m);
            m.SetSource(s);
            m.Play();
            m.MediaEnded += delegate { Menu._This._GameCanvas.Children.Remove(m); };
        }

        protected void CreateLocalPlayerReset()
        {
            PlaySound("jingle.mp3");

            _ScoreBoard.Hide();
            _Patrons = 30;
            _TotalPatrons = 90;
            Trace.WriteLine("CreateLocalPlayerRestart");
            for (int i = _Explosions.Count - 1; i >= 0; i--)
            {
                Explosion _Explosion = _Explosions[i];
                _Explosion.Remove();
                Trace.WriteLine(_Explosion + " " + i + " removed");
            }
            _Map._Canvas1.Children.Clear();

            if (_LocalClient._Team != Player.Team.spectator)
            {
                _LocalClient._PlayerState = SharedClient.PlayerState.removed;
                _Menu._GameState = Menu.GameState.alive;
                _LocalClient._PlayerState = SharedClient.PlayerState.alive;

                Point _Point = _Map.GetPos(_LocalClient._Team);
                int _dist = 100;
                do
                {
                    _LocalClient._StartPosX = (int)_Point.X + (Random.Next(-_dist, _dist));
                    _LocalClient._StartPosY = (int)_Point.Y + (Random.Next(-_dist, _dist));
                } while (_LocalPlayer.PlayerCollide() != null);
                _LocalPlayer._Life = 100;
            }
        }

        public int _Points { get { return _Menu._LocalClient._Points; } set { _Menu._LocalClient._Points = value; } }

        public void LocalPlayerDead(Player _Killer)
        {

            if (_Killer == _LocalPlayer)
            {
                _Points--;
            }
            else
            {
                _Sender.Send(new byte[] { (byte)PacketType.addPoint, (byte)_Killer._ID });
                //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.addPoint, (byte)_Killer._ID });
            }
            //if (_This._GameState == Menu.GameState.spectator) throw new Exception("Break");
            _LocalClient._Deaths++;
            _ScoreBoard.Show();
            _Menu._GameState = Menu.GameState.spectator;
            _SpeciateId = _Killer._ID;
            _Menu.ShowKilledMessage(_Killer._Client, _LocalPlayer._Client);
            _LocalClient._PlayerState = SharedClient.PlayerState.dead;
            Trace.WriteLine("Local Plyater Dead");
            SendCheckWins();
        }
        public ScoreBoard _ScoreBoard { get { return _Menu._ScoreBoard; } }
        public void CheckWins()
        {
            Trace.WriteLine("CheckWins");
            const int interval = 3000;
            if ((from pl in _Players where pl._Client._Team == Player.Team.cterr select pl).Count() == 0)
            {
                Trace.WriteLine("Terrorists Win");
                _CenterText.Text += "Terrorists Win\n";
                Menu._TimerA.AddMethod(interval, CreateLocalPlayerReset);
                
            }
            else if ((from pl in _Players where pl._Client._Team == Player.Team.terr select pl).Count() == 0)
            {
                Trace.WriteLine("Counter Terrorists Win");
                _CenterText.Text += "Counter Terrorists Win\n";
                Menu._TimerA.AddMethod(interval, CreateLocalPlayerReset);
            }
        }


        TextBlock _CenterText { get { return _Menu._CenterText; } set { _Menu._CenterText = value; } }


        public void Die(Player _Player)
        {
            Explosion _Explosion = new Explosion();
            _Player.PlaySound("death1.mp3");
            _Explosion._AnimatedBitmap = _Player._dbPlayer._PlayerDie;
            _Explosion._Position = _Player._Position;
            _Explosion._Angle = _Player._Angle;
            _Explosion._Game = this;
            _Explosion._Remove = false;
            _Explosion.Load();
        }
        float _ShootInterval = 100;
        public List<Explosion> _Explosions = new List<Explosion>();
        public Sender _Sender;

        int patrons;
        public int _Patrons
        {
            get { return patrons; }
            set
            {
                patrons = value;
                _Menu._patrons.Text = patrons.ToString();
            }
        }
        int totalPatrons;
        public int _TotalPatrons
        {
            get { return totalPatrons; }
            set
            {
                totalPatrons = value;
                _Menu._totalpatrons.Text = totalPatrons.ToString();
            }
        }

        public void Update()
        {

            for (int i = _Explosions.Count - 1; i >= 0; i--)
            {
                _Explosions[i].CheckVisibility(_LocalPlayer);
                _Explosions[i].Update();
            }
            for (int i = _Players.Count - 1; i >= 0; i--)
            {
                Player _Player = _Players[i];
                _Player.CheckVisibility(_LocalPlayer);
                _Player.Update();
            }

            for (int i = _AnimDamages.Count - 1; i >= 0; i--)
            {
                AnimDamage _AnimDamage = _AnimDamages[i];
                _AnimDamage.CheckVisibility(_LocalPlayer);
                _AnimDamage.Update();
            }


            UpdatePlayer();
            _Cursor.Update();
        }
        double _ShootTimeElapsed;
        protected void UpdatePlayer()
        {
            if (_Menu._GameState == Menu.GameState.alive)
            {
                int _distance = 2000;
                UpdateView(_distance, _LocalPlayer._Position);
                Point _Mouse = new Point();
                if (Menu._MouseEventArgs != null) _Mouse = Menu._MouseEventArgs.GetPosition(_Canvas);

                float _x = (float)(_Mouse.X - _LocalPlayer._x);
                float _y = (float)(_Mouse.Y - _LocalPlayer._y);
                Vector2 _Vector2 = new Vector2(_x, _y);
                _LocalPlayer._Angle = (Calculator.VectorToRadians(_Vector2) / Calculator.RadiansToDegreesRatio);

                if (Menu._MouseLeftButtonDown && _Patrons != 0 && !_LocalPlayer._isReloading)
                {
                    _ShootTimeElapsed += Menu._TimerA._TimeElapsed;
                    if (_ShootTimeElapsed > _ShootInterval)
                    {
                        
                        _ShootTimeElapsed = 0;
                        _Patrons--;
                        float _d = 2;
                        _d = _d * (_Cursor.Scale * _Cursor.Scale);
                        float _Angle = _LocalPlayer._Angle + (float)Random.Next(-_d, _d);

                        _Angle = Player.CorrectAngle(_Angle);
                        using (MemoryStream _MemoryStream = new MemoryStream())
                        {
                            BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                            if (_d == 0)
                            {
                                Shoot(_LocalPlayer._x, _LocalPlayer._y, _Angle, _LocalPlayer, true);
                                _BinaryWriter.Write((byte)PacketType.firstshoot);
                            }
                            else
                            {
                                _BinaryWriter.Write((byte)PacketType.shoot);
                                Shoot(_LocalPlayer._x, _LocalPlayer._y, _Angle, _LocalPlayer, false);
                            }
                            _BinaryWriter.Write(Sender.EncodeInt(_Angle, 0, 360));
                            _Sender.Send(_MemoryStream.ToArray());
                            //_This.Provider.SendMessage(_MemoryStream.ToArray());
                        }
                        _Cursor._Scale += .5f;
                        if (_Cursor._Scale > 4) _Cursor._Scale = 4;
                    }
                }
            }
            if (_GameState != Menu.GameState.alive)
            {
                if (_SpeciateId == -1)
                {
                    if (Menu._Keyboard.Contains(Key.Left))
                    {
                        _FreeViewPos.X -= 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Right))
                    {
                        _FreeViewPos.X += 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Down))
                    {
                        _FreeViewPos.Y += 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Up))
                    {
                        _FreeViewPos.Y -= 30;
                    }
                    _TranslateTransform.X = -_FreeViewPos.X * _Scale + _Menu._Width / 2;
                    _TranslateTransform.Y = -_FreeViewPos.Y * _Scale + _Menu._Height / 2;
                }
                else if (_Players.Count - 1 < _SpeciateId)
                {
                    _SpeciateId = -1;
                    WriteCenterText("Free View Mode");
                }
                else
                {
                    Player _Player = _Players[_SpeciateId];
                    UpdateView(2000, _Player._Position);
                }
            }
        }
        public void WriteCenterText(string text)
        {
            _CenterText.Text += text + "\n";
        }
        public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }
        public double _Width { get { return _Menu._Width * _Scale; } }
        public double _Height { get { return _Menu._Height * _Scale; } }
        private void UpdateView(int _distance, Vector2 _Pos)
        {
            _FreeViewPos.X = _Pos.X;
            _FreeViewPos.Y = _Pos.Y;
            double _mousex = (Menu._Mouse.X / _Menu._Width) - .5;
            double _mousey = (Menu._Mouse.Y / _Menu._Height) - .5;

            _TranslateTransform.X = ((-_Pos.X * _Scale) + (_Menu._Width / 2) - _mousex * _distance*_Scale);
            _TranslateTransform.Y = ((-_Pos.Y * _Scale) + (_Menu._Height / 2) - _mousey * _distance * _Scale);
        }
        int _SpeciateId = 0;
        bool _friendlyfire=false;
        public void Shoot(float _x, float _y, float _Angle, Player _ShootingPlayer, bool firstshoot)
        {            
            _ShootingPlayer.ShootAnimation();
            
            Vector2 _MaxShootPoint = new Vector2(0, -5000);
            Vector2 _PlayerPos = new Vector2(_x, _y);
            Calculator.RotateVector(ref _MaxShootPoint, Calculator.DegreesToRadians(_Angle));
            _MaxShootPoint += _PlayerPos;
            Line2 _Line2;
            List<Map.LV> _Collisions = _Map.Collision(_MaxShootPoint, _PlayerPos, out _Line2);

            if (_Collisions.Count > 0)
            {


                Vector2 _VectorHoleRotation = new Vector2(0, -10);
                Calculator.RotateVector(ref _VectorHoleRotation, Calculator.DegreesToRadians(_Angle));

                Vector2 _CollisionPos;
                _CollisionPos = _Collisions.First()._Vector2;
                
                Explosion _SparkExplosion = new Explosion();
                _SparkExplosion._VisibleToAll = true;
                _SparkExplosion._AnimatedBitmap = Menu._Database._Sparks.Random();
                _SparkExplosion._Position = _CollisionPos;
                _SparkExplosion._Game = this;
                _SparkExplosion.Load();
                _SparkExplosion.PlaySound(Helper.Random("ric_conc-1.mp3", "ric_conc-2.mp3"),1000);

                for (int i = 0, Power = 0; i < _Collisions.Count && Power < 3; i++, Power++)
                {
                    _CollisionPos = _Collisions[i]._Vector2;
                    Ellipse _Ellipse = new Ellipse();
                    _Ellipse.Width = 5;
                    _Ellipse.Height = 5;
                    _Ellipse.Fill = new SolidColorBrush(Colors.Black);
                    Canvas.SetLeft(_Ellipse, _CollisionPos.X - (_Ellipse.Width / 2) + _VectorHoleRotation.X);
                    Canvas.SetTop(_Ellipse, _CollisionPos.Y - (_Ellipse.Height / 2) + _VectorHoleRotation.Y);
                    _Map._Canvas1.Children.Add(_Ellipse);
                    _VectorHoleRotation = Vector2.Multiply(_VectorHoleRotation, -1);
                }

                for (int i = _Players.Count - 1; i >= 0; i--)
                {
                    Player _EnemyPlayer = _Players[i];
                    if (_ShootingPlayer._Team != _EnemyPlayer._Team && !_friendlyfire)
                        if (_ShootingPlayer != _EnemyPlayer)
                        {
                            Vector2 _CollisionPoint;
                            float dist = Calculator.DistanceBetweenPointAndLineSegment(_EnemyPlayer._Position, _CollisionPos, _PlayerPos, out _CollisionPoint);
                            if (dist < 40)
                            {
                                Explosion _BloodExplosion = new Explosion();
                                _BloodExplosion._Position = _CollisionPoint;
                                _BloodExplosion._AnimatedBitmap = Menu._Database._Blood.Random();
                                _BloodExplosion._Game = this;
                                _BloodExplosion._Angle = _ShootingPlayer._Angle + 180;
                                _BloodExplosion.PlaySound(Helper.Random("damage1.mp3", "damage2.mp3", "damage3.mp3"));
                                _BloodExplosion.Load();
                                if (_EnemyPlayer is LocalPlayer) /////////////shootLocalplayer
                                {
                                    LocalPlayer _LocalPlayer = (LocalPlayer)_EnemyPlayer;
                                    int damage;
                                    if (dist < 8 && firstshoot)
                                    {
                                        damage = 110;
                                        _LocalPlayer.PlaySound("headshot1.mp3");
                                    } else damage = 10;                                    
                                    float life = ((LocalPlayer)_LocalPlayer)._Life -= damage;

                                    if (life < 0) LocalPlayerDead(_ShootingPlayer);

                                    _Menu._DamageRotation.Angle = Calculator.VectorToRadians(_ShootingPlayer._Position - _LocalPlayer._Position) * Calculator.DegreesToRadiansRatio;
                                    ((Storyboard)_Menu.Resources["DamageStoryboard"]).Begin();

                                    _LocalPlayer._IsSlowDown = true;
                                }
                            }
                        }
                }
            }
        }


        internal void Dispose()
        {
            this._Canvas.Children.Clear();
        }

        public void MouseMove()
        {
            if (_Cursor != null)
            {
                _Cursor._x = Menu._Mouse.X;
                _Cursor._y = Menu._Mouse.Y;
            }
        }
    }    
}

