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
using System.Xml.Serialization;
using System.Collections.Generic;

namespace CSLIVE
{
    public interface IUpdate { void Update(); }    


    public partial class UserControl : System.Windows.Controls.UserControl //storing all global vars here
    {
        [Obsolete]
        public new string Name;//use _Nick
        
        public static Random _Random = new Random();
        public static Config _Config;
        public static LocalDatabase _LocalDatabase;
        public static Page _Page;
        public static UIElement _RootVisual { get { return _Page.Content; } set { _Page.Content = value; } }
    }
    public class LocalDatabase //Cookies
    {
        public string _Nick;
    }
    public static class Res //Resources resx
    {
        //nick,client,host,about
        public static string _ircon = @"NICK {0}
USER {1} {2} server :{3}
";
    }


}
