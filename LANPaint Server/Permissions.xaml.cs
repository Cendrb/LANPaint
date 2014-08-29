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
using Util;

namespace LANPaint_Server
{
    /// <summary>
    /// Interaction logic for Permissions.xaml
    /// </summary>
    public partial class Permissions : Window
    {
        PermissionsData data;
        public bool Saved { get; private set; }

        public Permissions(PermissionsData data)
        {
            InitializeComponent();

            this.data = data;

            Saved = false;

            manipulateOwnLines.IsChecked = data.ManipulateOwnLines;
            manipulateOtherLines.IsChecked = data.ManipulateOtherLines;
            manipulateOwnObjects.IsChecked = data.ManipulateOwnObjects;
            manipulateOtherObjects.IsChecked = data.ManipulateOtherObjects;
            wipeLines.IsChecked = data.WipeStrokes;
            wipeObjects.IsChecked = data.WipeObjects;
            usePointers.IsChecked = data.UsePointers;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Saved = true;

            data.ManipulateOwnLines = manipulateOwnLines.IsChecked.Value;
            data.ManipulateOtherLines = manipulateOtherLines.IsChecked.Value;
            data.ManipulateOwnObjects = manipulateOwnObjects.IsChecked.Value;
            data.ManipulateOtherObjects = manipulateOtherObjects.IsChecked.Value;
            data.WipeStrokes = wipeLines.IsChecked.Value;
            data.WipeObjects = wipeObjects.IsChecked.Value;
            data.UsePointers = usePointers.IsChecked.Value;

            Close();
        }
    }
}
