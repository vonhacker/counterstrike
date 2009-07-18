using System;

using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Mathematics;

namespace BallGame
{
    public class Camera
    {
        public static Camera _Current;
        static Game _Engine = Game._This;
        static Form1 _Form1 = Form1._This;
        public Vector2 _Pos = new Vector2(0,0);
        //public Vector2 _Offset { get { return -_Pos - new Vector2(_Form1.Width / 2, _Form1.Height / 2); } }
        public Camera()
        {
            if (_Current == null) _Current = this;
        }
    }
}
