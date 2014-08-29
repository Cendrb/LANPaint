using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace Util
{
    public class Painter
    {
        public event Action<Painter> Disconnected = delegate { };

        PainterReceiver receiver;
        PainterSender sender;
        LanCanvas lanCanvas;

        /// <summary>
        /// Provides methods for a server which send a command to remote
        /// </summary>
        public PainterSender.ServerHandle ServerHandle { get; private set; }
        /// <summary>
        /// Provides methods for a receiver to operate with canvas and events for server to detect incoming canvas operations
        /// </summary>
        public PainterReceiver.ReceiverHandle ReceiverHandle { get; private set; }
        /// <summary>
        /// Provides methods for operating with local canvas
        /// </summary>
        public LanCanvas.ManualHandle ManualHandle { get; private set; }
        public PermissionsData Permissions { get; private set; }
        public string LocalName { get; private set; }
        public string RemoteName { get; private set; }

        public Painter(LanCanvas lanCanvas, string name)
        {
            this.lanCanvas = lanCanvas;
            ManualHandle = lanCanvas.ManualHandler;
            LocalName = name;
        }

        private void receiver_Disconnected(PainterReceiver obj)
        {
            Disconnect();
        }

        public void Listen(IPAddress address, int inPort, int outPort)
        {
            TcpListener listener = new TcpListener(address, inPort);
            listener.Start();
            TcpClient remote = listener.AcceptTcpClient();
            listener.Stop();
            receiver = new PainterReceiver(remote, lanCanvas);
            Permissions = receiver.Permissions;
            RemoteName = receiver.RemoteName;
            ReceiverHandle = receiver.Handle;
            sender = new PainterSender(lanCanvas, LocalName);
            IPEndPoint endpoint = (IPEndPoint)remote.Client.RemoteEndPoint;
            sender.Connect(endpoint.Address, outPort);
            ServerHandle = sender.Handle;

            setupHandlers();
        }

        public void Connect(IPAddress address, int inPort, int outPort)
        {
            sender = new PainterSender(lanCanvas, LocalName);
            sender.Connect(address, outPort);
            ServerHandle = sender.Handle;
            TcpListener listener = new TcpListener(IPAddress.Any, inPort);
            listener.Start();
            TcpClient remote = listener.AcceptTcpClient();
            listener.Stop();
            receiver = new PainterReceiver(remote, lanCanvas);
            RemoteName = receiver.RemoteName;
            Permissions = receiver.Permissions;
            ReceiverHandle = receiver.Handle;

            setupHandlers();
        }

        private void setupHandlers()
        {
            lanCanvas.CanvasHandle.StrokeCollected += sender.SendStroke;
            lanCanvas.CanvasHandle.StrokeRemoved += sender.RemoveStroke;
            lanCanvas.CanvasHandle.PointerStrokeCollected += sender.SendPointerStroke;
            lanCanvas.CanvasHandle.WipedObjects += sender.WipeObjects;
            lanCanvas.CanvasHandle.WipedStrokes += sender.WipeStrokes;
            receiver.Disconnected += receiver_Disconnected;
        }

        /// <summary>
        /// Tries to get a canvas from the remote and replace the local one with it
        /// </summary>
        public void RequestWholeCanvas()
        {
            sender.AskForWholeCanvas();
        }

        /// <summary>
        /// Tries to disconnect from the remote
        /// </summary>
        public void Disconnect()
        {
            if (sender.Connected)
            {
                Disconnected(this);
                sender.SendDisconnect();
            }
        }

        public void UpdatePermissions()
        {
            sender.SendPermissions(Permissions);
        }
    }
}
