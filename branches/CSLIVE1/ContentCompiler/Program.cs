using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Xml.Serialization;

namespace CSLIVE.ContentCompiler
{
    public class Program
    {
        public string _ContentDirectory = "./ContentCompiler/Content/";
        public GameContentDataBase _GameContentDataBase = new GameContentDataBase();
        static void Main(string[] args)
        {
            Logging.Setup("../../../");
            new Program();            
        }                        
        public Program()
        {

            string[] dirs = Directory.GetDirectories(_ContentDirectory);
            
            foreach (string dir in dirs)
            {                
                GameContentDataBase.AnimatedBitmap an = new GameContentDataBase.AnimatedBitmap();
                an._Name =Path.GetFileName(dir);                
                foreach (string file in Directory.GetFiles(dir))
                    if (Regex.IsMatch(Path.GetExtension(file), "jpg|png", RegexOptions.IgnoreCase))
                        an._Bitmaps.Add(file.Strstr(_ContentDirectory).Replace(@"\", "/","./",""));
                
                if (an._Bitmaps.Count > 0)
                {
                    Image img = Bitmap.FromFile(_ContentDirectory+an._Bitmaps.First());
                    an._Width = img.Width;
                    an._Height = img.Height;
                    _GameContentDataBase._AnimatedBitmaps.Add(an);
                    
                }
            }
            Common._XmlSerializerContent.Serialize(_ContentDirectory + "/content.xml", _GameContentDataBase);

            FastZip _FastZip = new FastZip();
            _FastZip.CreateZip(Common._ClientBinPath + "/content.zip", _ContentDirectory, true, @"\.jpg|\.png|.\xml");            
        }
        
    }
}
