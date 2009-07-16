using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Mathematics;
using doru;
using CounterStrikeLive.Service;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using doru.Tcp;

namespace CounterStrikeLive
{
    public static class Extensions
    {

        public static void Send(this Sender _Sender, PacketType _PacketType, params object[] data)
        {
            _Sender.Send(Helper.JoinBytes((byte)_PacketType, Helper.JoinBytes(data)));
        }
        public static void Send(this Sender _Sender, byte[] data )
        {
            _Sender.Send(data);
        }
        public static void Send(this Sender _Sender, PacketType _PacketType)
        {
            _Sender.Send(new byte[] { (byte)_PacketType });
        }
        public static void Send(this Sender _Sender, PacketType _PacketType, byte[] data)
        {
            _Sender.Send(Helper.JoinBytes((byte)_PacketType, data));
        }

    }
    public static class Random
    {


        static System.Random _Random = new System.Random();
        public static int Next(int p)
        {
            return _Random.Next(p);
        }

        internal static int Next(int p, int _dist)
        {
            return (int)Math.Round(p + _Random.NextDouble() * (_dist - p));
        }
        internal static float Next(float p, float _dist)
        {
            return (float)(p + _Random.NextDouble() * (_dist - p));
        }
    }
    public class Button : System.Windows.Controls.Button
    {
        Menu _Menu = Menu._This;
        protected override void OnClick()
        {
            PlaySound("buttonclick.mp3");
            base.OnClick();
        }

        private void PlaySound(string s)
        {
            MediaElement _MediaElement = new MediaElement();
            _Menu._GameCanvas.Children.Add(_MediaElement);
            _MediaElement.SetSource(s);            
            _MediaElement.MediaEnded += delegate { _Menu._GameCanvas.Children.Remove(_MediaElement); };
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            PlaySound("buttonrollover.mp3");
            base.OnMouseEnter(e);
        }
    }
    public class ChildWindow : System.Windows.Controls.ChildWindow
    {
        public static ChildWindow _This;
        public ChildWindow()
        {
            if (_This != null)
                _This.Close();

            if (Menu.isnotBlend) this.Show();
            _This = this;
            KeyDown += new KeyEventHandler(ChildWindow_KeyDown);
            Menu._Keyboard.Clear();
        }

        void ChildWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }
        public Action Success;
        protected override void OnClosed(EventArgs e)
        {
            if (_This == null) return;
            _This = null;
            if (DialogResult == true && Success!= null) Success();
            base.OnClosed(e);
        }
        
    }
    public class MyObs<T> : IEnumerable<T>, INotifyCollectionChanged
    {
        ObservableCollection<T> _List = new ObservableCollection<T>();
        T[] a;
        public MyObs(int count)
        {
            _List.CollectionChanged += new NotifyCollectionChangedEventHandler(List_CollectionChanged);
            a = new T[count];
        }


        void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(sender, e);
        }
        public T this[int i]
        {
            get { return a[i]; }
            set
            {
                T oldValue = a[i];
                a[i] = value;

                if (oldValue == null)
                {
                    _List.Add(value);
                } else
                {
                    _List.Remove(oldValue);
                    if (value != null) _List.Add(value);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return _List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _List.GetEnumerator();
        }
    }
    public interface ClientListItem
    {
        Player.Team _Team { get; }
        float _ping { get; }
        int _Deaths { get; }
        int _Points { get; }
        int? _id { get; }
        string _Nick { get; }
    }
    public class Cursor
    {
        public float _x;
        public float _y;
        public Menu _Menu;
        public Canvas _Canvas = new Canvas();
        public void Load()
        {

            MakeLine(0);
            MakeLine(90);
            MakeLine(180);
            MakeLine(270);
            _Menu._CursorCanvas.Children.Add(_Canvas);
        }
        protected void MakeLine(int angle)
        {
            Line _Line = new Line();
            _Line.X1 = 5;
            _Line.X2 = 10;
            _Line.Y1 = 0;
            _Line.Y2 = 0;
            _Line.StrokeThickness = 3;
            _Line.Stroke = new SolidColorBrush(Colors.Red);
            RotateTransform _RotateTransform = new RotateTransform();
            _RotateTransform.Angle = angle;
            _Line.RenderTransform = _RotateTransform;
            _Canvas.Children.Add(_Line);
            _Canvas.RenderTransform = _ScaleTransform;
        }
        public float _Scale = 1;
        public float Scale { get { return _Scale - 1; } }
        public void Update()
        {
            _Scale -= (float)Menu._TimerA._SecodsElapsed * 3;
            if (_Scale < 1) _Scale = 1;
            _ScaleTransform.ScaleX = _Scale;
            _ScaleTransform.ScaleY = _Scale;
            Canvas.SetLeft(_Canvas, _x);
            Canvas.SetTop(_Canvas, _y);
        }
        ScaleTransform _ScaleTransform = new ScaleTransform();
    }
}
