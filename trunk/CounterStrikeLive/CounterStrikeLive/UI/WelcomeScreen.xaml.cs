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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;

namespace CounterStrikeLive
{
    public partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {
            InitializeComponent();
            _DataGrid.ItemsSource = _List;
            _DataGrid.SelectionChanged += new SelectionChangedEventHandler(DataGrid_SelectionChanged);
        }

        void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Item _Item = (_DataGrid.SelectedItem as Item);
            if (_Item == null)
                _serverip.Text = "localhost:4530";
            else
                _serverip.Text = _Item._Ip + ":" + _Item._Port;
        }

        public void Load()
        {
            this.Show();
            _Button.Click += new RoutedEventHandler(Button_Click);
            Download();

        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            //Item _Item = (Item)_DataGrid.SelectedItem;
            Match m = Regex.Match(_serverip.Text, @"([.\w]+?):(\d+)");
            this.Hide();
            _Menu._host = m.Groups[1].Value;
            _Menu._port = int.Parse(m.Groups[2].Value);
            _Menu.EnterNick();
        }


        public Menu _Menu;

        public class Item
        {
            public string _Name { get; set; }
            public string _Players { get; set; }
            public string _Port { get; set; }
            public string _Version { get; set; }
            public string _Ip { get; set; }
            public string _Map { get; set; }
        }
        public ObservableCollection<Item> list = new ObservableCollection<Item>();
        public ObservableCollection<Item> _List { get { return list; } set { } }

        void WebClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Trace.WriteLine("download Completed");
            if (!IsEnabled) return;
            if (e.Error != null) throw e.Error;
            MatchCollection _MatchCollection = Regex.Matches(e.Result, @">(?<Name>[\w\s]+)</a></td><td>(?<Map>[\w/.]+?)</td><td>(?<PlayerCount>\d+)</td><td>(?<Port>\d+)</td><td>(?<Version>[\d.]+)</td><td>(?<Ip>[\d.]+)</td></tr>", RegexOptions.IgnoreCase);
            Trace.WriteLine("Matches:" + _MatchCollection.Count);
            int old = _DataGrid.SelectedIndex;
            _List.Clear();
            foreach (Match _Match in _MatchCollection)
            {
                GroupCollection g = _Match.Groups;
                _List.Add(new Item { _Ip = g["Ip"].Value, _Name = g["Name"].Value, _Players = g["PlayerCount"].Value, _Port = g["Port"].Value, _Map = g["Map"].Value, _Version = g["Version"].Value });
            }
            _DataGrid.SelectedIndex = old;
            new Thread(delegate()
            {
                Thread.Sleep(3000);
                Dispatcher.BeginInvoke(Download);
            }).Start();

        }

        public void Download()
        {
            Trace.WriteLine("download Started");
            WebClient _WebClient = new WebClient();
            _WebClient.DownloadStringAsync(new Uri("http://cslive.mindswitch.ru/cs/serv.php?r=" + Random.Next(99999), UriKind.Absolute));
            _WebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(WebClientDownloadStringCompleted);
        }
    }
}
