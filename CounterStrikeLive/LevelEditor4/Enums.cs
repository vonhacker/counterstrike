using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSL.LevelEditor
{
    /// <summary>
    /// Our Editor has different modes for different operations
    /// </summary>
    public enum EditorMode
    {
        Select = 1,
        Ink =2,
        /// <summary>
        /// We can draw Polygons in this mode
        /// </summary>
        Polygon = 3, 
        /// <summary>
        /// We selected some point and now its in the moving state(to be placed somewhere else)
        /// </summary>
        MovingPoint = 4,
        Erase = 5,
    }
}
