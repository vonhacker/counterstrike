using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FarseerGames.FarseerPhysics.Mathematics;

namespace Editor
{
    public static class Ext
    {
        public static IEnumerable<Point> ToPoints(this IEnumerable<Vector2> v2) { return v2.Select(a => new Point(a.X, a.Y)); }
    }
}
