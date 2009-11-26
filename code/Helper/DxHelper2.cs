using System;
using System.Collections.Generic;
using System.Text;
#if(PocketPC)
using Microsoft.WindowsMobile.DirectX;
using Microsoft.WindowsMobile.DirectX.Direct3D;
#else
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
#endif
using System.Windows.Forms;

namespace doru
{
    public class Dx
    {
        public static Device InitDevice(Form _Form1)
        {
            PresentParameters d3dpp = new PresentParameters();
            d3dpp.Windowed = true;
            d3dpp.SwapEffect = SwapEffect.Discard;
            d3dpp.AutoDepthStencilFormat = DepthFormat.D16;
#if(PocketPC)
            Device d3dDevice = new Device(0, DeviceType.Default, _Form1, CreateFlags.None, d3dpp);
#else
            Device d3dDevice = new Device(0, DeviceType.Hardware, _Form1, CreateFlags.SoftwareVertexProcessing, d3dpp);
            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;

#endif

            d3dDevice.RenderState.Lighting = false;
            d3dDevice.RenderState.CullMode = Cull.None;
            d3dDevice.RenderState.FillMode = FillMode.WireFrame;
            d3dDevice.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1, 1f, 15000f);
            return d3dDevice;
        }
    }
}
