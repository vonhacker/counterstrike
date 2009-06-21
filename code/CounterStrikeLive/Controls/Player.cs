using System;
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

namespace CounterStrikeLive
{
    public class Player : GameObjA
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
            _Position = VectorWorld.Vector2D.Fazika(_Position, _OldPosition, 15, _Map.walls);
            //Line2 wall = _Map.CCollision(_Position, 25);
            //if (wall != null)
            //{
            //    _Position = _OldPosition;
            //    //float a= Calculator.DegreesToRadians(90);

            //    // Vector2 _Vectora = wall._cpoint - _Position;

            //    // Vector2 _Vectorb = new Vector2(0, (_Position - _OldPosition).Length());                

            //    // float rads = Calculator.VectorToRadians(_Vectora);
            //    // float rads2 = Calculator.VectorToRadians(wall._p2 - wall._p1)+1.57f;

            //    // Calculator.RotateVector(ref _Vectorb, rads2);
            //    // if (Math.Abs(rads-rads2) <1.57f)
            //    // {
            //    //     _Vectorb = Vector2.Multiply(_Vectorb, -1);
            //    // }

            //    // _Position -=_Vectorb;
            //}
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
					float dir2 = CorrectAngle(dir - _Angle);
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
                _MoveVector = Vector2.Multiply(_MoveVector, _V * (float)Menu._TimerA._SecodsElapsed);
                if (_IsSlowDown) _MoveVector = Vector2.Multiply(_MoveVector, .5f);
                _Position += _MoveVector;
            }
        }

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
        public float _Life { get { return _Client._Life; } set { _Client._Life = value; } }
        public double _slowdowntimeelapsed;
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
}
