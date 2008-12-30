using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CSL.LevelEditor
{
    public class MapDatabase
    {
        public class Image
        {
            public double Width;
            public double Height;
            public double X;
            public double Y;
            public string Path;
        }
        public Point TerroristStartPos;
        public Point CounterTerroristStartPos;
        public List<Layer> Layers = new List<Layer>();
        public class Layer
        {
            public List<Image> Images = new List<Image>();
            public List<Polygon> Polygons = new List<Polygon>();
        }
        public class Polygon
        {
            public Color Color;
            public List<Point> Points = new List<Point>();
        }
    }
}
