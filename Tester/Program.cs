using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LANPaint_Client;
using LANPaint_Server;
using Util;
using System.Threading;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread main = new Thread(new ThreadStart(start));
            main.SetApartmentState(ApartmentState.STA);
            main.Start();
            Console.ReadKey();
        }

        private static void start()
        {
            MainClient client = new MainClient();
            MainServer server = new MainServer();

            client.InitializeComponent();
            server.InitializeComponent();

            client.Show();
            server.Show();
        }
    }
}
