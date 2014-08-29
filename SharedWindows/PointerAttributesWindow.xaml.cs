using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SharedWindows
{
    /// <summary>
    /// Interaction logic for PointerAttributesWindow.xaml
    /// </summary>
    public partial class PointerAttributesWindow : Window
    {
        PointerData data;

        public PointerAttributesWindow(PointerData data)
        {
            InitializeComponent();

            this.data = data;

            stayTime.Value = data.StayTime;
            fadeTime.Value = data.FadeTime;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            data.StayTime = stayTime.Value.Value;
            data.FadeTime = fadeTime.Value.Value;
            Close();
        }

        private void brushProperties_Click(object sender, RoutedEventArgs e)
        {
            DrawingAttributesWindow dialog = new DrawingAttributesWindow(data.Attributes);
            dialog.ShowDialog();
        }
    }
}
