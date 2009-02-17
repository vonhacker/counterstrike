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

namespace ConsoleApplication1
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logging.Setup("../../../Content/");
            new Program();            
        }
        
        public DataBase _DataBase = new DataBase();
        
        public Program()
        {
                    
            string[] dirs=Directory.GetDirectories(".");
            foreach (string dir in dirs)
            {
                
                DataBase.AnimatedBitmap an = new DataBase.AnimatedBitmap();
                an._Name =Path.GetFileName(dir);                
                foreach (string file in Directory.GetFiles(dir))
                    if (Regex.IsMatch(Path.GetExtension(file), "jpg|png", RegexOptions.IgnoreCase))
                        an._Bitmaps.Add(file.Replace(@"\", "/","./",""));
                
                if (an._Bitmaps.Count > 0)
                {
                    Image img = Bitmap.FromFile(an._Bitmaps.First());
                    an._Width = img.Width;
                    an._Height = img.Height;
                    _DataBase._AnimatedBitmaps.Add(an);
                    
                }
            }
            Common._XmlSerializer.Serialize("./db.xml", _DataBase);
            
            FastZip _FastZip = new FastZip();
            _FastZip.CreateZip("content.zip","./",true,@"\.jpg|\.png|.\xml");            
        }
        
    }
}
