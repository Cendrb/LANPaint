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
using System.Windows.Input;
using Util;

namespace LANPaint_Server
{
    public class Server
    {
        public string Name { get; private set; }
        PainterClientCollection clients;
        public bool Listening { get; private set; }
        public InkCanvas MainCanvas { get; private set; }
        ItemCollection connectedClientsView;
        LanCanvas lanCanvas;

        public Server(InkCanvas canvas, string name, ItemCollection connectedClients)
        {
            //ConsoleManager.Show();

            MainCanvas = canvas;

            lanCanvas = new LanCanvas(canvas, new IdGenerator(), name, PermissionsData.Default);

            this.connectedClientsView = connectedClients;
            Name = name;
            clients = new PainterClientCollection();
        }

        public void StartAsync()
        {
            Task.Factory.StartNew(new Action(Start));
        }

        private void Start()
        {
            Thread.CurrentThread.Name = "New client register";
            Listening = true;
            while (Listening)
            {
                Painter painter = new Painter(lanCanvas, Name);
                painter.Listen(IPAddress.Any, StaticPenises.SC_PORT, StaticPenises.CS_PORT);
                PainterClient client = new PainterClient(painter, MainCanvas.Dispatcher);
                try
                {
                    clients.Add(client);
                    MainCanvas.Dispatcher.BeginInvoke(new Action(() => connectedClientsView.Add(client.ControlComponent)));
                    painter.Disconnected += painter_Disconnected;
                    painter.ReceiverHandle.StrokeReceived += (stroke) => sendStrokeToAllClientsBut(stroke, client);
                    painter.ReceiverHandle.StrokeRemoved += (stroke) => removeStrokeInAllClientsBut(stroke, client);
                    painter.ReceiverHandle.PointerStrokeReceived += (pointer) => sendPointerStrokeToAllClientsBut(pointer, client);
                    painter.ReceiverHandle.StrokesWiped += () => wipeAllClientsStrokesBut(client);
                    painter.ReceiverHandle.ObjectsWiped += () => wipeAllClientsObjectsBut(client);
                }
                catch (NameDuplicateException e)
                {
                    Log.Error(e);
                    painter.Disconnect();
                }
            }
        }

        private void painter_Disconnected(Painter obj)
        {
            PainterClient client = clients.Remove(obj);
            MainCanvas.Dispatcher.BeginInvoke(new Action(() => connectedClientsView.Remove(client.ControlComponent)));
            Log.Debug(String.Format("Client {0} disconnected", obj.RemoteName));
        }

        private void wipeAllClientsObjectsBut(PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    client.PainterPenis.ServerHandle.WipeObjects();
        }

        private void wipeAllClientsStrokesBut(PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    client.PainterPenis.ServerHandle.WipeStrokes();
        }

        private void sendPointerStrokeToAllClientsBut(SignedPointerStroke pointer, PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    client.PainterPenis.ServerHandle.SendPointerStroke(pointer);
        }

        private void sendStrokeToAllClientsBut(SignedStroke stroke, PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    client.PainterPenis.ServerHandle.SendSignedStroke(stroke);
        }

        private void removeStrokeInAllClientsBut(SignedStroke stroke, PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    client.PainterPenis.ServerHandle.RemoveSignedStroke(stroke);
        }

        public void Stop()
        {
            Listening = false;
        }
    }
}
