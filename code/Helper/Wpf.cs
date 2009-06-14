using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;


namespace doru
{
    public static class WpfExtensions
    {
        
        public static Line SetPoints(this Line _line,Point a , Point b)
        {
            _line.X1 = a.X; _line.X2 = b.X;
            _line.Y1 = a.Y; _line.Y2 = b.Y;
            return _line;
        }
        public static void Show(this Control _Control)
        {
            _Control.Visibility = Visibility.Visible;
            _Control.IsEnabled = true;
            _Control.Focus();
        }
        public static void Hide(this Control _Control)
        {
            _Control.Visibility = Visibility.Collapsed;
            _Control.IsEnabled = false;
        }
        public static void Toggle(this Control _Control)
        {
            if (_Control.Visibility == Visibility.Visible)
                _Control.Hide();
            else
                _Control.Show();
        }
    }
}
