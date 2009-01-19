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
using System.Diagnostics;
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
[assembly: AssemblyVersionAttribute("3.0.*")]
namespace CounterStrikeLive
{
    /// <summary>
    /// extended Trace
    /// </summary>
    public static class Trace
    {
        public static int id = 99;
        public static void WriteLine(object obj)
        {

            WriteLine(obj.ToString());
        }
        public static void WriteLine(string obj)
        {
            //if (!Debugger.IsAttached)            
            Menu._Menu.Dispatcher.BeginInvoke(new Action<object>(Menu._Menu.WriteLine), (object)("Client" + id + ": " + obj));
            Debug.WriteLine("Client" + id + ": " + obj);
        }
    }
    /// <summary>
    /// Protocol Headers    
    /// </summary>
    public enum PacketType : byte
    {             
        /// <summary>
        /// client->clients Chat message [[byte - length][text]]
        /// </summary>
        chat = 222,
        /// <summary>
        /// client->clients To Call method CheckWins() []
        /// </summary>
        checkWins = 154,
        /// <summary>
        /// client->clients SharedObject Property  [[Property id][value]]
        /// </summary>
        sharedObject = 232,
        /// <summary>
        /// client->clients Key Pressed [[key code]]
        /// </summary>
        keyDown = 133,
        /// <summary>
        /// client->clients Key Released [[key code]]
        /// </summary>
        keyUp = 134,        
        pong = 98,
        ping = 99,
        /// <summary>
        /// Server->client player Ping [[ping - int16]]
        /// </summary>
		pinginfo= 100,        
        /// <summary>
        /// client->client add point to player
        /// </summary>
        addPoint = 66,
        /// <summary>
        /// client->client extra damage shoot (head shoot) [byte - player angle]
        /// </summary>
        firstshoot = 45,
        /// <summary>
        /// client->client shoot [byte - player angle]
        /// </summary>
        shoot = 46,
        /// <summary>
        /// static server id
        /// </summary>
        serverid = 254,
        /// <summary>
        /// server->client sets player id [byte]
        /// </summary>
        playerid = 89,
        /// <summary>
        /// client->client player rotation 
        /// </summary>
        rotation = 56,
        /// <summary>
        /// server->clients player disconnected
        /// </summary>
        PlayerLeaved = 23,
        /// <summary>
        /// server->clients player connected
        /// </summary>
        PlayerJoined = 24,
        /// <summary>
        /// server->client map [byte len][string]
        /// </summary>
        map = 21,
        /// <summary>
        /// client->client [byte client id][data]
        /// </summary>
        sendTo = 27,
    }
    /// <summary>
    /// static random class
    /// </summary>
    public static class Random
    {
        static System.Random _Random = new System.Random();
        public static int Next(int min, int max)
        {            
            return _Random.Next(min, max);
        }
        public static float Next(float min, float max)
        {
            return min + ((float)_Random.NextDouble()) * (max - min);
        }
        public static int Next(int max)
        {
            return _Random.Next(max);
        }
    }        
    public partial class EnterNick : UserControl
    {
        public Menu _Menu;
        public EnterNick()
        {
            InitializeComponent();
            _ok.Click += new RoutedEventHandler(OkClick);
            _TextPannel.KeyDown += new KeyEventHandler(_TextPannel_KeyDown);
        }

        void OkClick(object sender, RoutedEventArgs e)
        {
            if (_TextPannel.Text.Length > 3)
            {
                _Menu._LocalDatabase._Nick = _TextPannel.Text;
                _TextPannel.Text = "";
                this.Hide();
                if (_LoadDb) _Menu.LoadDb();
            }
        }

        void _TextPannel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OkClick(null, null);
        }
        public bool _LoadDb = true;
        public void Show()
        {
            Extensions.Show(this);
            _TextPannel.Focus();
        }
    }   
    public class LocalDatabase
    {
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
    public class ChatTextBox : UserControl
    {
        public TextBlock _ChatTextBox = new TextBlock { Foreground = new SolidColorBrush(Colors.Yellow) };
        public ChatTextBox()
        {
            this.Content = _ChatTextBox;
        }
        public void Load()
        {
            this.Hide();
        }
        public Menu _Menu;
        Sender _Sender { get { return _Menu._Sender; } }
        new public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _ChatTextBox.Text.Length > 0)
            {
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write((byte)PacketType.chat);
                    _BinaryWriter.Write(this._ChatTextBox.Text);
                    _Sender.Send(_MemoryStream.ToArray());
                }
                _ChatTextBlock.Text += "\n" + _Menu._LocalClient._Nick + ": " + _ChatTextBox.Text;
                _ChatTextBox.Text = "";
                this.Hide();
            }
            else
                if (e.Key == Key.Back) _ChatTextBox.Text = _ChatTextBox.Text.Remove(_ChatTextBox.Text.Length - 1, 1);
                else
                    _ChatTextBox.Text += (char)(e.PlatformKeyCode + 32);
        }
        public TextBlock _ChatTextBlock { get { return _Menu._ChatTextBlock; } }

    }
    public partial class Menu : UserControl
    {                
        public MyObs<SharedClient> _Clients { get; set; }

        public static Menu _Menu;
        public Menu()
        {            
            _Menu = this;
            InitializeComponent();
            _Clients = new MyObs<SharedClient>(50);
            _ScoreBoard.LayoutRoot.DataContext = this;            
            Loaded += new RoutedEventHandler(PageLoaded);
        }

        public void WriteLine(object str)
        {            
            _Console.Text = str + "\n" + _Menu._Console.Text;
        }
        public double _Width { get { return _RootLayout.Width; } }
        public double _Height { get { return _RootLayout.Height; } }


        IsolatedStorageFile _IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForSite();

        string _Datafile = "data.xml";
        XmlSerializer _XmlSerializer2 = new XmlSerializer(typeof(LocalDatabase));

        public LocalDatabase _LocalDatabase = new LocalDatabase();
        public string host;
        void PageLoaded(object sender, RoutedEventArgs e)
        {            
            version.Text = Assembly.GetExecutingAssembly().FullName;
            KeyDown += new KeyEventHandler(Menu_KeyDown);

            ((Storyboard)Resources["DamageStoryboard"]).Begin();
            Application.Current.Host.Content.Resized += new EventHandler(Content_Resized);
            Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
            if (Application.Current.Host.Source.DnsSafeHost.Length == 0) throw new Exception("Break");
            _ChatTextBox._Menu = this;
            _ChatTextBox.Load();
            _ScoreBoard.Hide();
            _TeamSelect.Hide();
            _EnterNick.Hide();
            _WelcomeScreen._Menu = this;
            _WelcomeScreen.Hide();

            _EnterNick._Menu = this;
            _Console.Hide();            

            Match _Match = Regex.Match(Application.Current.Host.Source.Query, @"&ip=([\d\.]+)");
            if (_Match.Success)
            {
                host = _Match.Groups[1].Value;
                EnterNick();
            }
            else
            {
                _WelcomeScreen.Load();
            }            
        }
        public void EnterNick()
        {
            if (_IsolatedStorageFile.FileExists(_Datafile))
                using (FileStream _FileStream = _IsolatedStorageFile.OpenFile(_Datafile, FileMode.Open, FileAccess.Read))
                {
                    _LocalDatabase = (LocalDatabase)_XmlSerializer2.Deserialize(_FileStream);
                }
            if (_LocalDatabase._Nick == null)
            {
                _EnterNick.Show();
            }
            else LoadDb();
        }
        void Menu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Home || e.Key == Key.Unknown)
                _Console.Toggle();
        }

        

        public void WriteCenterText(string text)
        {
            _CenterText.Text += text + "\n";
        }
        public void LoadDb()
        {			
            WriteCenterText("Loading");            
            Application.Current.Exit += new EventHandler(Current_Exit);
            _Console.Show();
            _TeamSelect._SpectatorButton.Click += new RoutedEventHandler(SpectatorButton_Click);
            _TeamSelect._CTerroritsButton.Click += new RoutedEventHandler(CTerroritsButton_Click);
            _TeamSelect._TerroristsButton.Click += new RoutedEventHandler(TerroristsButton_Click);
            _TeamSelect._AutoSelectButton.Click += new RoutedEventHandler(AutoSelectButton_Click);
            Content_Resized(null, null);

            Stream _Stream = Application.GetResourceStream(new Uri("db.xml", UriKind.Relative)).Stream;
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database));
            _Database = (Database)_XmlSerializer.Deserialize(_Stream);


            foreach (Database.ILoad _ILoad in Database._ILoads)
            {
                _ILoad.Load();
            }
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs _SocketAsyncEventArgs = new SocketAsyncEventArgs();
            _SocketAsyncEventArgs.RemoteEndPoint = new DnsEndPoint(host, 4530); //port
            _SocketAsyncEventArgs.UserToken = _Socket;
            _Socket.ConnectAsync(_SocketAsyncEventArgs);
            Trace.WriteLine("Connecting");
            _SocketAsyncEventArgs.Completed += delegate(object o, SocketAsyncEventArgs e)
            {                                                
                Dispatcher.BeginInvoke(new Action(OnConnected));
            };

        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            ScriptObject _ScriptObject = (ScriptObject)HtmlPage.Window.GetProperty("screen");
            _RootLayout.Width = (double)_ScriptObject.GetProperty("width");
            _RootLayout.Height = (double)_ScriptObject.GetProperty("height");
        }

        void Content_Resized(object sender, EventArgs e)
        {
            _RootLayout.Width = Application.Current.Host.Content.ActualWidth;
            _RootLayout.Height = Application.Current.Host.Content.ActualHeight;
        }
        void AutoSelectButton_Click(object sender, RoutedEventArgs e)
        {
            int cterr = (from pl in _Clients where pl != null && pl._Team == Player.Team.cterr select pl).Count();
            int terr = (from pl in _Clients where pl != null && pl._Team == Player.Team.terr select pl).Count();
            if (cterr > terr)
                _Game.TerroristsButtonClick();
            else
                _Game.CTerroritsButtonClick();
        }
        void TerroristsButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.TerroristsButtonClick();
        }
        void CTerroritsButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.CTerroritsButtonClick();
        }
        void SpectatorButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.SpectatorButtonClick();
        }

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
        const int _RotationSendInterval = 100;
        protected void SendRotation()
        {
            if (STimer.TimeElapsed(_RotationSendInterval))
            {
                _SendSecondsElapsed = _SendSecondsElapsed % _RotationSendInterval;
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write((byte)PacketType.rotation);
                    _BinaryWriter.Write(Sender.Encode(_Game._LocalPlayer._Angle, 0, 360));
                    _Sender.Send(_MemoryStream.ToArray());
                }
            }
        }

        public void OnConnected()
        {
            Trace.WriteLine("Connected");
            if (_Socket.Connected == false) throw new Exception("Cannot Connect!");
            this.Cursor = Cursors.None;
            _Sender._Socket = _Listener._Socket = _Socket;            
            _Listener.Start();
            _Storyboard.Begin();
            _Storyboard.Completed += new EventHandler(Update);

            KeyDown += new KeyEventHandler(Page_KeyDown);
            KeyUp += new KeyEventHandler(PageKeyUp);
            MouseMove += new MouseEventHandler(Menu_MouseMove);

            //DispatcherTimer _DispatcherTimer = new DispatcherTimer();
            //_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
            //_DispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick); //send keys test
            //_DispatcherTimer.Start();

            MouseLeftButtonDown += new MouseButtonEventHandler(Menu_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(Menu_MouseLeftButtonUp);

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
            
            if (_ChatTextBox.IsEnabled)
            {
                _ChatTextBox.KeyDown(e);
                
            }
            else
            {                
                if (Key.Tab == e.Key || Key.Back == e.Key)
                    _ScoreBoard.Toggle();

                if (Key.T == e.Key || Key.Y == e.Key)
                    _ChatTextBox.Toggle();                

                if (e.Key == Key.Escape)
                    System.Windows.Application.Current.Host.Content.IsFullScreen = true;

                if (_Game != null) _Game.OnKeyDown(e.Key);
                if (!_Keyboard.Contains(e.Key)) _Keyboard.Add(e.Key);
            }
        }


        double _CenterTextTimeElapsed;
        double _KillTextElapsed;
        double _ChatTextElapsed;
        void ClearTextBlock(ref double _Elapsed, TextBlock _TextBlock)
        {
            if (_Elapsed > 2000)
            {
                _Elapsed = 0;
                int i = _TextBlock.Text.IndexOf('\n') + 1;
                if (i > 0) _TextBlock.Text = _TextBlock.Text.Remove(0, i);
                else _TextBlock.Text = "";
            }
            if (_TextBlock.Text.Length > 0)
                _Elapsed += STimer._TimeElapsed;

        }

        void Update(object sender, EventArgs e)
        {

            ClearTextBlock(ref _ChatTextElapsed, _ChatTextBlock);
            ClearTextBlock(ref _KillTextElapsed, _KillText);

            ClearTextBlock(ref _CenterTextTimeElapsed, _CenterText);

            _Storyboard.Begin();            

            List<Byte[]> _Messages = _Listener.GetMessages();
            foreach (byte[] _buffer in _Messages)
            {
                onReceive(_buffer);
            }

            if (_Listener._Connected == false)
            {
                throw new Exception("You Have Been Disconnected!");
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

            if (STimer.TimeElapsed(200))
            {
                double? fps = STimer.GetFps();
                if (fps != null)
                {
                    _FPS.Text = "FPS: " + ((int)fps).ToString() + "'";
                }
            }

            STimer.Update();
        }                

        public enum GameState
        {
            mapdownload, teamSelect, spectator, alive
        }
        public GameState _GameState = GameState.mapdownload;
        protected void onReceive(byte[] _data)
        {
            using (var _MemoryStream = new MemoryStream(_data))
            {
                var _BinaryReader = new BinaryReader(_MemoryStream);
                int _SenderId = _BinaryReader.ReadByte();
                var _PacketType = (PacketType)_BinaryReader.ReadByte();
                if (_SenderId == (int)PacketType.serverid)
                {
                    switch (_PacketType)
                    {
                        case PacketType.playerid:
                            {
                                int id = _BinaryReader.ReadByte();
                                Trace.id = id;
                                _LocalClient = new SharedClient();
                                _LocalClient._LocalDatabase = _LocalDatabase;
                                _LocalClient._id = id;
                                _LocalClient._Menu = this;
                                _LocalClient._Local = true;
                                _LocalClient.Add();
                                _LocalClient.SendAll(null);
                                Trace.WriteLine("ID Received:" + id);
                                break;
                            }
                        case PacketType.map:
                            {
                                _GameState = GameState.mapdownload;
                                string _Map = _BinaryReader.ReadString();
                                Trace.WriteLine("Map name Received:" + _Map);
                                LoadGame(Application.GetResourceStream(new Uri(_Map, UriKind.Relative)).Stream);
                            }
                            break;
                        case PacketType.ping:
                            {                                
                                _Sender.Send(new byte[]{ (byte)PacketType.pong});
                            }
							break;
                        default: throw new Exception("Break"); 
                    }
                }
                else
                {
					if (_LocalClient._id == null) throw new Exception();
					if (_SenderId == _LocalClient._id && _PacketType != PacketType.pinginfo) throw new Exception();
                    SharedClient _SharedClient = _Clients[_SenderId];
                    if (_PacketType != PacketType.PlayerJoined && _Clients[_SenderId] == null) throw new Exception("Break");
                    switch (_PacketType)
                    {
                        case PacketType.rotation:
                            {
                                byte angle = _BinaryReader.ReadByte();
                                if (_Clients[_SenderId] != null)
                                {
                                    SharedClient _RemoteClient = _Clients[_SenderId];
                                    if (_RemoteClient._PlayerState == SharedClient.PlayerState.alive)
                                    {
                                        Player _RemotePlayer = _RemoteClient._Player;
                                        _RemotePlayer._Angle = Listener.Decode(angle, 0, 360);
                                    }
                                }
                            }
                            break;
                        case PacketType.addPoint:
                            {
                                SharedClient _KillerClient = _Clients[_BinaryReader.Read()];
                                _KillerClient._Points++;
                                ShowKilledMessage(_KillerClient, _SharedClient);
                            }
                            break;
                        case PacketType.shoot: goto case PacketType.firstshoot;
                        case PacketType.firstshoot:
                            {

                                Player _RemotePlayer = _SharedClient._Player;
                                float _Angle = Listener.DecodeInt(_BinaryReader.ReadUInt16(), 0, 360);
                                _Game.Shoot(_RemotePlayer._x, _RemotePlayer._y, _Angle, _RemotePlayer, _PacketType == PacketType.firstshoot);
                            }
                            break;
                        case PacketType.PlayerJoined:
                            {
                                Trace.WriteLine("Player Joiend " + _SenderId);
                                SharedClient _RemoteClient = new SharedClient();
                                _RemoteClient._Menu = this;
                                _RemoteClient._id = _SenderId;
                                _RemoteClient.Add();
                                _LocalClient.SendAll(_SenderId);
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
                            Trace.WriteLine("Player Leaved " + _SenderId);
                            if (_Game != null) _Game.CheckWins();
                            break;
                        case PacketType.keyDown:
                            {
                                Key k =(Key)_BinaryReader.ReadByte();
                                if (_SharedClient._Player!=null)
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
                                _ChatTextBlock.Text += "\n" + _SharedClient._Nick + ": " + _BinaryReader.ReadString();
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
                                _Clients[_SenderId].onReceive(_BinaryReader);
                            }
                            break;
						default: throw new Exception();
                    }
                    if (_BinaryReader.BaseStream.Position != _BinaryReader.BaseStream.Length)
                        throw new Exception("Break");
                }
            }
        }

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
            if (_Game != null) _Game.Dispose();
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(MapDatabase));
            MapDatabase _MapDatabase = (MapDatabase)_XmlSerializer.Deserialize(_Map);
            _Game = new Game();
            _Game._Menu = this;
            _Game._Sender = _Sender;
            _Game.Load(_MapDatabase);
            _GameCanvas.Children.Add(_Game._Canvas);
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
    public class MapDatabase
    {
        public float _Scale;
        public Point _TStartPos;
        public Point _CStartPos;
        public List<Point> _StartPositions = new List<Point>();
        public class Image
        {
            public double Width;
            public double Height;
            public double X;
            public double Y;
            public string Path;
        }
        public List<Layer> _Layers = new List<Layer>();
        public class Layer
        {
            public List<Image> _Images = new List<Image>();
            public List<Polygon> _Polygons = new List<Polygon>();
        }
        public class Polygon
        {
            public Color _Color;
            public List<Point> _Points = new List<Point>();
        }
    }
    public class Map
    {
        public class LV
        {
            public Vector2 _Vector2;
            public Line2 _Line2;
        }
        public Point GetPos(Player.Team _Team)
        {
            if (_Team == Player.Team.cterr)
            {
                return _MapDatabase._CStartPos;
            }
            return _MapDatabase._TStartPos;
        }

        public Line2 CCollision(Vector2 pos1, float r)
        {
            foreach (MapDatabase.Polygon _Polygon in _Polygons)
            {
                Vector2? _oldPoint = null;
                foreach (Point _p in _Polygon._Points)
                {
                    Vector2 _point = new Vector2((float)_p.X, (float)_p.Y);
                    if (_oldPoint != null)
                    {
                        Vector2 Temp;
                        float dist = Calculator.DistanceBetweenPointAndLineSegment(pos1, _point, _oldPoint.Value, out Temp);
                        if (dist < r)
                        {
                            return new Line2 { _p1 = _oldPoint.Value, _p2 = _point, _cpoint = Temp };
                        }
                    }
                    _oldPoint = _point;
                }
            }
            return null;
        }

        public List<LV> Collision(Vector2 pos2, Vector2 pos1, out Line2 _wall)
        {
            _wall = null;
            List<LV> _HitPoints = new List<LV>();
            foreach (MapDatabase.Polygon _Polygon in _Polygons)
            {
                Vector2? _oldPoint = null;
                foreach (Point _p in _Polygon._Points)
                {
                    Vector2 _point = new Vector2((float)_p.X, (float)_p.Y);
                    if (_oldPoint != null)
                    {
                        Vector2? hitPoint = Physics.LineCollision(pos1, pos2, _point, _oldPoint.Value, true);

                        if (hitPoint != null)
                        {
                            Line2 _Line2 = new Line2 { _p1 = _point, _p2 = _oldPoint.Value };
                            _HitPoints.Add(new LV { _Vector2 = hitPoint.Value, _Line2 = _Line2 });
                        }
                    }
                    _oldPoint = _point;
                }
            }
            _HitPoints.Sort(new Comparison<LV>(delegate(LV a, LV b)
            {
                float adist = Vector2.Distance(pos1, a._Vector2);
                float bdist = Vector2.Distance(pos1, b._Vector2);
                if (adist > bdist) return 1;
                else if (adist < bdist) return -1;
                else return 0;
            }));
            return _HitPoints;
        }

        MapDatabase _MapDatabase;

        public Point GetStartPosition()
        {
            if (_StartPoints.Count == 0) throw new Exception("Break");
            Point _Point = _StartPoints[Random.Next(_StartPoints.Count)];
            return _Point;
        }
        public void LoadMap(MapDatabase _MapDatabase)
        {
            this._MapDatabase = _MapDatabase;
            foreach (MapDatabase.Layer _Layer in _MapDatabase._Layers)
            {
                foreach (MapDatabase.Image _dImage in _Layer._Images)
                {
                    Image _Image = new Image();
                    _Image.Source = new BitmapImage(new Uri(_dImage.Path, UriKind.Relative));
                    _Image.Width = _dImage.Width;
                    _Image.Height = _dImage.Height;
                    _Canvas.Children.Add(_Image);
                    Canvas.SetLeft(_Image, _dImage.X);
                    Canvas.SetTop(_Image, _dImage.Y);
                }
                foreach (MapDatabase.Polygon _dPolygon in _Layer._Polygons)
                {
                    if (_dPolygon._Color != Colors.Black)
                    {
                        if (_dPolygon._Points.First() == _dPolygon._Points.Last())
                        {
                            Polygon _Polygon = new Polygon();
                            foreach (Point _Point in _dPolygon._Points)
                            {
                                _Polygon.Points.Add(_Point);
                            }

                            _Polygon.Fill = new SolidColorBrush(_dPolygon._Color);

                            _Canvas.Children.Add(_Polygon);
                        }
                        else
                        {
                            Polyline _Polygon = new Polyline();
                            _Polygon.Stroke = new SolidColorBrush(_dPolygon._Color);
                            _Polygon.StrokeThickness = 3;
                            foreach (Point _Point in _dPolygon._Points)
                            {
                                _Polygon.Points.Add(_Point);
                            }

                            _Canvas.Children.Add(_Polygon);
                        }
                    }
                    else _Polygons.Add(_dPolygon);
                }
            }
            _Canvas.Children.Add(_Canvas1);
        }
        public Canvas _Canvas1 = new Canvas();
        public Canvas _Canvas = new Canvas();
        List<MapDatabase.Polygon> _Polygons = new List<MapDatabase.Polygon>();
        public List<Point> _StartPoints { get { return _MapDatabase._StartPositions; } set { _MapDatabase._StartPositions = value; } }

    }
    public partial class TeamSelect : UserControl
    {
        public TeamSelect()
        {
            InitializeComponent();
        }
    }
    public static class Arrays1
    {
        public static TSource Random<TSource>(this List<TSource> source)
        {
            return source[CounterStrikeLive.Random.Next(source.Count)];
        }        
    }
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
            _Scale -= (float)STimer._SecodsElapsed * 3;
            if (_Scale < 1) _Scale = 1;
            _ScaleTransform.ScaleX = _Scale;
            _ScaleTransform.ScaleY = _Scale;
            Canvas.SetLeft(_Canvas, _x);
            Canvas.SetTop(_Canvas, _y);
        }
        ScaleTransform _ScaleTransform = new ScaleTransform();
    }
    public class AnimDamage : GameObj
    {
        TextBlock _TextBlock = new TextBlock();
        public void Load(string text)
        {
            _Game._AnimDamages.Add(this);

            _TextBlock.Text = text;
            _TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);
            _TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _TextBlock.VerticalAlignment = VerticalAlignment.Center;
            _Canvas.Children.Add(_TextBlock);
            _Game._Canvas.Children.Add(_Canvas);

            Canvas.SetTop(_Canvas, _Position.Y);
            Canvas.SetLeft(_Canvas, _Position.X);
        }

        public void Update()
        {
            position.Y -= 3;
            Canvas.SetTop(_Canvas, _Position.Y);
            Canvas.SetLeft(_Canvas, _Position.X);
            _TextBlock.Opacity -= .03;
            if (_TextBlock.Opacity < 0)
            {
                _Game._Canvas.Children.Remove(_TextBlock);
                _Game._AnimDamages.Remove(this);
            }
        }        
    }
    public class Game
    {
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
                _TeamSelect.Toggle();

            if ((_Key == Key.PageUp || _Key == Key.R) && _TotalPatrons != 0 && _LocalPlayer != null)
            {
                _LocalPlayer._isReloading = true;
                _TotalPatrons -= 30;
                WriteCenterText("Reloading");

                STimer.AddMethod(1500, delegate { _Patrons = 30; if (_LocalPlayer != null) _LocalPlayer._isReloading = false; });
            }

            if ((_Key == Key.PageDown || _Key == Key.Space) && _GameState != Menu.GameState.alive)
            {
                _SpeciateId++;
                if (_SpeciateId < _Players.Count) WriteCenterText("Player: " + _Players[_SpeciateId]._Client._Nick);
            }
            if (Menu.GameState.alive == _Menu._GameState)
            {
                _Sender.Send(new byte[] { (byte)PacketType.keyDown, (byte)_Key });
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
                }
                _LocalPlayer.OnKeyUp(_Key);
            }
        }
        public List<Player> _Players = new List<Player>();
        public Map _Map;
        public Menu _Menu;
        public Canvas _Canvas = new Canvas();
        TranslateTransform _TranslateTransform = new TranslateTransform();
        TeamSelect _TeamSelect { get { return _Menu._TeamSelect; } }
        public Point _FreeViewPos;

        public void Load(MapDatabase _MapDatabase)
        {
            _Scale = .5;
            _FreeViewPos = _MapDatabase._CStartPos;
            _Menu._Console.Hide(); ;

            _TeamSelect.Show();

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
            _TeamSelect.Hide();
            SendCheckWins();
        }

        public void TerroristsButtonClick()
        {
            OnTeamSelect();
            _LocalClient._PlayerType = Database.PlayerType.TPlayer;
            _LocalClient._Team = Player.Team.terr;
            Debug.WriteLine("Terrorsits team selected");
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
            CheckWins();
        }
        public LocalPlayer _LocalPlayer { get { return (LocalPlayer)_LocalClient._Player; } }
        public SharedClient _LocalClient { get { return _Menu._LocalClient; } }

        public MyObs<SharedClient> _Clients { get { return _Menu._Clients; } }
        protected void CreateLocalPlayerReset()
        {
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
            }
            //if (_Menu._GameState == Menu.GameState.spectator) throw new Exception("Break");
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
                STimer.AddMethod(interval, CreateLocalPlayerReset);
                
            }
            else if ((from pl in _Players where pl._Client._Team == Player.Team.terr select pl).Count() == 0)
            {
                Trace.WriteLine("Counter Terrorists Win");
                _CenterText.Text += "Counter Terrorists Win\n";
                STimer.AddMethod(interval, CreateLocalPlayerReset);
            }
        }


        TextBlock _CenterText { get { return _Menu._CenterText; } set { _Menu._CenterText = value; } }


        public void Die(Player _Player)
        {
            Explosion _Explosion = new Explosion();
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
                    _ShootTimeElapsed += STimer._TimeElapsed;
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
                    _TranslateTransform.X = -_FreeViewPos.X*_Scale + _Menu._Width / 2;
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
                                _BloodExplosion.Load();
                                if (_EnemyPlayer is LocalPlayer) /////////////shootLocalplayer
                                {
                                    LocalPlayer _LocalPlayer = (LocalPlayer)_EnemyPlayer;
                                    int damage;
                                    if (dist < 8 && firstshoot)
                                        damage = 110;
                                    else damage = 10;                                    
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
    public class Player : Explosion
    {
		TextBlock _TextBlock = new TextBlock();

        public Team _Team { get { return _Client._Team; } }
        public override void CheckVisibility(LocalPlayer _LocalPlayer)
        {
            if (_LocalPlayer != null && _LocalPlayer._Team != this._Team)
                base.CheckVisibility(_LocalPlayer);
            else this._Visibility = Visibility.Visible;
        }
        public bool _IsSlowDown { get { return _Client._IsSlowDown; } set { _Client._IsSlowDown = value; } }

        public string _Nick { get { return _Client._Nick; } }
        public int _ID { get { return _Client._id.Value; } }

        public SharedClient _Client;
        public enum Team { spectator = 2, terr = 3, cterr = 4 };
        public Database.PlayerType _PlayerType { get { return _dbPlayer._PlayerType; } }
        public Database.Player _dbPlayer;
        List<Point> _Points = new List<Point>();
        public float _V = 350f;
        
        public List<Key> _Keys = new List<Key>();
        public virtual void OnKeyDown(Key _Key)
        {
            if (!_Keys.Contains(_Key)) _Keys.Add(_Key);
        }
        public void OnKeyUp(Key _Key)
        {
            if (_Keys.Contains(_Key)) _Keys.Remove(_Key);

        }
        
        ScaleTransform _ScaleTransform = new ScaleTransform();
        public List<Explosion> _Explosions2 = new List<Explosion>();
        ExplosionA _ExplosionA;
        public override void Load()
        {
            LoadNickName();
			_Canvas.Children.Add(_TextBlock);
            _ExplosionA = new ExplosionA();
            _ExplosionA._PaternCanvas = _Canvas2;
            _ExplosionA._Explosions = _Explosions2;
            _ExplosionA._AnimatedBitmap = Menu._Database._Gun;
            _ExplosionA.Load();
            _AnimatedBitmap = _dbPlayer._PlayerStay;
            base.Load();

        }

        private void LoadNickName()
        {
            _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = 1 / _Game._Scale;
            _TextBlock.TextAlignment = TextAlignment.Center;
            _TextBlock.RenderTransform = _ScaleTransform;
            _TextBlock.Width = 500;
            _TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetTop(_TextBlock, -100);
            Canvas.SetLeft(_TextBlock, -500);
            _TextBlock.Text = _Nick;
        }

        public override void Add()
        {
            _Game._Players.Add(this);
            _Game._Canvas.Children.Add(_Canvas);
        }
        public override void Remove()
        {
            _Client._Player = null;
            _Game._Players.Remove(this);
            _Game._Canvas.Children.Remove(_Canvas);
        }


        protected void UpdateCollisions()
        {
            Line2 wall = _Map.CCollision(_Position, 25);
            if (wall != null)
            {
                _Position = _OldPosition;
                //float a= Calculator.DegreesToRadians(90);

                // Vector2 _Vectora = wall._cpoint - _Position;

                // Vector2 _Vectorb = new Vector2(0, (_Position - _OldPosition).Length());                

                // float rads = Calculator.VectorToRadians(_Vectora);
                // float rads2 = Calculator.VectorToRadians(wall._p2 - wall._p1)+1.57f;

                // Calculator.RotateVector(ref _Vectorb, rads2);
                // if (Math.Abs(rads-rads2) <1.57f)
                // {
                //     _Vectorb = Vector2.Multiply(_Vectorb, -1);
                // }

                // _Position -=_Vectorb;
            }
        }
        public Vector2? PlayerCollide()
        {
            foreach (Player _Player in _Game._Players)
            {
                if (_Player != this)
                {
                    float _distance = Calculator.DistanceBetweenPointAndPoint(this._Position, _Player._Position);
                    if (_distance < 100)
                    {
                        Vector2 _Vector2 = _Player._Position - _Position;
                        _Vector2.Normalize();
                        _Vector2 = Vector2.Multiply(_Vector2, -1);
                        return _Vector2;
                    }
                }
            }
            return null;
        }

        public override void Update()
        {
            _frame += 30*(float)STimer._SecodsElapsed;
            if (_frame >= _AnimatedBitmap._BitmapImages.Count) _frame = 0;
            
            BitmapImage _BitmapImage = _AnimatedBitmap._BitmapImages[(int)_frame];
            try
            {
                _Image.Source = _BitmapImage;
            }
            catch { }

            UpdateKeys();

            UpdateTranslations();

            Vector2? _PlayerCollide = PlayerCollide();
            if (_PlayerCollide != null)
            {
                _Position = _OldPosition;
                _Position += _PlayerCollide.Value;
            }
            if (_OldPosition != default(Vector2) && _OldPosition != _Position)
            {
                UpdateCollisions();
                _AnimatedBitmap = _dbPlayer._PlayerRun;
            }
            else _AnimatedBitmap = _dbPlayer._PlayerStay;
            foreach (Explosion _Explosion in _Explosions2)
            {
                _Explosion.Update();
            }
            _OldPosition = _Position;
        }

        public Vector2 _MoveVector;
        protected virtual void UpdateKeys()
        {
            _MoveVector = new Vector2();
            if (_Keys.Contains(Key.Left) || _Keys.Contains(Key.A))
            {

                _MoveVector.X -= 1;                
            }
            if (_Keys.Contains(Key.Right) || _Keys.Contains(Key.D))
            {
                _MoveVector.X += 1;
            }

            if (_Keys.Contains(Key.Down) || _Keys.Contains(Key.S))
            {
                _MoveVector.Y += 1;
            }
            if (_Keys.Contains(Key.Up) || _Keys.Contains(Key.W))
            {
                _MoveVector.Y -= 1;
            }
            if (_MoveVector != default(Vector2))
            {
                _MoveVector.Normalize();
                _MoveVector = Vector2.Multiply(_MoveVector, _V * (float)STimer._SecodsElapsed);
                if (_IsSlowDown) _MoveVector = Vector2.Multiply(_MoveVector, .5f);
                _Position += _MoveVector;
            }
        }

        public Database _Database { get { return Menu._Database; } }
        public LocalPlayer _LocalPlayer { get { return _Game._LocalPlayer; } }
        public void ShootAnimation()
        {
            _ExplosionA._isPlaying = true;
        }
    }
    public class GameObj
    {
        public bool _VisibleToAll;

        public Vector2 position;
        public Vector2 _Position { get { return position; } set { if (value.X == float.NaN) throw new Exception("Break"); position = value; } }
        public Map _Map { get { return _Game._Map; } }
        protected Canvas _Canvas = new Canvas();
        public Game _Game;
        public virtual void CheckVisibility(LocalPlayer _LocalPlayer)
        {
            if (_LocalPlayer != null && this != _LocalPlayer && !_VisibleToAll && !_Game._IsEverthingVisible)
            {
                float a1 = Player.CorrectAngle(Calculator.VectorToRadians(this._Position - _LocalPlayer._Position) * Calculator.DegreesToRadiansRatio);                
                float a2 = Player.CorrectAngle(a1 - _LocalPlayer._Angle+45);
                if (Math.Abs(a2) < 90)
                {
                    Line2 wall;
                    if (_Map.Collision(this._Position, _LocalPlayer._Position, out wall).Count != 0)
                    {
                        this._Visibility = Visibility.Collapsed;
                    }
                    else this._Visibility = Visibility.Visible;
                }
                else this._Visibility = Visibility.Collapsed;
            }
            else this._Visibility = Visibility.Visible;
        }
        public Visibility _Visibility
        { get { return _Canvas.Visibility; } set { _Canvas.Visibility = value; } }

    }
    public class ExplosionA : Explosion
    {
        public override List<Explosion> _Explosions { get; set; }
        public override Canvas _PaternCanvas { get; set; }        
        public ExplosionA():base()
        {
            _Visibility = Visibility.Visible;
            _VisibleToAll = true;
            _Remove = false;
            _isPlaying = true;
        }
    }
    public class Explosion : GameObj
    {
        protected Vector2 _OldPosition;
        RotateTransform _RotateTransform = new RotateTransform();
        protected void UpdateTranslations()
        {
            Canvas.SetLeft(_Canvas, _x);
            Canvas.SetTop(_Canvas, _y);
            _RotateTransform.Angle = _Angle;
        }
        private float angle;
        public float _Angle
        {
            get
            {
                if (angle == float.NaN) return 0;
                return angle;
            }
            set
            {
                if (value == float.NaN) return;
                angle = CorrectAngle(value);
            }
        }

        public static float CorrectAngle(float value)
        {
            float angle;
            angle = value % 360;
            if (angle < 0) angle = 360 + angle;
            return angle;
        }

        public float _width { get { return _AnimatedBitmap._Width; } set { _AnimatedBitmap._Width = value; } }
        public float _height { get { return _AnimatedBitmap._Height; } set { _AnimatedBitmap._Height = value; } }



        public float _x
        {
            get { return _Position.X; }
            set
            {
                if (value == float.NaN) throw new Exception("Break");
                _OldPosition.X = position.X = value;
            }
        }
        public float _y { get { return _Position.Y; } set { _OldPosition.Y = position.Y = value; } }
        public Database.AnimatedBitmap _AnimatedBitmap;
        public float _frame;
        public bool _Remove = true;

		public Canvas _Canvas2 = new Canvas();
        public virtual void Load()
        {
            if (!_VisibleToAll) this._Visibility = Visibility.Collapsed;
                                    
			_Canvas2.RenderTransform = _RotateTransform;

            _Image = _AnimatedBitmap.GetImage();
            _Canvas2.Children.Add(_Image);
			_Canvas.Children.Add(_Canvas2);

            UpdateTranslations();
            Add();
        }
        public virtual void Add()
        {
            _Explosions.Add(this);
            _PaternCanvas.Children.Add(_Canvas);
        }
        public virtual List<Explosion> _Explosions { get { return _Game._Explosions; } set { } }
        public virtual Canvas _PaternCanvas { get { return _Map._Canvas; } set { } }
        public virtual void Remove()
        {
            _Explosions.Remove(this);
			_PaternCanvas.Children.Remove(_Canvas);
        }
        
        public Image _Image;
        public bool _isPlaying = true;
        public virtual void Update()
        {
            if (_isPlaying)
            {
                _frame += (float)(30 * STimer._SecodsElapsed);                
                if (_frame < _AnimatedBitmap._BitmapImages.Count)
                {
                    _Image.Source = _AnimatedBitmap._BitmapImages[(int)_frame] ;
                }
                else
                {
                    if (_Remove == true) Remove();
                    _isPlaying = false;
                    _frame = 0;
                }                                
            }
        }
    }
    public class LocalPlayer : Player
    {
        public bool _isReloading;
        public override void Load()
        {
            base.Load();
            this._Visibility = Visibility.Visible;
        }
        protected override void UpdateKeys()
        {
            if (_MoveVector != default(Vector2))
            {
                if (_Game._Cursor._Scale < 2) _Game._Cursor._Scale = 2;
            }
            base.UpdateKeys();
        }
        public float _Life { get { return _Client._Life; } set { _Client._Life = value; } }
        public double _slowdowntimeelapsed;
        public override void Update()
        {
            if (_IsSlowDown)
            {
                _slowdowntimeelapsed += STimer._TimeElapsed;
                if (_slowdowntimeelapsed > 1000)
                {
                    _IsSlowDown = false;
                    _slowdowntimeelapsed = 0;
                }
            }
            base.Update();
        }
    }
    public partial class ScoreBoard
    {
        public ScoreBoard()
        {
            InitializeComponent();
        }
    }
    public partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {            
            InitializeComponent();
            _DataGrid.ItemsSource = _List;
            
        }

        public void Load()
        {                        
            this.Show();
            _Button.Click += new RoutedEventHandler(Button_Click);
            Download();            
            
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            Item _Item = (Item)_DataGrid.SelectedItem;
            if (_Item == null) return;
            this.Hide();            
            _Menu.host = _Item._Ip;
            _Menu.EnterNick();    
        }

        
        public Menu _Menu;                                        
        
        public class Item
        {
            public string _Name { get; set; }
            public string _Players { get; set; }
            public string _Port { get; set; }
            public string _Version { get; set; }
            public string _Ip { get; set; }
            public string _Map { get; set; }
        }
        public ObservableCollection<Item> list = new ObservableCollection<Item>();
        public ObservableCollection<Item> _List { get { return list; } set { } }
        
        void WebClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Trace.WriteLine("download Completed");
            if (!IsEnabled) return;
            if (e.Error != null) throw e.Error;
            MatchCollection _MatchCollection = Regex.Matches(e.Result, @">(?<Name>[\w\s]+)</a></td><td>(?<Map>[\w/.]+?)</td><td>(?<PlayerCount>\d+)</td><td>(?<Port>\d+)</td><td>(?<Version>[\d.]+)</td><td>(?<Ip>[\d.]+)</td></tr>", RegexOptions.IgnoreCase);
            Trace.WriteLine("Matches:" + _MatchCollection.Count);
            int old = _DataGrid.SelectedIndex;
            _List.Clear();
            _List.Add(new Item { _Ip = "localhost" });
            foreach (Match _Match in _MatchCollection)  
            {
                GroupCollection g = _Match.Groups;
                _List.Add(new Item { _Ip = g["Ip"].Value, _Name = g["Name"].Value, _Players = g["PlayerCount"].Value, _Port = g["Port"].Value ,_Map = g["Map"].Value, _Version =g["Version"].Value});
            }
            _DataGrid.SelectedIndex = old;
            new Thread(delegate()
            {
                Thread.Sleep(3000);
                Dispatcher.BeginInvoke(Download);                
            }).Start();
            
        }
        
        public void Download()
        {
            Trace.WriteLine("download Started");
            WebClient _WebClient = new WebClient();
            _WebClient.DownloadStringAsync(new Uri("http://igorlevochkin.ig.funpic.org/cs/serv.php?r="+Random.Next(99999), UriKind.Absolute));
            _WebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(WebClientDownloadStringCompleted);
        }
    }
}

