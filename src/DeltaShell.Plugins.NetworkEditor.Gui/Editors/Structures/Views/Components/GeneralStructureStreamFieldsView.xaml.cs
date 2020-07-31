using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for GeneralStructureStreamFieldsView.xaml
    /// </summary>
    public partial class GeneralStructureStreamFieldsView : UserControl
    {
        public static readonly DependencyProperty Upstream1Property = 
            DependencyProperty.Register(nameof(Upstream1), 
                                        typeof(double), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(double),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Upstream2Property = 
            DependencyProperty.Register(nameof(Upstream2), 
                                        typeof(double), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(double),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Downstream1Property = 
            DependencyProperty.Register(nameof(Downstream1), 
                                        typeof(double), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(double),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Downstream2Property = 
            DependencyProperty.Register(nameof(Downstream2), 
                                        typeof(double), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(double),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public GeneralStructureStreamFieldsView()
        {
            InitializeComponent();
        }

        public double Upstream1
        {
            get => (double) GetValue(Upstream1Property);
            set => SetValue(Upstream1Property, value);
        }

        public double Upstream2
        {
            get => (double) GetValue(Upstream2Property);
            set => SetValue(Upstream2Property, value);
        }

        public double Downstream1
        {
            get => (double) GetValue(Downstream1Property);
            set => SetValue(Downstream1Property, value);
        }

        public double Downstream2
        {
            get => (double) GetValue(Downstream2Property);
            set => SetValue(Downstream2Property, value);
        }
    }
}
