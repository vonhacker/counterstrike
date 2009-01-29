using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

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
        [XmlElement("_TStartPos")]
        public Respawn TerroristStartPos;
        [XmlElement("_CStartPos")]
        public Respawn CounterTerroristStartPos;
        [XmlElement("_Layers", typeof(List<Layer>))]
        public List<Layer> Layers = new List<Layer>();
        public class Layer
        {
            [XmlElement("_Images", typeof(List<Image>))]
            public List<Image> Images = new List<Image>();
            [XmlElement("_Polygons", typeof(List<Polygon>))]
            public List<Polygon> Polygons = new List<Polygon>();
        }
        public class Polygon
        {
            [XmlElement("_Color")]
            public Color Color;
            [XmlElement("_Points", typeof(List<Point>))]
            public List<Point> Points = new List<Point>();
        }
    }
}
