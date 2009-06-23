using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows;
using doru;

namespace LevelEditor4
{
	public class MapDatabase
	{
		public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(MapDatabase));
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
    public enum State { arrow_l, cross, pen_i }
    public class Botbase
    {
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Botbase));
        [SZ]
        public List<TreePoint> _TreePoints = new List<TreePoint>();
        [SZ]
        public List<TreePoint> _CStartPos = new List<TreePoint>();
        [SZ]
        public List<TreePoint> _TStartPos = new List<TreePoint>();

    }
    public class TreePoint
    {
        public override string ToString()
        {
            return "x:" + _x + "y:" + _y + "way1:" + _Way.Count;
        }
        [SZ]
        public List<TreePoint> _Way = new List<TreePoint>();
        [SZ]
        public double _x;
        [SZ]
        public double _y;
        [SZ]
        public double _Thickness;
    }
}
