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
using CSL.Common;

namespace CSL.LevelEditor
{
    /// <summary>
    /// Interaction logic for UserControlButtons.xaml
    /// </summary>
    public partial class UserControlButtons : UserControl
    {
        public UserControlButtons()
        {
            //ressources
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Open();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }

        }

        private void buttonLayer1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SelectCanvas(1);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }


        private void buttonLayer2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SelectCanvas(2);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonLayer3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SelectCanvas(3);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.RemoveLastStroke();

            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonSelectMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SetSelectMode();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonPolygon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SetPolygon();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }

        }

        private void buttonErase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Erase();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonScaleAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Scale(true);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonScaleSubtract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Scale(false);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }

        }

        private void buttonSelectColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SelectColor();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonPageUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SetPage(true);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonPageDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SetPage(false);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Copy();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Cut();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.Paste();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonKeyB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.KeyB();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

        private void buttonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LevelEditorManager.Instance.SaveMap();
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }

    }
}
