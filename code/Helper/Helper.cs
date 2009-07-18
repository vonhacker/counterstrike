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
    public partial class H
    {
        public static DebugState _TraceState { get { return (DebugState)int.Parse(Resource1._TraceLevel); } }
    }
    
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
    public class Timer2 : TimerA { }
    //[DebuggerStepThrough]
    


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
