using doru;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Collections;
using System;
using System.Windows.Media;
using doru.Vectors;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Windows.Shapes;

namespace CSLIVE.Game.Controls
{    
    public class GameObj : UserControl, IUpdate
    {
        #region Vars
        TranslateTransform _TranslateTransform = new TranslateTransform();
        RotateTransform _RotateTransform = new RotateTransform();
        private ScaleTransform _ScaleTransform = new ScaleTransform();
        public TransformGroup _TransformGroup { get { return (TransformGroup)RenderTransform; } set { RenderTransform = value; } }

        public Vector _Position { get { return new Vector(_x, _y); } set { _x = value.X; _y = value.Y; } }
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

        public double _rad { get { return Calculator.DegreesToRadians(_angle); } set { _angle = value / Calculator.RadiansToDegreesRatio; } }

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


        #endregion
        public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }

        public bool _right
        {
            get { return _ScaleTransform.ScaleX > 0; }
            set
            {
                if (value == true && _ScaleTransform.ScaleX < 0 ||
                    value == false && _ScaleTransform.ScaleX > 0) _ScaleTransform.ScaleX *= -1;
            }
        }
        //public void Add(UIElement fw)
        //{
        //    LayoutRoot.Children.Add(fw);
        //}
        //public double _h { get { if (double.NaN == LayoutRoot.Height) throw new Exception(); return LayoutRoot.Height; } set { LayoutRoot.Height = value; } }
        //public double _w { get { return LayoutRoot.Width; } set { LayoutRoot.Width = value; } }



        public GameObj()
        {
            _TransformGroup = new TransformGroup();
            _TransformGroup.Children.Add(_RotateTransform);
            _TransformGroup.Children.Add(_ScaleTransform);
            _TransformGroup.Children.Add(_TranslateTransform);
        }
        public double _x { get { return _TranslateTransform.X; } set { _TranslateTransform.X = value; } }
        public double _y { get { return _TranslateTransform.Y; } set { _TranslateTransform.Y = value; } }

        public PerspCamera _PerspCamera;
        public virtual void Load()
        {
            _PerspCamera = new PerspCamera { _GameObj = this };
        }
        public virtual void Update()
        {

        }
    }
    public class AnimatedImage : ContentControl, IUpdate
    {

        public GameContentDataBase.AnimatedBitmap _AnimatedBitmap;
        public string _Name { get { return _AnimatedBitmap._Name; } }
        public int _Width { get { return _AnimatedBitmap._Width; } }
        public int _Height { get { return _AnimatedBitmap._Height; } }
        public List<BitmapImage> _Bitmaps = new List<BitmapImage>();
        public int _Interval = 30;
        TranslateTransform _TranslateTransform = new TranslateTransform();
        public static Dictionary<string, Stream> _Resources { get { return Game._Resources; } }
        public static TimerA _TimerA { get { return Page._TimerA; } }
        public AnimatedImage()
        {
            this.Content = new Image();
        }
        Image _Image { get { return (Content as Image); } }
        public void Load()
        {                                    
            UpdateLayout();
            _Image.Width = _Width;
            _Image.Height = _Height;
            this.RenderTransform = _TranslateTransform;

            Width = _Width;
            Height = _Height;
            _TranslateTransform.X = -Width / 2;
            _TranslateTransform.Y = -Height / 2;

            foreach (string s in _AnimatedBitmap._Bitmaps)
            {
                BitmapImage _BitmapImage = new BitmapImage();

                _Resources[s].Seek(0, SeekOrigin.Begin);
                _BitmapImage.SetSource(_Resources[s]);
                _Bitmaps.Add(_BitmapImage);
            }
        }
        float _frame;
        public void Reset()
        {
            _frame = 0;
        }
        DateTime _DateTime = DateTime.Now;
        public void Update()
        {
            if (DateTime.Now - _DateTime > TimeSpan.FromMilliseconds(500)) Reset();
            _DateTime = DateTime.Now;
            _frame += _Interval * (float)_TimerA._SecodsElapsed;
            if (_frame >= _Bitmaps.Count) _frame = 0;

            BitmapImage _BitmapImage = _Bitmaps[(int)_frame];
            //try
            //{
                (Content as Image).Source = _BitmapImage;
            //} catch { }
        }
    }    
}
