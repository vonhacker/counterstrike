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
using FarseerGames.FarseerPhysics.Mathematics;
using System.Diagnostics;

namespace VectorWorldWpf
{

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
        
        public Point pos { get { return new Point(_x, _y); } set { _x = value.X; _y = value.Y; } }
        public Point oldpos = new Point();
        public void Update()
        {
            oldpos = pos;
            int s = 5;
            if (Keyboard.IsKeyDown(Key.A)) _x -= s;
            if (Keyboard.IsKeyDown(Key.D)) _x += s;
            if (Keyboard.IsKeyDown(Key.W)) _y -= s;
            if (Keyboard.IsKeyDown(Key.S)) _y += s;
            pos =Vector2D.Fazika(pos, oldpos, 15, walls.ToList());
            //Vector2D way = new Vector2D(oldpos, pos);

            //Point newDot;            
            //Point newDotResult = new Point();
            //int countCross = 0;
            //// Находим стены пересекающиеся с новым положением и предположительные точки
            //foreach (Vector2D v in walls)
            //    if (Vector2D.checkCrossWalls(v, oldpos, pos, out newDot))
            //    {
            //        countCross++;
            //        newDotResult.X += newDot.X;
            //        newDotResult.Y += newDot.Y;
            //    }
            
            //if (countCross == 1) pos = newDotResult;
            //else if (countCross == 2)
            //{
            //    newDotResult.X /= (double)countCross;
            //    newDotResult.Y /= (double)countCross;
            //    pos = newDotResult;
            //    if (walls.Any(a => Calculator.DistanceBetweenPointAndLineSegment(pos, a.dot1, a.dot2) < 15))
            //        pos = oldpos;
                
            //}
            
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
