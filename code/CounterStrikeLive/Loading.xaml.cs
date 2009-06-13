using doru;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CounterStrikeLive
{
	public partial class Loading : UserControl
	{
        public string Text { set { TextBlock1.Text = value; this.Show(); Trace.WriteLine("Loading:" + value); } }
        public int Value { set { ProgressBar1.Value = value; this.Show(); } }
		public Loading()
		{
			// Required to initialize variables
			InitializeComponent();
		}
	}
}