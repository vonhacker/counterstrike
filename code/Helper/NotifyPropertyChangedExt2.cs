using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;

namespace doru
{
    public static class NotifyPropertyChangedExt2
    {

        private static Dictionary<Key, object> _Vars = new Dictionary<Key, object>();
        public struct Key
        {
            public int hash;
            public string prop;
        }
        public static void DisposeValues(this object _This)
        {            
            
            foreach (var a in (from a in _Vars where a.Key.hash == _This.GetHashCode() select a))
                _Vars.Remove(a.Key);
        }
        public static void Set<T>(this object _This, string s2, T o)
        {            
            Key key = new Key { hash = _This.GetHashCode(), prop = s2 };
            _Vars.AddOrCreate(key, o);
        }
		public static T Get<T>(this object _This, string s2)
		{
			return Get<T>(_This, s2, null);
		}
        public static T Get<T>(this object _This, string s2, object _default)
        {
            Key kk = new Key { hash = _This.GetHashCode(), prop = s2 };

            if (_Vars.ContainsKey(kk))
                return (T)_Vars[kk];
            else
            {
                T t = _default != null? (T)_default : default(T);				
                _Vars.Add(kk, t);
                return t;
            }
        }

    }
    //[Obsolete]
    //public static class NotifyPropertyChangedExt
    //{

    //    private static Dictionary<string, DependencyProperty> _Vars = new Dictionary<string, DependencyProperty>();
    //    public static void Set<T>(this DependencyObject _This, string s2, T _BindTo)
    //    {
    //        string s = _This.GetHashCode() + s2;
    //        if (!_Vars.ContainsKey(s)) _This.Create(s, _BindTo);
    //        _This.SetValue(_Vars[s], _BindTo);
    //    }
    //    private static void Create<T>(this DependencyObject _This, string s2, T _BindTo)
    //    {
    //        string s = s2;
    //        DependencyProperty dp = DependencyProperty.Register(s, typeof(T), _This.GetType(), new PropertyMetadata(_BindTo));
    //        _Vars.Add(s, dp);
    //    }

    //    public static T Get<T>(this DependencyObject _This, string s2)
    //    {
    //        string s = _This.GetHashCode() + s2;
    //        if (_Vars.ContainsKey(s))
    //            return (T)_This.GetValue(_Vars[s]);
    //        else
    //        {
    //            T key = default(T); ;
    //            //try
    //            //{
    //            //    key = Activator.CreateInstance<T>();
    //            //} catch { key = default(T); }
    //            _This.Create(s, key);
    //            return key;
    //        }
    //    }

    //}
}
