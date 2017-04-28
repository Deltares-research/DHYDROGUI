using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

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

        [Category("General")]
        [DisplayName("Is Gated")]
        public bool IsGated
        {
            get { return SimpleWeirFormula.IsGated; }
        }

        [Category("General")]
        [DisplayName("Discharge Coefficient")]
        [Description("Discharge coefficient Ce")]
        public double DischargeCoefficient
        {
            get { return SimpleWeirFormula.DischargeCoefficient; }
            set { SimpleWeirFormula.DischargeCoefficient = value; }
        }

        [Category("General")]
        [DisplayName("Lateral Contraction")]
        [Description("Lateral contraction Cw")]
        public double LateralContraction
        {
            get { return SimpleWeirFormula.LateralContraction; }
            set { SimpleWeirFormula.LateralContraction = value; }
        }
    }
}