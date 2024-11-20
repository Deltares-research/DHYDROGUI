using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

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

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Correction coefficient pos")]
        [Description("Correction coefficient flow direction(pos_cwcoef)")]
        public double CorrectionCoefficientPos
        {
            get { return RiverWeirFormula.CorrectionCoefficientPos; }
            set { RiverWeirFormula.CorrectionCoefficientPos = value; }
        }
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Correction coefficient neg")]
        [Description("Correction coefficient reverse direction(neg_cwcoef)")]
        public double CorrectionCoefficientNeg
        {
            get { return RiverWeirFormula.CorrectionCoefficientNeg; }
            set { RiverWeirFormula.CorrectionCoefficientNeg = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Submerge limit pos")]
        [Description("Submerge coefficient flow direction(pos_slimlimit)")]
        public double SubmergeLimitPos
        {
            get { return RiverWeirFormula.SubmergeLimitPos; }
            set { RiverWeirFormula.SubmergeLimitPos = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Submerge limit neg")]
        [Description("Submerge coefficient reverse direction(neg_slimlimit)")]
        public double SubmergeLimitNeg
        {
            get { return RiverWeirFormula.SubmergeLimitNeg; }
            set { RiverWeirFormula.SubmergeLimitNeg = value; }
        }
    }

}