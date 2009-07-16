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
using LevelEditor4;

[assembly: AssemblyVersionAttribute("4.0.*")]
namespace CounterStrikeLive
{

    public partial class Menu : UserControl
    {
        public static bool singleplayer;


        public static TimerA _TimerA = new TimerA();
        public static bool isnotBlend = Environment.Version.Major == 3;
        public MyObs<SharedClient> _Clients { get; set; }

        public static Menu _This;

        public static bool _SinglePlayer { get { return _This.Get<bool>("_SinglePlayer"); } set { _This.Set("_SinglePlayer", value); OnSinglePlayerSet(); } }

        private static void OnSinglePlayerSet()
        {
            //if (_SinglePlayer) throw new Exception();
            if(_SinglePlayer) Listener._Dissabled = true;
        }
        public static bool _Multiplayer { get { return !_SinglePlayer; } set { _SinglePlayer = !value; } }

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
                _Config = (Config)Config._XmlSerializer.Deserialize(s);
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
                _EnterNick.Success += delegate
                {
                    LoadDb();
                };
            } else LoadDb();
        }




        public void WriteCenterText(string text)
        {
            _CenterText.Text += text + "\n";
        }
        public void LoadDb()
        {
            new GameTypeSelect().Success += delegate
            {
                _tbotcount = Math.Min(GameTypeSelect._This._tBotsCount, 10);
                _ctbotcount = Math.Min(GameTypeSelect._This._ctBotsCount, 10);
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
                if (!_SinglePlayer)
                {
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
                } else
                    OnConnected();
            };
        }
        public int _tbotcount = 4;
        public int _ctbotcount = 4;
        public void OnConnected()
        {
            if (_Multiplayer)
            {
                if (_Socket.Connected == false) throw new Exception("Cannot Connect!");
                _Sender._Socket = _Listener._Socket = _Socket;
                _Sender._NetworkStream = _Listener._NetworkStream = _Config.GenerateClientLag ? new LagStream(_Socket) { interval = 2100 } : new NetworkStream(_Socket);
                _Listener.StartAsync("listener");
            } else
            {
                
                CreateLocalCLient(0);
                if (_Config._AutoSelect)
                {
                    _ctbotcount = 2;
                    _tbotcount = 4;
                }
                int i;
                for (i = 1; i < 1 + _ctbotcount; ++i)
                {
                    CreateBot(i, Player.Team.terr);
                }
                for (; i < 1 + _ctbotcount + _tbotcount; ++i)
                {
                    CreateBot(i, Player.Team.cterr);
                }

                MapSelect _MapSelect = new MapSelect();
                _MapSelect.Success += delegate
                {
                    LoadResources(_MapSelect.MapName);
                };
            }

            Trace.WriteLine("Connected");
            Loading1.Text = "Connected";
            Loading1.Value = 30;
            this.Cursor = Cursors.None;



            _Storyboard.Begin();
            _Storyboard.Completed += new EventHandler(Update);

            KeyDown += new KeyEventHandler(Page_KeyDown);
            KeyUp += new KeyEventHandler(PageKeyUp);

            MouseMove += new MouseEventHandler(Menu_MouseMove);



            MouseLeftButtonDown += new MouseButtonEventHandler(Menu_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(Menu_MouseLeftButtonUp);

        }
        
        public List<SharedClient> _Bots = new List<SharedClient>();
        public void CreateBot(int id,Player.Team team)
        {

            SharedClient _BotClient = new SharedClient();
            _BotClient._id = id;
            _BotClient._Menu = this;
            _BotClient._Bot = true;
            _BotClient.Add();
            _BotClient._Nick = "bot" + _BotClient._id;
            _BotClient._PlayerType = Database.PlayerType.TPlayer;
            _BotClient._Team = team;
            _Bots.Add(_BotClient);
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
                                CreateLocalCLient(id);
                                Trace.WriteLine("ID Received:" + id);                                ;
                            }
                            break;
                        case PacketType.SelectMap:
                            {
                                if (_Config._AutoSelect)
                                    _Sender.Send(PacketType.MapSelected, "estate.zip".ToBytes());
                                else
                                {
                                    SelectMap();
                                }
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
                                //LoadGame(App.GetResourceStream(new Uri(_Stream, UriKind.Relative)).Stream);
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
                } else
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
                                string s = _MemoryStream.ReadStringToEnd();
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
                                _Game.Shoot(_RemotePlayer._x, _RemotePlayer._y, _Angle, _RemotePlayer, _PacketType == PacketType.firstshoot);
                            }
                            break;
                        case PacketType.PlayerJoined:
                            {
                                Trace.WriteLine("Player Joiend " + SenderID);
                                SharedClient _RemoteClient = new SharedClient();
                                _RemoteClient._Menu = this;
                                _RemoteClient._id = SenderID;
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
                            Trace.WriteLine("Player Leaved " + SenderID);
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
                                _Chat.Text += _MemoryStream.ReadStringToEnd() + "\r\n";
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

        private void CreateLocalCLient(int id)
        {
            _LocalClient = new SharedClient();
            _LocalClient._LocalDatabase = _LocalDatabase;
            _LocalClient._id = id;
            _LocalClient._Menu = this;
            _LocalClient._Local = true;
            _LocalClient.Add();
            if (_Multiplayer)
                _LocalClient.SendAll(null);
        }

        private void SelectMap()
        {
            MapSelect _MapSelect = new MapSelect();

            _MapSelect.Success += delegate
            {
                _MapSelect.Trace("Closed");
                _Sender.Send(PacketType.MapSelected, _MapSelect.MapName.ToBytes());
            };
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

            if (_Multiplayer)
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
        const int _RotationSendInterval = 100;
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
                if (ChildWindow._This == null)
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
        void ClearTextBlock(ref double _Elapsed, TextBlock _TextBlock, int delay)
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

            ClearTextBlock(ref _CenterTextTimeElapsed, _CenterText, 2000);

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

        protected void LoadGame(Stream _Stream)
        {
            Loading1.Text = "Loading Game";
            if (_Game != null) _Game.Dispose();
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(MapDatabase));
            MapDatabase _MapDatabase = (MapDatabase)_XmlSerializer.Deserialize(_Stream);
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




}

