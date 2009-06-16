using doru;
using System; // Uri
using System.IO; // Stream
using System.Windows; // Application
using System.Windows.Controls; // TextBlock, Image
using System.Windows.Media.Imaging; // BitmapImage
using System.Net; // WebClient
using System.Windows.Resources; // StreamResourceInfo

namespace SilverlightApplication2
{

    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
             BitmapImage bm = new BitmapImage();
             bm.SetSource(App.GetResourceStream(new Uri("1.jpg", UriKind.Relative)).Stream);
             Image _Image = new Image();
             _Image.Source = bm;
             _Grid.Children.Add(_Image);
             _Image.SetX(-bm.PixelWidth / 2);
             //Image _Image2 = new Image();
             //_Image2.Source = bm;
             //_Grid.Children.Add(_Image2);
             //_Image2.SetX(100);
            //MediaElement _MediaElement = new MediaElement();
            //_MediaElement.Source = new Uri("ak47-1.mp3", UriKind.Relative);
            //_Grid.Children.Add(_MediaElement);
        }

        private void NewMethod()
        {
            Uri uri = new Uri("ZIPPackageWithImage.zip", UriKind.Relative);
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(
                webClient_OpenReadCompleted);
            webClient.OpenReadAsync(uri);
        }

        void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Stream zipPackageStream = e.Result;
            Image image = LoadImageFromZipPackage("ImageInZipPackage.png", zipPackageStream);
            _Grid.Children.Add(image);
        }

        public Image LoadImageFromZipPackage(string relativeUriString, Stream zipPackageStream)
        {
            Uri uri = new Uri(relativeUriString, UriKind.Relative);
            StreamResourceInfo zipPackageSri = new StreamResourceInfo(zipPackageStream, null);
            StreamResourceInfo imageSri = Application.GetResourceStream(zipPackageSri, uri);


            BitmapImage bi = new BitmapImage();
            bi.SetSource(imageSri.Stream);
            Image img = new Image();
            img.Source = bi;

            return img;
        }
    }
}    //public class Uri : System.Uri
//{
//    public Uri(string s,UriKind ur):base(s,ur)
//    {

//    }
//    public static implicit operator Uri(string url)
//    {
//        return new Uri(url, UriKind.Relative);
//    }
//}