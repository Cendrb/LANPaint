using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace SharedWindows
{
    public class PointerData
    {
        public int StayTime { get; set; }
        public int FadeTime { get; set; }
        public DrawingAttributes Attributes { get; set; }
    }
}
