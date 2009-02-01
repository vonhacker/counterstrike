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

namespace CounterStrikeLive
{
    public partial class EnterNick : UserControl
    {
        public Menu _Menu;
        public EnterNick()
        {
            InitializeComponent();
            _ok.Click += new RoutedEventHandler(OkClick);
            _TextPannel.KeyDown += new KeyEventHandler(_TextPannel_KeyDown);
        }

        void OkClick(object sender, RoutedEventArgs e)
        {
            if (_TextPannel.Text.Length > 3)
            {
                _Menu._LocalDatabase._Nick = _TextPannel.Text;
                _TextPannel.Text = "";
                this.Hide();
                if (_LoadDb) _Menu.LoadDb();
            }
        }

        void _TextPannel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OkClick(null, null);
        }
        public bool _LoadDb = true;
        public void Show()
        {
            Extensions.Show(this);
            _TextPannel.Focus();
        }
    }
}
