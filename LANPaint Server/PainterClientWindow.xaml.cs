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
using Util;

namespace LANPaint_Server
{
    /// <summary>
    /// Interaction logic for PainterClientWindow.xaml
    /// </summary>
    public partial class PainterClientWindow : UserControl
    {
        public PainterClient Client { get; private set; }

        public PainterClientWindow(PainterClient client)
        {
            InitializeComponent();

            DataContext = this;

            this.Client = client;

            nameLabel.Content = client.PainterPenis.RemoteName;
        }

        private void kickButton_Click(object sender, RoutedEventArgs e)
        {
            Client.PainterPenis.Disconnect();
        }

        private void permissionsButton_Click(object sender, RoutedEventArgs e)
        {
            Permissions perm = new Permissions(Client.PainterPenis.Permissions);
            perm.ShowDialog();
            if (perm.Saved)
                Client.PainterPenis.UpdatePermissions();
        }
    }
}
