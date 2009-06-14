using doru;
//using doru.Vectors;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Diagnostics;


namespace VectorWorld2Wpf
{
    public class Physics
    {
        public static Point Reflect(Point speed, Line wall)
        {
            Point _vector = new Point(wall.X1 - wall.X2, wall.Y1 - wall.Y2);
            _vector = Rotate(_vector, 90);

            Point normal = Normalize(_vector);
            return Reflect(speed, normal);
        }
        private static double Dot(ref Point _Point, ref Point _PointNormal)
        {
            return (_Point.X * _PointNormal.X) + (_Point.Y * _PointNormal.Y);
        }
        public static Point Reflect(Point _Point, Point _PointNormal)
        {
            double mul = Dot(ref _Point, ref _PointNormal);

            Point vmul = new Point(_PointNormal.X * mul * 2, _PointNormal.Y * mul * 2);
            Point reflect = new Point(_Point.X - vmul.X, _Point.Y - vmul.Y);
            return reflect;
        }
        public static Point Rotate(Point v, double a)
        {
            a = a / 180 * (double)(double)Math.PI;
            double tx = (v.X * (double)Math.Cos(a)) - (v.Y * (double)Math.Sin(a));
            double ty = (v.X * (double)Math.Sin(a)) + (v.Y * (double)Math.Cos(a));
            return new Point(tx, ty);

        }
        private static Point Normalize(Point _Vector)
        {
            double num2 = (_Vector.X * _Vector.X) + (_Vector.Y * _Vector.Y);
            double num = 1f / ((double)Math.Sqrt((double)num2));
            Point _Normal = new Point(_Vector.X * num, _Vector.Y * num);
            return _Normal;
        }
    }
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        public List<Point> _Points = new List<Point>()
        {
            new Point(100, 300),new Point(300, 200),new Point(200, 300)
        };
        

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = 800;
            this.Height = 600;
            Polyline _Polyline = new Polyline() { StrokeThickness = 2, Stroke = new SolidColorBrush(Colors.Red) };
                
            foreach (Point p in _Points)
            {
                _Polyline.Points.Add(p);
                
                
            }
            _Canvas.Children.Add(_Polyline);
            _Canvas.Children.Add(_Ellipse);

            new DispatcherTimer().StartRepeatMethod(.002, Update);

            _x = 0;
            _y = 0;
        }
        
        public Point pos { get { return new Point(_x, _y); } set { _x = value.X; _y = value.Y; } }
        
        public void Update()
        {
            int _dist = 15;
            Point oldpos = pos;
            int s = 5;
            if (Keyboard.IsKeyDown(Key.A)) _x -= s;
            if (Keyboard.IsKeyDown(Key.D)) _x += s;
            if (Keyboard.IsKeyDown(Key.W)) _y -= s;
            if (Keyboard.IsKeyDown(Key.S)) _y += s;
            if (pos == oldpos) return;
            Point? op= null;
            foreach (Point p in _Points)
            {
                if (op != null)
                {
                    Vector2 _out;
                    double dist = Calculator.DistanceBetweenPointAndLineSegment((Vector2)pos, (Vector2)op, (Vector2)p, out _out);
                    if (dist < _dist)
                    {
                        Vector2 rv = Physics.Reflect(new Point(pos.X - oldpos.X, pos.Y - oldpos.Y), new Line().SetPoints(op.Value, p));
                        pos += rv;
                    }
                }
                op = p;
            }
            
        }
        

        public double _x { get { return _Ellipse.GetX(); } set { _Ellipse.SetX(value); } }
        public double _y { get { return _Ellipse.GetY(); } set { _Ellipse.SetY(value); } }
        Ellipse _Ellipse = new Ellipse()
        {
            Width = 30,
            Height = 30,
            Fill = new SolidColorBrush(Colors.Black),
            RenderTransform = new TranslateTransform { X = -15, Y = -15 }
        };
    }
}
