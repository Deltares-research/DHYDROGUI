using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views.CommonElements
{
    /// <summary>
    /// Interaction logic for LabeledValueBox.xaml
    /// </summary>
    public partial class LabeledValueBox : UserControl
    {
        public static readonly DependencyProperty LabelProperty = 
            DependencyProperty.Register(nameof(Label), 
                                        typeof(string), 
                                        typeof(LabeledValueBox),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledValueBoxControl"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// The unit property
        /// </summary>
        public static readonly DependencyProperty UnitProperty = 
            DependencyProperty.Register(nameof(Unit),
                                        typeof(string),
                                        typeof(LabeledValueBox), 
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The value content property
        /// </summary>
        public static readonly DependencyProperty ValueContentProperty = 
            DependencyProperty.Register(nameof(ValueContent),
                                        typeof(string), 
                                        typeof(LabeledValueBox), 
                                        new FrameworkPropertyMetadata(default(string), 
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the Value of this <see cref="LabeledValueBoxControl"/>.
        /// </summary>
        public string ValueContent
        {
            get => (string) GetValue(ValueContentProperty);
            set => SetValue(ValueContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the unit of this <see cref="LabeledValueBoxControl"/>.
        /// </summary>
        public string Unit
        {
            get => (string) GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public LabeledValueBox()
        {
            InitializeComponent();
        }
    }
}
