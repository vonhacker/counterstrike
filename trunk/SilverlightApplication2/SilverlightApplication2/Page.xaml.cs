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
namespace SilverlightApplication2
{
    public partial class Page : UserControl
    {
        public static Page _Page;
        public Page()
        {
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

            _Player = new Player { _Canvas = _Canvas};
            _Player.Load();
            _Player._x = 100;
            _Player._y = 50;
            _Cursor = new Cursor() { _Canvas = _Canvas2};
            _Cursor.Load();
            Update();
        }

        void Page_KeyUp(object sender, KeyEventArgs e)
        {
            _Keys.Add(e.Key);
        }
        public static List<Key> _Keys = new List<Key>();
        void Page_KeyDown(object sender, KeyEventArgs e)
        {
            _Keys.Remove(e.Key);
        }
        Cursor _Cursor;
        Storyboard _Storyboard = new Storyboard();
        private void Update()
        {            
            _Player.Update();            
            _Cursor.Update();
            _Storyboard.Begin();
        }
        public static Point _Mouse;
        void Page_MouseMove(object sender, MouseEventArgs e)
        {            
            _Mouse= e.GetPosition(this);            
        }
        
        public new class Cursor : GameObj
        {
            public override void Load()
            {
                _Element = new Ellipse() { Width = 10, Height = 10, StrokeThickness = 2, Fill = new SolidColorBrush(Colors.Red) };
                base.Load();
            }
            public void Update()
            {
                _x = _Mouse.X;
                _y = _Mouse.Y;
            }
        }
        public abstract class GameObj
        {
            public FrameworkElement _Element;
            public double _x { get { return Canvas.GetLeft(_Element); } set { Canvas.SetLeft(_Element, value); } }
            public double _y { get { return Canvas.GetTop(_Element); } set { Canvas.SetTop(_Element, value); } }
            public Panel _Canvas;
            public virtual void Load()
            {                
                _Canvas.Children.Add(_Element);
            }
        }

        public class Player : GameObj
        {
            public double _h { get { if (double.NaN == _Element.Height) throw new Exception(); return _Element.Height; } set { _Element.Height = value; } }
            public double _w { get { return _Element.Width; } set { _Element.Width = value; } }

            public Point _pos { get { return new Point(_x, _y); } set { _x = value.X; _y = value.Y; } }
            public new Canvas _Element { get { return (Canvas)base._Element; } set { base._Element = value; } }
            public override void Load()
            {
                _Element = new Canvas();
                Ellipse _Ellipse = new Ellipse() { Width = 30, Height = 30, Fill = new SolidColorBrush(Colors.Black) };
                _w = _Ellipse.Width;
                _h = _Ellipse.Height;
                _Element.Children.Add(_Ellipse);
                Canvas.SetLeft(_Ellipse, -_Ellipse.Width / 2);
                Canvas.SetTop(_Ellipse, -_Ellipse.Height / 2);
                base.Load();
            }
            Point _S;
            public Panel _Map { get { return _Page._map; } }
            public Point _oldpos;
            public double gravitaty=5;
            public void Update()
            {

                //if (_pos == _oldpos) return;
                ////////////////////
                Point p = _pos;
                p.Y +=_h/2;                
                UIElement _wall = (UIElement)VisualTreeHelper.FindElementsInHostCoordinates(p, _Map).FirstOrDefault();
                if (_wall != null)
                {
                    _S.X = _S.X / 2;
                    _S.Y = -_S.Y / gravitaty;
                    _y = _oldpos.Y;
                }
                
                //////////////
                p.Y -= 10;
                _wall = (UIElement)VisualTreeHelper.FindElementsInHostCoordinates(p, _Map).FirstOrDefault();
                if (_wall != null)
                {
                    _y -= 10;                    
                }            
                
                //////////////
                p.Y -= 10;
                _wall = (UIElement)VisualTreeHelper.FindElementsInHostCoordinates(p, _Map).FirstOrDefault();
                if (_wall != null)
                {
                    _S.X = -_S.X;
                    _pos = _oldpos;

                    _y = _oldpos.Y;
                    _x = _oldpos.X;
                }

                _oldpos = _pos;
                _S.Y++;
                _x += _S.X;
                _y += _S.Y;

                if (((int)_S.X) == 0) _S.X = 0;
                UpdateKeyBoard();
            }

            private void UpdateKeyBoard()
            {
                foreach (Key key in _Keys)
                {
                    switch (key)
                    {
                        case Key.A:
                            _x.Trace();
                            _x--;
                            break;
                        case Key.D:
                            _x++;
                            break;
                    }

                }
            }
        }
    }
}
