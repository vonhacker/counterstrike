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
using CSL.LevelEditor.Properties;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows.Ink;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Markup;
using CSL.Common;
namespace CSL.LevelEditor
{
    /// <summary>
    /// Interaction logic for WindowEditor         
    /// </summary>
    public partial class WindowEditor : Window
    {
        public WindowEditor()
        {
            String gamePath = System.IO.Path.GetFullPath("../../../");
            Directory.SetCurrentDirectory(gamePath);
            InitializeComponent();
            Loaded += new RoutedEventHandler(WindowLoaded);
        }

        private double _Scale = 1;
        //TODO: make dynamical
        private String _filePath4MapDescriptor = @"C:\Source\cs\trunk\CounterStrikeLive\CounterStrikeLive\Content\map.xml";
        public const string _Filter = "map (*.xml)|*.xml";

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //initializations

            //TODO: exchange by buttons
           // this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, this.SaveFile));
           // this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, this.OpenFile));
            KeyDown += new KeyEventHandler(InkCanvasKeyDown);
           // KeyUp += new KeyEventHandler(WindowEditor_KeyUp);
        } 

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this._userControlCanvas.SetControl(_filePath4MapDescriptor);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonLayer1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SelectCanvas(1);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }


        private void buttonLayer2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SelectCanvas(2);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonLayer3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SelectCanvas(3);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.RemoveLastStroke();

            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonSelectMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SetMode(InkCanvasEditingMode.Select, CustomMode.select);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonPolygon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SetMode(InkCanvasEditingMode.None, CustomMode.polygon);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }

        }

        private void buttonErase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _userControlCanvas.SetMode(InkCanvasEditingMode.EraseByPoint, CustomMode.erase);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }


        //TODO: DO NOT REMOVE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Key oldkey;
        void InkCanvasKeyDown(object sender, KeyEventArgs e)
        {
            oldkey = e.Key;

            //TODO:
            //if (e.Key == Key.Add || e.Key == Key.Subtract)
            //{
            //    double _ScaleFactor = e.Key == Key.Add ? 1.2 : .8;
            //    _Scale *= _ScaleFactor;
            //    _ScaleText.Text = _Scale.ToString();
            //    foreach (InkCanvas _InkCanvas in _CanvasList.Children)
            //    {
            //        foreach (Stroke _Stroke in _InkCanvas.Strokes)
            //            for (int i = 0; i < _Stroke.StylusPoints.Count; i++)
            //            {
            //                StylusPoint _StylusPoint = _Stroke.StylusPoints[i];
            //                _Stroke.StylusPoints[i] = new StylusPoint(_StylusPoint.X * _ScaleFactor, _StylusPoint.Y * _ScaleFactor);
            //            }
            //        foreach (FrameworkElement _Image in _InkCanvas.Children)
            //        {
            //            InkCanvas.SetLeft(_Image, InkCanvas.GetLeft(_Image) * _ScaleFactor);
            //            InkCanvas.SetTop(_Image, InkCanvas.GetTop(_Image) * _ScaleFactor);
            //            _Image.Width = _Image.ActualWidth * _ScaleFactor;
            //            _Image.Height = _Image.ActualHeight * _ScaleFactor;
            //        }
            //    }
            //}

            //TODO: implement
            //if (e.Key == Key.C)
            //{
            //    SelectColor();
            //}
            //if (e.Key == Key.PageUp)
            //{
            //    StrokeCollection _StrokeCollection = _currentInkCanvas.GetSelectedStrokes();
            //    foreach (Stroke _Stroke in _StrokeCollection)
            //    {
            //        _currentInkCanvas.Strokes.Remove(_Stroke);
            //        _currentInkCanvas.Strokes.Add(_Stroke);
            //    }
            //}
            //if (e.Key == Key.PageDown)
            //{
            //    StrokeCollection _StrokeCollection = _currentInkCanvas.GetSelectedStrokes();
            //    foreach (Stroke _Stroke in _StrokeCollection)
            //    {
            //        _currentInkCanvas.Strokes.Remove(_Stroke);
            //        _currentInkCanvas.Strokes.Insert(0, _Stroke);
            //    }
            //}
            //if (Keyboard.IsKeyDown(Key.C) && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    _currentInkCanvas.CopySelection();
            //}
            //if (Keyboard.IsKeyDown(Key.X) && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    _InkCanvas.CutSelection();
            //}

            //TODO:
            //if (Keyboard.IsKeyDown(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    _currentInkCanvas.Paste();
            //}

            //TODO:
            //if (e.Key == Key.B)
            //{
            //    if (_PolygonsCanvas.Children.Count > 0)
            //        _PolygonsCanvas.Children.Clear();
            //    else
            //        foreach (InkCanvas _InkCanvas1 in _CanvasList.Children)
            //            foreach (Stroke _Stroke in _InkCanvas1.Strokes)
            //            {
            //                if (_Stroke.StylusPoints.Last() == _Stroke.StylusPoints.First())
            //                {
            //                    Polygon _Polygon = new Polygon();
            //                    foreach (StylusPoint _Point in _Stroke.StylusPoints)
            //                    {
            //                        _Polygon.Points.Add(new Point(_Point.X, _Point.Y));
            //                    }
            //                    _Polygon.Fill = new SolidColorBrush(_Stroke.DrawingAttributes.Color);
            //                    _PolygonsCanvas.Children.Add(_Polygon);
            //                }
            //            }
            //}
        }









    }
}
