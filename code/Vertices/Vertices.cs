using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsMobile.DirectX;
using Microsoft.WindowsMobile.DirectX.Direct3D;
using System.Runtime.InteropServices;
namespace Microsoft.Samples.MD3DM
{
    public class Vertices : Form
    {
        Device device = null;
        VertexBuffer vertexBuffer = null;
        public Vertices()
        {
            this.Text = "Direct3D Tutorial 2 - Vertices";
            this.MinimizeBox = false;
        }
        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                device = new Device(0, DeviceType.Default, this,CreateFlags.None, presentParams);
                OnCreateDevice(device, null);
            } catch (DirectXException)
            {
                return false;
            }
            return true;
        }
        void OnCreateDevice(object sender, EventArgs e)
        {            
            Device dev = (Device)sender;
            Pool vertexBufferPool = dev.DeviceCaps.SurfaceCaps.SupportsVidVertexBuffer ? Pool.VideoMemory : Pool.SystemMemory;            

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColored), 3, dev, 0,CustomVertex.TransformedColored.Format, vertexBufferPool);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
        }
        void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            VertexBuffer vb = (VertexBuffer)sender;
            GraphicsStream stm = vb.Lock(0, 0, 0);
            CustomVertex.TransformedColored[] verts =new CustomVertex.TransformedColored[3];
            verts[0].X = 150;
            verts[0].Y = 50;
            verts[0].Z = 0.5f;
            verts[0].Rhw = 1;
            verts[0].Color = Color.Aqua.ToArgb();
            verts[1].X = 250;
            verts[1].Y = 250;
            verts[1].Z = 0.5f;
            verts[1].Rhw = 1;
            verts[1].Color = Color.Brown.ToArgb();
            verts[2].X = 50;
            verts[2].Y = 250;
            verts[2].Z = 0.5f;
            verts[2].Rhw = 1;
            verts[2].Color = Color.LightPink.ToArgb();
            stm.Write(verts);
            vb.Unlock();
        }
        private void Render()
        {
            if (device != null)
            {
                device.Clear(ClearFlags.Target, Color.Blue,1.0f, 0);
                device.BeginScene();
                device.SetStreamSource(0, vertexBuffer, 0);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                device.EndScene();
                device.Present();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            this.Render();
            this.Invalidate();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if ((int)(byte)e.KeyChar == (int)Keys.Escape)
                this.Close();
        }
        static void Main()
        {
            Vertices frm = new Vertices();
            if (!frm.InitializeGraphics())
            {
                MessageBox.Show("Could not initialize Direct3D. This " +"tutorial will exit.");
                return;
            }
            Application.Run(frm);
        }
    }
}
