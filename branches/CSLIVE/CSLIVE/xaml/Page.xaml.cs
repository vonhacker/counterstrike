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

    public partial class Page : UserControl
    {
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
                _Config = (Config)Config._XmlSerializer.Deserialize(e2.Result);
                LoadIrc();
            };

            _Storyboard.Completed += new EventHandler(_Storyboard_Completed);
        }

        void _Storyboard_Completed(object sender, EventArgs e)
        {
            Update();
            _Storyboard.Begin();
        }
        public void Update()
        {
            Trace.WriteLine("page update");
        }
        void LoadIrc() //asking for EnterNick -> loading irc
        {            
            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();                
                _RootVisual = _EnterNick;
                _EnterNick._OnNick += delegate(string nick)
                {
                    _LocalDatabase._Nick = nick;
                    _RootVisual = new Irc();
                };
            }
            else
                _RootVisual = new Irc();
        }


        XmlSerializer _XmlSerializerLocal = new XmlSerializer(typeof(LocalDatabase));
        
        void Current_Exit(object sender, EventArgs e) //saving config on exit
        {
            _XmlSerializerLocal.Serialize("db.xml", _LocalDatabase);
            Trace.WriteLine("Exit");
        }
    }
}
