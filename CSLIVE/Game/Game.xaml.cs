#if(!SILVERLIGHT)
using System.Windows.Navigation;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using System.Windows.Shapes;
using doru;
using System.IO;
using System.ComponentModel;
using doru.Tcp;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using doru.Vectors;
using CSLIVE.Game.Controls;



namespace CSLIVE.Game
{

    public partial class Game : UserControl,IUpdate
    {
        #region vars
        public static Game _Game;
        public static Dictionary<string, Stream> _Resources = new Dictionary<string, Stream>();
        public static GameContentDataBase _GameContentDataBase;
        public BindableList<Player> _Players = new BindableList<Player>();
        public BindableList<IUpdate> _AnimatedSprites = new BindableList<IUpdate>();
        
        public Listener _Listener;
        public Sender _Sender;
        public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }
        public double _Width { get { Trace.Assert(this.ActualHeight != double.NaN); return this.ActualWidth * _Scale; } }
        public double _Height { get { return this.ActualHeight * _Scale; } }
        [Obsolete]
        public new double Width{get{return base.Width;}}
        [Obsolete]
        public new double Height { get { return base.Height; } }
        //public ScaleTransform _ScaleTransform = new ScaleTransform();//{ get { return Get<ScaleTransform>("_ScaleTransform"); } set { Set("_ScaleTransform", value); } }
        //public TranslateTransform _TranslateTransform = new TranslateTransform(); //{ get { return Get<TranslateTransform>("_TranslateTransform"); } set { Set("_TranslateTransform", value); } }        
        //public TransformGroup _TransformGroup { get { return this.Get<TransformGroup>("_TransformGroup"); } set { this.Set("_TransformGroup", value); } }
        public string _MapPath = "estate.zip";

        public Map _Map { get { return (Map)_MapContentControl.Content; } set { _MapContentControl.Content = value; } }

        public string _Ip;
        public int _Port;
        public List<Client> _Clients = new List<Client>();
        public Client _LocalClient = new LocalSharedObj<Client>();

        #endregion

        public Game() //перед привязки к DataContext заполняем все масивы которые будут привязаны
        {            
            _Game = this;
            Loaded += new RoutedEventHandler(Game_Loaded);            
            InitializeComponent();                                    

        }

        void Game_Loaded(object sender, RoutedEventArgs e)
        {
            _TeamSelect.Hide();
            _Players.BindTo(_PlayersCanvas.Children);
            _AnimatedSprites.BindTo(_PlayersCanvas.Children);            
            Load();
        }

        void TeamSelect__OnSelectDone(Player.Team _Team, Player.PlayerModel _PlayerModel)
        {
            OnTeamSelected(_Team, _PlayerModel);
        }
        public void Load() //downloadload game content->ContentDownloaded
        {
            
            _LocalClient.Load();
            new Downloader().Download("Content.zip", ContentDownloaded);
        }

        
        public void OnTeamSelected(Player.Team _Team, Player.PlayerModel _PlayerModel)
        {
            _LocalClient._PlayerModel = _PlayerModel;
            _LocalClient._Team = Player.Team.terr;
            _LocalClient._PlayerState = Client.PlayerState.alive;
        }

        public string _KillText { get { return _KillTextBlock.Text; } set { _KillTextBlock.Text = value; } }
        public void WriteKillText(string text)
        {
            _KillText += "\n" + text;
        }

        public void AddSprite(IUpdate us)
        {
            this._AnimatedSprites.Add(us);
        }
        public void RemoveSprite(IUpdate us)
        {
            this._AnimatedSprites.Remove(us);
        }

        public void ShowDamage(Vector pos, float change)
        {
            AnimDamage _AnimDamage = new AnimDamage { _Position = pos };
            _AnimDamage.Load(change.ToString());
        }



        public Camera _CurrentCamera;

        bool _ContentDownloaded;
        public void ContentDownloaded(Stream s) //download map->MapDownloaded
        {
            Trace.Assert(!_ContentDownloaded);
            _ContentDownloaded = true;
            Helper.LoadResourcesFromZip(s, _Resources);
            _GameContentDataBase = (GameContentDataBase)Common._XmlSerializerContent.Deserialize(_Resources["content.xml"]);
            new Downloader().Download(_MapPath, MapDownloaded);
        }

        public void MapDownloaded(Stream s) //->_Map.Load
        {
            Helper.LoadResourcesFromZip(s, Map._Resources);
            _Map = new Map();
            _Map.Load();
            Start();
        }
        public bool _Loaded;
        public void Start()
        {
            _CurrentCamera = new FreeCamera();//{ _FreeViewPos = new Point { X = 2800, Y = 2800 } };
#if(DEBUG)
            
            OnTeamSelected(Player.Team.terr, Player.PlayerModel.phoenix);
#else
            _TeamSelect.Show();
            _TeamSelect._OnSelectDone += new TeamSelect.OnSelectDone(TeamSelect__OnSelectDone);
#endif
            _Loaded = true;
        }


        public Player _Player { get { return _LocalClient._Player; } }
        public void Update()
        {
            if (!_Loaded) return;
            _CurrentCamera.Update();

            foreach (Client _Client in _Clients)
                _Client.Update();
            foreach (GameObj obj in _AnimatedSprites)
                obj.Update();

        }
    }

    [Obsolete]
    public class AnimDamage : UserControl, IUpdate
    {

        public Vector _Position { get; set; }
        TextBlock _TextBlock = new TextBlock();
        public Game _Game { get { return Game._Game; } }
        public void Load(string text)
        {
            _Game.AddSprite(this);
            _TextBlock.Text = text;
            _TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);
            _TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _TextBlock.VerticalAlignment = VerticalAlignment.Center;
            //LayoutRoot.Children.Add(_TextBlock);
        }

        public void Update()
        {
            //base.Update();
            //_y -= 3;
            _TextBlock.Opacity -= .03;
            if (_TextBlock.Opacity < 0)
                _Game.RemoveSprite(this);

        }
    }

    public class Camera
    {
        public static Game _Game { get { return Game._Game; } }
        public virtual void Update()
        {
        }
    }
    public class FreeCamera : Camera
    {
        public Point _FreeViewPos;
        public override void Update()
        {
            base.Update();
            _Game._TranslateTransform.X = -_FreeViewPos.X * _Game._Scale + _Game._Width/ 2;
            _Game._TranslateTransform.Y = -_FreeViewPos.Y * _Game._Scale + _Game._Width / 2;
            if (Page._Keys.Contains(Key.Left))
                _FreeViewPos.X -= 50;
            if (Page._Keys.Contains(Key.Right))
                _FreeViewPos.X += 50;
            if (Page._Keys.Contains(Key.Up))
                _FreeViewPos.Y -= 50;
            if (Page._Keys.Contains(Key.Down))
                _FreeViewPos.Y += 50;
        }

    }
    public class PerspCamera : Camera
    {
        public GameObj _GameObj;
        public int _distance = 2;
        public override void Update()
        {
            base.Update();
            //_FreeViewPos.GetX = _Pos.GetX;
            //_FreeViewPos.GetY = _Pos.GetY;
            double _mousex = (Page._Mouse.X / _Game._Width) - .5;
            double _mousey = (Page._Mouse.Y / _Game._Height) - .5;

            _Game._TranslateTransform.X = ((-_GameObj._x * _Game._Scale) + (_Game._Width / 2) - _mousex * _distance * _Game._Scale);
            _Game._TranslateTransform.Y = ((-_GameObj._y * _Game._Scale) + (_Game._Height / 2) - _mousey * _distance * _Game._Scale);
        }
    }

}
