
using System;
using System.Collections.Generic;

using System.Text;
using System.Xml.Serialization;
using System.IO;
using FarseerGames.FarseerPhysics.Mathematics;

namespace Editor
{
    public class Database
    {
        public static Database _This;
        public Database()
        {
            _This = this;
        }
        public Map _Map = new Map();
        public class Map
        {
            public List<Polygon> _Polygons = new List<Polygon>();
        }
        public static XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database), new Type[] { typeof(Polygon), typeof(Map), typeof(Vector2) });
        public class Polygon
        {
            public Vector2 _Position;
            public float _Rotation;
            public List<Vector2> _Points = new List<Vector2>();
            public bool _iswall;
        }
        
        

    }
    
}
