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
using CSL.Common;
using System.Windows.Ink;
using System.Text.RegularExpressions;
using System.IO;

namespace CSL.LevelEditor
{
    /// <summary>
    /// Interaction logic for UserControlCanvas.xaml
    /// </summary>
    public partial class UserControlCanvas : UserControl
    {
        public UserControlCanvas()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UserControlCanvas_Loaded);
        }

        private MapDatabase _mapDatabase = new MapDatabase();
        private InkCanvas _currentInkCanvas;
        private Stroke _Stroke;
        public int _curPointIndex;
        public EditorMode _curEditorMode;
        private double _Scale = 1;
        private const int _maxEndPointDistance = 5;


        void UserControlCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectCanvas(0);

                _currentInkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(InkCanvas_StrokeCollected);

                _canvasList.MouseDown += new MouseButtonEventHandler(InkCanvas_MouseDown);
                _canvasList.MouseMove += new MouseEventHandler(_InkCanvas_MouseMove);
                foreach (InkCanvas inkCanvas in _canvasList.Children)
                {
                    inkCanvas.SelectionChanged += new EventHandler(InkCanvas1SelectionChanged);
                }
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
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

        public double GetDistanceOf2Points(StylusPoint a, StylusPoint b)
        {
            return Distance(FromStylusPointToPoint(a), FromStylusPointToPoint(b));
        }

        public double Distance(Point a, Point b)
        {
            Vector c = Point.Subtract(a, b);
            return c.Length;
        }

        public static Point FromStylusPointToPoint(StylusPoint stylusPoint)
        {
            return new Point(stylusPoint.X, stylusPoint.Y);
        }

        void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            ConnectEndStylusPoints(e.Stroke);
        }

        /// <summary>
        /// Connects two ends of a stroke in one point, if this ends are near enough(!)
        /// </summary>
        /// <param name="stroke">Some stroke to correct the ends</param>
        private void ConnectEndStylusPoints(Stroke stroke)
        {
            double endPointDistance = GetDistanceOf2Points(stroke.StylusPoints.First(), stroke.StylusPoints.Last());
            if (endPointDistance < _maxEndPointDistance)
            {
                stroke.StylusPoints[stroke.StylusPoints.Count - 1] = stroke.StylusPoints.First();
            }
        }

        public void SelectColor()
        {
            StrokeCollection _StrokeCollection = _currentInkCanvas.GetSelectedStrokes();
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
                    double a = _BitmapImage.Width;
                    _Image.Source = _BitmapImage;
                    //Debugger.Break();
                    Point _Mouse = Mouse.GetPosition(_currentInkCanvas);
                    InkCanvas.SetTop(_Image, _Mouse.X);
                    InkCanvas.SetLeft(_Image, _Mouse.Y);
                    _currentInkCanvas.Children.Add(_Image);
                }
                //TODO: handle video files
            }

            // Mark the event as handled, so the control's native Drop handler is not called.
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

        void _InkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {

                //TODO: implement
                //String text = ((int)_pos.X) + ":" + ((int)_pos.Y);
                //_textBlockCoordinates.Text = text;
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }


        public void SaveFile(String filePath4MapDescriptor)
        {
            _mapDatabase.Layers = SaveLayers();

            _mapDatabase.CounterTerroristStartPos.X = InkCanvas.GetLeft(_counterTerroristStartPos);
            _mapDatabase.CounterTerroristStartPos.Y = InkCanvas.GetTop(_counterTerroristStartPos);
            _mapDatabase.TerroristStartPos.X = InkCanvas.GetLeft(_terroristStartPos);
            _mapDatabase.TerroristStartPos.Y = InkCanvas.GetTop(_terroristStartPos);

            Dal.SaveMapDatabase(filePath4MapDescriptor, _mapDatabase);
        }

        public void SelectCanvas(int canvasIndex)
        {
            InkCanvas oldInkCanvas = _currentInkCanvas;
            _currentInkCanvas = (InkCanvas)_canvasList.Children[canvasIndex];

            //copy images & strokes
            if (oldInkCanvas != null)
            {
                foreach (Image uIElement in oldInkCanvas.GetSelectedElements().OfType<Image>())
                {
                    oldInkCanvas.Children.Remove(uIElement);
                    _currentInkCanvas.Children.Add(uIElement);
                }
                foreach (Stroke stroke in oldInkCanvas.GetSelectedStrokes())
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift)) _currentInkCanvas.Strokes.Add(stroke.Clone());
                    if (Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        oldInkCanvas.Strokes.Remove(stroke);
                        _currentInkCanvas.Strokes.Add(stroke);
                    }
                }
            }

            //hide all canvases
            foreach (InkCanvas inkCanvas in _canvasList.Children)
            {
                inkCanvas.Opacity = .2;
                inkCanvas.IsHitTestVisible = false;
            }

            _currentInkCanvas.IsHitTestVisible = true;
            _currentInkCanvas.Opacity = 1;
            //SetMode(InkCanvasEditingMode.Select, CustomMode.select);
            _currentInkCanvas.Select(new StrokeCollection());
            if (null != oldInkCanvas)
                oldInkCanvas.Select(new StrokeCollection());
        }

        public void SetControl(String filePath4MapDescriptor)
        {
            _mapDatabase = Dal.GetMapDatabase(filePath4MapDescriptor);
            SelectCanvas(0);
            SetDataInControl4CurCanvas();
            SetMode(InkCanvasEditingMode.Select, EditorMode.Select);
        }

        public void SetDataInControl4CurCanvas()
        {
            try
            {
                _currentInkCanvas.Strokes.Clear();

                //Set start locations of terrorists and counterterrorist
                InkCanvas.SetLeft(_counterTerroristStartPos, _mapDatabase.CounterTerroristStartPos.X);
                InkCanvas.SetTop(_counterTerroristStartPos, _mapDatabase.CounterTerroristStartPos.Y);
                InkCanvas.SetLeft(_terroristStartPos, _mapDatabase.TerroristStartPos.X);
                InkCanvas.SetTop(_terroristStartPos, _mapDatabase.TerroristStartPos.Y);

                for (int i = 0; i < _mapDatabase.Layers.Count; i++)
                {
                    MapDatabase.Layer layer = _mapDatabase.Layers[i];
                    InkCanvas inkCanvas = (InkCanvas)_canvasList.Children[i];
                    _currentInkCanvas = inkCanvas;
                    foreach (MapDatabase.Image image in layer.Images)
                    {
                        Image img = new Image();
                        if (!File.Exists(image.Path))
                            throw new FileNotFoundException(image.Path);

                        
                        BitmapImage bitmapImage = new BitmapImage(new Uri(image.Path, UriKind.Relative));
                        img.Source = bitmapImage;
                        img.Stretch = Stretch.Fill;
                        img.Width = image.Width;
                        img.Height = image.Height;
                        InkCanvas.SetLeft(img, image.X);
                        InkCanvas.SetTop(img, image.Y);
                        inkCanvas.Children.Add(img);
                    }

                    foreach (MapDatabase.Polygon polygon in layer.Polygons)
                    {
                        //Create a collection of points of the current polygon
                        StylusPointCollection stylusPointCollection = new StylusPointCollection();
                        foreach (Point point in polygon.Points)
                        {
                            StylusPoint stylusPoint = new StylusPoint(point.X, point.Y);
                            stylusPointCollection.Add(stylusPoint);
                        }

                        //Add polygon as a new stroke
                        Stroke stroke = new Stroke(stylusPointCollection);
                        stroke.DrawingAttributes.Color = polygon.Color;
                        inkCanvas.Strokes.Add(stroke);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        public void SetMode(InkCanvasEditingMode mode, EditorMode customMode)
        {
            UpdateStrokeAndSetToNull();
            _currentInkCanvas.EditingMode = mode;
            _curEditorMode = customMode;
        }


        private void UpdateStrokeAndSetToNull()
        {
            if (_Stroke != null)
            {
                ConnectEndStylusPoints(_Stroke);
                //TODO: why this?
                _Stroke = null;
            }
        }

        private void NewPoint(Point _pos)
        {
            //foreach (InkCanvas _InkCanvas1 in _canvasList.Children)
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

        void InkCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point curCursorPosition = e.GetPosition(_currentInkCanvas);

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    //"Select" some near point on the canvas

                    UpdateStrokeAndSetToNull();
                    if (_curEditorMode == EditorMode.Polygon)
                    {
                        foreach (InkCanvas inkCanvas in _canvasList.Children)
                        {
                            foreach (Stroke stroke in inkCanvas.Strokes)
                            {
                                for (int pointIndex = 0; pointIndex < stroke.StylusPoints.Count; pointIndex++)
                                {
                                    StylusPoint stylusPoint = stroke.StylusPoints[pointIndex];
                                    double distanceOf2Points = ((Vector)(FromStylusPointToPoint(stylusPoint) - curCursorPosition)).Length;
                                    if (distanceOf2Points < _maxEndPointDistance * 2)
                                    {
                                        SelectPoint(pointIndex);
                                        _Stroke = stroke;
                                        _curEditorMode = EditorMode.MovingPoint;
                                    }
                                }
                            }
                        }
                    }
                }
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (_curEditorMode == EditorMode.Polygon)
                    {
                        curCursorPosition = GetCorrectCursorPosition(curCursorPosition);
                        AddNewPointToStroke(curCursorPosition);
                    }
                    else if (_curEditorMode == EditorMode.MovingPoint)
                    {
                        _curEditorMode = EditorMode.Polygon;
                        curCursorPosition = e.GetPosition(_currentInkCanvas);
                        if (_Stroke != null)
                        {
                            //move selected point to the new location
                            _Stroke.StylusPoints[_curPointIndex] = new StylusPoint(curCursorPosition.X, curCursorPosition.Y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void SelectPoint(int pointIndex)
        {
            _curPointIndex = pointIndex;
        }

        private void AddNewPointToStroke(Point curCursorPosition)
        {
            if (_Stroke == null)  //setfirst point
            {
                StylusPointCollection stylusPointCollection = new StylusPointCollection();
                stylusPointCollection.Add(new StylusPoint(curCursorPosition.X, curCursorPosition.Y));
                _Stroke = new Stroke(stylusPointCollection);
                _currentInkCanvas.Strokes.Add(_Stroke);

                _curPointIndex = 0;

            }
            else //add next point to current stroke
            {
                StylusPoint stylusPoint = new StylusPoint(curCursorPosition.X, curCursorPosition.Y);
                _curPointIndex++;
                _Stroke.StylusPoints.Insert(_curPointIndex, stylusPoint);
            }
        }

        private Point GetCorrectCursorPosition(Point curCursorPosition)
        {
            //update current cursor(mouse) position to some existring stylusPoint(only if near enough).
            foreach (InkCanvas inkCanvas in _canvasList.Children)
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    for (int i = 0; i < stroke.StylusPoints.Count; i++)
                    {
                        StylusPoint stylusPoint = stroke.StylusPoints[i];
                        double distanceOf2Points = ((Vector)(FromStylusPointToPoint(stylusPoint) - curCursorPosition)).Length;
                        if (distanceOf2Points < _maxEndPointDistance)
                        {
                            curCursorPosition = FromStylusPointToPoint(stylusPoint);
                        }
                    }
                }
            }
            return curCursorPosition;
        }

        private List<MapDatabase.Layer> SaveLayers()
        {
            List<MapDatabase.Layer> _Layers = new List<MapDatabase.Layer>();
            foreach (InkCanvas _InkCanvas in _canvasList.Children)
            {
                MapDatabase.Layer _Layer = new MapDatabase.Layer();
                foreach (Image _Image in _InkCanvas.Children.OfType<Image>())
                {
                    _Layer.Images.Add(new MapDatabase.Image
                    {
                        Path = ((BitmapImage)_Image.Source).UriSource.OriginalString,
                        X = InkCanvas.GetLeft(_Image),
                        Y = InkCanvas.GetTop(_Image),
                        Width = _Image.ActualWidth,
                        Height = _Image.ActualHeight
                    });
                }

                List<MapDatabase.Polygon> _Polygons = _Layer.Polygons;
                foreach (Stroke _Stroke in _InkCanvas.Strokes)
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
                    _Polygon.Color = _Stroke.DrawingAttributes.Color;
                    _Polygon.Points = _Points;
                    _Polygons.Add(_Polygon);
                }
                _Layers.Add(_Layer);
            }
            return _Layers;
        }

        internal void RemoveLastStroke()
        {
            if (_Stroke != null)
            {
                if (_curPointIndex > 0)
                {
                    _Stroke.StylusPoints.RemoveAt(_curPointIndex);
                    _curPointIndex--;
                }
                else
                {
                    _currentInkCanvas.Strokes.Remove(_Stroke);
                    _Stroke = null;
                }
            }
        }

        internal void Scale(bool shouldAdd)
        {
            double _ScaleFactor = shouldAdd ? 1.2 : .8;
            _Scale *= _ScaleFactor;
            //TODO: implement
            //     _ScaleText.Text = _Scale.ToString();
            foreach (InkCanvas _InkCanvas in _canvasList.Children)
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

        internal void SetPage(bool isPageUp)
        {
            if (isPageUp)
            {
                StrokeCollection _StrokeCollection = _currentInkCanvas.GetSelectedStrokes();
                foreach (Stroke _Stroke in _StrokeCollection)
                {
                    _currentInkCanvas.Strokes.Remove(_Stroke);
                    _currentInkCanvas.Strokes.Add(_Stroke);
                }
            }
            else
            {
                StrokeCollection _StrokeCollection = _currentInkCanvas.GetSelectedStrokes();
                foreach (Stroke _Stroke in _StrokeCollection)
                {
                    _currentInkCanvas.Strokes.Remove(_Stroke);
                    _currentInkCanvas.Strokes.Insert(0, _Stroke);
                }
            }
        }

        internal void Copy()
        {
            _currentInkCanvas.CopySelection();
        }

        internal void Cut()
        {
            _currentInkCanvas.CutSelection();
        }

        internal void Paste()
        {
            _currentInkCanvas.Paste();
        }

        internal void KeyB()
        {
            if (_polygonsCanvas.Children.Count > 0)
                _polygonsCanvas.Children.Clear();
            else
                foreach (InkCanvas _InkCanvas1 in _canvasList.Children)
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
                            _polygonsCanvas.Children.Add(_Polygon);
                        }
                    }
        }
    }
}
