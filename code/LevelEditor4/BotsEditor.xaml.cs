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
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls.Primitives;

namespace LevelEditor4
{
	
	public partial class BotsEditor : Window
	{
        public static BotsEditor _This;
        public List<TreePoint> _TreePoints { get { return _Botbase._TreePoints; } }
        public Thumb _Selected { get { return this.Get<Thumb>("_CurThumb"); } set { SelectionChanged(value); this.Set("_CurThumb", value); } }
        public double _Thickness { get { return SliderThickness.Value * 20; } }
        public Botbase _Botbase;
        public void SelectionChanged(Thumb nwtree)
        {
            if (_Selected != null) _Selected.Background = new SolidColorBrush(Colors.Black);
            if (nwtree != null) nwtree.Background = new SolidColorBrush(Colors.Blue);
        }
        public double _x { get { return _RootCanvas.GetX(); } set { _RootCanvas.SetX(value); } }
        public double _y { get { return _RootCanvas.GetY(); } set { _RootCanvas.SetY(value); } }
        public State _State { get { return this.Get<State>("_State"); } set { this.Set("_State", value); StateChanged(); } }
        public BotsEditor()
		{
            
            _This = this;
			InitializeComponent();
			Loaded += new RoutedEventHandler(BotsEditor_Loaded);
		}

		void BotsEditor_Loaded(object sender, RoutedEventArgs e)
		{            
            Logging.Setup();
            _x = _y = 0;
            _State = State.arrow_l;
			LoadData();
			LoadDb();
            LoadPath();
            
			Dispatcher.StartUpdate(Update);
            MouseDown += new MouseButtonEventHandler(BotsEditor_MouseDown);            
            KeyDown += new KeyEventHandler(BotsEditor_KeyDown);
		}
        
        new void StateChanged()
        {
            Cursor = new Cursor(Environment.CurrentDirectory + "/Res/" + _State + ".cur");            
        }
        
        void BotsEditor_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    _State = State.arrow_l;
                    break;                
                case Key.D2:
                    _State = State.pen_i;
                    break;
                case Key.S:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) SaveData();
                    break;

            }
        }
        private void SaveData()
        {
            MemoryStream ms = new Serializer().Serialize(_Botbase);
            Botbase nwbtbs = (Botbase)new Deserializer().Deserialize(ms);
            FileSerializer.Serialize("bots.raw", _Botbase);
        }
     
        private void LoadPath()
        {
            foreach (TreePoint item in _Botbase._TreePoints)            
                AddNewThumb(item);            
            
        }

        private Thumb AddNewThumb(TreePoint item)
        {
            
            Thumb _Thumb = _CanvasThumbs.Add(new Thumb { Width = 10, Height = 10, Background = new SolidColorBrush(Colors.Black) }.SetX(item._x).SetY(item._y).Center());
            _Thumb.Set("Point", item);
            _Thumb.DragDelta += new DragDeltaEventHandler(Thumb_DragDelta);
            _Thumb.PreviewMouseDown += new MouseButtonEventHandler(Thumb_MouseDown);
            return _Thumb;
        }
        void Thumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Thumb _Thumb = (Thumb)sender;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {

                if (_State == State.pen_i && _Selected != null)
                    _Selected.Get<TreePoint>("Point")._Way.Add(_Thumb.Get<TreePoint>("Point"));

                if (_State == State.arrow_l)
                    _Selected = _Thumb;
            }
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                TreePoint p2 =_Thumb.Get<TreePoint>("Point");
                foreach (TreePoint p in _TreePoints)
                    p._Way.Remove(p2);
                _TreePoints.Remove(p2);
                _CanvasThumbs.Children.Remove(_Thumb);
            }
        }

        void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb _Thumb = (Thumb)sender;
            if (_State == State.arrow_l)
            {
                Point pos = Mouse.GetPosition(_CanvasDraw);
                _Thumb.SetX(pos.X).SetY(pos.Y);                
                _Thumb.Get<TreePoint>("Point")._x = pos.X;
                _Thumb.Get<TreePoint>("Point")._y = pos.Y;
            }
        }
        
      

        
        void BotsEditor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(_RootCanvas);
    
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _Selected = null;
            }
            if (_State == State.pen_i)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    TreePoint _TreePoint = new TreePoint { _x = p.X, _y = p.Y, _Thickness = _Thickness };
                    if (_Selected != null)
                        _Selected.Get<TreePoint>("Point")._Way.Add(_TreePoint);
                    
                    _TreePoints.Add(_TreePoint);
                    _Selected = AddNewThumb(_TreePoint);
                }  
            }
            
        }
        
		public void Update()
		{
            
            _CanvasDraw.Children.Clear();
            foreach (TreePoint tp in _TreePoints)
                foreach(TreePoint tp2 in tp._Way)
                    _CanvasDraw.Add(new Line { X1 = tp._x, X2 = tp2._x, Y1 = tp._y, Y2 = tp2._y, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = tp._Thickness, StrokeEndLineCap = PenLineCap.Round, StrokeStartLineCap = PenLineCap.Round });
            foreach (TreePoint tp in _Botbase._TStartPos)
                _CanvasDraw.Add(new Ellipse { Width = 50, Height = 50, Fill = new SolidColorBrush(Colors.Red) }.Center().SetX(tp._x).SetY(tp._y));
            foreach (TreePoint tp in _Botbase._CStartPos)
                _CanvasDraw.Add(new Ellipse { Width = 50, Height = 50, Fill = new SolidColorBrush(Colors.Blue) }.Center().SetX(tp._x).SetY(tp._y));
            int speed = 100;
            if (Keyboard.IsKeyDown(Key.Left)) _x += speed;
            if (Keyboard.IsKeyDown(Key.Right)) _x -= speed;
            if (Keyboard.IsKeyDown(Key.Up)) _y += speed;
            if (Keyboard.IsKeyDown(Key.Down)) _y -= speed;
            _TextBlock.Text = _x + " " + _y;

		}
		private void LoadDb()
		{
			foreach (MapDatabase.Layer l in _MapDatabase._Layers)
				foreach (MapDatabase.Image img in l._Images)
				{                    
					Image _Image = new Image { Width = img.Width, Height = img.Height};
					_Image.SetPos(img.X, img.Y);
					_Image.SetSource(img.Path);
					_Canvas0.Children.Add(_Image);
				}

		}
		MapDatabase _MapDatabase;
		private void LoadData()
		{
			_MapDatabase = MapDatabase._XmlSerializer.DeserealizeOrCreate("Map.xml", new MapDatabase());
            _Botbase = FileSerializer.DeserualizeOrCreate("bots.raw", new Botbase());
		}

        private void SliderThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_Selected != null) _SelectedTreePoint._Thickness = _Thickness;
                
        }
        TreePoint _SelectedTreePoint { get { return _Selected.Get<TreePoint>("Point"); } set { _Selected.Set("Point", value); } }

        

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void MenuItemSelect_Click(object sender, RoutedEventArgs e)
        {
            _State = State.arrow_l;
        }

        private void MenuItemBrush_Click(object sender, RoutedEventArgs e)
        {
            _State = State.pen_i;
        }

        
        private void MenuItemCtp_Click(object sender, RoutedEventArgs e)
        {
            if (_Selected != null)
                _Botbase._CStartPos.Add(_SelectedTreePoint);
        }

        private void MenuItemTp_Click(object sender, RoutedEventArgs e)
        {
            if (_Selected != null)
                _Botbase._TStartPos.Add(_SelectedTreePoint);
        }

        
	}
}
