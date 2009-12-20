using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Starter
{
    public class DB
    {
        //public string System.Windows.Forms.Keys key;
        public List<Folder> folders = new List<Folder>();
        public List<Item> favs = new List<Item>();
    }
    public class Folder
    {
        public string path;
        public int level = -1;
    }
    public class Item 
    {
        public Item Clone()
        {
            return (Item)MemberwiseClone();
        }        
        public long size;
        public string keyword;
        public DateTime dt;        
        public string path;
        public override bool Equals(object obj)
        {
            
            Item i = (Item)obj;
            return this.path == i.path;
        }
        public override int GetHashCode()
        {            
            return path.GetHashCode();
        }
    }
}
