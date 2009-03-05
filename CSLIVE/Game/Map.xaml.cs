using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using CSLIVE.Game;
using System.Windows.Data;



namespace CSLIVE.Game
{

    
    public partial class Map : UserControl//map, parses map.xml, provides collision methods 
    {
        public Map() //sets to Content _Canvas
        {
            InitializeComponent();
        }
                
        public Point GetPos(Player.Team _Team)
        {
            if (_Team == Player.Team.cterr)
            {
                return _MapDatabase._CStartPos;
            }
            return _MapDatabase._TStartPos;
        }

        MapDatabase _MapDatabase;
        Random _Random = new Random();
        
        public static Dictionary<string, Stream> _Resources = new Dictionary<string, Stream>();
        public List<UIElement> _List = new List<UIElement>();
        public void Load()
        {            
            Trace.Assert(_Resources.ContainsKey("map.xml"));
            _MapDatabase = (MapDatabase)Common._XmlSerializerMap.Deserialize(_Resources["map.xml"]);
            foreach (MapDatabase.Layer _Layer in _MapDatabase._Layers)
            {
                foreach (MapDatabase.Image _dImage in _Layer._Images)
                {
                    Image _Image = new Image();
                    BitmapImage bm = new BitmapImage();
                    bm.SetSource(_Resources[_dImage.Path]);
                    _Image.Source = bm;
                    _Image.Width = _dImage.Width;
                    _Image.Height = _dImage.Height;                    
                    LayoutRoot.Children.Add(_Image);
                    Canvas.SetLeft(_Image, _dImage.X);
                    Canvas.SetTop(_Image, _dImage.Y);
                }
                foreach (MapDatabase.Polygon _dPolygon in _Layer._Polygons)
                {
                    if (_dPolygon._Color != Colors.Black)
                    {

                        if (_dPolygon._Points.First() == _dPolygon._Points.Last())
                        {
                            Polygon _Polygon = new Polygon();
                            foreach (Point _Point in _dPolygon._Points)
                            {
                                _Polygon.Points.Add(_Point);
                            }

                            _Polygon.Fill = new SolidColorBrush(_dPolygon._Color);

                            LayoutRoot.Children.Add(_Polygon);
                        } else
                        {
                            Polyline _Polygon = new Polyline();
                            _Polygon.Stroke = new SolidColorBrush(_dPolygon._Color);
                            _Polygon.StrokeThickness = 3;
                            foreach (Point _Point in _dPolygon._Points)
                            {
                                _Polygon.Points.Add(_Point);
                            }

                            LayoutRoot.Children.Add(_Polygon);
                        }
                    } else _Polygons.Add(_dPolygon);
                }
            }
        }
        
        List<MapDatabase.Polygon> _Polygons = new List<MapDatabase.Polygon>();        

        //public class LV
        //{
        //    public Vector _Vector;
        //    public Line2 _Line2;
        //}
        //public Line2 CCollision(Vector pos1, float r)
        //{
        //    foreach (MapDatabase.Polygon _Polygon in _Polygons)
        //    {
        //        Vector? _oldPoint = null;
        //        foreach (Point _p in _Polygon._Points)
        //        {
        //            Vector _point = new Vector((float)_p.GetX, (float)_p.GetY);
        //            if (_oldPoint != null)
        //            {
        //                Vector Temp;
        //                float dist = doru.WPF.Vectors.Calculator.DistanceBetweenPointAndLineSegment(pos1, _point, _oldPoint.Value, out Temp);
        //                if (dist < r)
        //                {
        //                    return new Line2 { _p1 = _oldPoint.Value, _p2 = _point, _cpoint = Temp };
        //                }
        //            }
        //            _oldPoint = _point;
        //        }
        //    }
        //    return null;
        //}

        //public List<LV> Collision(Vector pos2, Vector pos1, out Line2 _wall)
        //{
        //    _wall = null;
        //    List<LV> _HitPoints = new List<LV>();
        //    foreach (MapDatabase.Polygon _Polygon in _Polygons)
        //    {
        //        Vector? _oldPoint = null;
        //        foreach (Point _p in _Polygon._Points)
        //        {
        //            Vector _point = new Vector((float)_p.GetX, (float)_p.GetY);
        //            if (_oldPoint != null)
        //            {
        //                Vector? hitPoint = Physics.LineCollision(pos1, pos2, _point, _oldPoint.Value, true);

        //                if (hitPoint != null)
        //                {
        //                    Line2 _Line2 = new Line2 { _p1 = _point, _p2 = _oldPoint.Value };
        //                    _HitPoints.Add(new LV { _Vector = hitPoint.Value, _Line2 = _Line2 });
        //                }
        //            }
        //            _oldPoint = _point;
        //        }
        //    }
        //    _HitPoints.Sort(new Comparison<LV>(delegate(LV a, LV b)
        //    {
        //        float adist = Vector.Distance(pos1, a._Vector);
        //        float bdist = Vector.Distance(pos1, b._Vector);
        //        if (adist > bdist) return 1;
        //        else if (adist < bdist) return -1;
        //        else return 0;
        //    }));
        //    return _HitPoints;
        //}
    }
}
