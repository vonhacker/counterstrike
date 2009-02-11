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
namespace CSLIVE 
{
    public class LocalDatabase
    {
        public string _Nick;
    }
    
    public partial class UserControl : System.Windows.Controls.UserControl //storing all global vars here
    {
        public static Config _Config;
        public static LocalDatabase _LocalDatabase;
        public static Page _Page;
        public static UIElement _RootVisual { get { return _Page.Content; } set { _Page.Content = value; } }
    }
}
