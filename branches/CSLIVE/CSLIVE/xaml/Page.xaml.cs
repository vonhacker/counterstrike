using System.Xml.Serialization;
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
using System.IO;
using System.IO.IsolatedStorage;
using doru;


namespace CSLIVE
{

    public partial class Page : UserControl//holds _Page.Content and update it
    {
        public static LocalDatabase _LocalDatabase { get { return App._LocalDatabase; } set { App._LocalDatabase = value; } }
        public static Config _Config { get { return App._Config; } set { App._Config = value; } }
        public static Page _Page { get { return App._Page; } set { App._Page = value; } }
        public Page() //start ->page_loaded
        {            
            _Page = this;
            InitializeComponent();
            App.Current.Exit += new EventHandler(Current_Exit);
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        Storyboard _Storyboard = new Storyboard();

        void Page_Loaded(object sender, RoutedEventArgs e) //loading config ->loadIrc
        {            
            _LocalDatabase = (LocalDatabase)_XmlSerializerLocal.DeserealizeOrCreate("db.xml", new LocalDatabase());

            WebClient _WebClient = new WebClient();
            
            _WebClient.OpenReadAsync(new Uri("Config.xml", UriKind.Relative));
            _WebClient.OpenReadCompleted += delegate(object o, OpenReadCompletedEventArgs e2)
            {
                _Config = (Config)Common._XmlSerializerConfig.Deserialize(e2.Result);
                LoadIrc();
            };

            _Storyboard.Completed += new EventHandler(_Storyboard_Completed);
            _Storyboard.Begin();
        }

        void _Storyboard_Completed(object sender, EventArgs e)
        {             
            if(_Page.Content is IUpdate) ((IUpdate)_Page.Content).Update();
            _Storyboard.Begin();
        }
        
        void LoadIrc() //asking for EnterNick -> loading irc
        {            
            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();                
                _Page.Content = _EnterNick;
                _EnterNick._OnNick += delegate(string nick)
                {
                    _LocalDatabase._Nick = nick;
                    _Page.Content = new Irc();
                };
            }
            else
                _Page.Content = new Irc();
        }

        XmlSerializer _XmlSerializerLocal = new XmlSerializer(typeof(LocalDatabase));
        
        void Current_Exit(object sender, EventArgs e) //saving config on exit
        {
            _XmlSerializerLocal.Serialize("db.xml", _LocalDatabase);
            Trace.WriteLine("Exit");
        }
    }
}
