using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace LANPaint_Server
{
    public class PainterClient
    {
        public PainterReceiver Receiver { get; private set; }
        public PainterSender Sender { get; private set; }

        public PainterClient(PainterReceiver receiver, PainterSender sender)
        {
            Receiver = receiver;
            Sender = sender;
        }

        public static bool operator == (PainterClient first, PainterClient second)
        {
            return first.Receiver.Name == second.Receiver.Name;
        }

        public static bool operator !=(PainterClient first, PainterClient second)
        {
            return first.Receiver.Name != second.Receiver.Name;
        }

        public override bool Equals(object obj)
        {
            return Receiver.Name == (obj as PainterClient).Receiver.Name;
        }

        public override int GetHashCode()
        {
            return Receiver.Name.GetHashCode();
        }
    }
}
