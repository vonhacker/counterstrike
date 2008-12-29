using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CSL.Common
{
    public class InfoMessageBox
    {
        private InfoMessageBox() { }

        public static void Show(String message)
        {
            MessageBox.Show(message, "Info", MessageBoxButton.OK, 
                MessageBoxImage.Information, MessageBoxResult.OK, 
                MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
