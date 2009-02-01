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

namespace CounterStrikeLive
{
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
            _frame += 30 * (float)STimer._SecodsElapsed;
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
}
