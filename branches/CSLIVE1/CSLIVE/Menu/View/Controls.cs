using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CSLIVE.Menu.View
{

    public class ListBox : System.Windows.Controls.ListBox
    {
        public ListBox()
        {
            SelectionChanged += new SelectionChangedEventHandler(ListBox2_SelectionChanged);
        }

        void ListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIndex != -1)
                oldindex = SelectedIndex;
        }
        int oldindex;

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (this.Items.Count > oldindex)
                SelectedIndex = oldindex;
        }
    }
}

