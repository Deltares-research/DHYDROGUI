using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    /// <summary>
    /// Interaction logic for GeneralStructureWeirViewWpf.xaml
    /// </summary>
    public partial class GeneralStructureWeirFormulaViewWpf : UserControl
    {
        public static readonly DependencyProperty WeirFormulaProperty =
            DependencyProperty.Register(
                "WeirFormula",
                typeof(GeneralStructureWeirFormula),
                typeof(GeneralStructureWeirFormulaViewWpf),
                new FrameworkPropertyMetadata(new GeneralStructureWeirFormula(), OnWeirFormulaChangedCallback));

        public GeneralStructureWeirFormulaViewWpf()
        {
            InitializeComponent();
        }

        public GeneralStructureWeirFormula WeirFormula
        {
            get
            {
                return (GeneralStructureWeirFormula) GetValue(WeirFormulaProperty);
            }
            set
            {
                SetValue(WeirFormulaProperty, value);
            }
        }

        private static void OnWeirFormulaChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var f = sender as GeneralStructureWeirFormulaViewWpf;
            if (f != null)
            {
                f.DataContext = e.NewValue as IWeirFormula;
            }
        }
    }
}