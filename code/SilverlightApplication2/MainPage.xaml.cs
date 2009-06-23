using doru;
using System; // Uri
using System.IO; // Stream
using System.Windows; // Application
using System.Windows.Controls; // TextBlock, Image
using System.Windows.Media.Imaging; // BitmapImage
using System.Net; // WebClient
using System.Windows.Resources;
using System.Collections.Generic; // StreamResourceInfo

namespace SilverlightApplication2
{
    public class TestClass
    {
        [SZ]
        public TestClass _TestClass;
        [SZ]
        public string test = "sdasd";
        [SZ]
        public int a = 16;
        public TestClass()
        {
            _TestClass = this;
        }
    }
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            TestClass _TestClass = new TestClass();
            var a =Type.GetType(typeof(List<TestClass>).ToString());

        }

    }
}    