#if Physics2D
//using Physics2DDotNet;
//using Physics2DDotNet.Shapes;
//using AdvanceMath;
//using Physics2DDotNet.Detectors;
//using Physics2DDotNet.Solvers;
//using Physics2DDotNet.PhysicsLogics;
#else
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using Vector2D = FarseerGames.FarseerPhysics.Mathematics.Vector2;
#endif

using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using System.Net.Sockets;

using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Reflection;


namespace WpfApplication3
{
    public static class EXT
    {
        public static IEnumerable<Vector2D> ReverenceCorrect(IEnumerable<Vector2D> points)
        {

            float ttl = 0;
            Vector2D? oldp = null, oldv = null;
            foreach (Vector2D p in points)
            {
                if (oldp != null)
                {
                    Vector2D v = p - oldp.Value;
                    if (oldv != null)

                        ttl += MathD.ClampAngle(v.Angle - oldv.Value.Angle);
                    oldv = v;
                }
                oldp = p;
            }

            if (ttl > 0)
                return points;
            else
                return points.Reverse();
        }

        public static Point GetCenter(IEnumerable<Point> ps)
        {
            double xm = 0, ym = 0;
            foreach (Point p in ps)
            {
                xm += p.X;
                ym += p.Y;
            }
            Point center = new Point(xm / ps.Count(), ym / ps.Count());
            return center;            

        }
        public static IEnumerable<Vector2> ToVectors(this IEnumerable<Point> ie)
        {
            foreach (var a in ie)
                yield return a.ToVector2();
        }
        public static Point ToPoint(this Vector2 v)
        {
            return new Point(v.X, v.Y);
        }
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> ie, T item)
        {
            
            foreach (var a in ie)
                if(!item.Equals(a))
                    yield return a;            
        }
        public static IEnumerable<T> Add<T>(this IEnumerable<T> ie,T item)
        {
            foreach (var a in ie)
                yield return a;
            yield return item;
        }
        public static UIElement Remove(this FrameworkElement nw)
        {
            ((Panel)nw.Parent).Children.Remove(nw);
            return nw;
        }
        
        public static Polygon ToPolygon(this Polyline old)
        {
            Polygon nw = new Polygon();

            Panel pnl = old.Parent as Panel;
            pnl.Children.Remove(old);
            pnl.Children.Add(nw);
            nw.Fill = old.Fill;
            nw.StrokeThickness = old.StrokeThickness;
            nw.Stroke = old.Stroke;
            nw.Points = old.Points;
            return nw;
        }
        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2((float)p.X, (float)p.Y);
        }
        //public static object CastTo<T>(this T t, object t2)
        //{
        //    foreach (PropertyInfo a in t.GetType().GetProperties())
        //    {
        //        try
        //        {
        //            t2.GetType().GetProperty(a.Name).SetValue(t2, a.GetValue(t, null), null);
        //        } catch { }
        //    }
        //    return t2;
        //}
        //public static T2 CastTo<T, T2>(this T t) where T : IEnumerable<T>
        //{
        //    //foreach(T t in 
        //    T2 t2 = Activator.CreateInstance<T2>();
        //    foreach (PropertyInfo a in t.GetType().GetProperties())
        //    {
        //        try
        //        {
        //            t2.GetType().GetProperty(a.Name).SetValue(t2, a.GetValue(t, null), null);
        //        } catch { }
        //    }
        //    return t2;
        //}
        
        
        
    }
}
