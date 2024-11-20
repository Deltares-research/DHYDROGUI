using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.NGHS.Common.Gui.Components
{
    /// <summary>
    /// Interaction logic for LabeledCheckBoxRow.xaml
    /// </summary>
    public partial class LabeledCheckBoxRow : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(LabeledCheckBoxRow),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The IsChecked property
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked),
                                        typeof(bool),
                                        typeof(LabeledCheckBoxRow),
                                        new FrameworkPropertyMetadata(default(bool),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new instance of <see cref="LabeledCheckBoxRow"/>.
        /// </summary>
        public LabeledCheckBoxRow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledCheckBoxControl"/>.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
    }
}