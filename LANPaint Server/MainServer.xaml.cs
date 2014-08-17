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

namespace LANPaint_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainServer : Window
    {
        ServerPainterManager manager;
        public MainServer()
        {
            InitializeComponent();

            manager = new ServerPainterManager(mainCanvas, "Superserver penis");

            manager.StartAsync();

            mainCanvas.SizeChanged += mainCanvas_SizeChanged;
            mainCanvas.Width = 720;
            mainCanvas.Height = 480;
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            Width = canvas.Width + Margin.Left + Margin.Right;
            Height = canvas.Height + Margin.Top + Margin.Bottom;
        }
    }
}
