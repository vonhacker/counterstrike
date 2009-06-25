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
using doru;
using CounterStrikeLive.Service;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace CounterStrikeLive.Controls
{

    public class GameObj : DependencyObject
    {
        
        public bool _VisibleToAll;
        Menu _Menu = Menu._This;
        LocalDatabase _LocalDatabase = LocalDatabase._This;
        Config _Config = Config._This;
        public void PlaySound(string s, double distance)
        {
            
            //if (_Game._EnemyPlayer == this) return;

            double volume = GetVolume(distance);

            if (volume != 0)
            {
                MediaElement _MediaElement = new MediaElement();
                _Menu._GameCanvas.Children.Add(_MediaElement);
                _MediaElement.SetSource(s);
                _MediaElement.MediaEnded += delegate { _Menu._GameCanvas.Children.Remove(_MediaElement); };
                _MediaElement.Volume = volume;
            }
        }
        public void PlaySound(string s)
        {
            PlaySound(s, 5000);
        }
        public double GetVolume(double distance)
        {
            double x, y, x2, y2;

            if (_Game._LocalPlayer == null)
            {
                x = _Game._FreeViewPos.X;
                y = _Game._FreeViewPos.Y;
            } else
            {
                x = _Game._LocalPlayer._Position.X;
                y = _Game._LocalPlayer._Position.Y;
            }
            x2 = position.X;
            y2 = position.Y;

            double len = Math.Sqrt((x - x2).Pow() + (y - y2).Pow());
            double volume = Math.Max(0, 1 - len / distance) * _LocalDatabase.Volume;
            return volume;
        }


        public Vector2 position;
        ~GameObj()
        {
            this.DisposeValues();
        }
        public Vector2 _Position { get { return position; } set { if (value.X == float.NaN) throw new Exception("Break"); position = value; } }
        public Map _Map { get { return _Game._Map; } }
        protected Canvas _Canvas = new Canvas();
        public Game _Game;
        public virtual void CheckVisibility(Player _LocalPlayer)
        {
            if (_LocalPlayer != null && this != _LocalPlayer && !_VisibleToAll && !_Game._IsEverthingVisible)
            {
                float a1 = Animation.Cangl(Calculator.VectorToRadians(this._Position - _LocalPlayer._Position) * Calculator.DegreesToRadiansRatio);
                float a2 = Animation.Cangl(a1 - _LocalPlayer._Angle + 45);
                if (Math.Abs(a2) < 90)
                {
                    Line2 wall;
                    if (_Map.Collision(this._Position, _LocalPlayer._Position, out wall).Count != 0)
                    {
                        this._Visibility = Visibility.Collapsed;
                    } else this._Visibility = Visibility.Visible;
                } else this._Visibility = Visibility.Collapsed;
            } else this._Visibility = Visibility.Visible;
        }
        public Visibility _Visibility
        { get { return _Canvas.Visibility; } set { _Canvas.Visibility = value; } }
        public static float Cangl(float value)
        {
            float angle;
            angle = value % 360;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
    public class GameObjA : GameObj
    {
        protected Vector2 _OldPosition;
        public RotateTransform _RotateTransform = new RotateTransform();
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
                angle = Cangl(value);
            }
        }
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
        public virtual void Load()
        {
            if (!_VisibleToAll) this._Visibility = Visibility.Collapsed;
            _Canvas2.RenderTransform = _RotateTransform;
            _Canvas.Children.Add(_Canvas2);
            UpdateTranslations();
            Add();

        }
        public Canvas _Canvas2 = new Canvas();
        public virtual void Add()
        {
            _Explosions.Add(this);
            _PaternCanvas.Children.Add(_Canvas);
        }
        public virtual void Remove()
        {
            _Explosions.Remove(this);
            _PaternCanvas.Children.Remove(_Canvas);
        }

        public virtual List<GameObjA> _Explosions { get { return _Game._Explosions; } set { } }
        public virtual Canvas _PaternCanvas { get { return _Map._Canvas; } set { } }
        public bool _Remove = true;
        public virtual void Update()
        {
        }
    }
    public class AnimationB : GameObjA
    {
        public bool _isPlaying = true;
        public float _frame;
        public List<FolderList> fls;
        public string name { set { fls = FolderList.Find(value).fls; } }
        Image _Image = new Image();
        public AnimationB()
            : base()
        {

        }
        public override void Load()
        {
            base.Load();
            _Canvas2.Children.Add(_Image);
            _Image.Center(fls[0]._BitmapImage);
        }

        public override void Update()
        {
            if (_isPlaying)
            {

                _frame += (float)(30 * Menu._TimerA._SecodsElapsed);
                if (_frame < fls.Count)
                {
                    _Image.Source = fls[(int)_frame]._BitmapImage;
                } else
                {
                    if (_Remove == true) Remove();
                    _isPlaying = false;
                    _frame = 0;
                }
            }
        }
    }
    public class Animation : GameObjA
    {



        //public float _width { get { return _AnimatedBitmap._Width; } set { _AnimatedBitmap._Width = value; } }
        //public float _height { get { return _AnimatedBitmap._Height; } set { _AnimatedBitmap._Height = value; } }

        public Database.AnimatedBitmap _AnimatedBitmap;
        public float _frame;


        public override void Load()
        {
            base.Load();
            _Image = _AnimatedBitmap.GetImage();
            _Canvas2.Children.Add(_Image);
        }





        public Image _Image;
        public bool _isPlaying = true;
        public override void Update()
        {
            if (_isPlaying)
            {

                _frame += (float)(30 * Menu._TimerA._SecodsElapsed);
                if (_frame < _AnimatedBitmap._BitmapImages.Count)
                {
                    _Image.Source = _AnimatedBitmap._BitmapImages[(int)_frame];
                } else
                {
                    if (_Remove == true) Remove();
                    _isPlaying = false;
                    _frame = 0;
                }
            }
        }
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
    public class AnimationA : Animation
    {
        public override List<GameObjA> _Explosions { get; set; }
        public override Canvas _PaternCanvas { get; set; }
        public AnimationA()
            : base()
        {
            _Visibility = Visibility.Visible;
            _VisibleToAll = true;
            _Remove = false;
            _isPlaying = true;
        }
    }
}
