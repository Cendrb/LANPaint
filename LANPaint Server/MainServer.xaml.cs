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
        Server manager;
        public MainServer()
        {
            InitializeComponent();

            BasicServerData basic = new BasicServerData(SystemParameters.PrimaryScreenWidth - 50, SystemParameters.PrimaryScreenHeight - 50);
            basic.ShowDialog();

            if (basic.Success)
            {
                manager = new Server(mainCanvas, basic.nameTextBox.Text, clientsListBox.Items);

                manager.StartAsync();

                mainCanvas.SizeChanged += mainCanvas_SizeChanged;
                mainCanvas.Width = basic.xSize.Value.Value;
                mainCanvas.Height = basic.ySize.Value.Value;
            }
            else
                Close();
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            double canvasPartWidth = canvas.Width + Margin.Left + Margin.Right;
            Width = canvasPartWidth + canvasPartWidth / 3;
            Height = canvas.Height + Margin.Top + Margin.Bottom;
        }
    }
}
