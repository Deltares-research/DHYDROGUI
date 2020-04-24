using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class SimpleWeirFormulaProperties : WeirFormulaProperties
    {
        public SimpleWeirFormulaProperties(SimpleWeirFormula simpleWeirFormula, IWeir weir) : base(simpleWeirFormula, weir)
        {
        }

        private SimpleWeirFormula SimpleWeirFormula
        {
            get { return (SimpleWeirFormula) weirFormula; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Is gated")]
        public bool IsGated
        {
            get { return SimpleWeirFormula.IsGated; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Discharge coefficient")]
        [Description("Discharge coefficient Ce.")]
        public double DischargeCoefficient
        {
            get { return SimpleWeirFormula.DischargeCoefficient; }
            set { SimpleWeirFormula.DischargeCoefficient = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Lateral contraction")]
        [Description("Lateral contraction Cw.")]
        public double LateralContraction
        {
            get { return SimpleWeirFormula.LateralContraction; }
            set { SimpleWeirFormula.LateralContraction = value; }
        }
    }
}