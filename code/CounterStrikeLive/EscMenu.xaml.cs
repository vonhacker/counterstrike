using doru;
using CounterStrikeLive.Service;
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
    public partial class EscMenu : ChildWindow
    {
        
        public EscMenu()
        {            
            InitializeComponent();            
        }

    
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void VoteMap_Click(object sender, RoutedEventArgs e)
        {
            MapSelect _MapSelect = new MapSelect();
            _MapSelect.Success += delegate
            {

                Menu._This._Sender.Send(PacketType.voteMap, _MapSelect.Name.ToBytes());
                Menu._This._Chat.Text += Game._This._LocalClient._Nick + " Voted Map " + _MapSelect.Name;
            };
        }

        private void full_screen_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Host.Content.IsFullScreen = App.Current.Host.Content.IsFullScreen == true ? false : true;
            this.DialogResult = true;
        }

        private void select_team_Click(object sender, RoutedEventArgs e)
        {
            new TeamSelect();
            this.DialogResult = true;
        }

        private void show_scoreboard_Click(object sender, RoutedEventArgs e)
        {
            Menu._This._ScoreBoard.Toggle();
            this.DialogResult = true;
        }

        private void text_message_Click(object sender, RoutedEventArgs e)
        {
            new ChatBox();
            this.DialogResult = true;
        }

        private void Change_Nick_Click(object sender, RoutedEventArgs e)
        {
            new EnterNick();
            this.DialogResult = true;
        }
    }
}

