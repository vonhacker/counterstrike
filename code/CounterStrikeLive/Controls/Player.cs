using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Windows.Media.Imaging;
using doru;
using CounterStrikeLive.Service;
using System.ComponentModel;
using doru.Tcp;
using System.Reflection;
using System.IO;
using CounterStrikeLive.Controls;
using LevelEditor4;

namespace CounterStrikeLive
{
    public static class Ext
    {
        public static Vector2 ToVector(this TreePoint tp)
        { return new Vector2((float)tp._x, (float)tp._y); }
        public static Vector2 Normalize2(this Vector2 v)
        {
            v.Normalize();
            return v;
        }
    }
    public class Player : GameObjA
    {
        public double _slowdowntimeelapsed;
        TextBlock _TextBlock = new TextBlock();
        public float _Life { get { return _Client._Life; } set { _Client._Life = value; } }
        public Team _Team { get { return _Client._Team; } }
        public override void CheckVisibility(Player _LocalPlayer)
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
        
        
        List<Point> _Points = new List<Point>();
        public const float _speed = 350f;

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
        public List<Animation> _Explosions2 = new List<Animation>();

        //public Canvas _Canvas2 = new Canvas();
        public override void Load()
        {

            _Menu._GameCanvas.Children.Add(_MediaElement);
            LoadNickName();
            _Canvas.Children.Add(_TextBlock);
	     	if(_Team ==  Team.cterr) _PlaeyerModel = Model.gsg9;            
            if (!_VisibleToAll) this._Visibility = Visibility.Collapsed;
            _Canvas2.RenderTransform = _RotateTransform;
            _PlayerImage = new Image();
			_GunImage = new Image();
			_Canvas2.Children.Add(_GunFire);
			
			_Canvas2.Children.Add(_GunImage);
            _Canvas2.Children.Add(_PlayerImage);
            _Canvas.Children.Add(_Canvas2);
            UpdateTranslations();
            Add();

            _PlayerImage.SetX(-GetPl().PixelWidth / 2);
            _PlayerImage.SetY(-GetPl().PixelHeight / 2);
			_GunImage.SetX(-GetPl().PixelWidth / 2);
			_GunImage.SetY(-GetPl().PixelHeight / 2);
			_GunFire.SetX(-GetPl().PixelWidth / 2);
			_GunFire.SetY(-GetPl().PixelHeight / 2);
        }
		
        Image _PlayerImage;
		Image _GunImage;
		Image _GunFire = new Image() { Source = FolderList.Find("ak47_Shoot.png")._BitmapImage, Visibility = Visibility.Collapsed };

        //public Image GetImage()
        //{
        //    Image _PlayerImage = new Image();
        //    Canvas.SetLeft(_PlayerImage, -_Width / 2 + _x);
        //    Canvas.SetTop(_PlayerImage, -_Height / 2 + _y);
        //    _PlayerImage.Width = _Width;
        //    _PlayerImage.Height = _Height;
        //    _PlayerImage.Source = _BitmapImages[0];
        //    return _PlayerImage;
        //}
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
            _Position = doru.VectorWorld.Vector2D.Fazika(_Position, _OldPosition, 15, _Map.walls);            
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
        FolderList _FolderList = FolderList._This;

        public float _pframe;
		public enum State { _run, _run_back, _run_left, _run_right, _stay, _reload }
        public enum Model { phoenix, ak47, gsg9 }
        public State _State = State._stay;
		public Model _PlaeyerModel = Model.phoenix;
		public Model _GunModel = Model.ak47;
		public override void Update()
		{
            if (_OldPosition == default(Vector2)) _OldPosition = _Position;
			_PlayerImage.Source = GetPl();
			_GunImage.Source = GetGun();

			if (!_isReloading) UpdateKeys();
			
			UpdateTranslations();

            Vector2? _PlayerCollide = PlayerCollide();
            if (_PlayerCollide != null)
            {
                _Position = _OldPosition;
                _Position += _PlayerCollide.Value;
			}
			if (_isReloading) _State = State._reload;
			else
				if (_OldPosition != default(Vector2) && _OldPosition != _Position)
				{
					UpdateCollisions();
					float dir = Calculator.VectorToRadians(_Position - _OldPosition) * Calculator.DegreesToRadiansRatio;
					float dir2 = Cangl(dir - _Angle);
					_State = State._run;
					if (dir2 > 45 && dir2 < 135) _State = State._run_right;
					if (dir2 < 315 && dir2 > 225) _State = State._run_left;
					if (dir2 > 135 && dir2 < 225) _State = State._run_back;
					PlayWalk();

				} else _State = State._stay;
			foreach (Animation _Explosion in _Explosions2)
			{
				_Explosion.Update();
            }
            _OldPosition = _Position;
        }
        private BitmapImage GetPl() {

			List<FolderList> nm = FolderList.Find(_PlaeyerModel + "" + _State).fls;
			_pframe += 30 * (float)Menu._TimerA._SecodsElapsed;
			if (_pframe >= nm.Count) _pframe = 0;
			BitmapImage _BitmapImage = nm[(int)_pframe]._BitmapImage;
			return _BitmapImage;
		}
		float _gframe;
        private BitmapImage GetGun() 
		{
			List<FolderList> nm = FolderList.Find(_GunModel+ "" + _State).fls;
			_gframe += 30 * (float)Menu._TimerA._SecodsElapsed;
			if (_pframe >= nm.Count) _pframe = 0;
			BitmapImage _BitmapImage = nm[(int)_pframe]._BitmapImage;
			return _BitmapImage;
		}
        

        MediaElement _MediaElement = new MediaElement();
        Menu _Menu = Menu._This;
        int _soundid = 0;
        private void PlayWalk()
        {
            if (_MediaElement.CurrentState == MediaElementState.Paused || _MediaElement.CurrentState == MediaElementState.Closed)
            {
                _soundid++;
                _MediaElement.SetSource("concrete" + ((_soundid % 4) + 1) + ".mp3");
                _MediaElement.Volume = GetVolume(2000);
                _MediaElement.Play();

            }
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
                _MoveVector = Vector2.Multiply(_MoveVector, _speed * (float)Menu._TimerA._SecodsElapsed);
                if (_IsSlowDown) _MoveVector = Vector2.Multiply(_MoveVector, _slowdownspeed);
                _Position += _MoveVector;
            }
        }
        public const float _slowdownspeed = .5f;
        public Database _Database { get { return Menu._Database; } }
        public LocalPlayer _LocalPlayer { get { return _Game._LocalPlayer; } }

        public void ReloadSound()
        {
            PlaySound("ak47_reload.mp3");
        }
        public void ShootAnimation()
        {
            PlaySound("ak47-1.mp3");
			_GunFire.Visibility = Visibility.Visible;
			Menu._TimerA.AddMethod(50, delegate { _GunFire.Visibility = Visibility.Collapsed; });
        }
		public bool _isReloading { get { return _Client._IsReloading; } set { _Client._IsReloading=value; } }
    }
	
	public class LocalPlayer : Player
    {
        
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
        
        
        public override void Update()
        {
            if (_IsSlowDown)
            {
                _slowdowntimeelapsed += Menu._TimerA._TimeElapsed;
                if (_slowdowntimeelapsed > 1000)
                {
                    _IsSlowDown = false;
                    _slowdowntimeelapsed = 0;
                }
            }
            base.Update();
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

        bool isReloading;
        [SharedObject(4)]
        public bool _IsReloading
        {
            get { return isReloading; }
            set
            {
                if (isReloading != value)
                {
                    isReloading = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("_isReloading"));
                }

            }
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
                } else if (playerState == PlayerState.dead)
                {
                    _Game.Die(_Player);
                    _Player.Remove();
                } else if (playerState == PlayerState.removed)
                {
                    if (_Player != null)
                        _Player.Remove();
                }
            }
        }

        private void CreatePlayer()
        {
            Trace.Assert(!(_Local && _Bot));
            if (_Local)
                _Player = new LocalPlayer();
            else if (_Bot)
                _Player = new BotPlayer();
            else
                _Player = new Player();
            if (_PlayerType != Database.PlayerType.TPlayer && _PlayerType != Database.PlayerType.CPlayer) throw new Exception("Break");

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
        public bool _Bot;
    }
}
