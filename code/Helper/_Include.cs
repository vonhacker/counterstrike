using System;
using System.Collections.Generic;


using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Diagnostics;


namespace doru
{
    public class SZ : Attribute
    {

    }
#if (!SILVERLIGHT)
    public static class FileSerializer 
    {
        public static void Serialize(string file, object o)
        {
            File.WriteAllBytes(file, new Serializer().Serialize(o).ToArray());
        }
        public static T DeserualizeOrCreate<T>(string file, T t)
        {
            try
            {
                return (T)new Deserializer().Deserialize(new MemoryStream(File.ReadAllBytes(file)));
            } catch
            {
                Debug.WriteLine("could not open");
                return t;
            }
        }
        public static object Deserialize(string file)
        {
            return new Deserializer().Deserialize(new MemoryStream(File.ReadAllBytes(file)));
        }
    }
#endif
    public class Serializer
    {
        bool used;
        int id;        
        Dictionary<object, int> _dict = new Dictionary<object, int>();

        MemoryStream _ms = new MemoryStream();
        public MemoryStream Serialize(object o)
        {
            if (used) throw new Exception("used");
            used = true;
            return serialize(o);
        }
        private MemoryStream serialize(object o)
        {            
            id++;
            _dict.Add(o, id);
            _ms.WriteString(o.GetType().ToString());
            foreach (MemberInfo mi in o.GetType().GetMembers())
            {
                if (!mi.GetCustomAttributes(false).Any(a => a is SZ)) continue;
                if (mi is FieldInfo)
                {
                    FieldInfo p = (FieldInfo)mi;
                    object value = p.GetValue(o);
                    WriteValue(value);
                }
            }            
            return _ms;
        }

        private void WriteValue(object value)
        {            
            _ms.WriteByte(66);
            if (value == null)
            {
                _ms.WriteString(typeof(System.Reflection.Missing).ToString());
                return;
            }
            _ms.WriteString(value.GetType().ToString());
            if (value is int) _ms.Write((int)value);
            else if (value is string) _ms.WriteString((string)value);
            else if (value is Enum) _ms.Write(value.ToString());
            else if (value is float) _ms.Write((float)value);
            else if (value is byte) _ms.Write((byte)value);
            else if (value is bool) _ms.Write((bool)value);
            else if (value is double) _ms.Write((double)value);
            else if (value is IList)
            {
                var l= (IList)value;
                _ms.Write(l.Count);
                foreach (object o2 in l)
                {                    
                    WriteValue(o2);
                }
            } else
            {
                if (_dict.ContainsKey(value))
                    _ms.Write(_dict[value]);
                else
                {
                    _ms.Write(-1);
                    serialize(value);
                }
            }
        }

    }
    public class Deserializer
    {

        Dictionary<int, object> _dict = new Dictionary<int, object>();
        int id;
        bool used;
        public Stream ms;
        public object Deserialize(Stream ms)
        {
            if (used) throw new Exception(this + "used");
            used = true;            
            ms.Seek(0, SeekOrigin.Begin);
            this.ms = ms;
            object o = Deserialize();
            if (ms.Position != ms.Length) throw new Exception();
            return o;
        }
        private object Deserialize()
        {                        
            id++;
            string s = ms.ReadString();
            object _Object = Activator.CreateInstance(Type.GetType(s));
            _dict.Add(id, _Object);
            foreach (MemberInfo mi in _Object.GetType().GetMembers())
            {
                if (!mi.GetCustomAttributes(false).Any(a => a is SZ)) continue;
                if (mi is FieldInfo)
                {
                    FieldInfo _PropertyInfo = (FieldInfo)mi;
                    //Type type = _PropertyInfo.FieldType;
                    _PropertyInfo.SetValue(_Object, ReadValue());
                }
            }
            
            return _Object;
            
        }

        private object ReadValue()
        {
            byte b = ms.ReadB();
            string s = ms.ReadString();
            Type type = Type.GetType(s);
            if (type == null) throw new Exception("could not find type");
            if (b != 66) throw new Exception("error");
            if (type.Equals(typeof(System.Reflection.Missing)))
                return null;
            if (type.Equals(typeof(int)))
                return ms.ReadInt32();
            else if (type.Equals(typeof(string)))
                return ms.ReadString();
            else if (type.BaseType == typeof(Enum))
                return Enum.Parse(type, ms.ReadString(), false);
            else if (type.Equals(typeof(float)))
                return ms.ReadFloat();
            else if (type.Equals(typeof(double)))
                return ms.ReadDouble();
            else if (type.Equals(typeof(bool)))
                return ms.ReadBoolean();
            else if (type.GetInterface("IList",false) != null)
            {
                object l = Activator.CreateInstance(type);

                int count = ms.ReadInt32();

                for (int i = 0; i < count; i++)
                    l.GetType().GetMethod("Add").Invoke(l, new object[] { ReadValue() });

                return l;
            } else if (type.Assembly != Assembly.GetCallingAssembly()) throw new Exception("not our type");
            else
            {
                int id2 = ms.ReadInt32();
                if (id2 == -1)
                    return Deserialize();
                else
                    return _dict[id2];
            }

        }
    }
}
