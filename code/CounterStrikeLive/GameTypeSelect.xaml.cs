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
    public partial class GameTypeSelect : ChildWindow
    {
        public static new GameTypeSelect _This; 
        public GameTypeSelect()
        {
            _This = this;
            InitializeComponent();
            Loaded += new RoutedEventHandler(GameTypeSelect_Loaded);
        }

        void GameTypeSelect_Loaded(object sender, RoutedEventArgs e)
        {
            if (Config._This._AutoSelect)
            {
                Menu._SinglePlayer = true;
                DialogResult = true;
            }
        }

        private void Multiplayer_Click(object sender, RoutedEventArgs e)
        {
            Menu._Multiplayer = true;
            DialogResult = true;
        }

        private void SinglePlayer_Click(object sender, RoutedEventArgs e)
        {
            Menu._SinglePlayer = true;
            DialogResult = true;
        }
        public int _ctBotsCount { get { try { return int.Parse(ct_bots.Text.Trim()); } catch { return 4; } } }
        public int _tBotsCount { get { try { return int.Parse(t_bots.Text.Trim()); } catch { return 4; } } }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            new Credits().Success += delegate { new GameTypeSelect().Success = Success; };
        }
        
                
    }
}

