using System;
using System.Collections.Generic;
using System.Configuration;

using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;

namespace CSLIVE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new Page();
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            App.Current.RootVisual.Dispatcher.BeginInvoke(delegate() { MessageBox.Show(e.ExceptionObject + ""); });
        }
    }
}
