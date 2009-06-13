using doru;
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
using VectorWorld;

namespace VectorWorldWpf
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        private Vector2D[] walls = new Vector2D[] {
            new Vector2D(new Point(100.0f, 300.0f), new Point(300.0f, 200.0f)),
            new Vector2D(new Point(200.0f, 300.0f), new Point(300.0f, 200.0f))
        };

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = 800;
            this.Height = 600;
            foreach (Vector2D v in walls)
            {
                Line l = new Line() { X1 = v.dot1.X, Y1 = v.dot1.Y, X2 = v.dot1.Y, Y2 = v.dot2.Y };
                l.StrokeThickness = 2;
                l.Stroke = new SolidColorBrush(Colors.Black);
                _Canvas.Children.Add(l);
            }
            _Canvas.Children.Add(_Ellipse);

            new DispatcherTimer().StartRepeatMethod(.002, Update);

            _x = 0;
            _y = 0;
        }
        double minDistance = 15;
        public Point lastDot { get { return new Point(_x, _y); } set { _x = value.X; _y = value.Y; } }
        public Point Dot = new Point();
        public void Update()
        {
            Dot = lastDot;
            int s = 5;
            if (Keyboard.IsKeyDown(Key.A)) Dot.X -= s;
            if (Keyboard.IsKeyDown(Key.D)) Dot.X += s;
            if (Keyboard.IsKeyDown(Key.W)) Dot.Y -= s;
            if (Keyboard.IsKeyDown(Key.S)) Dot.Y += s;

            Vector2D way = new Vector2D(lastDot, Dot);

            Point newDot;
            Point newDotResult = new Point();
            int countCross = 0;
            // Находим стены пересекающиеся с новым положением и предположительные точки
            foreach (Vector2D v in walls)
                if (checkCrossWalls(v, lastDot, Dot, out newDot))
                {
                    countCross++;
                    newDotResult.X += newDot.X;
                    newDotResult.Y += newDot.Y;
                }
            if (countCross == 0) lastDot = Dot;
            else if (countCross == 1) lastDot = newDotResult;
            else
            {
                newDotResult.X /= (double)countCross;
                newDotResult.Y /= (double)countCross;
                lastDot = newDotResult;
            }
        }
        private bool checkCrossWalls(Vector2D wall, Point lastDot, Point Dot, out Point newDot)
        {
            Vector2D way = new Vector2D(lastDot, Dot);
            Point cross;
            bool isCross = wall.cross(way, out cross);
            double distance = wall.distance(way.dot2);
            if (isCross || Math.Abs(distance) < minDistance)
            {
                Vector2D n;
                if (isCross)
                    n = new Vector2D(Dot, distance < 0 ? -wall.normal() : wall.normal());
                else
                    n = new Vector2D(Dot, distance < 0 ? wall.normal() : -wall.normal());
                if (wall.cross(n, out cross, false))
                {
                    n = new Vector2D(cross, n);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                } else if (Math.Sqrt(Math.Pow(wall.dot1.X - Dot.X, 2.0f) + Math.Pow(wall.dot1.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot1, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                } else if (Math.Sqrt(Math.Pow(wall.dot2.X - Dot.X, 2.0f) + Math.Pow(wall.dot2.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot2, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                }
            }
            newDot = Dot;
            return false;
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
