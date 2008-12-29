using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSL.Common
{
    /// <summary>
    /// Interaction logic for WindowException.xaml
    /// </summary>
    public partial class WindowException : Window
    {
        public WindowException()
        {
            InitializeComponent();
        }

        public void SetText(Exception ex)
        {
            
            String exception = ex.ToString()
                            + "\n\n#### Message: ####\n" + ex.Message
                            + "\n\n#### Source: ####\n" + ex.Source
                            + "\n\n#### Stack trace: ####\n" + ex.StackTrace;
            textBoxException.Text = exception;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxException.Text);
        }
    }
}
