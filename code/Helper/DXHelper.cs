using System;

using System.Collections.Generic;
using System.Text;

#if PocketPC
using Microsoft.WindowsMobile.DirectX.Direct3D;
using Matrix=Microsoft.WindowsMobile.DirectX.Matrix;
using Vector3 = Microsoft.WindowsMobile.DirectX.Vector3;
#else
using Microsoft.DirectX.Direct3D;
using Matrix = Microsoft.DirectX.Matrix;
using Vector3 = Microsoft.DirectX.Vector3;
#endif
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
        
        public static IEnumerable<CustomVertex.PositionColored> ToVertex(IEnumerable<CPoint2D[]> polygons)
        {
            foreach (CPoint2D[] polygon in polygons)
                foreach (CPoint2D cp in polygon)
                    yield return new CustomVertex.PositionColored((float)cp.X, (float)cp.Y, 0,  Color.Black.ToArgb());
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
        Device device;
        Form _Form;
        public void Load(Form form)
        {
            _Form = form;
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;
            //presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            //presentParams.EnableAutoDepthStencil = true;
#if(PocketPC)
            device = new Device(0, DeviceType.Default, form, CreateFlags.None, presentParams);
#else
            device = new Device(0, DeviceType.Hardware, form, CreateFlags.SoftwareVertexProcessing, presentParams);            
            device.DeviceReset += delegate {  };
#endif            
        }

        public IEnumerable<IPolygon> _Polygons;


        public void Draw(float x , float y)
        {
            CameraPositioning(x,y);
            device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);
            try
            {
                device.BeginScene();
            } catch { }            
            UpdateVertex();
            //int s = 10;
            //array = new CustomVertex.PositionColored[3]{
            //    new CustomVertex.PositionColored(s,0,0,Color.Red.ToArgb()),
            //    new CustomVertex.PositionColored(s,s,0,Color.Red.ToArgb()),
            //    new CustomVertex.PositionColored(0,s,0,Color.Red.ToArgb()),                                
                
            //};
            //_trianglecount = 1;
            
            device.Transform.World = Matrix.Translation(x,y, 0);
#if (!PocketPC)
            device.VertexFormat = CustomVertex.PositionColored.Format;
#endif
            using (VertexBuffer _VertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), array.Length, device, Usage.None, CustomVertex.PositionColored.Format, vertexBufferPool))
            {
                _VertexBuffer.SetData(array, 0, LockFlags.None);
                device.SetStreamSource(0, _VertexBuffer, 0);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, _trianglecount);
                device.EndScene();
                device.Present();
            }
        }
        private void CameraPositioning(float x , float y)
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,  1, 1f, 15000f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, 2000), new Vector3(0, 0, 0), new Vector3(0, -1, 0));
            //device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, -1000), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.None;
        }
#if(PocketPC)
        Pool vertexBufferPool = Pool.VideoMemory;
#else
        Pool vertexBufferPool = Pool.Default;
#endif
        void UpdateVertex()
        {

            IEnumerable<CustomVertex.PositionColored> vertex = new CustomVertex.PositionColored[0];
            _trianglecount = 0;
            foreach (IPolygon p in _Polygons)
            {
                CPolygonShape cp = new CPolygonShape(H.ToArray(ToCpoints(p.GetPoints())));
                cp.CutEar();
                _trianglecount += cp.NumberOfPolygons;
                vertex = H.Concat(vertex, ToVertex(cp.Polygons()));
            }
            array = H.ToArray(vertex);
            Trace.Assert(array.Length > 0);
            
            
        }
        CustomVertex.PositionColored[] array;
        public IEnumerable<T> Reverence<T>(IEnumerable<T> t)
        {
            List<T> list = new List<T>(t);
            list.Reverse();
            foreach (T i in list)            
                yield return i;            
        }
        public int _trianglecount;

    }
}
