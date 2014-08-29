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
    /// Interaction logic for DrawingAttributesWindow.xaml
    /// </summary>
    public partial class DrawingAttributesWindow : Window
    {
        public bool Saved { get; private set; }

        DrawingAttributes attributes;

        public DrawingAttributesWindow(DrawingAttributes attributes)
        {
            InitializeComponent();
            this.attributes = attributes;

            Saved = false;

            colorSelector.SelectedColor = attributes.Color;

            brushWidth.Value = attributes.Width;
            brushHeight.Value = attributes.Height;

            if (attributes.StylusTip == StylusTip.Ellipse)
                ellipseShape.IsChecked = true;
            else
                rectangleShape.IsChecked = true;

            fitToCurve.IsChecked = attributes.FitToCurve;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Saved = true;

            attributes.Color = colorSelector.SelectedColor;

            attributes.Width = brushWidth.Value.Value;
            attributes.Height = brushHeight.Value.Value;

            if (ellipseShape.IsChecked.Value)
                attributes.StylusTip = StylusTip.Ellipse;
            else
                attributes.StylusTip = StylusTip.Rectangle;

            attributes.FitToCurve = fitToCurve.IsChecked.Value;

            Close();
        }
    }
}
