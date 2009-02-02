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
using System.Windows.Forms;
using D=System.Drawing;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
namespace Resizer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            StringCollection _StringCollection = System.Windows.Clipboard.GetFileDropList();
            ////String[] _list = new string[_StringCollection.Count];
            //_StringCollection.CopyTo(_list, 0);
            string s = "";
            foreach (string _string in _StringCollection)
            {
                s += "<string>/arctic/" + System.IO.Path.GetFileName(_string) + "</string>";
            }
            System.Windows.Clipboard.SetText(s);
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
            Directory.SetCurrentDirectory(System.IO.Path.GetFullPath("../../"));
        }
        OpenFileDialog _OpenFileDialog;
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            _OpenFileDialog = new OpenFileDialog();
            _OpenFileDialog.Multiselect = true;
            if (_OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OnSuccess();                
            }

        }
        Image _Image = new Image();
        private void OnSuccess()
        {
            MouseDown += new MouseButtonEventHandler(Window1_MouseDown);
            MouseMove += new System.Windows.Input.MouseEventHandler(Window1_MouseMove);
            MouseUp += new MouseButtonEventHandler(Window1_MouseUp);
            _Canvas.Children.Add(_Image);            
            string fname = _OpenFileDialog.FileNames[0];
            _Image.Source = new BitmapImage(new Uri(fname));

            _Canvas.Children.Add(_Rectangle);
            _Rectangle.Stroke = new SolidColorBrush(Colors.Blue);
            _Rectangle.StrokeThickness = 2;
            KeyDown += new System.Windows.Input.KeyEventHandler(Window1_KeyDown);
        }

        void Window1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!_Selected)
                {
                    p1.X = 0;
                    p1.Y = 0;
                    _Width = _Image.ActualWidth;
                    _Height = _Image.ActualHeight;
                }
                foreach (string fn in _OpenFileDialog.FileNames)
                {
                    D.RectangleF r = new System.Drawing.RectangleF((float)p1.X, (float)p1.Y, (float)_Width, (float)_Height);
                    D.Bitmap _Bitmap = new D.Bitmap(fn);
                    D.Bitmap _Bitmap1 = _Bitmap.Clone(r, D.Imaging.PixelFormat.Format32bppArgb);                                        
                    for (int x = 0; x < _Bitmap1.Width; x++)
			        {
                        for (int y = 0; y < _Bitmap1.Height; y++)
                        {
                            D.Color _Color = _Bitmap1.GetPixel(x,y);
                            if (_Color.R == 255 && _Color.B == 255 && _Color.G == 0)
                            {                                
                                _Bitmap1.SetPixel(x, y, D.Color.Transparent);
                            }
                        }
			        }                    
                    _Bitmap1.Save("a" + System.IO.Path.GetFileName(fn));                    
                }                
                this.Close();
            }
        }

        bool _Selected;
        void Window1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _Selected = true;
        }

        Rectangle _Rectangle = new Rectangle();
        Point p2;
        double _Width;
        double _Height;
        void Window1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                p2= e.GetPosition(_Canvas);
                _Width = p2.X - p1.X;
                _Height = p2.Y - p1.Y;
                if (_Width > 0) _Rectangle.Width = _Width;
                if (_Height > 0) _Rectangle.Height = _Height;
            }
        }

        Point p1;
        void Window1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            p1 = e.GetPosition(_Canvas);
            Canvas.SetLeft(_Rectangle, p1.X);
            Canvas.SetTop(_Rectangle, p1.Y);
        }
    }
}
