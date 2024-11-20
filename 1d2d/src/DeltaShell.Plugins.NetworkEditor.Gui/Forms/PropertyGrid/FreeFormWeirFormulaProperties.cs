using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class FreeFormWeirFormulaProperties : WeirFormulaProperties
    {
        public FreeFormWeirFormulaProperties(FreeFormWeirFormula freeFormWeirFormula, IWeir weir) : base(freeFormWeirFormula, weir)
        {
        }

        private FreeFormWeirFormula FreeFormWeirFormula
        {
            get { return (FreeFormWeirFormula)weirFormula; }
        }

        [DisplayName("Crest width")]
        public double CrestWidth
        {
            get { return FreeFormWeirFormula.CrestWidth; }
        }

        [DisplayName("Crest level")]
        public double CrestLevel
        {
            get { return FreeFormWeirFormula.CrestLevel; }
        }

        [DisplayName("Discharge coefficient")]
        [Description("Discharge coefficient Ce")]
        public double DischargeCoefficient
        {
            get { return FreeFormWeirFormula.DischargeCoefficient; }
            set { FreeFormWeirFormula.DischargeCoefficient = value; }
        }
    }
}