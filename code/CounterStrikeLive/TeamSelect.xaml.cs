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
    public partial class TeamSelect : ChildWindow 
    {
        public TeamSelect()
        {
            InitializeComponent();            
            _Game = Game._This;
        }

    
        Game _Game;
       
        private void AutoSelectButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.AutoSelectClick();
            this.DialogResult = true;
            
        }
        private void CTerroritsButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.CTerroritsButtonClick();
            this.DialogResult = true;
        }
        private void SpectatorButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.SpectatorButtonClick();
            this.DialogResult = true;
            
        }

        private void TerroristsButton_Click(object sender, RoutedEventArgs e)
        {
            _Game.TerroristsButtonClick();
            this.DialogResult = true;
        }

        
    }
}
