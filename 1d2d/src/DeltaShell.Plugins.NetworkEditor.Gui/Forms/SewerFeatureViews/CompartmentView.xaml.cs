using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for CompartmentView.xaml
    /// </summary>
    public partial class CompartmentView : UserControl
    {
        public static readonly DependencyProperty CompartmentProperty = DependencyProperty.Register(
            nameof(Compartment), typeof(Compartment), typeof(CompartmentView), new PropertyMetadata(default(Compartment), PropertyChangedCallback));

        public CompartmentView()
        {
            InitializeComponent();
        }

        public Compartment Compartment
        {
            get { return (Compartment) GetValue(CompartmentProperty); }
            set { SetValue(CompartmentProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as CompartmentView;
            if (view == null) return;

            view.ViewModel.Compartment = dependencyPropertyChangedEventArgs.NewValue as Compartment;
        }
    }
}
