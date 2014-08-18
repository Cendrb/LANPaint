using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LANPaint_Client;
using LANPaint_Server;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Server server = new Server(serverCanvas, "Vzdálený penis");
            server.Start();
            Client painter = new Client(clientCanvas, "Klientský penis");
            
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            Width = canvas.Width + Margin.Left + Margin.Right;
            Height = (canvas.Height + Margin.Top + Margin.Bottom) * 2;
        }
    }
}
