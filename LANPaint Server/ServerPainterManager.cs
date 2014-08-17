﻿using System;
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
using Util;

namespace LANPaint_Server
{
    public class ServerPainterManager
    {
        public string Name { get; private set; }
        PainterClientCollection clients;
        public bool Listening { get; private set; }
        TcpListener listener;
        public InkCanvas MainCanvas { get; private set; }

        public ServerPainterManager(InkCanvas canvas, string name)
        {
            Name = name;
            clients = new PainterClientCollection();
            listener = new TcpListener(IPAddress.Any, StaticPenises.CS_PORT);
            MainCanvas = canvas;
        }

        public void StartAsync()
        {
            Task.Factory.StartNew(new Action(Start));
        }

        public void Start()
        {
            Thread.CurrentThread.Name = "New client register";
            listener.Start();
            Listening = true;
            while (Listening)
            {
                TcpClient client = listener.AcceptTcpClient();
                PainterReceiver receiver = new PainterReceiver(client, MainCanvas);
                PainterSender sender = new PainterSender(MainCanvas, Name);
                PainterClient painter = new PainterClient(receiver, sender);
                try
                {
                    clients.Add(painter);
                    IPEndPoint remote = (IPEndPoint)client.Client.RemoteEndPoint;
                    painter.Sender.Connect(remote.Address, StaticPenises.SC_PORT);
                    painter.Receiver.Disconnected += Receiver_Disconnected;
                    painter.Receiver.StrokeReceived += (stroke) => sendStrokeToAllClientsBut(stroke, painter);
                    painter.Receiver.StrokeRemoved += (stroke) => removeStrokeInAllClientsBut(stroke, painter);
                    painter.Sender.SendWholeCanvas();
                }
                catch (NameDuplicateException e)
                {
                    Log.Error(e);
                    painter.Sender.SendDisconnect();
                    client.Close();
                }
            }
        }

        private void sendStrokeToAllClientsBut(SignedStroke stroke, PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    Task.Factory.StartNew(new Action(() => client.Sender.SendStroke(stroke)));
        }

        private void removeStrokeInAllClientsBut(SignedStroke stroke, PainterClient but)
        {
            foreach (PainterClient client in clients)
                if (client != but)
                    Task.Factory.StartNew(new Action(() => client.Sender.RemoveStroke(stroke)));
        }

        private void Receiver_Disconnected(PainterReceiver obj)
        {
            clients.Remove(obj).Sender.Disconnect();
            Log.Debug(String.Format("Client {0} disconnected", obj.Name));
        }

        public void Stop()
        {
            Listening = false;
            listener.Stop();
        }
    }
}