using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using Util;

namespace penis
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(new Action(client));
            Task.Factory.StartNew(new Action(server));

            Console.ReadKey();
        }

        private static void client()
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5156));
            byte[] dan = new byte[100];
            client.Client.Receive(dan);
        }

        private static void server()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5156);
            listener.Start();
            TcpClient remote = listener.AcceptTcpClient();
            listener.Stop();
            remote.Client.Send(BitConverter.GetBytes(69));
        }

        private static void fap(string gay)
        {
            lock(gay)
            {
                Console.WriteLine(gay);
                Console.WriteLine("fap begin");
                Thread.Sleep(3000);
                Console.WriteLine("fap end");
            }
        }

        private static void paf(string gay)
        {
            lock (gay)
            {
                Console.WriteLine(gay);
                Console.WriteLine("paf begin");
                Thread.Sleep(3000);
                Console.WriteLine("paf end");
            }
        }


        static bool Penis<T>(T fap, T paf)
        {
            return fap.Equals(paf);
        }
    }

    class Penios
    {
        public int Length { get; private set; }
        public Penios(int length)
        {
            this.Length = length;
        }

        public static bool operator == (Penios first, Penios second)
        {
            return first.Length == second.Length;
        }

        public static bool operator !=(Penios first, Penios second)
        {
            return first.Length != second.Length;
        }
        public override bool Equals(object obj)
        {
            return this == obj as Penios;
        }
        public override int GetHashCode()
        {
            return Length;
        }
    }
}
