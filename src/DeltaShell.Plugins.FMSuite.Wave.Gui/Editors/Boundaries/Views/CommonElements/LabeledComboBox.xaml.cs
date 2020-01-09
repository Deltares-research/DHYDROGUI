using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Views.CommonElements
{
    /// <summary>
    /// Interaction logic for LabeledComboBox.xaml
    /// </summary>
    public partial class LabeledComboBox : UserControl
    {
        public static readonly DependencyProperty LabelProperty = 
            DependencyProperty.Register(nameof(Label), 
                                        typeof(string), 
                                        typeof(LabeledComboBox), 
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledComboBoxControl"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

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
        /// Gets or sets the selected item of this <see cref="LabeledComboBoxControl"/>.
        /// </summary>
        public object SelectedItem
        {
            get => (object) GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// The items source property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = 
            DependencyProperty.Register(nameof(ItemsSource), 
                                        typeof(IEnumerable), 
                                        typeof(LabeledComboBox), 
                                        new PropertyMetadata(default(IEnumerable)));

        /// <summary>
        /// Gets or sets the items source of this <see cref="LabeledComboBoxControl"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

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
        /// Gets or sets the item template of this <see cref="LabeledComboBoxControl"/>.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Creates a new <see cref="LabeledComboBox"/>.
        /// </summary>
        public LabeledComboBox()
        {
            InitializeComponent();
        }
    }
}
