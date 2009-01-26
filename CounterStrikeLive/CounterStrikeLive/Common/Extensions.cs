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

namespace CounterStrikeLive
{
    public static class Extensions
    {
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
