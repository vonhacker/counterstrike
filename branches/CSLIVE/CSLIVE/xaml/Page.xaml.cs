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
using System.Diagnostics;
using System.Windows.Threading;


namespace CSLIVE
{

    public partial class Page : UserControl//holds _Page.Content and update it
    {
        public static LocalDatabase _LocalDatabase { get { return App._LocalDatabase; } set { App._LocalDatabase = value; } }
        public static Config _Config { get { return App._Config; } set { App._Config = value; } }
        public static Page _Page { get { return App._Page; } set { App._Page = value; } }
        public static IDictionary<string, string> _InitParams { get { return App._InitParams; } set { App._InitParams = value; } }
        public Page() //start ->page_loaded
        {
#if(!SILVERLIGHT)
            Logging.Setup("../../../CSLIVE.Web/ClientBin");
#endif
            _Page = this;
            InitializeComponent();

            App.Current.Exit += delegate
            {
                Exit();
            };                
            Loaded += new RoutedEventHandler(Page_Loaded);
      
        }

        

        

        void Page_Loaded(object sender, RoutedEventArgs e) //loading config ->loadIrc
        {

            _LocalDatabase = (LocalDatabase)_XmlSerializerLocal.DeserealizeOrCreate("db.xml", new LocalDatabase());                        

            
            DispatcherTimer _DispatcherTimer = new DispatcherTimer();
            _DispatcherTimer.Interval = TimeSpan.FromMilliseconds(2);
            _DispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick);
            _DispatcherTimer.Start();
            new Downloader().Download("Config.xml", delegate(Stream s)
            {
                _Config = (Config)Common._XmlSerializerConfig.Deserialize(s);
                EnterNick();
            });
        }

        void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_Page.Content is IUpdate) ((IUpdate)_Page.Content).Update();
        }



        private void LoadGame()
        {
            if (_InitParams.ContainsKey("ip")) { }
            else
                _Page.Content = new Irc();
        }
        
        

        void EnterNick() //asking for EnterNick -> loading irc
        {
            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();
                _Page.Content = _EnterNick;
                _EnterNick._OnNick += delegate(string nick)
                {
                    _LocalDatabase._Nick = nick;
                    LoadGame();
                };
            }
            else
                LoadGame();
        }

        XmlSerializer _XmlSerializerLocal = new XmlSerializer(typeof(LocalDatabase));

        void Exit() //saving config on exit
        {
            _XmlSerializerLocal.Serialize("db.xml", _LocalDatabase);
            Trace.WriteLine("Exit");
        }
    }
}
