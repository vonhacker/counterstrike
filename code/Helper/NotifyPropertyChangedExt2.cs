using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace doru
{
    public static class NotifyPropertyChangedExt2
    {

        private static Dictionary<KK, object> _Vars = new Dictionary<KK, object>();
        public struct KK
        {
            public object obj;
            public string prop;
        }
        public static void Set<T>(this object _This, string s2, T o)
        {
            string s = _This.GetHashCode() + s2;
            KK kk = new KK { obj = _This, prop = s2 };
            if (!_Vars.ContainsKey(kk)) _Vars.Add(kk, _This);
            _Vars[kk] = o;
        }

        public static T Get<T>(this object _This, string s2)
        {
            KK kk = new KK { obj = _This, prop = s2 };

            if (_Vars.ContainsKey(kk))
                return (T)_Vars[kk];
            else
            {
                T t;
                try
                {
                    t = Activator.CreateInstance<T>();
                } catch { t = default(T); }
                _Vars.Add(kk, t);
                return t;
            }
        }

    }
}
