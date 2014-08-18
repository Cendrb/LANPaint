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
        public PainterClientWindow ControlComponent { get; private set; }

        public PainterReceiver Receiver { get; private set; }
        public PainterSender Sender { get; private set; }

        public int PermissionLevel { get; set; }
        public PermissionType PermissionType { get; set; }

        public PainterClient(PainterReceiver receiver, PainterSender sender, PermissionType permissionType, int permissionLevel)
        {
            Receiver = receiver;
            Sender = sender;
            PermissionLevel = permissionLevel;
            PermissionType = permissionType;

            sender.MainCanvas.Dispatcher.Invoke(new Action(() => ControlComponent = new PainterClientWindow(this)));
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
