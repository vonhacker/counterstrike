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
using doru.Vectors;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Browser;
using CSLIVE.Menu;


namespace CSLIVE 
{
    public interface IUpdate { void Update(); }

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

    public partial class Page : UserControl //глобальные переменые, Contnet содержит один из контролов irc,game и тд 
    {
        #region Vars
        public static bool _MouseLeftButtonDown;
        public static Vector _Mouse;
        public static Vector _MouseMove;
        public static TimerA _TimerA = new TimerA();
        public static List<Key> _Keys = new List<Key>();
        public static IDictionary<string, string> _InitParams = new Dictionary<string, string>();
        public static Random _Random = new Random();
        public static Config _Config = new Config();
        public static LocalDatabase _LocalDatabase;
        public static Page _Page;
        public static MouseEventArgs _MouseEventArgs;
        #endregion

        public Page() 
        {
#if(!SILVERLIGHT)
            Logging.Setup("../../../CSLIVE.Web/ClientBin");
#endif
            _Page = this;
            InitializeComponent();            
            Loaded+=new RoutedEventHandler(Page_Loaded);
            Application.Current.Host.Content.Resized +=new EventHandler(Content_Resized);
            Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
            Content_Resized(null, null);
        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            ScriptObject _ScriptObject = (ScriptObject)HtmlPage.Window.GetProperty("screen");
            Width = (double)_ScriptObject.GetProperty("width");
            Height = (double)_ScriptObject.GetProperty("height");
        }
        void Content_Resized(object sender, EventArgs e)
        {
            this.Width = App.Current.Host.Content.ActualWidth;
            this.Height = App.Current.Host.Content.ActualHeight;
        }
        void Page_Loaded(object sender, RoutedEventArgs e) //добовляем обрабочики клавиш, мыши и обнавления, качаем конфиг
        {                        
            App.Current.Exit += delegate
            {
                Exit();
            };

            _LocalDatabase = (LocalDatabase)_XmlSerializerLocal.DeserealizeOrCreate("db.xml", new LocalDatabase());

            KeyDown += new KeyEventHandler(Page_KeyDown);
            KeyUp += new KeyEventHandler(Page_KeyUp);
            MouseLeftButtonDown += new MouseButtonEventHandler(Page_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(Page_MouseLeftButtonUp);
            DispatcherTimer _DispatcherTimer = new DispatcherTimer();
            _DispatcherTimer.Interval = TimeSpan.FromMilliseconds(2);
            _DispatcherTimer.Tick += new EventHandler(Update);
            _DispatcherTimer.Start();
            new Downloader().Download("Config.xml", delegate(Stream s)
            {
                //_Config = (Config)Common._XmlSerializerConfig.Deserialize(s);
                OnConfigLoaded();
            });
            
        }

        public void Page_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _MouseLeftButtonDown = false;
        }

        public void Page_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _MouseLeftButtonDown = true;
        }

        public void Page_KeyUp(object sender, KeyEventArgs e) //удаляем клавишу из масива _keys
        {
            _Keys.Remove(e.Key);
        }

        public void Page_KeyDown(object sender, KeyEventArgs e) //записываем в масив _Keys клавишу
        {
            if (!_Keys.Contains(e.Key))
                _Keys.Add(e.Key);
        }

        public void Page_MouseMove(object sender, MouseEventArgs e) //сохраняем позиции в Point _Mouse и _MouseMove
        {
            _MouseEventArgs = e;
            Point _GetPosition = e.GetPosition(this);
            if (_Mouse != default(Vector)) _MouseMove = new Vector((float)_GetPosition.X, (float)_GetPosition.Y) - _Mouse;
            _Mouse = new Vector((float)_GetPosition.X, (float)_GetPosition.Y);
        }
        

        void Update(object sender, EventArgs e) //обновляем конент
        {
            
            if (_Page.Content is IUpdate) ((IUpdate)_Page.Content).Update();
            _TimerA.Update();
        }



        private void LoadIrc()
        {
            if (_InitParams.ContainsKey("ip")) { }
            else
                _Page.Content = new Irc();
        }



        void OnConfigLoaded() //asking for OnConfigLoaded -> loading irc
        {
#if(DEBUG)
            _Page.Content = new Game.Game();
#else

            if (_LocalDatabase._Nick == null)
            {
                EnterNick _EnterNick = new EnterNick();
                _Page.Content = _EnterNick;
                _EnterNick._OnNick += delegate(string nick)
                {
                    _LocalDatabase._Nick = nick;
                    LoadIrc();
                };
            }
            else
                LoadIrc();
#endif
        }

        XmlSerializer _XmlSerializerLocal = new XmlSerializer(typeof(LocalDatabase));

        void Exit() //saving config on exit
        {
            _XmlSerializerLocal.Serialize("db.xml", _LocalDatabase);
            Trace.WriteLine("Exit");
        }
    }


    
}
