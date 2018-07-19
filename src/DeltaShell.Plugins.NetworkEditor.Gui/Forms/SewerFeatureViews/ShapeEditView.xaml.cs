using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ShapeEditView.xaml
    /// </summary>
    public partial class ShapeEditView : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(object), typeof(ShapeEditView), new PropertyMetadata(default(object), PropertyChangedCallback));

        public ShapeEditView()
        {
            InitializeComponent();
        }

        public object Source
        {
            get { return (object) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ShapeEditView;
            if (view == null) return;

            view.ViewModel.Source = dependencyPropertyChangedEventArgs.NewValue;
        }
    }
}
