using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeVisualisationControl.xaml
    /// </summary>
    public partial class ManholeVisualisation : UserControl
    {
        public static readonly DependencyProperty ManholeProperty = DependencyProperty.Register(
            nameof(Manhole),
            typeof(Manhole),
            typeof(ManholeVisualisation),
            new PropertyMetadata(default(ObservableCollection<Manhole>), PropertyChangedCallback));

        public ManholeVisualisation()
        {
            InitializeComponent();
            ViewModel.ContainerWidth = () => ViewGrid.ActualWidth;
            ViewModel.ContainerHeight = () => ViewGrid.ActualHeight;
            ViewModel.SetWindowSize = SetViewGridSize;
        }

        public Manhole Manhole
        {
            get { return (Manhole)GetValue(ManholeProperty); }
            set { SetValue(ManholeProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisation;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }

        private void SetViewGridSize()
        {
            var ratio = ViewModel.HeigthWidthRatio;
            if (double.IsNaN(ratio)) return;

            if (UserControl.ActualHeight / ratio < UserControl.ActualWidth)
            {
                // Adjust height to available height, scale width by ratio
                var height = UserControl.ActualHeight;
                ViewGrid.Height = height;
                ViewGrid.Width = height / ratio;
                return;
            }

            // Adjust width to available width, scale height by ratio
            var width = UserControl.ActualWidth;
            ViewGrid.Width = width;
            ViewGrid.Height = width * ratio;

        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetViewGridSize();
            ViewModel.UpdateShapePositions();
        }
    }
}