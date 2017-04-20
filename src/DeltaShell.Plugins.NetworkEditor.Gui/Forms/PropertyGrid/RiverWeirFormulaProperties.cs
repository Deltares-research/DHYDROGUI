using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class RiverWeirFormulaProperties : WeirFormulaProperties
    {
        public RiverWeirFormulaProperties(RiverWeirFormula riverWeirFormula, IWeir weir) : base(riverWeirFormula, weir)
        {
        }

        private RiverWeirFormula RiverWeirFormula
        {
            get { return (RiverWeirFormula)weirFormula; }
        }

        [Category("General")]
        [DisplayName("Correction Coefficient Pos")]
        [Description("Correction coefficient flow direction(pos_cwcoef)")]
        public double CorrectionCoefficientPos
        {
            get { return RiverWeirFormula.CorrectionCoefficientPos; }
            set { RiverWeirFormula.CorrectionCoefficientPos = value; }
        }
        
        [Category("General")]
        [DisplayName("Correction Coefficient Neg")]
        [Description("Correction coefficient reverse direction(neg_cwcoef)")]
        public double CorrectionCoefficientNeg
        {
            get { return RiverWeirFormula.CorrectionCoefficientNeg; }
            set { RiverWeirFormula.CorrectionCoefficientNeg = value; }
        }

        [Category("General")]
        [DisplayName("Submerge Limit Pos")]
        [Description("Submerge coefficient flow direction(pos_slimlimit)")]
        public double SubmergeLimitPos
        {
            get { return RiverWeirFormula.SubmergeLimitPos; }
            set { RiverWeirFormula.SubmergeLimitPos = value; }
        }

        [Category("General")]
        [DisplayName("Submerge Limit Neg")]
        [Description("Submerge coefficient reverse direction(neg_slimlimit)")]
        public double SubmergeLimitNeg
        {
            get { return RiverWeirFormula.SubmergeLimitNeg; }
            set { RiverWeirFormula.SubmergeLimitNeg = value; }
        }
    }

}