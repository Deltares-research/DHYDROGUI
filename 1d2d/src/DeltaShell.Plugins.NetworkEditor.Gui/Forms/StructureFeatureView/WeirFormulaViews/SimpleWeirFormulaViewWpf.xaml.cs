using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    /// <summary>
    /// Interaction logic for SimpleWeirFormulaViewWpf.xaml
    /// </summary>
    public partial class SimpleWeirFormulaViewWpf : UserControl
    {
        public SimpleWeirFormulaViewWpf()
        {
            InitializeComponent();
        }

        public SimpleWeirFormula WeirFormula
        {
            get { return (SimpleWeirFormula)GetValue(WeirFormulaProperty); }
            set { SetValue(WeirFormulaProperty, value); }
        }

        public static readonly DependencyProperty WeirFormulaProperty =
            DependencyProperty.Register(
                "WeirFormula",
                typeof(SimpleWeirFormula),
                typeof(SimpleWeirFormulaViewWpf),
                new FrameworkPropertyMetadata(new SimpleWeirFormula(), OnWeirFormulaChangedCallback));

        private static void OnWeirFormulaChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SimpleWeirFormulaViewWpf f = sender as SimpleWeirFormulaViewWpf;
            if (f != null)
            {
                f.DataContext = e.NewValue as IWeirFormula;
            }
        }
    }
}
