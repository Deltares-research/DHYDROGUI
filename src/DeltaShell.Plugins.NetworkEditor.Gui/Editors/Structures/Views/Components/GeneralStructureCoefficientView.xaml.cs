using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for GeneralStructureCoefficientView.xaml
    /// </summary>
    public partial class GeneralStructureCoefficientView : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(GeneralStructureCoefficientView),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The value content first property
        /// </summary>
        public static readonly DependencyProperty ValueContentFirstProperty =
            DependencyProperty.Register(nameof(ValueContentFirst),
                                        typeof(string),
                                        typeof(GeneralStructureCoefficientView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The value content second property
        /// </summary>
        public static readonly DependencyProperty ValueContentSecondProperty =
            DependencyProperty.Register(nameof(ValueContentSecond),
                                        typeof(string),
                                        typeof(GeneralStructureCoefficientView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new <see cref="GeneralStructureCoefficientView"/>.
        /// </summary>
        public GeneralStructureCoefficientView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the value stored in the first value box.
        /// </summary>
        public string ValueContentFirst
        {
            get => (string) GetValue(ValueContentFirstProperty);
            set => SetValue(ValueContentFirstProperty, value);
        }

        /// <summary>
        /// Gets or sets the value stored in the second value box.
        /// </summary>
        public string ValueContentSecond
        {
            get => (string) GetValue(ValueContentSecondProperty);
            set => SetValue(ValueContentSecondProperty, value);
        }
    }
}