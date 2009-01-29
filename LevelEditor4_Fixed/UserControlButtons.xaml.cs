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
            
            InitializeComponent();
            
            //ressources
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);
            this.buttonOpen.Content = this.Resources["open_48_png"];
            this.buttonSaveFile.Content = this.Resources["save_48_png"];
            this.buttonLayer1.Content = this.Resources["x1_png"];
            this.buttonLayer2.Content = this.Resources["x2_png"];
            this.buttonLayer3.Content = this.Resources["x3_png"];
            this.buttonRemoveLast.Content = this.Resources["remove_png"];
            this.buttonSelectMode.Content = this.Resources["select_png"];
            this.buttonPolygon.Content = this.Resources["polygon_png"];
            this.buttonErase.Content = this.Resources["erase1_png"];
            this.buttonScaleAdd.Content = this.Resources["scale_up_png"];
            this.buttonScaleSubtract.Content = this.Resources["scale_down_png"];
            this.buttonSelectColor.Content = this.Resources["color_png"];
            this.buttonPageUp.Content = this.Resources["page_up_png"];
            this.buttonPageDown.Content = this.Resources["page_down_png"];
            this.buttonCopy.Content = this.Resources["copy_png"];
            this.buttonCut.Content = this.Resources["cut_png"];
            this.buttonPaste.Content = this.Resources["paste_png"];
            this.buttonKeyB.Content = this.Resources["stop_png"];

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
