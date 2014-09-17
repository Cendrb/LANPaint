using Microsoft.Win32;
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
        Server server;
        public MainServer()
        {
            InitializeComponent();

            saveMenu.Click += saveMenu_Click;
            loadMenu.Click += loadMenu_Click;

            BasicServerData basic = new BasicServerData(SystemParameters.PrimaryScreenWidth - 50, SystemParameters.PrimaryScreenHeight - 50);
            basic.ShowDialog();

            if (basic.Success)
            {
                server = new Server(mainCanvas, basic.nameTextBox.Text, clientsListBox.Items);

                server.StartAsync();

                mainCanvas.SizeChanged += mainCanvas_SizeChanged;
                mainCanvas.Width = basic.xSize.Value.Value;
                mainCanvas.Height = basic.ySize.Value.Value;
            }
            else
                Close();
        }

        private void loadMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "LANPaint saved canvas (*.cnv)|*.cnv|All Files (*.*)|*.*";
            dialog.ShowDialog();

            if (dialog.FileName != String.Empty && dialog.CheckFileExists)
            {
                server.LoadCanvas(dialog.FileName);

                server.RefreshClientsCanvases();
            }
        }

        private void saveMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "LANPaint saved canvas (*.cnv)|*.cnv|All Files (*.*)|*.*";
            dialog.ShowDialog();

            if (dialog.FileName != String.Empty && dialog.CheckPathExists)
            {
                server.SaveCanvas(dialog.FileName);
            }
        }

        public void mainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InkCanvas canvas = sender as InkCanvas;
            double canvasPartWidth = canvas.Width + Margin.Left + Margin.Right;
            Width = canvasPartWidth + 220;
            Height = canvas.Height + Margin.Top + Margin.Bottom;
        }
    }
}
