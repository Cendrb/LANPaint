using System;
using System.Collections.Generic;
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
        InkCanvas canvas;
        NetworkStream stream;
        SignedStrokeCollection signedStrokes;
        public string Name { get; private set; }

        public bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        public PainterSender(InkCanvas canvas, string name, SignedStrokeCollection collection)
        {
            signedStrokes = collection;
            this.canvas = canvas;
            Name = name;
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

                stream.Write(StringBitConverter.GetBytes(Name, sizeof(char) * 128), 0, sizeof(char) * 128);

                double x = canvas.Dispatcher.Invoke(new Func<double>(() => canvas.Width));
                double y = canvas.Dispatcher.Invoke(new Func<double>(() => canvas.Height));
                stream.Write(BitConverter.GetBytes(x), 0, sizeof(double));
                stream.Write(BitConverter.GetBytes(y), 0, sizeof(double));
            }
        }

        public void SendWholeCanvas()
        {
            lock (stream)
            {
                stream.WriteByte(Commands.CS_SEND_WHOLE_CANVAS);
                Stream strokesStream = new MemoryStream();
                canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Save(strokesStream)));
                stream.Write(BitConverter.GetBytes(strokesStream.Length), 0, sizeof(long));
                byte[] temp = new byte[strokesStream.Length];
                strokesStream.Seek(0, SeekOrigin.Begin);
                strokesStream.Read(temp, 0, (int)strokesStream.Length);
                stream.Write(temp, 0, temp.Length);
            }
        }


        public void SendStroke(SignedStroke stroke)
        {
            lock (stream)
            {
                stream.WriteByte(Commands.CS_SEND_STROKE);
                byte[] bytes = StrokeBitConverter.GetBytes(stroke);
                stream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void RemoveStroke(SignedStroke stroke)
        {
            lock (stream)
            {
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

        public static class Commands
        {
            // S = server
            // C = client
            // CS = client to server

            public const byte CS_SEND_WHOLE_CANVAS = 0;
            public const byte CS_SEND_STROKE = 2;
            public const byte CS_REMOVE_STROKE = 69;
            public const byte CS_SEND_OBJECT = 3;
            public const byte CS_REMOVE_OBJECT = 4;
            public const byte C_DISCONNECT = 255;
        }
    }
}
