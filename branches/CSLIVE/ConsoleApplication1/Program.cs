using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CSLIVE.Server;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        public Program()
        {
            Player _Player = new Player();
            //LocalSharedManager _SharedManager = new LocalSharedManager { _SharedObject = _Player, _OnBytesToSend = SharedManager__OnBytesToSend };
            _Player._name = "teasd";
        }

        void SharedManager__OnBytesToSend(byte[] bytes)
        {
            
        }

        public class Player : INotifyPropertyChanged
        {
            public string _name;
            [SharedObject(1)]
            public string name { get { return _name; } set { _name = value; PropertyChanged(this, new PropertyChangedEventArgs("name")); } }


            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion
        }
        
    }
}
