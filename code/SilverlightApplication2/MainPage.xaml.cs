using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApplication2
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        MediaElement _MediaElement;
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var a =Resources["ak47-1.mp3"];
            var b = App.GetResourceStream(new Uri("ak47-1.mp3",UriKind.Relative));
            _MediaElement = new MediaElement();
            _Grid.Children.Add(_MediaElement);
            _MediaElement.SetSource(b.Stream);
            //_MediaElement.Source = new Uri("parabolic.funwithskullstep.20090416.mp3", UriKind.Relative);
            //_MediaElement.Play();
        }
    }
}
