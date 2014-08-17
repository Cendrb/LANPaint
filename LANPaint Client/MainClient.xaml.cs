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

namespace LANPaint_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainClient : Window
    {
        Painter painter;

        public MainClient()
        {
            InitializeComponent();

            Random rnd = new Random();

            painter = new Painter(mainCanvas, "Penis" + rnd.Next());
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            Width = canvas.Width + Margin.Left + Margin.Right;
            Height = canvas.Height + Margin.Top + Margin.Bottom;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            painter.Sender.SendDisconnect();
        }

    }
}
