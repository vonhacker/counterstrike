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
using System.Configuration;

namespace ConsoleApplication3
{
    public class intlist : List<int> { }
    class Program
    {
        static string[] args { get { return Environment.GetCommandLineArgs(); } }
        static void Main(string[] args)
        {            
            new Program();
        }
        public IEnumerable<char> ToCharList(string s)
        {
            foreach (char q in s)
                yield return q;
        }
        public List<string> strs = new List<string>() {"a","b","c" };

        public Program()        
        {
            var a= strs.Cast<string>();
            strs.Remove("a");
            var b=a.ToArray();
            //IEnumerable<char> test = ;
            
            //Settings.Default.Setting = 2;
            //Settings.Default.Save();
            
            Console.WriteLine(Settings.Default.Setting);
            Console.ReadLine();
        }
  

    }
}
