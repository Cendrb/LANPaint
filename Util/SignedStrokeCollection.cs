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
        private List<SignedStroke> signedStrokes = new List<SignedStroke>();

        public event Action<SignedStroke> StrokeAdded = delegate { };
        public event Action<SignedStroke> StrokeRemoved = delegate { };

        public void Add(SignedStroke item)
        {
            signedStrokes.Add(item);
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

        public byte[] Save()
        {

        }

        public SignedStroke GetSignedWithBase(Stroke item)
        {
            foreach (SignedStroke signed in signedStrokes)
                if (signed.IsBase(item))
                    return signed;
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
