using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for StructuresComboBox.xaml
    /// </summary>
    public partial class StructuresComboBox : UserControl
    {
        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(StructuresComboBox),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The selected item property
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem),
                                        typeof(object),
                                        typeof(StructuresComboBox),
                                        new FrameworkPropertyMetadata(default(object),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The items source property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource),
                                        typeof(IEnumerable),
                                        typeof(StructuresComboBox),
                                        new PropertyMetadata(default(IEnumerable)));

        /// <summary>
        /// The item template property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate),
                                        typeof(DataTemplate),
                                        typeof(StructuresComboBox),
                                        new FrameworkPropertyMetadata(default(DataTemplate),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public StructuresComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="StructuresComboBox"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected item of this <see cref="StructuresComboBox"/>.
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Gets or sets the items source of this <see cref="StructuresComboBox"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the item template of this <see cref="StructuresComboBox"/>.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
    }
}