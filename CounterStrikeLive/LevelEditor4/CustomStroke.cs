using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;

namespace CSL.LevelEditor
{
    public class CustomStroke : Stroke
    {
        public CustomStroke(StylusPointCollection stylusPointCollection)
            : base(stylusPointCollection)
        {
        }
        //public string _ImagePath;
    }
}
