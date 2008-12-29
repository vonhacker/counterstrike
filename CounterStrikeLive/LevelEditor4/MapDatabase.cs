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
        public Point _TStartPos;
        public Point _CStartPos;
        public List<Layer> _Layers = new List<Layer>();
        public class Layer
        {
            public List<Image> _Images = new List<Image>();
            public List<Polygon> _Polygons = new List<Polygon>();
        }
        public class Polygon
        {
            public Color _Color;
            public List<Point> _Points = new List<Point>();
        }
    }
}
