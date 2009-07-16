using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;


using doru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Reflection;
using Vector2D=FarseerGames.FarseerPhysics.Mathematics.Vector2;

namespace WpfApplication3
{
    public class Item
    {
        static Window1 _Window1 = Window1._This;
        static State2 _State2 { get { return _Window1._State2; } }
        public Body _Body;
        public Polygon _Polygon = new Polygon().Default().AddTo(_Window1._RootCanvas);
        RotateTransform rt = new RotateTransform();
        public Point _startpos;

        public Item Load(IEnumerable<Point> points)
        {
            
            _startpos=GetCenter(points);
            IEnumerable<Vector2D> list = Move(points, _startpos);
            vertex = new Vertices(list.ToArray());
            vertex.SubDivideEdges(10);            
            Load();
            return this;

        }
        public IEnumerable<Vector2D> Move(IEnumerable<Point> ps,Point offset)
        {            
            foreach (Point p in ps)
            {
                yield return new Vector2D((float)(p.X - offset.X), (float)(p.Y-offset.Y));
            }            
        }

        private static Point GetCenter(IEnumerable<Point> ps)
        {
            return new Vertices(ps.ToVectors().ToArray()).GetCentroid().ToPoint();
        }


        bool wall { get { return _State2 == State2.wall; } }
        public Vertices vertex;
        public Item Load()
        {
            
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(rt);

            _Polygon.RenderTransform = tg;
            foreach (var v in vertex)
                _Polygon.Points.Add(new Point(v.X, v.Y));                        
        
            _Window1._Items.Add(this);
            return this;
        }
        
        public Item CreateBody()
        {            
            _Body = BodyFactory.Instance.CreatePolygonBody(engine, vertex, 1);            
            _Body.Position = _startpos.ToVector2();
            Geom geom = GeomFactory.Instance.CreatePolygonGeom(engine,_Body, vertex, 20);
            _Body.IsStatic = wall;
            geom.CollisionGroup = MathD.r.Next(int.MaxValue);
            
            return this;
        }

        public PhysicsSimulator engine { get { return _Window1.engine; } }
        public Item Update()
        {
            
            _Polygon.SetY(_Body.Position.Y);
            _Polygon.SetX(_Body.Position.X);
            rt.Angle = _Body.Rotation / Math.PI * 180;            
            return this;
        }
    }
}
