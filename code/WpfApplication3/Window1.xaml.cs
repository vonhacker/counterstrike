using doru;
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
using System.Windows.Threading;

namespace WpfApplication3
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            //Logging.Setup();
           // Loaded += new RoutedEventHandler(Window1_Loaded);
        }
        MediaElement _MediaElement;
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            _MediaElement = new MediaElement();
            _Grid.Children.Add(_MediaElement);
            _MediaElement.LoadedBehavior = MediaState.Manual;
            
            _MediaElement.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(_MediaElement_MediaFailed);
            _MediaElement.Source = new Uri("parabolic.funwithskullstep.20090416.mp3", UriKind.Relative);
            _MediaElement.Play();
            new DispatcherTimer().StartRepeatMethod(.5, Update);
        }

        void _MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
        public void Update()
        {
            _MediaElement.Play();
        }
    }
}
