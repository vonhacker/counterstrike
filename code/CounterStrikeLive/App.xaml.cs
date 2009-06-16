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
using System.Diagnostics;
using System.Threading;

namespace CounterStrikeLive
{
    public partial class App : System.Windows.Application
    {
        public static int _ID = Random.Next(99);
        public App()
        {            
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }
        Menu menu;
        Menu _Menu{get{return menu;} set{menu = value;}}
        private void Application_Startup(object sender, StartupEventArgs e)
        {
                        
            this.RootVisual =_Menu= new Menu();
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if(!Debugger.IsAttached)
                MessageBox.Show(e.ExceptionObject.ToString(), "Fatal Error (press ctrl+c to copy)", MessageBoxButton.OK);
            if (Thread.CurrentThread.ThreadState == ThreadState.Background)
            {
                Debugger.Break(); 
                MessageBox.Show(e.ExceptionObject.ToString(), "Fatal Error (press ctrl+c to copy)", MessageBoxButton.OK);
            }
        }
        
    }
}
