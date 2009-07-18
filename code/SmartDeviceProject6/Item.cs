using doru;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Editor;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using PolygonCuttingEar;
using GeometryUtility;
//using Microsoft.WindowsMobile.DirectX.Direct3D;

namespace BallGame
{
    public class Item : EPolygon
    {
        public static new Item _This;
        public Item()
        {
            _This = this;
        }        
        public override EPolygon Load()
        {
            base.Load();                                 
            
            CreateBody();                
            return this;
        }
        Body _Body;
        static PhysicsSimulator ps = Game._This._ps;
        bool _wall { get { return _base._iswall; } }
        Geom geom;
        public Item CreateBody()
        {
            _Body = BodyFactory.Instance.CreatePolygonBody(ps, _Vertices, 1);
            _Body.Position = _Position;
            geom = GeomFactory.Instance.CreatePolygonGeom(ps, _Body, _Vertices, 20);
            _Body.IsStatic = _wall;
            geom.CollisionGroup = H.r.Next(int.MaxValue);
            
            return this;
        }
        
        public override void Update()
        {
            _Position = _Body.Position;
            _Rotation = _Body.Rotation;
            base.Update();
        }

        private Vector2 GetCenter()
        {
            return _Vertices.GetCentroid();

        }
        public override IEnumerable<Vector2> GetPoints()
        {
            return geom.WorldVertices;
        }
    }
    public class EPolygon : doru.IPolygon
    {
        public static EPolygon _This;
        static Game _Game { get { return Game._This; } }
        static Dx _Dx { get { return Dx._This; } }
        public EPolygon()
        {
            _This = this;
        }
        public virtual IEnumerable<Vector2> GetPoints()
        {
            throw new Exception();
            //foreach (Vector2 v2 in _Vertices)
            //{
            //    Vector2 v = v2;
            //    Calculator.RotateVector(ref v, _Rotation);
            //    yield return v + _Position - _Game._CurCam._Offset;                
                
            //}
        }
        internal virtual Vector2 _Position { get; set; }
        internal virtual float _Rotation { get; set; }
        public Vertices _Vertices;
        public Database.Polygon _base;
        public virtual void Update()
        {

        }
        public bool loaded;
        int _trianglecount;
        public virtual EPolygon Load()
        {
            H.Assert(_base._Points.Count > 2);
            H.Assert(!loaded);
            loaded = true;
            _Vertices = new Vertices(_base._Points.ToArray());


            //CPolygonShape cp = new CPolygonShape(H.ToArray(ToCpoints(_Vertices)));
            //cp.CutEar();
            //_trianglecount += cp.NumberOfPolygons;
            //vertex = H.ToArray(H.Concat(vertex, ToVertex(cp.Polygons())));

            _Game._EPolygons.Add(this);
            _Position = _Vertices.GetCentroid();
            Vector2 inv = -_Position;
            _Vertices.Translate(ref inv);
            return this;
        }
        //CustomVertex.PositionColored[] vertex;
        //public static IEnumerable<CustomVertex.PositionColored> ToVertex(IEnumerable<CPoint2D[]> polygons)
        //{
        //    foreach (CPoint2D[] polygon in polygons)
        //        foreach (CPoint2D cp in polygon)
        //            yield return new CustomVertex.PositionColored((float)cp.X, (float)cp.Y, 0, Color.Black.ToArgb());
        //}

        //public static IEnumerable<CPoint2D> ToCpoints(IEnumerable<Vector2> points)
        //{

        //    foreach (Vector2 p in points)
        //        yield return new CPoint2D(p.X, p.Y);
        //}

    }
}
