using System.Windows;
using System.Windows.Controls;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.Common.Gui
{
    /// <summary>
    /// Interaction logic for CoordinateSystemPicker.xaml
    /// </summary>
    public partial class CoordinateSystemPicker : UserControl
    {
        public static readonly DependencyProperty CoordinateSystemProperty = DependencyProperty.Register(
            "CoordinateSystem", typeof(ICoordinateSystem), typeof(CoordinateSystemPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CoordinateSystemPropertyChangedCallback));

        private bool internalCrsUpdate;

        public CoordinateSystemPicker()
        {
            InitializeComponent();
            ViewModel.UpdateCoordinateSystemAction = (crs) =>
            {
                if (internalCrsUpdate) return;
                SetCurrentValue(CoordinateSystemProperty, crs);
            };
        }

        public ICoordinateSystem CoordinateSystem
        {
            get { return (ICoordinateSystem) GetValue(CoordinateSystemProperty); }
            set { SetValue(CoordinateSystemProperty, value); }
        }

        private void CoordinateSystemListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedCoordinateSystem == null) return;
            CoordinateSystemButton.IsChecked = false;
        }

        private static void CoordinateSystemPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!(dependencyObject is CoordinateSystemPicker control)) return;
            var viewModel = control.ViewModel;

            if (e.Property != CoordinateSystemProperty) return;

            control.internalCrsUpdate = true;
            viewModel.SelectedCoordinateSystem = e.NewValue as ICoordinateSystem;
            control.internalCrsUpdate = false;

            control.CoordinateSystemListView.ScrollIntoView(control.CoordinateSystemListView.SelectedItem);
        }
    }
}
