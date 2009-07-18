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


namespace BallGame
{
    public class Game
    {
        public static Game _This;
        public Game() { _This = this; }
        public List<EPolygon> _EPolygons = new List<EPolygon>();

        public Database db;
        IEnumerable<IPolygon> GetIPolygons()
        {
            foreach (var ip in _EPolygons)
                yield return ip;
        }
        Dx _Dx;
        static Form1 _Form1 { get { return Form1._This; } }
        public void Load()
        {
            
            //Debugger.Break();
            new Camera();
            LoadXml();
            LoadDb();
            _Dx = new Dx();
            _Dx._Polygons = GetIPolygons();
            _Dx.Load(_Form1);
            loaded = true;
        }
        public static Stream GetFile(string file)
        {
                return Form1._Assembly.GetManifestResourceStream("BallGame." + file);
        }
        void LoadDb()
        {
            H.Assert(db._Map._Polygons.Count > 0);
            foreach (Database.Polygon dp in db._Map._Polygons)
                new Item { _base = dp  }.Load();

        }
        public static bool loaded;
        void LoadXml()
        {
            db = (Database)Database._XmlSerializer.Deserialize(GetFile("db.xml"));

        }
        public Camera _CurCam { get { return Camera._Current; } }

        internal void Draw()
        {
            if (loaded)
                _Dx.Draw(_CurCam._Pos.X,_CurCam._Pos.Y);

        }
        public PhysicsSimulator _ps = new PhysicsSimulator(new Vector2(0,20));


        internal void Update()
        {
            if (!loaded) return;            
            _ps.Update((float)(_TimerA._SecodsElapsed > .2 ? .2 : _TimerA._SecodsElapsed));
            foreach (var p in _EPolygons) p.Update();
            
            _TimerA.Update();
        }
        public static TimerA _TimerA = new TimerA();
    }
}
