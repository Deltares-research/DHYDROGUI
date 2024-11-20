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
        [DisplayName("Correction coefficient")]
        [Description("Correction coefficient")]
        public double CorrectionCoefficient
        {
            get { return SimpleWeirFormula.CorrectionCoefficient; }
            set { SimpleWeirFormula.CorrectionCoefficient = value; }
        }
    }
}