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

            InitializeComponent();
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
    }
}
