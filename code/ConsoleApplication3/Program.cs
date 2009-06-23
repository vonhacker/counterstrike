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
            var a=Type.GetType(typeof(List<TestClass>).ToString());
             //var tp =Type.GetType(new List<string>().GetType().FullName);
            TestClass _TestClass = new TestClass();
            _TestClass.list.Add("sadsad");
            _TestClass.list.Add(3);
            _TestClass.list.Add(_TestClass);
            _TestClass.list.Add(new TestClass() { _TestClass = new TestClass { _TestClass = _TestClass } });

            Serializer _Serializer = new Serializer();
            MemoryStream ms = _Serializer.Serialize(_TestClass);
            TestClass _TestClass1 = (TestClass)new Deserializer().Deserialize(ms);
            Debugger.Break();
        }
        
    }
}
