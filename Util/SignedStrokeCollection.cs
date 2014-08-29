using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace Util
{/*
    public class SignedStrokeCollection : ICollection<SignedStroke>
    {
        public const long DIVIDER = -1414454545644;
        public byte[] DIVIDER_BYTES = BitConverter.GetBytes(DIVIDER);

        private List<SignedStroke> signedStrokes = new List<SignedStroke>();

        public event Action<SignedStroke> StrokeAdded = delegate { };
        public event Action<SignedStroke> StrokeRemoved = delegate { };

        public SignedStrokeCollection(IdGenerator generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// Generates signed stroke and adds it to inner collection
        /// </summary>
        /// <param name="item">Original stroke</param>
        /// <param name="ownerName">Owner of stroke</param>
        /// <returns>Generated signed stroke</returns>
        public SignedStroke AddAsSigned(Stroke item, string ownerName)
        {
            SignedStroke signed = generateSigned(item, ownerName);
            signedStrokes.Add(signed);
            StrokeAdded(signed);
            return signed;
        }

        private SignedStroke generateSigned(Stroke original, string ownerName)
        {
            SignedStroke signed = new SignedStroke(original);
            signed.Id = generator.GetNextId();
            signed.Owner = ownerName;
            return signed;
        }

        public void Add(SignedStroke item)
        {
            signedStrokes.Add(item);
            StrokeAdded(item);
        }

        public void Add(SignedStroke item, bool invokeEvent)
        {
            signedStrokes.Add(item);

            if(invokeEvent)
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

        public void Deserialize(IEnumerable<byte> data)
        {
            Clear();
            byte[][] strokes = StaticPenises.Divide<byte>(data, DIVIDER_BYTES);
            foreach (byte[] stroke in strokes)
            {
                if (stroke.Length > 0)
                {
                    Add(StrokeBitConverter.GetSignedStroke(stroke));
                }
            }
        }

        public byte[] Serialize()
        {
            List<byte> datalist = new List<byte>();
            foreach (SignedStroke stroke in signedStrokes)
            {
                datalist.AddRange(DIVIDER_BYTES);
                datalist.AddRange(StrokeBitConverter.GetBytes(stroke));
            }
            return datalist.ToArray();
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

        public IEnumerator<SignedStroke> GetEnumerator()
        {
            return signedStrokes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }*/
}
