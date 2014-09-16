using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SharedWindows;
using System.IO;

namespace Util
{
    public class LanCanvas
    {
        const long DIVIDER = -1414454545644;
        byte[] DIVIDER_BYTES = BitConverter.GetBytes(DIVIDER);

        event Action WipedStrokes = delegate { };
        event Action WipedObjects = delegate { };

        double Height
        {
            get
            {
                return canvas.Dispatcher.Invoke(new Func<double>(() => canvas.Height));
            }
            set
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Height = value));
            }
        }
        double Width
        {
            get
            {
                return canvas.Dispatcher.Invoke(new Func<double>(() => canvas.Width));
            }
            set
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Width = value));
            }
        }

        public string OwnerName { get; private set; }
        public CanvasEventsHandle CanvasHandle { get; private set; }
        public ManualHandle ManualHandler { get; private set; }
        public PermissionsData Permissions { get; private set; }

        SignedStrokeEraser eraser;
        SignedStrokeDrawer drawer;
        SignedStrokeCutter cutter;
        SignedPointerStrokeDrawer pointerDrawer;

        ModeChanger modeChanger;
        CursorLibrary cursorLibrary;

        IdGenerator generator;
        StylusShape eraserShape;

        LanCanvasEditingMode editingMode;
        public LanCanvasEditingMode EditingMode
        {
            get
            {
                return editingMode;
            }
            set
            {
                editingMode = value;
                canvas.Dispatcher.Invoke(new Action(() => canvas.Cursor = cursorLibrary.GetCursor(editingMode)));
            }
        }

        public DrawingAttributes DefaultDrawingAttributes { get; set; }

        public PointerData PointerData { get; set; }

        private InkCanvas canvas;

        public LanCanvas(InkCanvas canvas, IdGenerator generator, string owner, PermissionsData permissions)
        {
            eraserShape = new EllipseStylusShape(2, 2);

            PointerData = new SharedWindows.PointerData();
            PointerData.Attributes = new DrawingAttributes();
            PointerData.Attributes.Color = Colors.Red;
            PointerData.StayTime = 1500;
            PointerData.FadeTime = 500;

            OwnerName = owner;
            this.generator = generator;
            this.canvas = canvas;
            this.Permissions = permissions;

            DefaultDrawingAttributes = new DrawingAttributes();

            cursorLibrary = new CursorLibrary();

            modeChanger = new ModeChanger();
            UpdatePermissions();

            canvas.Dispatcher.Invoke(new Action(canvasInit));

            drawer = new SignedStrokeDrawer(canvas.Strokes, generator, OwnerName);

            eraser = new SignedStrokeEraser(canvas.Strokes, permissions, OwnerName);

            cutter = new SignedStrokeCutter(canvas.Strokes);

            pointerDrawer = new SignedPointerStrokeDrawer(canvas.Strokes, OwnerName, generator, canvas.Dispatcher);

            ManualHandler = new ManualHandle(canvas);

            CanvasHandle = new CanvasEventsHandle(this, drawer, eraser, pointerDrawer);
        }

        private void canvasInit()
        {
            canvas.ForceCursor = true;
            canvas.EditingMode = InkCanvasEditingMode.None;
            canvas.MouseLeftButtonDown += (sender, args) => startUsingTool(args.GetPosition(canvas));
            canvas.MouseLeftButtonUp += (sender, args) => stopUsingTool(args.GetPosition(canvas));
            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseWheel += canvas_MouseWheel;
            canvas.KeyDown += canvas_KeyDown;
            canvas.LostMouseCapture += (sender, args) => stopUsingTool(args.GetPosition(canvas));
            canvas.MouseRightButtonUp += (sender, args) => openMenu(args.GetPosition(canvas));
        }

        public void UpdatePermissions()
        {
            modeChanger.Reset();
            modeChanger.Modes.Add(LanCanvasEditingMode.None);
            if (Permissions.ManipulateOwnLines)
                modeChanger.Modes.Add(LanCanvasEditingMode.Ink);
            if (Permissions.ManipulateOwnLines || Permissions.ManipulateOtherLines)
                modeChanger.Modes.Add(LanCanvasEditingMode.Eraser);
            if (Permissions.UsePointers)
                modeChanger.Modes.Add(LanCanvasEditingMode.Pointer);
            EditingMode = modeChanger.ActiveMode();
        }

        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (!drawer.Drawing && !eraser.Erasing && !cutter.Cutting)
                switch (e.Key)
                {
                    case Key.F8:
                        wipeStrokes();
                        break;
                    case Key.F9:
                        wipeObjects();
                        break;
                }
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!drawer.Drawing && !eraser.Erasing && !cutter.Cutting && !pointerDrawer.Drawing)
                if (e.Delta > 0)
                    EditingMode = modeChanger.NextMode();
                else
                    EditingMode = modeChanger.PreviousMode();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(canvas);
            if (drawer.Drawing)
            {
                drawer.AddPoint(new StylusPoint(position.X, position.Y));
            }
            if (eraser.Erasing)
            {
                eraser.AddPoint(position);
            }
            if (cutter.Cutting)
            {
                cutter.AddPoint(position);
            }
            if (pointerDrawer.Drawing)
            {
                pointerDrawer.AddPoint(new StylusPoint(position.X, position.Y));
            }
        }

        private void openMenu(Point position)
        {
            if (!drawer.Drawing && !eraser.Erasing && !cutter.Cutting && !pointerDrawer.Drawing)
            {
                bool strokeFound = false;
                foreach (Stroke stroke in canvas.Strokes)
                    if (stroke.HitTest(position))
                    {
                        strokeFound = true;
                        openStrokeMenu(stroke);
                        break;
                    }
                if (!strokeFound)
                    openToolMenu();
            }
        }

        private void openToolMenu()
        {
            switch (EditingMode)
            {
                case LanCanvasEditingMode.Ink:
                    openAttributesMenu(DefaultDrawingAttributes);
                    break;
                case LanCanvasEditingMode.Pointer:
                    openPointerMenu();
                    break;
            }
        }

        private void openPointerMenu()
        {
            PointerAttributesWindow dialog = new PointerAttributesWindow(PointerData);
            dialog.ShowDialog();
        }

        private void openStylusShapeMenu()
        {

        }

        private void openAttributesMenu(DrawingAttributes attr)
        {
            DrawingAttributesWindow dialog = new DrawingAttributesWindow(attr);
            dialog.ShowDialog();
        }

        private void openStrokeMenu(Stroke stroke)
        {

        }

        private void startUsingTool(Point position)
        {
            switch (editingMode)
            {
                case LanCanvasEditingMode.Ink:
                    if (!drawer.Drawing)
                        drawer.BeginDrawingStroke(new StylusPoint(position.X, position.Y), DefaultDrawingAttributes);
                    break;
                case LanCanvasEditingMode.Eraser:
                    if (!eraser.Erasing)
                        eraser.BeginEraserPath(position, eraserShape);
                    break;
                case LanCanvasEditingMode.Pointer:
                    if (!pointerDrawer.Drawing)
                        pointerDrawer.BeginDrawingPointerStroke(new StylusPoint(position.X, position.Y), PointerData);
                    break;
            }
        }

        private void stopUsingTool(Point position)
        {
            switch (editingMode)
            {
                case LanCanvasEditingMode.Ink:
                    if (drawer.Drawing)
                        drawer.FinishDrawingStroke(new StylusPoint(position.X, position.Y));
                    break;
                case LanCanvasEditingMode.Eraser:
                    if (eraser.Erasing)
                        eraser.FinishEraserPath(position);
                    break;
                case LanCanvasEditingMode.Pointer:
                    if (pointerDrawer.Drawing)
                        pointerDrawer.FinishDrawingStroke(new StylusPoint(position.X, position.Y));
                    break;
            }
        }

        private void wipeStrokes()
        {
            if (Permissions.WipeStrokes)
            {
                canvas.Strokes.Clear();
                WipedStrokes();
            }
        }

        private void wipeObjects()
        {
            if (Permissions.WipeObjects)
            {
                canvas.Children.Clear();
                WipedObjects();
            }
        }

        /// <summary>
        /// Converts all the data from canvas to the given stream
        /// </summary>
        /// <param name="target">Target stream for the serialized data</param>
        public void Serialize(Stream target)
        {
            target.Write(BitConverter.GetBytes(generator.ActiveId), 0, sizeof(UInt32));
            target.Write(BitConverter.GetBytes(Width), 0, sizeof(double));
            target.Write(BitConverter.GetBytes(Height), 0, sizeof(double));
            IEnumerator<Stroke> enumerator = canvas.Dispatcher.Invoke(new Func<IEnumerator<Stroke>>(() => canvas.Strokes.GetEnumerator()));
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is SignedPointerStroke)
                {
                    target.WriteByte(StrokeBitConverter.SIGNED_POINTER_STROKE_SIGNAL);
                    StrokeBitConverter.Serialize(target, (SignedPointerStroke)enumerator.Current);
                }
                else
                {
                    target.WriteByte(StrokeBitConverter.SIGNED_STROKE_SIGNAL);
                    StrokeBitConverter.Serialize(target, (SignedStroke)enumerator.Current);
                }
            }
            target.WriteByte(StrokeBitConverter.STROKES_END);
        }
        /// <summary>
        /// Wipes all the previous data and loads them from the given stream
        /// </summary>
        /// <param name="source">The data source</param>
        public void Deserialize(Stream source)
        {
            byte[] idBytes = new byte[sizeof(UInt32)];
            source.Read(idBytes, 0, idBytes.Length);
            generator.ActiveId = BitConverter.ToUInt32(idBytes, 0);

            byte[] widthBytes = new byte[sizeof(double)];
            source.Read(widthBytes, 0, widthBytes.Length);
            Width = BitConverter.ToDouble(widthBytes, 0);

            byte[] heightBytes = new byte[sizeof(double)];
            source.Read(heightBytes, 0, heightBytes.Length);
            Height = BitConverter.ToDouble(heightBytes, 0);

            canvas.Strokes.Clear();

            bool end = false;

            byte[] command = new byte[1];
            while (!end)
            {
                source.Read(command, 0, command.Length);
                switch (command[0])
                {
                    case StrokeBitConverter.SIGNED_POINTER_STROKE_SIGNAL:
                        canvas.Strokes.Add(StrokeBitConverter.GetSignedPointerStroke(source));
                        break;
                    case StrokeBitConverter.SIGNED_STROKE_SIGNAL:
                        canvas.Strokes.Add(StrokeBitConverter.GetSignedStroke(source));
                        break;
                    case StrokeBitConverter.STROKES_END:
                        end = true;
                        break;
                    default:
                        Console.WriteLine("Unknown data command received");
                        end = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Provides events invoked by the InkCanvas
        /// </summary>
        public class CanvasEventsHandle
        {
            public event Action<SignedStroke> StrokeCollected = delegate { };
            public event Action<SignedStroke> StrokeRemoved = delegate { };
            public event Action<SignedPointerStroke> PointerStrokeCollected = delegate { };
            public event Action WipedStrokes = delegate { };
            public event Action WipedObjects = delegate { };

            public CanvasEventsHandle(LanCanvas canvas, SignedStrokeDrawer drawer, SignedStrokeEraser eraser, SignedPointerStrokeDrawer pointerDrawer)
            {
                drawer.StrokeDrawn += drawer_StrokeDrawn;
                eraser.StrokeErased += eraser_StrokeErased;
                pointerDrawer.PointerStrokeDrawn += pointerDrawer_PointerStrokeDrawn;
                canvas.WipedStrokes += canvas_WipedStrokes;
                canvas.WipedObjects += canvas_WipedObjects;
            }

            private void pointerDrawer_PointerStrokeDrawn(SignedPointerStroke obj)
            {
                PointerStrokeCollected(obj);
            }

            private void eraser_StrokeErased(SignedStroke obj)
            {
                StrokeRemoved(obj);
            }

            private void drawer_StrokeDrawn(SignedStroke obj)
            {
                StrokeCollected(obj);
            }

            private void canvas_WipedObjects()
            {
                WipedObjects();
            }

            private void canvas_WipedStrokes()
            {
                WipedStrokes();
            }
        }

        /// <summary>
        /// Provides methods for accessing the canvas without invoking events, 
        /// designed for server and receiver commands
        /// </summary>
        public class ManualHandle
        {
            InkCanvas canvas;
            public ManualHandle(InkCanvas canvas)
            {
                this.canvas = canvas;
            }

            public void AddSignedStroke(SignedStroke signed)
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Add(signed)));
            }

            public void AddSignedPointerStroke(SignedPointerStroke pointer)
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Add(pointer)));
                pointer.Fade(canvas.Dispatcher, (x) => canvas.Strokes.Remove(x));
            }

            public SignedStroke RemoveSignedStrokeWithId(long id)
            {
                List<Stroke> strokes = canvas.Dispatcher.Invoke(new Func<List<Stroke>>(() => canvas.Strokes.ToList()));
                foreach (Stroke stroke in strokes)
                    if (((SignedStroke)stroke).GetIdentifier() == id)
                    {
                        canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Remove(stroke)));
                        return (SignedStroke)stroke;
                    }
                return null;
            }

            public void RemoveSignedStroke(SignedStroke stroke)
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Remove(stroke)));
            }

            public void WipeStrokes()
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Strokes.Clear()));
            }

            public void WipeObjects()
            {
                canvas.Dispatcher.Invoke(new Action(() => canvas.Children.Clear()));
            }
        }

        class ModeChanger
        {
            int counter = 0;

            public List<LanCanvasEditingMode> Modes { get; private set; }

            public ModeChanger()
            {
                Modes = new List<LanCanvasEditingMode>();
            }

            public void Reset()
            {
                Modes.Clear();
                counter = 0;
            }

            public LanCanvasEditingMode PreviousMode()
            {
                counter--;
                if (counter < 0)
                    counter = Modes.Count - 1;
                return Modes[counter];
            }

            public LanCanvasEditingMode ActiveMode()
            {
                return Modes[counter];
            }

            public LanCanvasEditingMode NextMode()
            {
                counter++;
                if (counter > Modes.Count - 1)
                    counter = 0;
                return Modes[counter];
            }
        }

        class CursorLibrary
        {
            public Cursor GetCursor(LanCanvasEditingMode mode)
            {
                switch (mode)
                {
                    case LanCanvasEditingMode.None:
                        return Cursors.Arrow;
                    case LanCanvasEditingMode.Ink:
                        return Cursors.Pen;
                    case LanCanvasEditingMode.Eraser:
                        return Cursors.No;
                    case LanCanvasEditingMode.Printer:
                        return Cursors.IBeam;
                    case LanCanvasEditingMode.Knife:
                        return Cursors.Arrow;
                    case LanCanvasEditingMode.Pointer:
                        return Cursors.Cross;

                }
                return Cursors.Arrow;
            }
        }

        public class SignedPointerStrokeDrawer
        {
            public event Action<SignedPointerStroke> PointerStrokeDrawn = delegate { };

            string ownerName;
            IdGenerator generator;
            Dispatcher dispatcher;

            StylusPointCollection activePoints;
            SignedPointerStroke activeStroke;
            StrokeCollection signedStrokes;
            public bool Drawing { get; private set; }

            public SignedPointerStrokeDrawer(StrokeCollection strokes, string owner, IdGenerator generator, Dispatcher dispatcher)
            {
                this.ownerName = owner;
                this.generator = generator;
                this.dispatcher = dispatcher;
                signedStrokes = strokes;
            }

            public void BeginDrawingPointerStroke(StylusPoint beginning, PointerData data)
            {
                activePoints = new StylusPointCollection();
                activePoints.Add(beginning);
                activeStroke = new SignedPointerStroke(activePoints, data.Attributes.Clone(), data.StayTime, data.FadeTime);
                activeStroke.Id = generator.GetNextId();
                activeStroke.Owner = ownerName;
                signedStrokes.Add(activeStroke);
                Drawing = true;
            }

            public void AddPoint(StylusPoint point)
            {
                if (Drawing)
                {
                    activePoints.Add(point);
                }
                else
                    throw new NotSupportedException("Not drawing a stroke. Failed to add a point.");
            }

            public void FinishDrawingStroke(StylusPoint endPoint)
            {
                if (Drawing)
                {
                    activePoints.Add(endPoint);
                    PointerStrokeDrawn(activeStroke);

                    activeStroke.Fade(dispatcher, (x) => signedStrokes.Remove(x));

                    Drawing = false;
                    activePoints = null;
                    activeStroke = null;
                }
                else
                    throw new NotSupportedException("Not drawing a stroke. Failed to finish the drawing.");
            }
        }

        public class SignedStrokeCutter
        {
            public bool Cutting { get; private set; }

            StrokeCollection signedStrokes;
            List<Point> cutterPathPoints;

            Stroke path;
            StylusPointCollection pathPoints;

            readonly StylusShape cutterShape = new EllipseStylusShape(0.1, 0.1);

            public SignedStrokeCutter(StrokeCollection collection)
            {
                signedStrokes = collection;
                cutterPathPoints = new List<Point>();
            }

            public void BeginCutterPath(Point beginning)
            {
                cutterPathPoints.Clear();
                Cutting = true;
                cutterPathPoints.Add(beginning);
            }

            public void AddPoint(Point point)
            {
                if (Cutting)
                {
                    cutterPathPoints.Add(point);
                }
                else
                    throw new NotSupportedException("Not cutting. Failed to add a point.");
            }

            public void FinishCutterPath(Point end)
            {
                if (Cutting)
                {
                    Cutting = false;
                    cutterPathPoints.Add(end);
                    cutIfIntersects();
                }
                else
                    throw new NotSupportedException("Not cutting. Failed to finish cutting.");
            }

            public void cutIfIntersects()
            {
                foreach (Stroke stroke in signedStrokes.ToList())
                    if (stroke.HitTest(cutterPathPoints, cutterShape))
                    {
                        signedStrokes.Remove(stroke);

                        signedStrokes.Add(stroke.GetEraseResult(cutterPathPoints, cutterShape));
                    }
            }
        }

        public class SignedStrokeEraser
        {
            public bool Erasing { get; private set; }
            public event Action<SignedStroke> StrokeErased = delegate { };

            PermissionsData permissions;
            string ownerName;

            StrokeCollection signedStrokes;
            List<Point> eraserPathPoints;
            StylusShape eraserShape;

            public SignedStrokeEraser(StrokeCollection collection, PermissionsData permissions, string ownerName)
            {
                this.permissions = permissions;
                this.ownerName = ownerName;
                signedStrokes = collection;
                eraserPathPoints = new List<Point>();
            }

            public void BeginEraserPath(Point beginning, StylusShape eraserShape)
            {
                this.eraserShape = eraserShape;
                Erasing = true;
                eraserPathPoints.Clear();
                eraserPathPoints.Add(beginning);
                removeStrokeIfIntersects();
            }

            public void AddPoint(Point point)
            {
                if (Erasing)
                {
                    eraserPathPoints.Add(point);
                    removeStrokeIfIntersects();
                }
                else
                    throw new NotSupportedException("Not erasing. Failed to add a point.");
            }

            public void FinishEraserPath(Point end)
            {
                if (Erasing)
                {
                    eraserPathPoints.Add(end);
                    Erasing = false;
                    removeStrokeIfIntersects();
                }
                else
                    throw new NotSupportedException("Not erasing. Failed to finish erasing.");
            }

            private void removeStrokeIfIntersects()
            {
                foreach (Stroke stroke in signedStrokes.ToList())
                    if (stroke.HitTest(eraserPathPoints, eraserShape))
                    {
                        if ((((SignedStroke)stroke).Owner == ownerName && permissions.ManipulateOwnLines) || (((SignedStroke)stroke).Owner != ownerName && permissions.ManipulateOtherLines))
                        {
                            signedStrokes.Remove(stroke);
                            StrokeErased((SignedStroke)stroke);
                        }
                    }
            }
        }

        public class SignedStrokeDrawer
        {
            public event Action<SignedStroke> StrokeDrawn = delegate { };

            public bool Drawing { get; private set; }

            string ownerName;

            IdGenerator generator;
            StrokeCollection signedStrokes;

            SignedStroke actuallyDrawnStroke;
            StylusPointCollection activePoints;

            public SignedStrokeDrawer(StrokeCollection collection, IdGenerator generator, string ownerName)
            {
                this.ownerName = ownerName;
                this.generator = generator;
                signedStrokes = collection;
            }

            public void BeginDrawingStroke(StylusPoint beginning, DrawingAttributes attributes)
            {
                Drawing = true;
                activePoints = new StylusPointCollection();
                activePoints.Add(beginning);
                actuallyDrawnStroke = new SignedStroke(activePoints, attributes.Clone());
                actuallyDrawnStroke.Id = generator.GetNextId();
                actuallyDrawnStroke.Owner = ownerName;
                signedStrokes.Add(actuallyDrawnStroke);
            }

            public void AddPoint(StylusPoint point)
            {
                if (Drawing)
                {
                    activePoints.Add(point);
                }
                else
                    throw new NotSupportedException("Not drawing a stroke. Failed to add a point.");
            }

            public void FinishDrawingStroke(StylusPoint endPoint)
            {
                if (Drawing)
                {
                    activePoints.Add(endPoint);
                    StrokeDrawn(actuallyDrawnStroke);

                    Drawing = false;
                    activePoints = null;
                    actuallyDrawnStroke = null;
                }
                else
                    throw new NotSupportedException("Not drawing a stroke. Failed to finish the drawing.");
            }
        }
    }
}
