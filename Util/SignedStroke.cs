using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;

namespace Util
{
    public class SignedStroke : Stroke
    {
        public uint Id { get; set; }
        public string Owner { get; set; }

        public SignedStroke(StylusPointCollection points)
            : base(points)
        {

        }

        public SignedStroke(StylusPointCollection points, DrawingAttributes attr)
            : base(points, attr)
        {

        }

        public SignedStroke(Stroke stroke)
            : base(stroke.StylusPoints, stroke.DrawingAttributes)
        {

        }

        public long GetIdentifier()
        {
            long identifier = Owner.GetHashCode();
            identifier += Id;
            return identifier;
        }

        public static bool operator ==(SignedStroke first, SignedStroke second)
        {
            return first.GetIdentifier() == second.GetIdentifier();
        }

        public static bool operator !=(SignedStroke first, SignedStroke second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            if (obj is SignedStroke)
                return this == (SignedStroke)obj;
            else
            {
                Log.Warning(String.Format("Compared {0} with SignedStroke", obj.GetType()));
                return false;
            }
        }

        public bool IsBase(Stroke stroke)
        {
            return stroke.DrawingAttributes == DrawingAttributes && stroke.StylusPoints == StylusPoints;
        }
    }
}
