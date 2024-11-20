using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeltaShell.NGHS.Common.Gui.Components
{
    /// <summary>
    /// Interaction logic for FileSelectorBoxRow.xaml
    /// </summary>
    public partial class FileSelectorBoxRow : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(FileSelectorBoxRow),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The button command property
        /// </summary>
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand),
                                        typeof(ICommand),
                                        typeof(FileSelectorBoxRow),
                                        new FrameworkPropertyMetadata(default(ICommand)));

        /// <summary>
        /// The button command parameter property
        /// </summary>
        public static readonly DependencyProperty ButtonCommandParameterProperty =
            DependencyProperty.Register(nameof(ButtonCommandParameter),
                                        typeof(object),
                                        typeof(FileSelectorBoxRow),
                                        new FrameworkPropertyMetadata(default(object)));

        /// <summary>
        /// Gets or sets the button command parameter.
        /// </summary>
        public object ButtonCommandParameter
        {
            get => GetValue(ButtonCommandParameterProperty);
            set => SetValue(ButtonCommandParameterProperty, value);
        }

        /// <summary>
        /// The value content property
        /// </summary>
        public static readonly DependencyProperty ValueContentProperty =
            DependencyProperty.Register(nameof(ValueContent),
                                        typeof(string),
                                        typeof(FileSelectorBoxRow),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The enable text field property
        /// </summary>
        public static readonly DependencyProperty HasEnabledTextFieldProperty =
            DependencyProperty.Register(nameof(HasEnabledTextField),
                                        typeof(bool),
                                        typeof(FileSelectorBoxRow),
                                        new FrameworkPropertyMetadata(default(bool),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets a value indicating whether the text field should be enabled.
        /// </summary>
        public bool HasEnabledTextField
        {
            get => (bool)GetValue(HasEnabledTextFieldProperty);
            set => SetValue(HasEnabledTextFieldProperty, value);
        }

        /// <summary>
        /// Creates a new <see cref="FileSelectorBoxRow"/>.
        /// </summary>
        public FileSelectorBoxRow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="FileSelectorBoxRow"/>.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the button command of this <see cref="FileSelectorBoxRow"/>.
        /// </summary>
        public ICommand ButtonCommand
        {
            get => (ICommand)GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the value content of this <see cref="FileSelectorBoxRow"/>.
        /// </summary>
        public string ValueContent
        {
            get => (string)GetValue(ValueContentProperty);
            set => SetValue(ValueContentProperty, value);
        }
    }
}
