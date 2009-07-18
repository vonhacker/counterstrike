using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Linq;
using doru.Mathematics;
using doru.VectorWorld;
using doru;
using LevelEditor4;
using doru.Vectors;

namespace CounterStrikeLive
{    
    public class Map
    {
        public static Map _This;
        public Map()
        {
            _This = this;
        }
        
        public Point GetPos(Player.Team _Team)
        {
            if(_Team == Player.Team.cterr)
            {
                return _MapDatabase._CStartPos;
            }
            return _MapDatabase._TStartPos;
        }

        public doru.Mathematics.Line2 CCollision(Vector2 pos1, float r)
        {
            foreach(MapDatabase.Polygon _Polygon in _Polygons)
            {
                Vector2? _oldPoint = null;
                foreach(Point _p in _Polygon._Points)
                {
                    Vector2 _point = new Vector2((float)_p.X, (float)_p.Y);
                    if(_oldPoint != null)
                    {
                        Vector2 Temp;
                        float dist = DCalculator.DistanceBetweenPointAndLineSegment(pos1, _point, _oldPoint.Value, out Temp);
                        if(dist < r)
                        {
                            return new Line2 { _p1 = _oldPoint.Value, _p2 = _point, _cpoint = Temp };
                        }
                    }
                    _oldPoint = _point;
                }
            }
            return null;
        }        
        
        public List<LV> Collision(Vector2 pos2, Vector2 pos1, out doru.Mathematics.Line2 _wall)
        {
            
            _wall = null;
            List<LV> _HitPoints = new List<LV>();
            foreach(MapDatabase.Polygon _Polygon in _Polygons)
            {
                Vector2? _oldPoint = null;
                foreach(Point _p in _Polygon._Points)
                {
                    Vector2 _point = new Vector2((float)_p.X, (float)_p.Y);
                    if(_oldPoint != null)
                    {
                        Vector2? hitPoint = Physics.LineCollision(pos1, pos2, _point, _oldPoint.Value, true);

                        if(hitPoint != null)
                        {
                            Line2 _Line2 = new Line2 { _p1 = _point, _p2 = _oldPoint.Value };
                            _HitPoints.Add(new LV { _Vector2 = hitPoint.Value, _Line2 = _Line2 });
                        }
                    }
                    _oldPoint = _point;
                }
            }
            _HitPoints.Sort(new Comparison<LV>(delegate(LV a, LV b)
            {
                float adist = Vector2.Distance(pos1, a._Vector2);
                float bdist = Vector2.Distance(pos1, b._Vector2);
                if(adist > bdist) return 1;
                else if(adist < bdist) return -1;
                else return 0;
            }));
            return _HitPoints;
        }

        MapDatabase _MapDatabase;
        
        public List<Vector2D> walls = new List<Vector2D>();
        Menu _Menu = Menu._This;
        public Botbase _Botbase;
        public void LoadMap(MapDatabase _MapDatabase)
        {
            if (Menu._SinglePlayer)
            {
                _Botbase = (Botbase)new Deserializer().Deserialize(Menu._Resources["bots.raw"]);
                foreach (TreePoint tp in _Botbase._TreePoints)
                    foreach (TreePoint tp2 in tp._Way)
                        tp2._Way.Add(tp);
            }
            this._MapDatabase = _MapDatabase;
            foreach(MapDatabase.Layer _Layer in _MapDatabase._Layers)
            {
                foreach(MapDatabase.Image _dImage in _Layer._Images)
                {
                    Image _Image = new Image();
                    BitmapImage bm = new BitmapImage();
                    bm.SetSource(Menu._Resources[_dImage.Path]);
                    _Image.Source = bm; //= new BitmapImage(new Uri(_dImage.Path, UriKind.Relative));                                        
                    _Image.Width = _dImage.Width;
                    _Image.Height = _dImage.Height;
                    _Canvas.Children.Add(_Image);
                    Canvas.SetLeft(_Image, _dImage.X);
                    Canvas.SetTop(_Image, _dImage.Y);
                }
                foreach(MapDatabase.Polygon _dPolygon in _Layer._Polygons)
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

                            _Canvas.Children.Add(_Polygon);
                        } else
                        {
                            Polyline _Polygon = new Polyline();
                            _Polygon.Stroke = new SolidColorBrush(_dPolygon._Color);
                            _Polygon.StrokeThickness = 3;
                            foreach (Point _Point in _dPolygon._Points)
                            {
                                _Polygon.Points.Add(_Point);
                            }

                            _Canvas.Children.Add(_Polygon);
                        }
                    } else
                    {
                        Point? p1 = null;
                        foreach (Point p2 in _dPolygon._Points)
                        {
                            
                            if (p1 != null)
                                walls.Add(new Vector2D(p1.Value, p2));
                            p1 = p2;
                        }
                        _Polygons.Add(_dPolygon);

                    }
                }
            }
            _Canvas.Children.Add(_Canvas1);
        }
        public Canvas _Canvas1 = new Canvas();
        public Canvas _Canvas = new Canvas();
        List<MapDatabase.Polygon> _Polygons = new List<MapDatabase.Polygon>();        
        public class LV
        {
            public Vector2 _Vector2;
            public doru.Mathematics.Line2 _Line2;
        }
    }

}
