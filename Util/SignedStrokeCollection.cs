using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace Util
{
    public class SignedStrokeCollection : ICollection<SignedStroke>
    {
        public const long DIVIDER = -1414454545644;
        public byte[] DIVIDER_BYTES = BitConverter.GetBytes(DIVIDER);

        private List<SignedStroke> signedStrokes = new List<SignedStroke>();

        public event Action<SignedStroke> StrokeAdded = delegate { };
        public event Action<SignedStroke> StrokeRemoved = delegate { };

        public void Add(SignedStroke item)
        {
            signedStrokes.Add(item);
            StrokeAdded(item);
        }

        public void Add(SignedStroke item, bool invokeEvent)
        {
            signedStrokes.Add(item);
            if (invokeEvent)
                StrokeAdded(item);
        }

        public void Clear()
        {
            signedStrokes.Clear();
        }

        public bool Contains(SignedStroke item)
        {
            return signedStrokes.Contains(item);
        }

        public bool Contains(Stroke item)
        {
            foreach (SignedStroke signed in signedStrokes)
                if (signed.IsBase(item))
                    return true;
            return false;
        }

        public void Load(byte[] data)
        {
            Clear();
            byte[][] strokes = StaticPenises.Divide<byte>(data, DIVIDER_BYTES);
            foreach (byte[] stroke in strokes)
            {
                if (stroke.Length > 0)
                {
                    Add(StrokeBitConverter.GetStroke(stroke));
                }
            }
        }

        public byte[] Save()
        {
            List<byte> datalist = new List<byte>();
            foreach (SignedStroke stroke in signedStrokes)
            {
                datalist.AddRange(DIVIDER_BYTES);
                datalist.AddRange(StrokeBitConverter.GetBytes(stroke));
            }
            return datalist.ToArray();
        }

        public SignedStroke GetSignedWithBase(Stroke item)
        {
            lock (signedStrokes)
            {
                foreach (SignedStroke signed in signedStrokes)
                    if (signed.IsBase(item))
                        return signed;
            }
            return null;
        }

        public void CopyTo(SignedStroke[] array, int arrayIndex)
        {
            signedStrokes.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return signedStrokes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(SignedStroke item)
        {
            StrokeRemoved(item);
            return signedStrokes.Remove(item);
        }

        public bool Remove(SignedStroke item, bool invokeEvent)
        {
            if (invokeEvent)
                StrokeRemoved(item);
            return signedStrokes.Remove(item);
        }

        public bool Remove(Stroke item)
        {
            foreach (SignedStroke signed in signedStrokes.ToList())
            {
                if (signed.IsBase(item))
                {
                    signedStrokes.Remove(signed);
                    StrokeRemoved(signed);
                    return true;
                }
            }
            return false;
        }

        public bool Remove(Stroke item, bool invokeEvent)
        {
            foreach (SignedStroke signed in signedStrokes.ToList())
            {
                if (signed.IsBase(item))
                {
                    signedStrokes.Remove(signed);
                    if (invokeEvent)
                        StrokeRemoved(signed);
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<SignedStroke> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
