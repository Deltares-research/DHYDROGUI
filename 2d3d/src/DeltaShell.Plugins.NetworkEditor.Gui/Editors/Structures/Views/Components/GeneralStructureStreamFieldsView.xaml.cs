using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for GeneralStructureStreamFieldsView.xaml
    /// </summary>
    public partial class GeneralStructureStreamFieldsView : UserControl
    {
        /// <summary>
        /// The upstream1 property
        /// </summary>
        public static readonly DependencyProperty Upstream1Property =
            DependencyProperty.Register(nameof(Upstream1),
                                        typeof(string),
                                        typeof(GeneralStructureStreamFieldsView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The upstream2 property
        /// </summary>
        public static readonly DependencyProperty Upstream2Property =
            DependencyProperty.Register(nameof(Upstream2),
                                        typeof(string),
                                        typeof(GeneralStructureStreamFieldsView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The downstream1 property
        /// </summary>
        public static readonly DependencyProperty Downstream1Property =
            DependencyProperty.Register(nameof(Downstream1),
                                        typeof(string),
                                        typeof(GeneralStructureStreamFieldsView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The downstream2 property
        /// </summary>
        public static readonly DependencyProperty Downstream2Property =
            DependencyProperty.Register(nameof(Downstream2),
                                        typeof(string),
                                        typeof(GeneralStructureStreamFieldsView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new <see cref="GeneralStructureStreamFieldsView"/>.
        /// </summary>
        public GeneralStructureStreamFieldsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the upstream1.
        /// </summary>
        public string Upstream1
        {
            get => (string) GetValue(Upstream1Property);
            set => SetValue(Upstream1Property, value);
        }

        /// <summary>
        /// Gets or sets the upstream2.
        /// </summary>
        public string Upstream2
        {
            get => (string) GetValue(Upstream2Property);
            set => SetValue(Upstream2Property, value);
        }

        /// <summary>
        /// Gets or sets the downstream1.
        /// </summary>
        public string Downstream1
        {
            get => (string) GetValue(Downstream1Property);
            set => SetValue(Downstream1Property, value);
        }

        /// <summary>
        /// Gets or sets the downstream2.
        /// </summary>
        public string Downstream2
        {
            get => (string) GetValue(Downstream2Property);
            set => SetValue(Downstream2Property, value);
        }
    }
}