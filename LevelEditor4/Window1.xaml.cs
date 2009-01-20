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
using LevelEditor4.Properties;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows.Ink;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Markup;
using doru;
namespace LevelEditor4
{
    /// <summary>
    /// Interaction logic for Window1.xaml              
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {            
            Spammer3.Setup(System.IO.Path.GetDirectoryName(Settings.Default.FilePath)+"../../"); 
            InitializeComponent();
            Loaded += new RoutedEventHandler(WindowLoaded);
        }
        XmlSerializer _XmlSerializer;
        double _Scale = 1;

        public void SelectCanvas(int i)
        {
            InkCanvas _oldInkCanvas = _InkCanvas;
            _InkCanvas = (InkCanvas)_CanvasList.Children[i];

            if (_oldInkCanvas != null)
            {

                foreach (Image _UIElement in _oldInkCanvas.GetSelectedElements().OfType<Image>())
                {
                    _oldInkCanvas.Children.Remove(_UIElement);
                    _InkCanvas.Children.Add(_UIElement);
                }
                foreach (Stroke _Stroke in _oldInkCanvas.GetSelectedStrokes())
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift)) _InkCanvas.Strokes.Add(_Stroke.Clone());
                    if (Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        _oldInkCanvas.Strokes.Remove(_Stroke);
                        _InkCanvas.Strokes.Add(_Stroke);
                    }
                }
            }
            foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
            {
                _InkCanvas1.Opacity = .2;
                _InkCanvas1.IsHitTestVisible = false;
            }

            _InkCanvas.IsHitTestVisible = true;
            _InkCanvas.Opacity = 1;
            //SetMode(InkCanvasEditingMode.Select, CustomMode.select);
            _InkCanvas.Select(new StrokeCollection());
            if (null != _oldInkCanvas)
                _oldInkCanvas.Select(new StrokeCollection());

        }
        Key oldkey;
        void InkCanvasKeyDown(object sender, KeyEventArgs e)
        {
            oldkey = e.Key;
            for (int i = 0; i < 5; i++)
                if (Keyboard.IsKeyDown(Key.D1 + i))
                    SelectCanvas(i);
            //D = (int)e.Key - 83;
            //if (D >= 0 && D <= 5)
            //    SelectCanvas(D);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                if (_Stroke != null)
                {
                    if (_curentPoint > 0)
                    {
                        _Stroke.StylusPoints.RemoveAt(_curentPoint);
                        _curentPoint--;
                    }
                    else
                    {
                        _InkCanvas.Strokes.Remove(_Stroke);
                        _Stroke = null;
                    }
                }
            }

            if (e.Key == Key.Q)
            {
                SetMode(InkCanvasEditingMode.Select, CustomMode.select);
            }
            if (e.Key == Key.W)
            {
                SetMode(InkCanvasEditingMode.None, CustomMode.polygon);
            }
            if (e.Key == Key.E)
            {
                SetMode(InkCanvasEditingMode.EraseByPoint, CustomMode.erase);
            }

            if (e.Key == Key.Add || e.Key == Key.Subtract)
            {
                double _ScaleFactor = e.Key == Key.Add ? 1.2 : .8;
                _Scale *= _ScaleFactor;
                _ScaleText.Text = _Scale.ToString();
                foreach (InkCanvas _InkCanvas in _CanvasList.Children)
                {
                    foreach (Stroke _Stroke in _InkCanvas.Strokes)
                        for (int i = 0; i < _Stroke.StylusPoints.Count; i++)
                        {
                            StylusPoint _StylusPoint = _Stroke.StylusPoints[i];
                            _Stroke.StylusPoints[i] = new StylusPoint(_StylusPoint.X * _ScaleFactor, _StylusPoint.Y * _ScaleFactor);
                        }
                    foreach (FrameworkElement _Image in _InkCanvas.Children)
                    {
                        InkCanvas.SetLeft(_Image, InkCanvas.GetLeft(_Image) * _ScaleFactor);
                        InkCanvas.SetTop(_Image, InkCanvas.GetTop(_Image) * _ScaleFactor);
                        _Image.Width = _Image.ActualWidth * _ScaleFactor;
                        _Image.Height = _Image.ActualHeight * _ScaleFactor;
                    }
                }
            }
            if (e.Key == Key.C)
            {
                SelectColor();
            }
            if (e.Key == Key.PageUp)
            {
                StrokeCollection _StrokeCollection = _InkCanvas.GetSelectedStrokes();
                foreach (Stroke _Stroke in _StrokeCollection)
                {
                    _InkCanvas.Strokes.Remove(_Stroke);
                    _InkCanvas.Strokes.Add(_Stroke);
                }
            }
            if (e.Key == Key.PageDown)
            {
                StrokeCollection _StrokeCollection = _InkCanvas.GetSelectedStrokes();
                foreach (Stroke _Stroke in _StrokeCollection)
                {
                    _InkCanvas.Strokes.Remove(_Stroke);
                    _InkCanvas.Strokes.Insert(0, _Stroke);
                }
            }
            if (Keyboard.IsKeyDown(Key.C) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _InkCanvas.CopySelection();
            }
            //if (Keyboard.IsKeyDown(Key.X) && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    _InkCanvas.CutSelection();
            //}
            if (Keyboard.IsKeyDown(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _InkCanvas.Paste();
            }
            if (e.Key == Key.B)
            {
                if (_PolygonsCanvas.Children.Count > 0)
                    _PolygonsCanvas.Children.Clear();
                else
                    foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
                        foreach (Stroke _Stroke in _InkCanvas1.Strokes)
                        {
                            if (_Stroke.StylusPoints.Last() == _Stroke.StylusPoints.First())
                            {
                                Polygon _Polygon = new Polygon();
                                foreach (StylusPoint _Point in _Stroke.StylusPoints)
                                {
                                    _Polygon.Points.Add(new Point(_Point.X, _Point.Y));
                                }
                                _Polygon.Fill = new SolidColorBrush(_Stroke.DrawingAttributes.Color);
                                _PolygonsCanvas.Children.Add(_Polygon);
                            }
                        }
            }

        }
        public enum CustomMode
        {
            select, ink, polygon, erase
        }
        public CustomMode _CurCustomMode;

        private void SetMode(InkCanvasEditingMode _mode, CustomMode _CustomMode)
        {
            SetStroke();
            _InkCanvas.EditingMode = _mode;
            _CurCustomMode = _CustomMode;
        }
        CustomStroke _Stroke;

        private void SetStroke()
        {
            if (_Stroke != null)
            {
                InkCanvas_StrokeCollected(_InkCanvas, new InkCanvasStrokeCollectedEventArgs(_Stroke));
                _Stroke = null;
            }
        }
        public double Distance(StylusPoint a, StylusPoint b)
        {
            return Distance(Convert(a), Convert(b));
        }

        public double Distance(Point a, Point b)
        {
            Vector c = Point.Subtract(a, b);
            return c.Length;
        }

        public static Point Convert(StylusPoint v)
        {
            return new Point(v.X, v.Y);
        }
        void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            double dist = Distance(e.Stroke.StylusPoints.First(), e.Stroke.StylusPoints.Last());
            if (dist < 5)
            {
                e.Stroke.StylusPoints[e.Stroke.StylusPoints.Count - 1] = e.Stroke.StylusPoints.First();
            }
        }

        private List<MapDatabase.Layer> SaveLayers()
        {
            List<MapDatabase.Layer> _Layers = new List<MapDatabase.Layer>();
            foreach (InkCanvas _InkCanvas in _CanvasList.Children)
            {
                MapDatabase.Layer _Layer = new MapDatabase.Layer();
                foreach (Image _Image in _InkCanvas.Children.OfType<Image>())
                {
                    _Layer._Images.Add(new MapDatabase.Image
                    {
                        Path = ((BitmapImage)_Image.Source).UriSource.OriginalString,
                        X = InkCanvas.GetLeft(_Image),
                        Y = InkCanvas.GetTop(_Image),
                        Width = _Image.ActualWidth,
                        Height = _Image.ActualHeight
                    });
                }

                List<MapDatabase.Polygon> _Polygons = _Layer._Polygons;
                foreach (CustomStroke _Stroke in _InkCanvas.Strokes)
                {
                    List<Point> _Points = new List<Point>();
                    foreach (StylusPoint _StylusPoint in _Stroke.StylusPoints)
                    {
                        Point _Point = new Point();
                        _Point.X = (int)_StylusPoint.X;
                        _Point.Y = (int)_StylusPoint.Y;
                        _Points.Add(_Point);
                    }
                    MapDatabase.Polygon _Polygon = new MapDatabase.Polygon();
                    _Polygon._Color = _Stroke.DrawingAttributes.Color;
                    _Polygon._Points = _Points;
                    _Polygons.Add(_Polygon);
                }
                _Layers.Add(_Layer);
            }
            return _Layers;
        }

        
        public void SaveFile(object sender, RoutedEventArgs e)
        {
            _MapDatabase._Layers = SaveLayers();

            _MapDatabase._CStartPos.X = InkCanvas.GetLeft(_CStartPos);
            _MapDatabase._CStartPos.Y = InkCanvas.GetTop(_CStartPos);
            _MapDatabase._TStartPos.X = InkCanvas.GetLeft(_TStartPos);
            _MapDatabase._TStartPos.Y = InkCanvas.GetTop(_TStartPos);

            MemoryStream _MemoryStream = new MemoryStream();
            _XmlSerializer.Serialize(_MemoryStream, _MapDatabase);

            byte[] _Buffer = _MemoryStream.ToArray();
            File.WriteAllBytes(_FilePath, _Buffer);
        }
        public string _FilePath = System.IO.Path.GetFileName(Settings.Default.FilePath);
        public const string _Filter = "map (*.xml)|*.xml";

        void _InkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point _pos = e.GetPosition(_InkCanvas);
            if (e.RightButton == MouseButtonState.Pressed && _Stroke != null)
            {
                _Stroke.StylusPoints[_curentPoint] = new StylusPoint(_pos.X, _pos.Y);
            }
            
            String text = ((int)_pos.X) + ":" + ((int)_pos.Y);
            _TextBlock.Text = text;
        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            

        }

        InkCanvas _InkCanvas;
        void WindowLoaded(object sender, RoutedEventArgs e)
        {

            SelectCanvas(0);

            _XmlSerializer = new XmlSerializer(typeof(MapDatabase));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, this.SaveFile));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, this.OpenFile));
            _InkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(InkCanvas_StrokeCollected);
            KeyDown += new KeyEventHandler(InkCanvasKeyDown);
            KeyUp += new KeyEventHandler(Window1_KeyUp);
            _CanvasList.MouseDown += new MouseButtonEventHandler(InkCanvas_MouseDown);
            _CanvasList.MouseMove += new MouseEventHandler(_InkCanvas_MouseMove);
            foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
            {
                _InkCanvas1.SelectionChanged += new EventHandler(InkCanvas1SelectionChanged);
            }

            OpenFile(Settings.Default.FilePath);
            SetMode(InkCanvasEditingMode.Select, CustomMode.select);
        }
        StrokeCollection _oldStrokeCollection;
        void InkCanvas1SelectionChanged(object sender, EventArgs e)
        {
            InkCanvas _InkCanvas = (InkCanvas)sender;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (_oldStrokeCollection != null)
                {
                    foreach (Stroke _Stroke in _InkCanvas.GetSelectedStrokes())
                        if (!_oldStrokeCollection.Contains(_Stroke))
                            _oldStrokeCollection.Add(_Stroke);
                    _InkCanvas.Select(_oldStrokeCollection);
                }
            }
            _oldStrokeCollection = _InkCanvas.GetSelectedStrokes();

        }

        void Window1_KeyUp(object sender, KeyEventArgs e)
        {
        }


        void SelectColor()
        {
            StrokeCollection _StrokeCollection = _InkCanvas.GetSelectedStrokes();
            if (_StrokeCollection.Count > 0)
            {
                System.Windows.Forms.ColorDialog _ColorDialog = new System.Windows.Forms.ColorDialog();
                if (_ColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    foreach (Stroke _Stroke in _StrokeCollection)
                    {

                        Color _Color = Color.FromArgb(255, _ColorDialog.Color.R, _ColorDialog.Color.G, _ColorDialog.Color.B);

                        _Stroke.DrawingAttributes.Color = _Color;
                        //StreamGeometry _Geometry = _Stroke.GetGeometry();

                    }

            }
        }
        void MediaPanel_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            foreach (string fileName in fileNames)
            {
                if (Regex.IsMatch(System.IO.Path.GetExtension(fileName), "jpg|png|bmp|gif", RegexOptions.IgnoreCase))
                    e.Effects = DragDropEffects.Copy;
            }

            // Mark the event as handled, so control's native DragOver handler is not called.
            e.Handled = true;
        }
        public static string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

        void MediaPanel_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            foreach (string fileName in fileNames)
            {
                string _Ext = System.IO.Path.GetExtension(fileName);

                // Handles image files
                if (Regex.IsMatch(_Ext, "jpg|png|bmp|gif", RegexOptions.IgnoreCase))
                {
                    Image _Image = new Image();
                    _Image.Stretch = Stretch.Fill;

                    BitmapImage _BitmapImage = new BitmapImage(new Uri(RelativePath(Environment.CurrentDirectory, fileName), UriKind.Relative));
                    double a=_BitmapImage.Width;
                    _Image.Source = _BitmapImage;
                    //Debugger.Break();
                    Point _Mouse = Mouse.GetPosition(_InkCanvas);
                    InkCanvas.SetTop(_Image, _Mouse.X);
                    InkCanvas.SetLeft(_Image, _Mouse.Y);
                    _InkCanvas.Children.Add(_Image);
                }
                //TODO: handle video files
            }

            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        public int _curentPoint;        

        void InkCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point _pos = e.GetPosition(_InkCanvas);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                SetStroke();
                if (_CurCustomMode == CustomMode.polygon)
                {
                    foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
                        foreach (CustomStroke _Stroke1 in _InkCanvas1.Strokes)
                            for (int i = 0; i < _Stroke1.StylusPoints.Count; i++)
                            {
                                StylusPoint _StylusPoint = _Stroke1.StylusPoints[i];
                                double l = ((Vector)(Convert(_StylusPoint) - _pos)).Length;
                                if (l < 10)
                                {
                                    _curentPoint = i;
                                    _Stroke = _Stroke1;
                                }
                            }
                }                
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_CurCustomMode == CustomMode.polygon)
                {
                    
                    //if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
                            foreach (CustomStroke _Stroke1 in _InkCanvas1.Strokes)
                                for (int i = 0; i < _Stroke1.StylusPoints.Count; i++)
                                {
                                    StylusPoint _StylusPoint = _Stroke1.StylusPoints[i];
                                    double l = ((Vector)(Convert(_StylusPoint) - _pos)).Length;
                                    if (l < 5)
                                    {
                                        _pos = Convert(_StylusPoint);
                                    }
                                }
                    }

                    if (_Stroke == null)
                    {
                        StylusPointCollection _StylusPointCollection = new StylusPointCollection();
                        Point _Point = _pos;
                        _StylusPointCollection.Add(new StylusPoint(_Point.X, _Point.Y));
                        _Stroke = new CustomStroke(_StylusPointCollection);
                        _InkCanvas.Strokes.Add(_Stroke);

                        _curentPoint = 0;

                    }
                    else
                    {
                        StylusPoint _StylusPoint = new StylusPoint(_pos.X, _pos.Y);
                        _curentPoint++;
                        _Stroke.StylusPoints.Insert(_curentPoint, _StylusPoint);
                    }
                }
            }            
        }

        private void NewPoint(Point _pos)
        {
            //foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
            //    foreach (CustomStroke _Stroke1 in _InkCanvas1.Strokes)
            //        for (int i = 0; i < _Stroke1.StylusPoints.Count; i++)
            //        {                        
            //            StylusPoint _StylusPoint = _Stroke1.StylusPoints[i];
            //            double l = ((Vector)(Convert(_StylusPoint)-_pos)).Length;
            //            if (l < 3)
            //            {
            //                _Stroke = _Stroke1;
            //                _curentPoint = i;
            //                return;
            //            }
            //        }


        }

        public class MapDatabase
        {
            public class Image
            {
                public double Width;
                public double Height;
                public double X;
                public double Y;
                public string Path;
            }
            public Point _TStartPos;
            public Point _CStartPos;
            public List<Layer> _Layers = new List<Layer>();
            public class Layer
            {
                public List<Image> _Images = new List<Image>();
                public List<Polygon> _Polygons = new List<Polygon>();
            }
            public class Polygon
            {
                public Color _Color;
                public List<Point> _Points = new List<Point>();
            }
        }
        MapDatabase _MapDatabase = new MapDatabase();

        private void OpenFile(string _FilePath) //Loaddata
        {
            if (!File.Exists(_FilePath)) { Trace.WriteLine("file not exists " + _FilePath); return; }
            byte[] _Buffer = File.ReadAllBytes(_FilePath);
            MemoryStream _MemoryStream = new MemoryStream(_Buffer);
            _MapDatabase = (MapDatabase)_XmlSerializer.Deserialize(_MemoryStream);

            _InkCanvas.Strokes.Clear();

            InkCanvas.SetLeft(_CStartPos, _MapDatabase._CStartPos.X);
            InkCanvas.SetTop(_CStartPos, _MapDatabase._CStartPos.Y);
            InkCanvas.SetLeft(_TStartPos, _MapDatabase._TStartPos.X);
            InkCanvas.SetTop(_TStartPos, _MapDatabase._TStartPos.Y);
            for (int i = 0; i < _MapDatabase._Layers.Count; i++)
            {
                MapDatabase.Layer _Layer = _MapDatabase._Layers[i];
                InkCanvas _InkCanvas1 = (InkCanvas)_CanvasList.Children[i];
                foreach (MapDatabase.Image _DImage in _Layer._Images)
                {
                    Image _Image = new Image();
                    if (!File.Exists(_DImage.Path)) Debugger.Break();

                    BitmapImage _BitmapImage = new BitmapImage(new Uri(_DImage.Path, UriKind.Relative));
                    double a = _BitmapImage.Width;
                    _Image.Source = _BitmapImage;
                    
                    _Image.Width = _DImage.Width;
                    _Image.Height = _DImage.Height;
                    InkCanvas.SetLeft(_Image, _DImage.X);
                    InkCanvas.SetTop(_Image, _DImage.Y);
                    _InkCanvas1.Children.Add(_Image);
                }
                foreach (MapDatabase.Polygon _Polygon in _Layer._Polygons)
                {
                    StylusPointCollection _StylusPointCollection = new StylusPointCollection();
                    foreach (Point _Point in _Polygon._Points)
                    {
                        StylusPoint _StylusPoint = new StylusPoint(_Point.X, _Point.Y);
                        _StylusPointCollection.Add(_StylusPoint);
                    }

                    CustomStroke _Stroke = new CustomStroke(_StylusPointCollection);
                    _Stroke.DrawingAttributes.Color = _Polygon._Color;
                    _InkCanvas1.Strokes.Add(_Stroke);
                }
            }
        }
        public class CustomInkCanvas : InkCanvas
        {

        }
        class CustomStroke : Stroke
        {
            public CustomStroke(StylusPointCollection _StylusPointCollection)
                : base(_StylusPointCollection)
            {
            }
            //public string _ImagePath;
        }


    }
}
