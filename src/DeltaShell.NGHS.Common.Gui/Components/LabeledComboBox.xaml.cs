using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.NGHS.Common.Gui.Components
{
    /// <summary>
    /// Interaction logic for LabeledComboBox.xaml
    /// </summary>
    public partial class LabeledComboBox : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(LabeledComboBox),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The selected item property
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem),
                                        typeof(object),
                                        typeof(LabeledComboBox),
                                        new FrameworkPropertyMetadata(default(object),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The items source property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource),
                                        typeof(IEnumerable),
                                        typeof(LabeledComboBox),
                                        new PropertyMetadata(default(IEnumerable)));

        /// <summary>
        /// The item template property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate),
                                        typeof(DataTemplate),
                                        typeof(LabeledComboBox),
                                        new FrameworkPropertyMetadata(default(DataTemplate),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new <see cref="LabeledComboBox"/>.
        /// </summary>
        public LabeledComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledComboBox"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected item of this <see cref="LabeledComboBox"/>.
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Gets or sets the items source of this <see cref="LabeledComboBox"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the item template of this <see cref="LabeledComboBox"/>.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
    }
}