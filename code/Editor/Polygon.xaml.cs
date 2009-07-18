using doru;
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

namespace Editor
{
    /// <summary>
    /// Interaction logic for Polygon.xaml
    /// </summary>
    public partial class Polygon : UserControl
    {
        public Polygon()
        {
            
            InitializeComponent();
            Loaded += new RoutedEventHandler(Polygon_Loaded);
        }
        public bool _Selected { get { return this.Get<bool>("Selected"); } set { SC(value); this.Set("Selected", value); } }

        
        private void SC(bool value)
        {
            if (value) _Window1._WallMenuItem.IsChecked = _wall;
            _Polygon.Stroke = value ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Black);
            
        }
        public Database.Polygon _base = new Database.Polygon();
        public bool _wall { get { return _base._iswall; } set { _base._iswall = value; } }
        Window1 _Window1 = Window1._This;
        public void Remove()
        {
            _Window1._PolygonsCanvas.Children.Remove(this);
        }
        void Polygon_Loaded(object sender, RoutedEventArgs e)
        {
            _Polygon.Tag = this;
            _Window1._WallMenuItem.IsChecked = _wall;
            _Polygon.Default();
            
        }
        public PointCollection Points { get { return _Polygon.Points; } set { _Polygon.Points = value; } }
    }
}
