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

namespace CSLIVE
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {        
        public Window1()
        {
            InitializeComponent();
            KeyDown += new KeyEventHandler(Window1_KeyDown);
            KeyUp += new KeyEventHandler(Window1_KeyUp);
            MouseMove += new MouseEventHandler(Window1_MouseMove);
            MouseLeftButtonDown += new MouseButtonEventHandler(Window1_MouseLeftButtonDown);
            
        }

        void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _Page.Page_MouseLeftButtonDown(sender, e);
            _Page.Page_MouseLeftButtonUp(sender, e);
        }
        public Page _Page { get { return Content as Page; } }
        void Window1_MouseMove(object sender, MouseEventArgs e)
        {
            _Page.Page_MouseMove(sender, e);
        }
        
        void Window1_KeyUp(object sender, KeyEventArgs e)
        {
            _Page.Page_KeyUp(sender, e);
        }

        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            _Page.Page_KeyDown(sender, e);
        }
    }
}
