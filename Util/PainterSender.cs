using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Threading;

namespace Util
{
    public class PainterSender
    {
        public event Action<PainterSender> Disconnected = delegate { };

        TcpClient client;
        NetworkStream stream;
        public string LocalName { get; private set; }
        public ServerHandle Handle { get; private set; }

        LanCanvas lanCanvas;

        public bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        public PainterSender(LanCanvas lanCanvas, string name)
        {
            this.lanCanvas = lanCanvas;
            LocalName = name;
            Handle = new ServerHandle(this);
        }

        public void ConnectAsync(IPAddress address, int port)
        {
            Task.Factory.StartNew(new Action(() => Connect(address, port)));
        }

        public void Connect(IPAddress address, int port)
        {
            client = new TcpClient();
            lock (client)
            {
                client.Connect(address, port);

                stream = client.GetStream();

                stream.Write(StringBitConverter.GetBytes(LocalName, sizeof(char) * 128), 0, sizeof(char) * 128);
            }
        }

        public void AskForWholeCanvas()
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.SC_SEND_WHOLE_CANVAS);

                byte[] arrayLengthBytes = new byte[sizeof(int)];
                stream.Read(arrayLengthBytes, 0, arrayLengthBytes.Length);
                int arrayLength = BitConverter.ToInt32(arrayLengthBytes, 0);
                byte[] canvasBytes = new byte[arrayLength];
                stream.Read(canvasBytes, 0, canvasBytes.Length);
                lanCanvas.Deserialize(canvasBytes);
            }
        }

        public void SendPermissions(PermissionsData data)
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.CS_SEND_PERMISSIONS);
                stream.WriteByte(data.Serialize());
            }
        }

        public void WipeStrokes()
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.C_S_WIPE_STROKES);
            }
        }


        public void WipeObjects()
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.C_S_WIPE_OBJECTS);
            }
        }

        public void SendPointerStroke(SignedPointerStroke pointer)
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.CS_SEND_POINTER);
                byte[] bytes = StrokeBitConverter.Serialize(pointer);
                stream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void SendStroke(SignedStroke stroke)
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.CS_SEND_STROKE);
                byte[] bytes = StrokeBitConverter.Serialize(stroke);
                stream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void RemoveStroke(SignedStroke stroke)
        {
            lock (stream)
            {
                sendBeginByte();
                stream.WriteByte(Commands.CS_REMOVE_STROKE);
                stream.Write(BitConverter.GetBytes(stroke.GetIdentifier()), 0, sizeof(long));
            }
        }

        public void SendDisconnect()
        {
            lock (stream)
            {
                if (Connected)
                {
                    sendBeginByte();
                    stream.WriteByte(Commands.C_DISCONNECT);
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            lock (stream)
                client.Close();
            Disconnected(this);
        }

        private void sendBeginByte()
        {
            stream.WriteByte(Commands.BEGIN_BYTE);
        }

        public class ServerHandle
        {
            private PainterSender painterSender;

            public ServerHandle(PainterSender painterSender)
            {
                this.painterSender = painterSender;
            }

            public void SendSignedStroke(SignedStroke signed)
            {
                painterSender.SendStroke(signed);
            }

            public void SendPointerStroke(SignedPointerStroke pointer)
            {
                painterSender.SendPointerStroke(pointer);
            }

            public void RemoveSignedStroke(SignedStroke signed)
            {
                painterSender.RemoveStroke(signed);
            }

            public void WipeStrokes()
            {
                painterSender.WipeStrokes();
            }

            public void WipeObjects()
            {
                painterSender.WipeObjects();
            }
        }

        public static class Commands
        {
            // S = server
            // C = client
            // CS = client to server

            public const byte BEGIN_BYTE = 250;

            public const byte SC_SEND_WHOLE_CANVAS = 169;
            public const byte C_S_WIPE_STROKES = 58;
            public const byte C_S_WIPE_OBJECTS = 59;
            public const byte CS_SEND_POINTER = 8;
            public const byte CS_SEND_STROKE = 2;
            public const byte CS_REMOVE_STROKE = 69;
            public const byte CS_SEND_OBJECT = 3;
            public const byte CS_REMOVE_OBJECT = 4;
            public const byte CS_SEND_PERMISSIONS = 23;
            public const byte C_DISCONNECT = 255;
        }
    }
}
