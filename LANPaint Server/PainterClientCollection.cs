using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace LANPaint_Server
{
    public class PainterClientCollection : ICollection<PainterClient>
    {
        private List<PainterClient> clients = new List<PainterClient>();

        public void Add(PainterClient item)
        {
            if (clients.Contains(item))
                throw new NameDuplicateException("Collection already contains client named " + item.Receiver.Name);
            else
                clients.Add(item);
        }

        public void Clear()
        {
            clients.Clear();
        }

        public bool Contains(PainterClient item)
        {
            return clients.Contains(item);
        }

        public void CopyTo(PainterClient[] array, int arrayIndex)
        {
            clients.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return clients.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(PainterClient item)
        {
            return clients.Remove(item);
        }

        public PainterClient Remove(PainterReceiver item)
        {
            foreach (PainterClient painter in clients.ToArray())
                if (painter.Receiver.Name == item.Name)
                {
                    clients.Remove(painter);
                    return painter;
                }
            return null;
        }

        public IEnumerator<PainterClient> GetEnumerator()
        {
            return clients.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
