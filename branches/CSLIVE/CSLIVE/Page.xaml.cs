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
        public Page()
        {
            InitializeComponent();
            App.Current.Exit += new EventHandler(Current_Exit);
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        Storyboard _Storyboard = new Storyboard();

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsolatedStorageFile.FileExists("db.xml"))
                using (FileStream _FileStream = _IsolatedStorageFile.OpenFile("db.xml", FileMode.Open, FileAccess.Read))
                    _LocalDatabase = (LocalDatabase)_XmlSerializer.Deserialize(_FileStream);

            WebClient _WebClient = new WebClient();
            _WebClient.OpenReadAsync(new Uri("Config.xml",UriKind.Relative));
            _WebClient.OpenReadCompleted += delegate(object o, OpenReadCompletedEventArgs e2)
            {
                _Config = (Config)_XmlSerializer.Deserialize(e2.Result);
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
        void LoadIrc()
        {            
            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();                
                App.Current.RootVisual = _EnterNick;
                _EnterNick._OnNick += delegate(string nick)
                {
                    _LocalDatabase._Nick = nick;
                    App.Current.RootVisual = new Irc();
                };
            }
            else
                App.Current.RootVisual = new Irc();
        }
        
        IsolatedStorageFile _IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForSite();

        XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Config));
        void Current_Exit(object sender, EventArgs e)
        {            
            using(FileStream _FileStream = _IsolatedStorageFile.OpenFile("db.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                _XmlSerializer.Serialize(_FileStream, _LocalDatabase);
            }
            Trace.WriteLine("Exit");
        
        }
    }
}
