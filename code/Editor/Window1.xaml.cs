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
using System.IO;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Editor
{
    
    public partial class Window1 : Window
    {
        public static Window1 _This;
        public Window1()
        {
            _This = this;
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        Polyline _Polyline;
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            Logging.Setup();
            _Root.MouseDown += new MouseButtonEventHandler(Root_MouseDown);
            this.KeyDown += new KeyEventHandler(Root_KeyDown);
            Dispatcher.StartUpdate(Update);
            Load();
        }

        void Root_KeyDown(object sender, KeyEventArgs e)
        {
            this.Trace(e.Key);
            switch (e.Key)
            {
                case Key.D1:
                    _State = State.draw;
                    break;
                case Key.D2:
                    _State = State.select;
                    break;
            }
        }
        public State _State { get { return this.Get<State>("_State", State.draw); } set { StateChanged(value); this.Set("_State", value); } }
        
        private new void StateChanged(State value)
        {            
            SetCursor(value);
            switch (value)
            {
                case State.draw:
                    
                    break;
                case State.select:
                    
                    break;
            }
        }
        void SetCursor(State s)
        {
            Cursor = new Cursor(Environment.CurrentDirectory + "/res/" + s + ".cur");
        }
        public enum State { select, draw };
        public Canvas _RootCanvas { get { return _Root; } }
        void Root_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mps = Mouse.GetPosition(_RootCanvas);
            HitTestResult _HitTestResult = VisualTreeHelper.HitTest(_PolygonsCanvas, mps);


            if (_State == State.draw)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (null == _Polyline)
                    {
                        _Polyline = new Polyline() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2 }.AddTo(_RootCanvas);
                    }
                    _Polyline.Points.Add(new Point(mps.X, mps.Y));
                }
                if (Mouse.RightButton == MouseButtonState.Pressed && _Polyline!=null  && _Polyline.Points.Count > 2)
                {
                    IEnumerable<Point> _Points = _Polyline.Points;
                    CreatePolygon(_Points);
                    _Polyline = null;
                }
            }

            if (_State == State.select && e.LeftButton == MouseButtonState.Pressed)
            {
                if (_Polygon != null) _Polygon._Selected = false;


                if (_HitTestResult != null)
                {
                    _Polygon = (Polygon)(_HitTestResult.VisualHit as System.Windows.Shapes.Polygon).Tag;
                    _Polygon._Selected = true;
                } else _Polygon = null;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_HitTestResult != null) ((Polygon)(_HitTestResult.VisualHit as System.Windows.Shapes.Polygon).Tag).Remove();
            }
        }
        public Polygon _Polygon { get { return this.Get<Polygon>("_Polygon"); } set { SelectionChanged(value); this.Set("_Polygon", value); } }

        private void SelectionChanged(Polygon value)
        {
            _WallMenuItem.IsEnabled = value != null;
        }
        private void CreatePolygon(IEnumerable<Point> _Points)
        {
            if (_Points.Count() <= 2) return;
            new Polygon() { Points = new PointCollection(_Points) }.AddTo(_PolygonsCanvas);
            
        }
       
        IEnumerable<Polygon> _Polygons { get { foreach (Polygon p in _PolygonsCanvas.Children) yield return p; } }
        public Database _Db { get { return Database._This; } set { Database._This = value; } }
        void Save()
        {
            _Db._Map._Polygons.Clear();
            foreach (Polygon pp in _Polygons)
            {
                H.Assert(pp.Points.Count > 2);
                Database.Polygon dp = pp._base;                
                dp._Points.Clear();
                dp._Points = pp.Points.Select(a => new Vector2((float)a.X, (float)a.Y)).ToList();
                _Db._Map._Polygons.Add(dp);
            }

            SaveXml();
            
        }
        void Load()
        {
            LoadXml();
            foreach (Database.Polygon p in _Db._Map._Polygons)
            {
                if (p._Points.Count <= 2) continue;
                new Polygon() { Points = new PointCollection(p._Points.ToPoints()), _base = p }.AddTo(_PolygonsCanvas);
            }
        }
        void SaveXml()
        {
            Database._XmlSerializer.Serialize("db.xml", _Db);
        }
        void LoadXml()
        {
            
            Database._XmlSerializer.DeserealizeOrCreate("db.xml", new Database());
        }

        void Update()
        { 
            
        }

        

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Wall_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_Polygon != null)
            _Polygon._wall = false;
        }

        private void Wall_Checked(object sender, RoutedEventArgs e)
        {
            if(_Polygon!=null)
            _Polygon._wall = true;
        }
    }
}
