using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.NGHS.Common.Gui.Components
{
    /// <summary>
    /// Interaction logic for LabeledValueBoxRow.xaml
    /// </summary>
    public partial class LabeledValueBoxRow : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(LabeledValueBoxRow),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The unit property
        /// </summary>
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit),
                                        typeof(string),
                                        typeof(LabeledValueBoxRow),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The value content property
        /// </summary>
        public static readonly DependencyProperty ValueContentProperty =
            DependencyProperty.Register(nameof(ValueContent),
                                        typeof(string),
                                        typeof(LabeledValueBoxRow),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public LabeledValueBoxRow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the Value of this <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public string ValueContent
        {
            get => (string)GetValue(ValueContentProperty);
            set => SetValue(ValueContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the unit of this <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
    }
}