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
    

    #region SharedObjectProviderClasses
    public interface ISh
    {
        object _SharedObj { get; set; }
    }
    public class SharedObjectAttribute : Attribute
    {
        public int _Priority;
        public SharedObjectAttribute() { }
        public SharedObjectAttribute(int _Priority)
        {
            this._Priority = _Priority;
        }
    }

    public abstract class SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public T _Object = Activator.CreateInstance<T>();
        protected List<PropertyInfo> _Properties;
        public static implicit operator T(SharedObj<T> s)
        {
            return s._Object;
        }
        public SharedObj()
        {
            _Properties = (from p in _Object.GetType().GetProperties()
                           from a in p.GetCustomAttributes(true)
                           where a is SharedObjectAttribute
                           orderby (a as SharedObjectAttribute)._Priority
                           select p).ToList();
            if (_Properties.Count == 0) throw new Exception("sharedobject does not have attributes");
            _Object._SharedObj = this;
        }
    }
    public class LocalSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public MemoryStream _ms = new MemoryStream();
        public LocalSharedObj()
            : base()
        {
            _Object.PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);
        }
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serialize(ms);
                return ms.ToArray();
            }
        }
        public void Serialize(MemoryStream ms)
        {
            for (int i = 0; i < _Properties.Count; i++)
                if (_Properties[i].GetCustomAttributes(true).Any(a => a is SharedObjectAttribute))
                    WriteBytes(i, ms);
        }
        public byte[] GetChanges()
        {
            if (_ms.Length == 0) return null;
            using (MemoryStream ms2 = _ms)
            {
                _ms = new MemoryStream();
                return ms2.ToArray();
            }
        }
        private void WriteBytes(int i, MemoryStream _ms)
        {
            PropertyInfo _PropertyInfo = _Properties[i];
            object value = _PropertyInfo.GetValue(_Object, null);

            _ms.WriteByte((byte)i);
            if (value is int) _ms.Write((Int16)(int)value);
            else if (value is string) _ms.WriteString((string)value);
            else if (value is Enum) _ms.Write(value.ToString());
            else if (value is float) _ms.Write((float)value);
            else if (value is byte) _ms.Write((byte)value);
            else if (value is bool) _ms.Write((bool)value);
            else if (value is double) _ms.Write((double)value);
            else throw new Exception("Shared Send Unkown value");

        }

        void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int i = _Properties.IndexOf(_Object.GetType().GetProperty(e.PropertyName));
            Trace.Assert(i != -1);
            WriteBytes(i, _ms);
        }
    }
    public class RemoteSharedObj<T> : SharedObj<T> where T : class, INotifyPropertyChanged, ISh
    {
        public bool _Serialized = false;
        public RemoteSharedObj() : base() { }
        public event Action _OnSerialized;
        public void OnBytesToRead(MemoryStream ms)
        {
            while (ms.Position != ms.Length)
            {
                int id = ms.ReadByte();
                PropertyInfo _PropertyInfo = _Properties[id];
                if (_PropertyInfo.GetCustomAttributes(true).FirstOrDefault(a => a is SharedObjectAttribute) == null)
                    throw new Exception("Break");
                Type type = _PropertyInfo.PropertyType;
                if (type.IsAssignableFrom(typeof(int)))
                    _PropertyInfo.SetValue(_Object, ms.ReadInt16(), null);
                else if (type.IsAssignableFrom(typeof(string)))
                    _PropertyInfo.SetValue(_Object, ms.ReadString(), null);
                else if (type.BaseType == typeof(Enum))
                    _PropertyInfo.SetValue(_Object, Enum.Parse(type, ms.ReadString(), false), null);
                else if (type.IsAssignableFrom(typeof(float)))
                    _PropertyInfo.SetValue(_Object, ms.ReadFloat(), null);
                else if (type.IsAssignableFrom(typeof(bool)))
                    _PropertyInfo.SetValue(_Object, ms.ReadBoolean(), null);
                else throw new Exception("Break");
            }
            if (_Serialized == false)
            {
                if (_OnSerialized != null) _OnSerialized();
                _Serialized = true;
            }
        }

    }
    #endregion
}
