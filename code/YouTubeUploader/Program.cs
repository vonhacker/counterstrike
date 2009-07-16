using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using YouTubeUploader.Properties;

namespace YouTubeUploader
{

    public class DB
    {
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(DB));
        public static DB _This = DB._XmlSerializer.DeserealizeOrCreate("db.xml", new DB());        
        public string Title{get;set;}
        public string Disc { get; set; }
        public string Keys { get; set; }
        public List<int> SizeList = new List<int>();
        public void Save()
        {
            _XmlSerializer.Serialize("db.xml", this);
        }
    }
    public class Program
    {
        public static DB _DB { get { return DB._This; } }
        static string[] args;
        [System.STAThreadAttribute()]
        static void Main(string[] args)
        {

            new Program(args);
        }
        
        public static string fn;
        public static int filelen;

        public Program(string[] args)
        {
            Logging.Setup();
            if (args.Length == 0) Program.args = new string[] { "1.jpg" };
            else Program.args = args;
            fn = Program.args[0];
            filelen = (int)new FileInfo(fn).Length;
            
            
            if (args.Length > 0 || Debugger.IsAttached)
            {
                if (Window1._IntList.Contains(filelen))
                    if (MessageBox.Show("File Already Uploaded, Reupload", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                new Application { StartupUri = new Uri("Window1.xaml", UriKind.Relative) }.Run();
            }
             else
            {                
                Shell.Register("GomPlayer.avi", "upload to Youtube");
                this.Trace("registered");
            }
        }
        

    }
}
