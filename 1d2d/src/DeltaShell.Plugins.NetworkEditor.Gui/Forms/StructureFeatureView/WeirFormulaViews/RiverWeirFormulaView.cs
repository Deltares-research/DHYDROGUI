using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    public partial class RiverWeirFormulaView : UserControl, IView
    {
        public RiverWeirFormulaView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets or gets image set on the title of the view.
        /// </summary>
        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private RiverWeirFormula data;

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        public object Data
        {
            get { return data; }
            set
            {
                data = (RiverWeirFormula) value;
                bindingSourceRiverWeir.DataSource = data;
            }
        }

        private static IFunction EditReductionTable(IFunction source, string title)
        {
            EditFunctionDialog editFunctionDialog = new EditFunctionDialog {Text = title};
            IFunction dialogData = (IFunction) source.Clone();
            editFunctionDialog.ColumnNames = new[] { "H2-Zs / H1-Zs", "Reduction" };
            editFunctionDialog.Data = dialogData;
            return DialogResult.OK == editFunctionDialog.ShowDialog() ? dialogData : source;
        }

        private void buttonFlowReduction_Click(object sender, System.EventArgs e)
        {
            RiverWeirFormula riverWeirFormula = data;
            riverWeirFormula.SubmergeReductionPos = EditReductionTable(riverWeirFormula.SubmergeReductionPos,
                                                                       "Reduction Curve Table for Positive Flow");
        }

        private void buttonReductionReverse_Click(object sender, System.EventArgs e)
        {
            RiverWeirFormula riverWeirFormula = data;
            riverWeirFormula.SubmergeReductionNeg = EditReductionTable(riverWeirFormula.SubmergeReductionNeg,
                                                                       "Reduction Curve Table for Reverse Flow");
        }
    }
}