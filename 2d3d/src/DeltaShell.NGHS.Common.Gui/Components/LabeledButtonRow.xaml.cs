using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeltaShell.NGHS.Common.Gui.Components
{
    /// <summary>
    /// Interaction logic for LabeledButtonRow.xaml
    /// </summary>
    public partial class LabeledButtonRow : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(LabeledButtonRow),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The button label property
        /// </summary>
        public static readonly DependencyProperty ButtonLabelProperty =
            DependencyProperty.Register(nameof(ButtonLabel),
                                        typeof(string),
                                        typeof(LabeledButtonRow),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The button command property
        /// </summary>
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand),
                                        typeof(ICommand),
                                        typeof(LabeledButtonRow),
                                        new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Creates a new <see cref="LabeledButtonRow"/>.
        /// </summary>
        public LabeledButtonRow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledButtonRow"/>.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the button label of this <see cref="LabeledButtonRow"/>.
        /// </summary>
        public string ButtonLabel
        {
            get => (string)GetValue(ButtonLabelProperty);
            set => SetValue(ButtonLabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the button command of this <see cref="LabeledButtonRow"/>.
        /// </summary>
        public ICommand ButtonCommand
        {
            get => (ICommand)GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }
    }
}