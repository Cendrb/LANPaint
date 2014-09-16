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
using System.Windows.Shapes;

namespace LANPaint_Server
{
    /// <summary>
    /// Interaction logic for BasicServerData.xaml
    /// </summary>
    public partial class BasicServerData : Window
    {
        public bool Success { get; private set; }

        public BasicServerData(double defaultX, double defaultY)
        {
            InitializeComponent();

            xSize.Value = defaultX;
            ySize.Value = defaultY;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if(xSize.Value > 0 && ySize.Value > 0 && nameTextBox.Text != String.Empty)
            {
                Success = true;

                Close();
            }
        }
    }
}
