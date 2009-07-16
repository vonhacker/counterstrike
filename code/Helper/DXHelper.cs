using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsMobile.DirectX.Direct3D;
using System.Windows.Forms;
using System.Drawing;
using PolygonCuttingEar;
using GeometryUtility;
using FarseerGames.FarseerPhysics.Mathematics;
using System.IO;
using System.Diagnostics;
using System.Collections;


namespace doru
{
    public interface IPolygon
    {
        IEnumerable<Vector2> GetPoints();
    }
    

    public class Dx
    {
        
        public static IEnumerable<CustomVertex.TransformedColored> ToVertex(IEnumerable<CPoint2D[]> polygons)
        {
            foreach (CPoint2D[] polygon in polygons)
                foreach (CPoint2D cp in polygon)
                    yield return new CustomVertex.TransformedColored((float)cp.X, (float)cp.Y, .5f, 1, Color.Black.ToArgb());
        }

        public static IEnumerable<CPoint2D> ToCpoints(IEnumerable<Vector2> points)
        {
            
            foreach (Vector2 p in points)
                yield return new CPoint2D(p.X, p.Y);
        }

        public static Dx _This;

        public Dx()
        {

            _This = this;
        }
        Device dev;
        
        public void Load(Form form)
        {
            
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;
            dev = new Device(0, DeviceType.Default, form, CreateFlags.None, presentParams);
            
        }

        public IEnumerable<IPolygon> _Polygons;



        public void Draw()
        {
            dev.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);
            try
            {
                dev.BeginScene();
            } catch { }            
            UpdateVertex();
            dev.SetStreamSource(0, _vertexBuffer, 0);
            dev.DrawPrimitives(PrimitiveType.TriangleList, 0, _trianglecount);
            dev.EndScene();
            dev.Present();
        }
        
        private CustomVertex.TransformedColored[] GetOldVerts()
        {
            var verts = new CustomVertex.TransformedColored[3]
            {
                new CustomVertex.TransformedColored(32,0,0,1,System.Drawing.Color.Red.ToArgb()),
                new CustomVertex.TransformedColored(32,32,0,1,System.Drawing.Color.Red.ToArgb()),
                new CustomVertex.TransformedColored(0,32,0,1,System.Drawing.Color.Red.ToArgb()),                
            };                        
            return verts;
        }

        void UpdateVertex()
        {
            
            Pool vertexBufferPool = dev.DeviceCaps.SurfaceCaps.SupportsVidVertexBuffer ? Pool.VideoMemory : Pool.SystemMemory;
            IEnumerable<CustomVertex.TransformedColored> vertex = new CustomVertex.TransformedColored[0];
            _trianglecount = 0;
            foreach (IPolygon p in _Polygons)
            {
                CPolygonShape cp = new CPolygonShape(H.ToArray(ToCpoints(p.GetPoints())));
                cp.CutEar();
                _trianglecount += cp.NumberOfPolygons;
                vertex = H.Concat(vertex, ToVertex(cp.Polygons()));
            }
            CustomVertex.TransformedColored[] array = H.ToArray(Reverence(vertex));
            Trace.Assert(array.Length > 0);
            _vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColored), array.Length, dev, 0, CustomVertex.TransformedColored.Format, vertexBufferPool);
            _vertexBuffer.SetData(array, 0, LockFlags.None);
        }
        public IEnumerable<T> Reverence<T>(IEnumerable<T> t)
        {
            List<T> list = new List<T>(t);
            list.Reverse();
            foreach (T i in list)            
                yield return i;            
        }
        VertexBuffer _vertexBuffer;
        public int _trianglecount;

    }
}
