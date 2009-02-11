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
using doru;

namespace CSLIVE
{
    public partial class EnterNick : UserControl
    {        
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
                _OnNick(_TextPannel.Text);
                _TextPannel.Text = "";                                                
            }
        }
        public delegate void OnNick(string nick);
        public OnNick _OnNick;
        void _TextPannel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OkClick(null, null);
        }        
    }
}
