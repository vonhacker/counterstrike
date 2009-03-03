//using doru;
//using System.Windows;
//using System.Windows.Controls;
//using System.Collections.Specialized;
//using System.Collections;
//using System;
//using System.Windows.Media;
//using doru.Vectors;
//using System.Windows.Media.Imaging;
//using System.Collections.Generic;
//using System.IO;
//using System.Windows.Shapes;

//namespace CSLIVE.Controls
//{
//    public class BindableGrid : System.Windows.Controls.Grid
//    {
//        public INotifyCollectionChanged Source
//        {
//            get { return (INotifyCollectionChanged)GetValue(SourceProperty); }
//            set { SetValue(SourceProperty, value); }
//        }

//        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
//        public static readonly DependencyProperty SourceProperty =
//            DependencyProperty.Register("Source", typeof(INotifyCollectionChanged), typeof(Grid), new PropertyMetadata(OnSourceSet));

//        public static void OnSourceSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
            
//            BindableGrid _Grid = ((BindableGrid)d);
//            foreach (UIElement ui in (IEnumerable)_Grid.Source)
//                _Grid.Children.Add(ui);
            
//            _Grid.Source.CollectionChanged += new NotifyCollectionChangedEventHandler(_Grid.Source_CollectionChanged);
//        }

//        void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {

//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    foreach (UIElement ui in e.NewItems)
//                        Children.Add(ui);
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    foreach (UIElement ui in e.OldItems)
//                        Children.Add(ui);
//                    break;
//            }
//        }

//    }
//    public class BindableCanvas : System.Windows.Controls.Canvas
//    {

//        public INotifyCollectionChanged Source
//        {
//            get { return (INotifyCollectionChanged)GetValue(SourceProperty); }
//            set { SetValue(SourceProperty, value); }
//        }

//        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
//        public static readonly DependencyProperty SourceProperty =
//            DependencyProperty.Register("Source", typeof(INotifyCollectionChanged), typeof(Canvas), new PropertyMetadata(OnSourceSet));

//        public static void OnSourceSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            BindableCanvas _Canvas = ((BindableCanvas)d);
//            foreach (UIElement ui in (IEnumerable)_Canvas.Source)
//                _Canvas.Children.Add(ui);

//            _Canvas.Source.CollectionChanged += new NotifyCollectionChangedEventHandler(_Canvas.Source_CollectionChanged);
//        }

//        void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {

//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    foreach (object ui in e.NewItems)
//                        Children.Add(Cv(ui));
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    foreach (object ui in e.OldItems)
//                        Children.Remove(Cv(ui));
//                    break;
//            }
//        }
//        public UIElement Cv(object o)
//        {
//            if (o is IView) return ((IView)o)._View;
//            else return (UIElement)o;
//        }


//    }
//    public class MyContentControl : ContentControl
//    {
//        public object Child
//        {
//            get { return (object)GetValue(TestProperty); }
//            set { SetValue(TestProperty, value); }
//        }
//        public static readonly DependencyProperty TestProperty = DependencyProperty.Register("Child", typeof(object), typeof(MyContentControl), new PropertyMetadata(OnPropertyChangedCallback));
//        public static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            (d as MyContentControl).Content = ((IView)e.NewValue)._View;
//        }

//    }
//    public class GameObj :UserControl, IUpdate
//    {
//        #region Vars
//        TranslateTransform _TranslateTransform  = new TranslateTransform();
//        RotateTransform _RotateTransform = new RotateTransform();
//        private ScaleTransform _ScaleTransform = new ScaleTransform();
//        public TransformGroup _TransformGroup { get { return this.Get<TransformGroup>("_TransformGroup"); } set { this.Set("_TransformGroup", value); } }

//        public Vector _Position { get { return new Vector(_x, _y); } set { _x = value.X; _y = value.Y; } }
//        public Vector s;
//        public Vector _S
//        {
//            get { return s; }
//            set
//            {
                
//                s = value;
//                _RotateTransform.Angle = Calculator.VectorToRadians(s);
//            }
//        }

//        public double _rad { get { return Calculator.DegreesToRadians(_angle); } }

//        public Vector power;
//        public Vector _power
//        {
//            get { return power; }
//            set
//            {
//                s = value;
//                Calculator.RotateVector(ref s, _rad);
//                power = value;
//            }
//        }

//        public double _angle
//        {
//            get { return _RotateTransform.Angle; }
//            set
//            {
//                _RotateTransform.Angle = value;
//                Vector v = Calculator.RadiansToVector(_rad);
//                s = new Vector(s.X * s.Length(), s.Y * s.Length());
//            }
//        }

        
//        #endregion
//        public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }
        
//        public bool _right
//        {
//            get { return _ScaleTransform.ScaleX > 0; }
//            set
//            {
//                if (value == true && _ScaleTransform.ScaleX < 0 ||
//                    value == false && _ScaleTransform.ScaleX > 0) _ScaleTransform.ScaleX *= -1;
//            }
//        }
//        //public void Add(UIElement fw)
//        //{
//        //    LayoutRoot.Children.Add(fw);
//        //}
//        //public double _h { get { if (double.NaN == LayoutRoot.Height) throw new Exception(); return LayoutRoot.Height; } set { LayoutRoot.Height = value; } }
//        //public double _w { get { return LayoutRoot.Width; } set { LayoutRoot.Width = value; } }
        
        
        
//        public GameObj()
//        {            
//            _TransformGroup.Children.Add(_RotateTransform);            
//            _TransformGroup.Children.Add(_ScaleTransform);
//            _TransformGroup.Children.Add(_TranslateTransform);            
            
//        }
//        public double _x { get { return _TranslateTransform.X; } set { _TranslateTransform.X = value; } }
//        public double _y{ get { return _TranslateTransform.Y; } set { _TranslateTransform.Y = value; } }

//        public PerspCamera _PerspCamera;
//        public virtual void Load()
//        {
//            _PerspCamera = new PerspCamera { _GameObj = this };
//        }
//        public virtual void Update()
//        {

//        }
//    }
//    public class AnimatedImage : ContentControl, IUpdate
//    {

//        public GameContentDataBase.AnimatedBitmap _AnimatedBitmap;
//        public string _Name { get { return _AnimatedBitmap._Name; } }
//        public int _Width { get { return _AnimatedBitmap._Width; } }
//        public int _Height { get { return _AnimatedBitmap._Height; } }
//        public List<BitmapImage> _Bitmaps = new List<BitmapImage>();
//        public int _Interval = 30;
//        TranslateTransform _TranslateTransform = new TranslateTransform();
//        public static Dictionary<string, Stream> _Resources { get { return Game._Resources; } }
//        public static TimerA _TimerA { get { return Page._TimerA; } }
//        public AnimatedImage()
//        {
//            this.Content = new Ellipse { Width = 20, Height = 20, Fill = new SolidColorBrush(Colors.Brown) };
//        }
//        Image _Image { get { return (Content as Image); } }
//        public void Load()
//        {
//            //this.Content = new Image() { Width = 200, Height = 200 };// { Width = 20, Height = 20, Fill = new SolidColorBrush(Colors.Black) };
//            Image _Image = new Image();
//            (this.Content as Ellipse).Fill = new SolidColorBrush(Colors.Black) ;
//            UpdateLayout();
//            _Image.Width = _Width;
//            _Image.Height = _Height;
//            this.RenderTransform = _TranslateTransform;

//            Width = _Width;
//            Height = _Height;
//            _TranslateTransform.X = -Width / 2;
//            _TranslateTransform.Y = -Height / 2;

//            foreach (string s in _AnimatedBitmap._Bitmaps)
//            {
//                BitmapImage _BitmapImage = new BitmapImage();

//                _Resources[s].Seek(0, SeekOrigin.Begin);
//                _BitmapImage.SetSource(_Resources[s]);
//                _Bitmaps.Add(_BitmapImage);
//            }
//        }
//        float _frame;
//        public void Reset()
//        {
//            _frame = 0;
//        }
//        DateTime _DateTime = DateTime.Now;
//        public void Update()
//        {
//            if (DateTime.Now - _DateTime > TimeSpan.FromMilliseconds(500)) Reset();
//            _DateTime = DateTime.Now;
//            _frame += _Interval * (float)_TimerA._SecodsElapsed;
//            if (_frame >= _Bitmaps.Count) _frame = 0;

//            BitmapImage _BitmapImage = _Bitmaps[(int)_frame];
//            try
//            {
//                (Content as Image).Source = _BitmapImage;
//            } catch { }
//        }
//    }
//    //public class MyPropertyMetadata : PropertyMetadata
//    //{
//    //    public MyPropertyMetadata(Action a)
//    //        : base(new PropertyChangedCallback(delegate(DependencyObject d, DependencyPropertyChangedEventArgs e) { a(); }))
//    //    {

//    //    }
//    //    public static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    //    {

//    //    }
//    //}
//}
