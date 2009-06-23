using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;

using System.Windows.Controls;
using System.ComponentModel;


namespace doru
{
    public enum DebugState : int { Tcp = 13 }
    

    
    //[DebuggerStepThrough]
    public class ExceptionC : Exception
    {
        public ExceptionC(string s) : base(s) { }
        public override string ToString()
        {            
            return Message;
        }
    }
    //[DebuggerStepThrough]
    public class ExceptionB : Exception
    {
        public ExceptionB(string s) : base(s) { }
    }

    //[DebuggerStepThrough]
    public class ExceptionA : Exception { public ExceptionA(string s) : base(s) { } public ExceptionA() { } };

    //[DebuggerStepThrough]    
    public class Downloader
    {
        public delegate void OpenReadCompleted(Stream s);

        public void Download(string s, OpenReadCompleted _OpenReadCompleted)
        {
#if(SILVERLIGHT)
            WebClient _WebClient = new WebClient();
            _WebClient.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e)
            {
                _OpenReadCompleted(e.Result);
            };
            _WebClient.OpenReadAsync(new Uri(s,UriKind.Relative));
#else
            _OpenReadCompleted(File.OpenRead(s));
#endif
        }
    }
    //[DebuggerStepThrough]    


    
        



    //[DebuggerStepThrough]
    [Obsolete("use TimerA")]
    public static class STimer
    {
        public static void AddMethod(double _Time, Action _Action)
        {

            _Timer.AddMethod(_Time, _Action);
        }

        static TimerA _Timer = new TimerA();
        public static double _TimeElapsed { get { return _Timer._TimeElapsed; } }
        public static void Update()
        {
            _Timer.Update();
        }
        public static double _SecodsElapsed { get { return _Timer._TimeElapsed / 1000; } }
        public static bool TimeElapsed(int _Milisecconds)
        {
            return _Timer.TimeElapsed(_Milisecconds);
        }

        public static double? GetFps()
        {
            return _Timer.GetFps();
        }
    }
    [Obsolete("use TimerA")]
    //[DebuggerStepThrough]
    public class Timer2 : TimerA { }
    //[DebuggerStepThrough]
    public class TimerA
    {
        DateTime _DateTime = DateTime.Now;
        double oldtime;

        int fpstimes;
        double totalfps;
        public double GetFps()
        {
            if (fpstimes > 0)
            {
                double fps = (totalfps / fpstimes);
                fpstimes = 0;
                totalfps = 0;
                if (fps == double.PositiveInfinity) return 0;
                return fps;
            }
            else return 0;
        }
        double time;
        public void Update()
        {
            while ((DateTime.Now - _DateTime).TotalMilliseconds - oldtime == 0) Thread.Sleep(1);

            time = (DateTime.Now - _DateTime).TotalMilliseconds;
            _TimeElapsed = time - oldtime;
            oldtime = time;
            fpstimes++;
            totalfps += 1000 / _TimeElapsed;

            UpdateActions();
        }

        private void UpdateActions()
        {
            for (int i = _List.Count - 1; i >= 0; i--)
            {
                CA _CA = _List[i];
                _CA._Miliseconds -= _TimeElapsed;
                if (_CA._Miliseconds < 0)
                {
                    _List.Remove(_CA);
                    _CA._Action();
                }
            }
        }

        public double _TimeElapsed = 0;
        public double _SecodsElapsed { get { return _TimeElapsed / 1000; } }
        public double _oldTime { get { return time - _TimeElapsed; } }
        public bool TimeElapsed(double _Milisecconds)
        {
            if (_TimeElapsed > _Milisecconds) return true;
            if (time % _Milisecconds < _oldTime % _Milisecconds)
                return true;
            else
                return false;
        }

        public void AddMethod(double _Miliseconds, Action _Action)
        {
            if (_List.FirstOrDefault(a => a._Action == _Action) == null)
                _List.Add(new CA { _Action = _Action, _Miliseconds = _Miliseconds });
        }

        List<CA> _List = new List<CA>();
        class CA
        {
            public double _Miliseconds;
            public Action _Action;
        }
    }


    //first byte is length, if length is more than 254 then first byte is 255 second is uint16 packet length 


   
    
    //namespace BigEndian
    //{
    //    public static class Extensions
    //    {
    //        public static UInt32 ReadUInt32(this Stream _Stream)
    //        {
    //            return BitConverter.ToUInt32(_Stream.Read(4).ReverseA(), 0);
    //        }
    //        public static UInt16 ReadUInt16(this Stream _Stream)
    //        {
    //            return BitConverter.ToUInt16(_Stream.Read(2).ReverseA(), 0);
    //        }
    //        public static void WriteUint32(this Stream _Stream, UInt32 i)
    //        {
    //            _Stream.Write(BitConverter.GetBytes(i).ReverseA());
    //        }
    //        public static void WriteUint16(this Stream _Stream, UInt16 i)
    //        {
    //            _Stream.Write(BitConverter.GetBytes(i).ReverseA(2), 0, 2);
    //        }
    //    }
    //}
}
