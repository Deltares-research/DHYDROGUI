using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PumpShapeControl.xaml
    /// </summary>
    public partial class PumpShapeControl : UserControl
    {
        public static readonly DependencyProperty PumpShapeProperty = DependencyProperty.Register(nameof(PumpShape), typeof(PumpShape), typeof(PumpShapeControl), new PropertyMetadata(default(PumpShape), PropertyChangedCallback));

        public PumpShape PumpShape
        {
            get { return (PumpShape) GetValue(PumpShapeProperty); }
            set { SetValue(PumpShapeProperty, value); }
        }

        public PumpShapeControl()
        {
            InitializeComponent();
            ViewModel.GetActualWidth = () => ViewGrid.ActualWidth;
            ViewModel.GetActualHeight = () => ViewGrid.ActualHeight;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as PumpShapeControl;
            if (view == null) return;

            var pumpShape = dependencyPropertyChangedEventArgs.NewValue as PumpShape;
            if (pumpShape != null)
            {
                view.ViewModel.PumpShape = pumpShape;
            }
        }


        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSizes();
        }

        private void UpdateSizes()
        {
            ViewModel.Update();
        }

        
    }
}
