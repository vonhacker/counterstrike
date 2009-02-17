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
using doru;
using ICSharpCode.SharpZipLib.Zip;
using ConsoleApplication1;
using System.Diagnostics;
using doru.WPF.Vectors;
using System.Windows.Threading;


namespace WpfApplication6
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class App
    {
        public static TimerA _TimerA = new TimerA();
        public static Dictionary<string, Stream> _Resources = new Dictionary<string, Stream>();
    }
    public partial class Window1 : Window
    {
        public Window1()
        {
            Logging.Setup("../../../");
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        public static string _ContentFolder = "./Content/content.zip";
        public static DataBase _DataBase;
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            FileStream fs = File.Open(_ContentFolder, FileMode.Open);
            FastZip _FastZip = new FastZip();
            Helper.OpenZip(fs, App._Resources);
            _DataBase = (DataBase)Common._XmlSerializer.Deserialize(App._Resources["db.xml"]);
            KeyDown += new KeyEventHandler(Window1_KeyDown);
            KeyUp += new KeyEventHandler(Window1_KeyUp);

            _Player = new Player() { _x = 100, _y = 100 };
            _Player.Load();
            LayoutRoot.Children.Add(_Player);
            new DispatcherTimer().StartRepeatMethod(.01, Update);
        }
        public void Update()
        {
            App._TimerA.Update();
            _Player.Update();
        }
        Player _Player;
        void Window1_KeyUp(object sender, KeyEventArgs e)
        {
            _Keys.Remove(e.Key);
        }
        public static List<Key> _Keys = new List<Key>();
        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_Keys.Contains(e.Key))
            {
                _Keys.Add(e.Key);
            }
        }



    }
    public class Player : GameObj
    {

        ContentControl _ContentControl = new ContentControl();
        public AnimatedImage _CurrentAnimation { get { return (AnimatedImage)_ContentControl.Content; } set { _ContentControl.Content = value; } }
        public static DataBase _DataBase { get { return Window1._DataBase; } }
        public override void Load()
        {

            Add(_ContentControl);
            _runup =LoadBitmap("phoenix_run");
            _runleft = LoadBitmap("Phoenix_run_left");
            _runright = LoadBitmap("Phoenix_run_right");
            _rundown = LoadBitmap("phoenix_run_back");
            _stay = LoadBitmap("Phoenix_stay");
            _die = LoadBitmap("phoenix_dead");
            _CurrentAnimation = _runup;
            base.Load();
        }

        private AnimatedImage LoadBitmap(string s)
        {
            AnimatedImage  anim = new AnimatedImage();
            anim._AnimatedBitmap = _DataBase.Get(s);
            anim.Load();
            return anim;
        }
        AnimatedImage _runleft;
        AnimatedImage _runright;
        AnimatedImage _runup;
        AnimatedImage _rundown;
        AnimatedImage _stay;
        AnimatedImage _die;
        public override void Update()
        {
            _CurrentAnimation.Update();
            UpdateKeyboard();
            _x += _S.X;
            _y += _S.Y;
            base.Update();
        }
        
        public static List<Key> _Keys { get { return Window1._Keys; } }
        private void UpdateKeyboard()
        {
            _CurrentAnimation = _stay;
            _power = new Vector();
            if (_Keys.Contains(Key.Q))
            {
                _angle -= 3;                
            }
            else if (_Keys.Contains(Key.E))
            {
                _angle += 3;                
            }                        

            if (_Keys.Contains(Key.A))
            {
                _CurrentAnimation = _runleft;
                _power += new Vector(-3, 0);
            }
            if (_Keys.Contains(Key.D))
            {
                _CurrentAnimation = _runright;
                _power += new Vector(3, 0);
            }

            if (_Keys.Contains(Key.Space))
            {
                _CurrentAnimation = _die;
            }
            if (_Keys.Contains(Key.W))
            {
                _CurrentAnimation = _runup;
                _power += new Vector(0, -5);
            }
            else if (_Keys.Contains(Key.S))
            {
                _CurrentAnimation = _rundown;
                _power += new Vector(0, 5);
            }
        }

    }
    public class AnimatedImage : Image
    {

        public DataBase.AnimatedBitmap _AnimatedBitmap;
        public string _Name { get { return _AnimatedBitmap._Name; } }
        public int _Width { get { return _AnimatedBitmap._Width; } }
        public int _Height { get { return _AnimatedBitmap._Height; } }
        public List<BitmapImage> _Bitmaps = new List<BitmapImage>();
        public int Interval = 30 / 1000;
        TranslateTransform _TranslateTransform = new TranslateTransform();
        public static Dictionary<string, Stream> _Resources { get { return App._Resources; } }
        public static TimerA _TimerA { get { return App._TimerA; } }
        public void Load()
        {
            this.RenderTransform = _TranslateTransform;

            Width = _Width;
            Height = _Height;
            _TranslateTransform.X = -Width / 2;
            _TranslateTransform.Y = -Height / 2;

            foreach (string s in _AnimatedBitmap._Bitmaps)
            {
                BitmapImage _BitmapImage = new BitmapImage();

                _Resources[s].Seek(0, SeekOrigin.Begin);
                _BitmapImage.BeginInit();
                _BitmapImage.StreamSource = _Resources[s];
                _BitmapImage.EndInit();
                _Bitmaps.Add(_BitmapImage);
            }
        }
        float _frame;

        public void Update()
        {

            _frame += 30 * (float)_TimerA._SecodsElapsed;
            if (_frame >= _Bitmaps.Count) _frame = 0;

            BitmapImage _BitmapImage = _Bitmaps[(int)_frame];
            try
            {
                Source = _BitmapImage;
            }
            catch { }
        }
    }
    public abstract class GameObj : UserControl
    {
        public Vector s;
        public Vector _S
        {
            get { return s; }
            set
            {                
                s = value;
                _RotateTransform.Angle = Calculator.VectorToRadians(s);
            }
        }
        
        public double _rad { get { return Calculator.DegreesToRadians(_angle); } }

        public Vector power;
        public Vector _power
        {
            get { return power; }
            set
            {
                s = value;
                Calculator.RotateVector(ref s, _rad);                
                power = value;
            }
        }

        public double _angle
        {
            get { return _RotateTransform.Angle; }
            set
            {
                _RotateTransform.Angle = value;
                Vector v = Calculator.RadiansToVector(_rad);
                s = new Vector(s.X * s.Length(), s.Y * s.Length());
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
}
