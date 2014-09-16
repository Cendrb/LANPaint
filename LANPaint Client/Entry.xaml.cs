using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LANPaint_Client
{
    /// <summary>
    /// Interaction logic for Entry.xaml
    /// </summary>
    public partial class Entry : Window
    {
        public Entry()
        {
            InitializeComponent();

            Visibility = System.Windows.Visibility.Hidden;

            ConnectDialog dialog = new ConnectDialog();
            dialog.ShowDialog();
        }
    }
}
