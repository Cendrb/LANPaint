using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Util;

namespace LANPaint_Server
{
    public class PainterClient
    {
        public PainterClientWindow ControlComponent { get; private set; }
        public Painter PainterPenis { get; private set; }

        public PainterClient(Painter painter, Dispatcher dispatcher)
        {
            PainterPenis = painter;
            dispatcher.Invoke(new Action(() => ControlComponent = new PainterClientWindow(this)));
        }

        public static bool operator == (PainterClient first, PainterClient second)
        {
            return first.PainterPenis.RemoteName == second.PainterPenis.RemoteName;
        }

        public static bool operator !=(PainterClient first, PainterClient second)
        {
            return first.PainterPenis.RemoteName != second.PainterPenis.RemoteName;
        }

        public override bool Equals(object obj)
        {
            return PainterPenis.RemoteName == (obj as PainterClient).PainterPenis.RemoteName;
        }

        public override int GetHashCode()
        {
            return PainterPenis.RemoteName.GetHashCode();
        }
    }
}
