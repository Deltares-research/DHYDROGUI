using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeVisualisationControl.xaml
    /// </summary>
    public partial class ManholeVisualisationControl : UserControl
    {
        public static readonly DependencyProperty ManholeProperty = DependencyProperty.Register(
            nameof(Manhole), 
            typeof(Manhole), 
            typeof(ManholeVisualisationControl), 
            new PropertyMetadata(default(ObservableCollection<Manhole>), PropertyChangedCallback));

        public ManholeVisualisationControl()
        {
            InitializeComponent();
            ViewModel.ContainerWidth = () => ViewGrid.ActualWidth;
            ViewModel.ContainerHeight = () => ViewGrid.ActualHeight;
            ViewModel.SetWindowSize = SetWindowSize;
        }

        public Manhole Manhole
        {
            get { return (Manhole) GetValue(ManholeProperty); }
            set { SetValue(ManholeProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisationControl;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }

        private void SetWindowSize()
        {
            var ratio = ViewModel.HeigthWidthRatio;

            if (double.IsNaN(ratio)) return;

            if (ratio >= 1)
            {
                var width = ViewGrid.ActualHeight / ratio;
                if (width > UserControl.Width)
                {
                    width = UserControl.Width;
                    ViewGrid.Height = width * ratio;
                }

                ViewGrid.Width = width;
                return;
            }

            if (ratio < 1)
            {
                var height = ViewGrid.ActualWidth * ratio;
                if (height > UserControl.Height)
                {
                    height = UserControl.Height;
                    ViewGrid.Width = height / ratio;
                }
                ViewGrid.Height = height;
                return;
            }

            
        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetWindowSize();
            ViewModel.UpdateShapePositions();
        }
    }
}