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
    public class Config
    {
        public string _Irc;
    }
    public partial class UserControl : System.Windows.Controls.UserControl
    {
        public static Config _Config;
        public static LocalDatabase _LocalDatabase;
    }
}
