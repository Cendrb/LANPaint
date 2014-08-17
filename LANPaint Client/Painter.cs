using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Util;

namespace LANPaint_Client
{
    public class Painter
    {
        public InkCanvas MainCanvas { get; private set; }
        public PainterSender Sender { get; private set; }
        public PainterReceiver Receiver { get; private set; }
        public string Name { get; private set; }
        private ToolRoller toolRoller;
        private SignedStrokeCollection signedStrokes;

        public Painter(InkCanvas canvas, string name)
        {
            MainCanvas = canvas;
            Name = name;

            signedStrokes = new SignedStrokeCollection();
            signedStrokes.StrokeAdded += (penis) => MainCanvas.Dispatcher.Invoke(new Action(() => MainCanvas.Strokes.Add(penis)));
            signedStrokes.StrokeRemoved += (penis) => MainCanvas.Dispatcher.Invoke(new Action(() => MainCanvas.Strokes.Remove(penis)));
            toolRoller = new Painter.ToolRoller();
            Sender = new PainterSender(MainCanvas, Name, signedStrokes);

            MainCanvas.EraserShape = new EllipseStylusShape(1, 1);
            MainCanvas.DefaultDrawingAttributes.FitToCurve = true;
            MainCanvas.DefaultDrawingAttributes.Height = 2;
            MainCanvas.DefaultDrawingAttributes.Width = 2;
            MainCanvas.MouseWheel += Canvas_MouseWheel;
            MainCanvas.StrokesReplaced += Canvas_StrokesReplaced;
            MainCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            MainCanvas.Width = 69;
            MainCanvas.Height = 23;

            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), StaticPenises.SC_PORT);
            listener.Start();
            Sender.Connect(IPAddress.Parse("127.0.0.1"), StaticPenises.CS_PORT);
            Receiver = new PainterReceiver(listener.AcceptTcpClient(), MainCanvas, signedStrokes);
            listener.Stop();
            MainCanvas.Width = Receiver.XCanvasSize;
            MainCanvas.Height = Receiver.YCanvasSize;
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            foreach (Stroke addedStroke in e.Added)
            {
                SignedStroke addedSignedStroke = generateSignedStroke(addedStroke);
                signedStrokes.Add(addedSignedStroke);
                Task.Factory.StartNew(new Action(() => Sender.SendStroke(addedSignedStroke)));
            }
            foreach (Stroke removedStroke in e.Removed)
            {
                SignedStroke signed = signedStrokes.GetSignedWithBase(removedStroke);
                signedStrokes.Remove(signed);
                Task.Factory.StartNew(new Action(() => Sender.RemoveStroke(signed)));
            }
        }

        private SignedStroke generateSignedStroke(Stroke stroke)
        {
            SignedStroke signed = new SignedStroke(stroke);
            signed.Owner = Name;
            signed.Id = IdGenerator.GetNextId();
            return signed;
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Canvas_StrokesReplaced(object sender, InkCanvasStrokesReplacedEventArgs e)
        {
            e.NewStrokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                MainCanvas.EditingMode = toolRoller.NextTool();
            else
                MainCanvas.EditingMode = toolRoller.PreviousTool();
            changeCursor(MainCanvas.EditingMode);
        }

        private void changeCursor(InkCanvasEditingMode mode)
        {
            if (mode == InkCanvasEditingMode.EraseByPoint)
                MainCanvas.Cursor = Cursors.Arrow;
        }

        class ToolRoller
        {
            private InkCanvasEditingMode[] tools;
            private int index;
            public ToolRoller()
            {
                tools = new InkCanvasEditingMode[4];
                tools[0] = InkCanvasEditingMode.Ink;
                tools[1] = InkCanvasEditingMode.EraseByStroke;
                tools[2] = InkCanvasEditingMode.EraseByPoint;
                tools[3] = InkCanvasEditingMode.Select;
            }

            public InkCanvasEditingMode NextTool()
            {
                index += 1;
                index %= tools.Length;
                return tools[index];
            }

            public InkCanvasEditingMode PreviousTool()
            {
                index -= 1;
                if (index < 0)
                    index = tools.Length - 1;
                return tools[index];
            }
        }
    }
}
