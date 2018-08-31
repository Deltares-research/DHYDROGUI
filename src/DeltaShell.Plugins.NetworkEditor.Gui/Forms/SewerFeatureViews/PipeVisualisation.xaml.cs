using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PipeVisualisationControl.xaml
    /// </summary>
    public partial class PipeVisualisation : UserControl
    {
        public static readonly DependencyProperty PipeProperty = DependencyProperty.Register(
            nameof(Pipe), 
            typeof(Pipe), 
            typeof(PipeVisualisation), 
            new PropertyMetadata(default(Pipe), PropertyChangedCallback));

        public PipeVisualisation()
        {
            InitializeComponent();
            ViewModel.GetActualWidth = () => ViewGrid.ActualWidth;
            ViewModel.GetActualHeight = () => ViewGrid.ActualHeight;
            ViewModel.DrawingCanvas = () => DrawingCanvas;
        }

        public Pipe Pipe
        {
            get { return (Pipe)GetValue(PipeProperty); }
            set { SetValue(PipeProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as PipeVisualisation;
            if (view == null) return;

            view.ViewModel.Pipe = dependencyPropertyChangedEventArgs.NewValue as Pipe;
        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel?.Update();
        }


    }
}
