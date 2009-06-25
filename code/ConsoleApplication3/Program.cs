using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ConsoleApplication3
{
    public class TestClass
    {     
        
        
        [SZ]
        public int a;
        [SZ]
        public List<object> list = new List<object>();
        [SZ]
        public TestClass _TestClass;
        public TestClass()
        {
            
        }
        public override string ToString()
        {
            return a+"";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            new Program();            
        }
        public Program()
        {
            object sa = new object();
            int a = GC.GetGeneration(this);

        }
        
    }
}
