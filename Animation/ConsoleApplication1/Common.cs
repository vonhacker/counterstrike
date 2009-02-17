using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using doru;

namespace ConsoleApplication1
{
    public class Common
    {
        public static XmlSerializer _XmlSerializer = Helper.CreateSchema("content", typeof(DataBase));
        
    }
    public class DataBase
    {
        public AnimatedBitmap Get(string s)
        {
            return _AnimatedBitmaps.FirstOrDefault(a => a._Name == s);
        }
        public List<AnimatedBitmap> _AnimatedBitmaps = new List<AnimatedBitmap>();
        public class AnimatedBitmap
        {
            public string _Name;
            public int _Width;
            public int _Height;
            public List<string> _Bitmaps = new List<string>();
        }
    }
    
    
}
