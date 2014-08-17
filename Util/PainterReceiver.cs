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
        public event Action<SignedStroke> StrokeReceived = delegate { };
        public event Action<SignedStroke> StrokeRemoved = delegate { };

        TcpClient remote;
        InkCanvas canvas;
        NetworkStream remoteStream;
        SignedStrokeCollection signedStrokes;

        public string Name { get; private set; }

        public double XCanvasSize { get; private set; }
        public double YCanvasSize { get; private set; }

        public PainterReceiver(TcpClient remote, InkCanvas canvas, SignedStrokeCollection collection)
        {
            signedStrokes = collection;
            this.canvas = canvas;
            if (remote == null || !remote.Connected)
                throw new ApplicationException("Passed client is not connected");
            this.remote = remote;
            remoteStream = remote.GetStream();
            getNameFromRemote();
            getCanvasSizeFromRemote();
            startListeningForCommands();
        }

        private void startListeningForCommands()
        {
            Task.Factory.StartNew(new Action(listener));
        }

        private void listener()
        {
            while (remote.Connected)
            {
                byte[] command = new byte[1];
                remoteStream.Read(command, 0, 1);
                switch (command[0])
                {
                    case PainterSender.Commands.CS_SEND_WHOLE_CANVAS:
                        receiveWholeCanvas();
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

        private void receiveWholeCanvas()
        {
            byte[] arrayLengthBytes = new byte[sizeof(long)];
            remoteStream.Read(arrayLengthBytes, 0, arrayLengthBytes.Length);
            long arrayLength = BitConverter.ToInt64(arrayLengthBytes, 0);
            byte[] canvasBytes = new byte[arrayLength];
            remoteStream.Read(canvasBytes, 0, canvasBytes.Length);
            StrokeCollection strokes = new StrokeCollection(new MemoryStream(canvasBytes));
            canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes = strokes));
        }

        private void getNameFromRemote()
        {
            byte[] nameBuffer = new byte[sizeof(char) * 128];
            remoteStream.Read(nameBuffer, 0, nameBuffer.Length);
            Name = StringBitConverter.GetString(nameBuffer).Replace("\0", "");
        }

        private void getCanvasSizeFromRemote()
        {
            byte[] xBuffer = new byte[sizeof(double)];
            remoteStream.Read(xBuffer, 0, xBuffer.Length);
            XCanvasSize = BitConverter.ToDouble(xBuffer, 0);

            byte[] yBuffer = new byte[sizeof(double)];
            remoteStream.Read(yBuffer, 0, yBuffer.Length);
            YCanvasSize = BitConverter.ToDouble(yBuffer, 0);
        }

        private void receiveIdAndRemoveStroke()
        {
            byte[] idBuffer = new byte[sizeof(long)];
            remoteStream.Read(idBuffer, 0, idBuffer.Length);
            long id = BitConverter.ToInt64(idBuffer, 0);
            try
            {
                StrokeRemoved(removeStrokeWithId(id));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private SignedStroke removeStrokeWithId(long id)
        {
            StrokeCollection strokes = canvas.Dispatcher.Invoke(new Func<StrokeCollection>(() => canvas.Strokes));
            List<SignedStroke> strokesToRemove = (from stroke in strokes.ToArray()
                                                  where ((SignedStroke)stroke).GetIdentifier() == id
                                                  select stroke as SignedStroke).ToList();
            if (strokesToRemove.Count() > 1)
                throw new ApplicationException(String.Format("Found two strokes with same id {0}", id));
            if (strokesToRemove.Count() > 0)
                signedStrokes.Remove(strokesToRemove.First());
            else
                throw new ApplicationException(String.Format("No strokes with specified id {0} were found", id));
            return strokesToRemove.First();
        }

        private void disconnect()
        {
            Disconnected(this);
            remote.Close();
        }

        private void receiveStrokeAndAddToCanvas()
        {
            // gets size
            byte[] sizeBuffer = new byte[sizeof(int)];
            remoteStream.Read(sizeBuffer, 0, sizeBuffer.Length);
            int size = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] bytes = new byte[size];
            remoteStream.Read(bytes, 0, bytes.Length);
            try
            {
                SignedStroke stroke = StrokeBitConverter.GetStroke(bytes);
                signedStrokes.Add(stroke);
                StrokeReceived(stroke);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
