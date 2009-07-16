using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using Vector2D = FarseerGames.FarseerPhysics.Mathematics.Vector2;
using PhysicsEngine = FarseerGames.FarseerPhysics.PhysicsSimulator;

using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Reflection;

namespace WpfApplication3
{
    
    public enum State2 { deflt, wall }
    public partial class Window1 : Window
    {
        public static Window1 _This;
        public Window1()
        {
            
            _This = this;
            InitializeComponent();
            Logging.Setup();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }


        public PhysicsEngine engine;
        
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            
            engine = new PhysicsEngine(new Vector2(0,100));

    

            Dispatcher.StartUpdate(UpdateWpf);
            _RootCanvas.MouseDown += new MouseButtonEventHandler(RootCanvas_MouseDown);
            KeyDown += new KeyEventHandler(Window1_KeyDown);
            
        }
        public Vector2D[] CreateVertex()
        {
            Single Ld2 = 50 / 2;
            Single Wd2 = 25 / 2;
            Vector2D[] vertexes = new Vector2D[]
            {
                new Vector2D(Wd2, 0),
                new Vector2D(Wd2, Ld2),
                new Vector2D(5, Ld2+7),
                new Vector2D(-5, Ld2+7),
                new Vector2D(-Wd2, Ld2),
                new Vector2D(-Wd2, 0),
                new Vector2D(-(Wd2+4), -Ld2/2+6),
                new Vector2D(-Wd2+2, -Ld2),
                new Vector2D(0, -Ld2),
                new Vector2D(Wd2-2, -Ld2),
                new Vector2D(Wd2+4, -Ld2/2+6),
            };
            return vertexes;
        }
        public enum State { pen, circle,box,body,line };
        public State _State { get { return this.Get<State>("_State",State.pen); } set { StateC(value); this.Set("_State", value); } }

        private void StateC(State nwstate)
        {
            tbstate.Text = nwstate.ToString();
        }
        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    _State = State.pen;
                    break;
                case Key.D2:
                    _State = State.circle;
                    break;
                case Key.D3:
                    _State = State.box;
                    break;
                case Key.D4:
                    _State = State.body;
                    break;
                case Key.D5:
                    _State = State.line;
                    break;     
                case Key.Q:
                    _State2 = State2.deflt;
                    break;
                case Key.W:
                    _State2 = State2.wall;
                    break;                
                
            }
        }
        Polyline _Polyline;

        void RootCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            Point mps = Mouse.GetPosition(_RootCanvas);            
            switch (_State)
            {
                case State.line:                    
                case State.pen:                    
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {                        
                        if (null == _Polyline)
                        {
                            _Polyline = new Polyline() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2 }.AddTo(_RootCanvas);
                        }
                        _Polyline.Points.Add(new Point(mps.X, mps.Y));
                    }
                    if (Mouse.RightButton == MouseButtonState.Pressed && _Polyline.Points.Count > 2)
                    {
                        IEnumerable<Point> _Points = _Polyline.Points;                    
                         new Item().Load(_Points).CreateBody();
                        _Polyline.Remove();
                        _Polyline = null;


                    }
                    break;
                case State.circle:
                    {
                        Body body = BodyFactory.Instance.CreateCircleBody(engine, 5, 5);
                        body.Position = mps.ToVector2();
                        Geom geom =GeomFactory.Instance.CreateCircleGeom(engine,body, 5, 5);
                        geom.CollisionGroup = MathD.r.Next(int.MaxValue);
                        
                        Item _Item = new Item() { _startpos = mps, vertex = geom.LocalVertices, _Body = body }.Load();
                    } break;
                case State.box:
                    {
                        //Item _Item = new Item() { _startpos = mps }.Load(VertexHelper.CreateRectangle(20, 20));
                    } break;
                case State.body:
                    {
                        new Item() { _startpos = mps, vertex = new Vertices(CreateVertex()) }.Load().CreateBody();
                    } break;                             
                
            }
        }
        public State2 _State2 { get { return this.Get<State2>("_State2", State2.deflt); } set { State2C(value); this.Set("_State2", value); } }

        private void State2C(State2 value)
        {
            TextBlockstate2.Text = value.ToString();
        }
        
        public List<Item> _Items = new List<Item>();




        TimerA _TimerA = new TimerA();
        void UpdateWpf()
        {
            foreach (var a in _Items)            
                a.Update();

            engine.Update((float)_TimerA._SecodsElapsed);
            //if (Keyboard.IsKeyDown(Key.Left)) _GravityField.gravity.X -= 10;
            //if (Keyboard.IsKeyDown(Key.Right)) _GravityField.gravity.X += 10;
            //if (Keyboard.IsKeyDown(Key.Up)) _GravityField.gravity.Y -= 10;
            //if (Keyboard.IsKeyDown(Key.Down)) _GravityField.gravity.Y += 10;
            _TimerA.Update();
            
            
        }
        
    }
    
}
