using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Util;

namespace LANPaint_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainClient : Window
    {
        Painter painter;

        bool closed = false;

        public MainClient(string name, IPAddress target)
        {
            InitializeComponent();

            LanCanvas lanCanvas = new LanCanvas(mainCanvas, new IdGenerator(), name, PermissionsData.Default);
            painter = new Painter(lanCanvas, name);
            painter.Disconnected += painter_Disconnected;
            painter.Connect(target, StaticPenises.CS_PORT, StaticPenises.SC_PORT);
            painter.RequestWholeCanvas();
        }

        private void painter_Disconnected(Painter obj)
        {
            if (!closed)
            {
                Dispatcher.Invoke(new Action(() => Close()));
                MessageBox.Show("Unable to communicate with the server", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            Width = canvas.Width + Margin.Left + Margin.Right;
            Height = canvas.Height + Margin.Top + Margin.Bottom;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closed = true;
            painter.Disconnect();
        }

    }
}
