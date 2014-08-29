using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Util
{
    public class PainterReceiver
    {
        public event Action<PainterReceiver> Disconnected = delegate { };

        event Action<SignedStroke> StrokeReceived = delegate { };
        event Action<SignedStroke> StrokeRemoved = delegate { };
        event Action<SignedPointerStroke> PointerStrokeReceived = delegate { };
        event Action StrokesWiped = delegate { };
        event Action ObjectsWiped = delegate { };

        TcpClient remote;
        NetworkStream remoteStream;
        LanCanvas lanCanvas;

        public ReceiverHandle Handle { get; private set; }
        public PermissionsData Permissions { get; private set; }
        public string RemoteName { get; private set; }

        public PainterReceiver(TcpClient remote, LanCanvas lanCanvas)
        {
            this.lanCanvas = lanCanvas;
            Permissions = lanCanvas.Permissions;
            if (remote == null || !remote.Connected)
                throw new ApplicationException("Passed client is not connected");
            this.remote = remote;
            remoteStream = remote.GetStream();
            getNameFromRemote();
            startListeningForCommands();

            Handle = new ReceiverHandle(this);
        }

        private void startListeningForCommands()
        {
            Task.Factory.StartNew(new Action(listener));
        }

        private void listener()
        {
            try
            {
                Thread.CurrentThread.Name = "Receiver thread for " + RemoteName;
                while (remote.Connected)
                {
                    byte[] command = new byte[1];
                    remoteStream.Read(command, 0, 1);
                    switch (command[0])
                    {
                        case PainterSender.Commands.CS_SEND_POINTER:
                            receivePointerAndAddToCanvas();
                            break;
                        case PainterSender.Commands.CS_SEND_PERMISSIONS:
                            receiveAndApplyPermissions();
                            break;
                        case PainterSender.Commands.C_S_WIPE_STROKES:
                            wipeStrokes();
                            break;
                        case PainterSender.Commands.C_S_WIPE_OBJECTS:
                            wipeObjects();
                            break;
                        case PainterSender.Commands.SC_SEND_WHOLE_CANVAS:
                            sendWholeCanvas();
                            break;
                        case PainterSender.Commands.CS_SEND_STROKE:
                            receiveStrokeAndAddToCanvas();
                            break;
                        case PainterSender.Commands.CS_REMOVE_STROKE:
                            receiveIdAndRemoveStroke();
                            break;
                        case PainterSender.Commands.C_DISCONNECT:
                            disconnect();
                            break;
                    }
                }
            }
            catch (IOException e)
            {
                Log.Error(e);
            }
        }

        private void wipeObjects()
        {
            lanCanvas.ManualHandler.WipeObjects();
            ObjectsWiped();
        }

        private void receiveAndApplyPermissions()
        {
            byte[] permissions = new byte[1];
            remoteStream.Read(permissions, 0, 1);
            Permissions.Deserialize(permissions[0]);
            lanCanvas.UpdatePermissions();
        }

        private void wipeStrokes()
        {
            lanCanvas.ManualHandler.WipeStrokes();
            StrokesWiped();
        }

        private void sendWholeCanvas()
        {
            byte[] data = lanCanvas.Serialize();
            remoteStream.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
            remoteStream.Write(data, 0, data.Length);
        }

        private void getNameFromRemote()
        {
            byte[] nameBuffer = new byte[sizeof(char) * 128];
            remoteStream.Read(nameBuffer, 0, nameBuffer.Length);
            RemoteName = StringBitConverter.GetString(nameBuffer).Replace("\0", "");
        }

        private void receiveIdAndRemoveStroke()
        {
            byte[] idBuffer = new byte[sizeof(long)];
            remoteStream.Read(idBuffer, 0, idBuffer.Length);
            long id = BitConverter.ToInt64(idBuffer, 0);

            SignedStroke removedStroke = lanCanvas.ManualHandler.RemoveSignedStrokeWithId(id);
            StrokeRemoved(removedStroke);
        }

        private void disconnect()
        {
            Disconnected(this);
            remote.Close();
        }

        private void receivePointerAndAddToCanvas()
        {
            // gets size
            byte[] sizeBuffer = new byte[sizeof(int)];
            remoteStream.Read(sizeBuffer, 0, sizeBuffer.Length);
            int size = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] bytes = new byte[size];
            remoteStream.Read(bytes, 0, bytes.Length);
            SignedPointerStroke stroke = StrokeBitConverter.GetSignedPointerStroke(bytes);
            lanCanvas.ManualHandler.AddSignedPointerStroke(stroke);
            PointerStrokeReceived(stroke);
        }

        private void receiveStrokeAndAddToCanvas()
        {
            // gets size
            byte[] sizeBuffer = new byte[sizeof(int)];
            remoteStream.Read(sizeBuffer, 0, sizeBuffer.Length);
            int size = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] bytes = new byte[size];
            remoteStream.Read(bytes, 0, bytes.Length);
            SignedStroke stroke = StrokeBitConverter.GetSignedStroke(bytes);
            lanCanvas.ManualHandler.AddSignedStroke(stroke);
            StrokeReceived(stroke);
        }

        public void Disconnect()
        {
            disconnect();
        }

        /// <summary>
        /// Provides events invoked by PainterReceiver
        /// </summary>
        public class ReceiverHandle
        {

            public event Action<SignedStroke> StrokeReceived = delegate { };
            public event Action<SignedStroke> StrokeRemoved = delegate { };
            public event Action<SignedPointerStroke> PointerStrokeReceived = delegate { };
            public event Action StrokesWiped = delegate { };
            public event Action ObjectsWiped = delegate { };

            public ReceiverHandle(PainterReceiver receiver)
            {
                receiver.StrokeReceived += (stroke) => StrokeReceived(stroke);
                receiver.StrokeRemoved += (stroke) => StrokeRemoved(stroke);
                receiver.PointerStrokeReceived += (pointer) => PointerStrokeReceived(pointer);
                receiver.StrokesWiped += () => StrokesWiped();
                receiver.ObjectsWiped += () => ObjectsWiped();
            }
        }
    }
}
