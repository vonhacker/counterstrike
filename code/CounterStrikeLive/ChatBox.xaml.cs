using doru;
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
using CounterStrikeLive.Service;

namespace CounterStrikeLive
{
    public partial class ChatBox : ChildWindow
    {
        
        public ChatBox()
        {
            InitializeComponent();
            _TextBox.KeyDown += new KeyEventHandler(_TextBox_KeyDown);
            
        }
        Menu _Menu = Menu._This;
        protected override void OnClosed(EventArgs e)
        {
            
            base.OnClosed(e);
        }
        void _TextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                string txt=Game._This._LocalClient._Nick + ":" + _TextBox.Text + "\r\n";
                _Menu._Sender.Send(PacketType.chat, txt.ToBytes());
                _Menu._Chat.Text += txt;
                this.Close();
            }

        }


    }
}

