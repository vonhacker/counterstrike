using System;
using System.Collections.Generic;

using System.Text;

namespace doru
{
    public class MathD
    {
        public static Random r = new Random();
        public static Single ClampAngle(Single angle)
        {
            float Pi = PI;
            float TwoPi = PI * 2;
            if (-Pi <= angle && angle < Pi) { return angle; }
            Single rem = (angle + Pi) % (TwoPi);
            return rem + ((rem < 0) ? (Pi) : (-Pi));
        }

        public static float PI { get { return (float)Math.PI; } }
        public static float Angle360(float value)//0 to 360
        {
            float angle;
            angle = value % 360;
            if (angle < 0) angle += 360;
            return angle;
        }
        public static float Angle180(float a) //-180 to 180
        {
            a = Angle360(a);
            return a > 180 ? a - 360 : a;
        }

        public static float Angle314(float value)//0 to pi
        {
            float angle;
            angle = value % PI;
            if (angle < 0) angle += PI;
            return angle;
        }

        public static float Angle157(float a) //-pi/2 to pi/2
        {
            a = Angle314(a);
            return a > PI/2 ? a - PI : a;
        }

    }
}
