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
using CSLIVE.Game;

namespace CSLIVE.Game.Controls
{
    public partial class TeamSelect : UserControl
    {
        #region Vars
        public delegate void OnSelectDone(Player.Team _Team, Player.PlayerModel _PlayerModel);
        public event OnSelectDone _OnSelectDone;
        Player.Team _Team;
        #endregion
        void TeamSelect_Loaded(object sender, RoutedEventArgs e)
        {
            _CTerroritsButton.Click += new RoutedEventHandler(CTerroritsButton_Click);
            _TerroristsButton.Click += new RoutedEventHandler(TerroristsButton_Click);
            _SpectatorButton.Click += new RoutedEventHandler(SpectatorButton_Click);
            _AutoSelectButton.Click += new RoutedEventHandler(AutoSelectButton_Click);
            Show();
        }
        private void OnTeamSelect(Player.Team team)
        {
            _Team = team;
            SwitchTo(_SelectPlayerGrid);
        }
        
        public TeamSelect()
        {
            Loaded += new RoutedEventHandler(TeamSelect_Loaded);
            InitializeComponent();                        
        }

        
        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
        public void Show()
        {
            SwitchTo(_SelectTeamGrid);
        }
        private void SwitchTo(FrameworkElement o)
        {
            this.Visibility = Visibility.Visible;
            _SelectPlayerGrid.Visibility = Visibility.Collapsed;
            _SelectTeamGrid.Visibility = Visibility.Collapsed;
            if(o!=null) o.Visibility = Visibility.Visible;
        }
        #region TeamSellect Events
        void AutoSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OnTeamSelect(Player.Team.auto);
        }

        void SpectatorButton_Click(object sender, RoutedEventArgs e)
        {
            OnTeamSelect(Player.Team.spectator);
        }

        void TerroristsButton_Click(object sender, RoutedEventArgs e)
        {
            OnTeamSelect(Player.Team.terr);
        }
                
        void CTerroritsButton_Click(object sender, RoutedEventArgs e)
        {
            OnTeamSelect(Player.Team.cterr);
        }
        #endregion
        #region PlayerSelect Events
        private void Phoenix_ButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            _OnSelectDone(_Team, Player.PlayerModel.phoenix);
        }
        #endregion
    }
}
