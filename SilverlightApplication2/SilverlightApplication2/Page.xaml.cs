using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using doru;
using FarseerGames.FarseerPhysics.Mathematics;
namespace SilverlightApplication2
{

    public partial class Page : UserControl
    {

        public static Page _Page;
        public Page()
        {
            //Point
            _Page = this;
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }
        Player _Player;
        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MouseMove += new MouseEventHandler(Page_MouseMove);
            KeyDown += new KeyEventHandler(Page_KeyDown);
            KeyUp += new KeyEventHandler(Page_KeyUp);
            _Storyboard.Completed += delegate { Update(); };

            _Player = new Player();
            AddToPage(_Player);
            _Player.Load();
            _Player._x = 100;
            _Player._y = 50;

            Update();
        }

        void Page_KeyUp(object sender, KeyEventArgs e)
        {
            _Keys.Remove(e.Key);
        }
        public static List<Key> _Keys = new List<Key>();
        public static Key _LastKey;
        void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_Keys.Contains(e.Key))
            {
                _LastKey = e.Key;
                _Keys.Add(e.Key);
            }
        }

        Storyboard _Storyboard = new Storyboard();
        private void Update()
        {
            foreach (GameObj g in _GameObjects.ToArray())
                g.Update();
            _Player.Update();

            _Storyboard.Begin();
        }
        public static Point _Mouse;
        void Page_MouseMove(object sender, MouseEventArgs e)
        {
            _Mouse = e.GetPosition(this);
        }

        public new class Cursor : GameObj
        {
            public override void Load()
            {
                _Canvas.Children.Add(new Ellipse() { Width = 10, Height = 10, StrokeThickness = 2, Fill = new SolidColorBrush(Colors.Red) });
                base.Load();
            }
            public void Update()
            {
                _x = _Mouse.X;
                _y = _Mouse.Y;
            }
        }
        public static List<GameObj> _GameObjects = new List<GameObj>();
        public static void RemoveFromPage(GameObj obj)
        {
            _Page._Canvas.Children.Remove(obj);
            if (!_GameObjects.Contains(obj)) throw new Exception("object does not exsts");
            _GameObjects.Remove(obj);
        }
        public static void AddToPage(GameObj gobj)
        {
            if (_GameObjects.Contains(gobj)) throw new Exception("already added");
            _Page._Canvas.Children.Add(gobj);
            _GameObjects.Add(gobj);
        }
        public class Gun : GameObj
        {
            public int _fire { get { return int.Parse(_TextBlock.Text); } set { _TextBlock.Text = value.ToString(); } }
            public Player _Player;
            public override void Load()
            {

                _angle = 90;
                base.Load();
                _TextBlock = new TextBlock { Text = "0", Width = 40, Height = 20 };
                Add(_TextBlock);
                _TextBlock.Center(0, -10);
                //_Canvas.Children.Add(_TextBlock);

                Rectangle _Rectangle = new Rectangle { Width = 10, Height = 40, Fill = new SolidColorBrush(Colors.Red) };
                Add(_Rectangle);
                _Rectangle.Center(0, -10);

            }
            TextBlock _TextBlock;
            public override void Update()
            {
                foreach (Key key in _Keys)
                {
                    switch (key)
                    {
                        case Key.S:
                            _angle += 3;
                            break;
                        case Key.W:
                            _angle -= 3;
                            break;
                        case Key.F:
                            _fire++;
                            break;
                    }
                }
                if (!_Keys.Contains(Key.F) && _fire != 0)
                {
                    Patron _Patron = new Patron { _x = _Player._x, _y = _Player._y, _angle = _angle, _power = _fire, _right = _Player._right };
                    _Patron.Load();
                    _fire = 0;
                }
                base.Update();
            }
        }
        public class Explosion : GameObj
        {
            Ellipse _Ellipse;
            public override void Load()
            {
                AddToPage(this);
                base.Load();

                Ellipse _Ellipse2 = new Ellipse { Width = 50, Height = 50, Fill = new SolidColorBrush(Colors.White) };                
                _Page._map.Children.Add(_Ellipse2);
                _Ellipse2.Center(_x, _y);                
                _Ellipse = new Ellipse { Width = 5, Height = 5, Fill = new SolidColorBrush(Colors.Yellow) };
                _Ellipse.Center();
                Add(_Ellipse);
                _Ellipse.Center();
            }
            public override void Update()
            {
                _Scale += .7;
                if (_Scale > 10)
                {
                    RemoveFromPage(this);
                }
                base.Update();
            }
        }
        public class Patron : GameObj
        {
            public override void Load()
            {
                AddToPage(this);
                base.Load();
                Ellipse _Ellipse = new Ellipse { Fill = new SolidColorBrush(Colors.Blue), Height = 5, Width = 5 };
                Add(_Ellipse);
                _Ellipse.Center();
            }
            public override void Update()
            {
                if (Collision(_x, _y))
                {
                    Explosion _Explosion = new Explosion();
                    _Explosion._x = _x;//-_S.X;
                    _Explosion._y = _y;//-_S.Y;
                    _Explosion.Load();
                    RemoveFromPage(this);
                }
                _x += _right ? _S.X : -_S.X;
                _y += _S.Y;
                _S.Y++;
                
                base.Update();
            }
        }
        public abstract class GameObj : UserControl
        {

            public Point _S;
            public float _rad { get { return Calculator.DegreesToRadians((float)_angle); } }
            public double _power
            {
                get { return _S.ToVect().Length(); }
                set
                {
                    Vector2 v = Calculator.RadiansToVector(_rad);
                    _S.X = v.X * value;
                    _S.Y = v.Y * value;
                }
            }
            private ScaleTransform _ScaleTransform;
            public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }
            RotateTransform _RotateTransform;
            public bool _right
            {
                get { return _ScaleTransform.ScaleX > 0; }
                set
                {
                    if (value == true && _ScaleTransform.ScaleX < 0 ||
                        value == false && _ScaleTransform.ScaleX > 0) _ScaleTransform.ScaleX *= -1;
                }
            }
            public void Add(UIElement fw)
            {
                _Canvas.Children.Add(fw);
            }
            public double _h { get { if (double.NaN == _Canvas.Height) throw new Exception(); return _Canvas.Height; } set { _Canvas.Height = value; } }
            public double _w { get { return _Canvas.Width; } set { _Canvas.Width = value; } }
            public Canvas _Canvas { get; private set; }

            TransformGroup _TransformGroup = new TransformGroup();

            public double _angle { get { return _RotateTransform.Angle; } set { _RotateTransform.Angle = value; } }
            public GameObj()
            {
                _RotateTransform = new RotateTransform();
                _TransformGroup.Children.Add(_RotateTransform);
                _ScaleTransform = new ScaleTransform();
                _TransformGroup.Children.Add(_ScaleTransform);
                this.RenderTransform = _TransformGroup;
                _Canvas = new Canvas();
                this.Content = _Canvas;
            }
            public double _x { get { return Canvas.GetLeft(this); } set { Canvas.SetLeft(this, value); } }
            public double _y { get { return Canvas.GetTop(this); } set { Canvas.SetTop(this, value); } }

            public virtual void Load()
            {

            }
            public virtual void Update()
            {

            }

        }
        public static bool Collision(double x, double y) { return Collision(new Point(x, y)); }
        public static bool Collision(Point p)
        {
            foreach (FrameworkElement fw in VisualTreeHelper.FindElementsInHostCoordinates(p, _Page._map))
            {
                if (fw is Ellipse) return false;
                if (fw is Path) return true;
            }
            return false;
        }
        public class Player : GameObj
        {
            public Point _pos { get { return new Point(_x, _y); } set { _x = value.X; _y = value.Y; } }
            public override void Load()
            {

                Ellipse _Ellipse = new Ellipse() { Width = 30, Height = 30, Fill = new SolidColorBrush(Colors.Black) };
                _w = _Ellipse.Width;
                _h = _Ellipse.Height;
                Add(_Ellipse);
                _Gun = new Gun { _Player = this };
                Add(_Gun);
                _Gun.Load();
                _Ellipse.Center();
                base.Load();
            }



            public Panel _Map { get { return _Page._map; } }
            public Point _oldpos;
            public double gravitaty = 5;
            Gun _Gun;

            public override void Update()
            {

                _Gun.Update();

                //if (_pos == _oldpos) return;
                ////////////////////
                Point p = _pos;
                p.Y += _h / 2;
                
                if (Collision(p))
                {
                    _S.X = _S.X / 2;
                    _S.Y = -_S.Y / gravitaty;
                    _y = _oldpos.Y;
                }

                //////////////
                p.Y -= 10;                
                if (Collision(p))
                {
                    _y -= 10;
                }

                //////////////
                p.Y -= 10;

                if (Collision(p))
                {
                    _S.X = -_S.X;
                    _pos = _oldpos;

                    _y = _oldpos.Y;
                    _x = _oldpos.X;
                }

                _oldpos = _pos;
                _S.Y += .5;
                _x += _S.X;
                _y += _S.Y;

                if (((int)_S.X) == 0) _S.X = 0;
                UpdateKeyBoard();
                base.Update();
            }

            //public bool left{get{ return this._sca
            private void UpdateKeyBoard()
            {


                foreach (Key key in _Keys)
                {
                    _LastKey = key;
                    if (_S.X == 0)
                    {
                        switch (key)
                        {
                            case Key.A:
                                _right = false;
                                _x--;
                                break;
                            case Key.D:
                                _right = true;
                                _x++;
                                break;
                            case Key.Space:
                                _S.X = _right ? 3 : -3;
                                _S.Y = -5;
                                _y -= 10;
                                break;
                        }
                    }
                }
            }
        }
    }
    public static class Extensions
    {
        public static void SetPos(this FrameworkElement fw, double x, double y)
        {
            Canvas.SetLeft(fw, x);
            Canvas.SetTop(fw, y);

        }
        public static Vector2 ToVect(this Point p)
        {
            return new Vector2((float)p.X, (float)p.Y);
        }
        public static FrameworkElement Center(this FrameworkElement _Ellipse) { return Center(_Ellipse, 0, 0); }
        public static FrameworkElement Center(this FrameworkElement _Ellipse, double x, double y)
        {
            Canvas.SetLeft(_Ellipse, -_Ellipse.Width / 2 + x);
            Canvas.SetTop(_Ellipse, -_Ellipse.Height / 2 + y);
            return _Ellipse;
        }
    }
}
