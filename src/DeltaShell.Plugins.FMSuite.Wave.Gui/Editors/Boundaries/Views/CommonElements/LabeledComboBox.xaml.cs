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

        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), 
                                        typeof(object), 
                                        typeof(LabeledComboBox), 
                                        new FrameworkPropertyMetadata(default(object),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedItem
        {
            get => (object) GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = 
            DependencyProperty.Register(nameof(ItemsSource), 
                                        typeof(IEnumerable), 
                                        typeof(LabeledComboBox), 
                                        new PropertyMetadata(default(IEnumerable)));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty = 
            DependencyProperty.Register(nameof(ItemTemplate), 
                                        typeof(DataTemplate), 
                                        typeof(LabeledComboBox), 
                                        new FrameworkPropertyMetadata(default(DataTemplate), 
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public LabeledComboBox()
        {
            InitializeComponent();
        }
    }
}
