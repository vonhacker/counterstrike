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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSL.LevelEditor.Properties;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows.Ink;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Markup;
using CSL.Common;
namespace CSL.LevelEditor
{
    /// <summary>
    /// Interaction logic for WindowEditor         
    /// </summary>
    public partial class WindowEditor : Window
    {
        public WindowEditor()
        {
            LevelEditorManager.Instance.WindowEditor = this;

            //TODO: remove?
            String gamePath = System.IO.Path.GetFullPath("../../../");
            Directory.SetCurrentDirectory(gamePath);
            string s = Environment.CurrentDirectory;
            InitializeComponent();

            //ressources
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);
            this.Icon = (this.Resources["editor_ico"] as Image).Source;

            LoadSettings();
        }

        private void LoadSettings()
        {
            this.Left = Properties.Settings.Default.WindowLocation.X;
            this.Top = Properties.Settings.Default.WindowLocation.Y;
            this.Width = Properties.Settings.Default.WindowSize.Width;
            this.Height = Properties.Settings.Default.WindowSize.Height;
        }


        private void SaveSettings()
        {
            Properties.Settings.Default.WindowLocation = new Point(this.Left, this.Top);
            Properties.Settings.Default.WindowSize = new Size(this.Width, this.Height);
            Properties.Settings.Default.Save();
        }

        //TODO: remove???
       // public const string _Filter = "map (*.xml)|*.xml";

        public UserControlCanvas UserControlCanvas
        {
            get { return _userControlCanvas; }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            //TODO: check if we are save?
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
                throw;
            }
        }

    }
}
