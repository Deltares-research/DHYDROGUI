using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PipeVisualisationControl.xaml
    /// </summary>
    public partial class SewerConnectionVisualization : UserControl
    {
        public static readonly DependencyProperty SewerConnectionProperty = DependencyProperty.Register(
            nameof(SewerConnection), 
            typeof(ISewerConnection), 
            typeof(SewerConnectionVisualization), 
            new PropertyMetadata(default(Pipe), PropertyChangedCallback));

        public SewerConnectionVisualization()
        {
            InitializeComponent();
            ViewModel.GetActualWidth = () => ViewGrid.ActualWidth;
            ViewModel.GetActualHeight = () => ViewGrid.ActualHeight;
            ViewModel.DrawingCanvas = () => DrawingCanvas;
        }

        public ISewerConnection SewerConnection
        {
            get { return (Pipe)GetValue(SewerConnectionProperty); }
            set { SetValue(SewerConnectionProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as SewerConnectionVisualization;
            if (view == null) return;

            view.ViewModel.SewerConnection = dependencyPropertyChangedEventArgs.NewValue as ISewerConnection;
        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel?.Update();
        }
    }
}
