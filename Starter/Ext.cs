using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starter
{
    public static class EXT
    {

        public static T Next<T>(this IEnumerable<T> tt, T t) where T : class
        {

            T o = null;
            foreach (var a in tt)
            {
                if (o != null && o == t) return a;
                o = a;
            }
            return tt.First();
        }
        public static T Prev<T>(this IEnumerable<T> tt, T t) where T : class
        {
            T o = null;
            foreach (var a in tt)
            {
                if (o != null && a == t) return o;
                o = a;
            }
            return o;
        }



        public static T GetAt<T>(this IEnumerable<T> t, int i)
        {
            foreach (var a in t)
            {
                if (i == 0) return a;
                i--;
            }
            return default(T);
        }
    }
}
