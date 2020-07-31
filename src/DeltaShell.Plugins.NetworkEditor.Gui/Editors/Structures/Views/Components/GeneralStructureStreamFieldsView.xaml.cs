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
                                        typeof(string), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Upstream2Property = 
            DependencyProperty.Register(nameof(Upstream2), 
                                        typeof(string), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Downstream1Property = 
            DependencyProperty.Register(nameof(Downstream1), 
                                        typeof(string), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty Downstream2Property = 
            DependencyProperty.Register(nameof(Downstream2), 
                                        typeof(string), 
                                        typeof(GeneralStructureStreamFieldsView), 
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public GeneralStructureStreamFieldsView()
        {
            InitializeComponent();
        }

        public string Upstream1
        {
            get => (string) GetValue(Upstream1Property);
            set => SetValue(Upstream1Property, value);
        }

        public string Upstream2
        {
            get => (string) GetValue(Upstream2Property);
            set => SetValue(Upstream2Property, value);
        }

        public string Downstream1
        {
            get => (string) GetValue(Downstream1Property);
            set => SetValue(Downstream1Property, value);
        }

        public string Downstream2
        {
            get => (string) GetValue(Downstream2Property);
            set => SetValue(Downstream2Property, value);
        }
    }
}
