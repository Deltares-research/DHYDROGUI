using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class PierWeirFormulaProperties : WeirFormulaProperties
    {
        public PierWeirFormulaProperties(PierWeirFormula pierWeirFormula, IWeir weir): base(pierWeirFormula, weir)
        {
        }

        private PierWeirFormula PierWeirFormula
        {
            get { return (PierWeirFormula)weirFormula; }
        }

        [DisplayName("Number of piers")]
        [Description("Number Of piers.")]
        public int NumberOfPiers
        {
            get { return PierWeirFormula.NumberOfPiers; }
            set { PierWeirFormula.NumberOfPiers = value; }
        }

        [DisplayName("Upstream face pos")]
        [Description("Upstream face flow direction P.")]
        public double UpstreamFacePos
        {
            get { return PierWeirFormula.UpstreamFacePos; }
            set { PierWeirFormula.UpstreamFacePos = value; }
        }

        [DisplayName("Upstream face neg")]
        [Description("Upstream face reverse direction P.")]
        public double UpstreamFaceNeg
        {
            get { return PierWeirFormula.UpstreamFaceNeg; }
            set { PierWeirFormula.UpstreamFaceNeg = value; }
        }

        [DisplayName("Design head pos")]
        [Description("Design head of weir flow H0.")]
        public double DesignHeadPos
        {
            get { return PierWeirFormula.DesignHeadPos; }
            set { PierWeirFormula.DesignHeadPos = value; }
        }

        [DisplayName("Design head neg")]
        [Description("Design head of weir reverse H0.")]
        public double DesignHeadNeg
        {
            get { return PierWeirFormula.DesignHeadNeg; }
            set { PierWeirFormula.DesignHeadNeg = value; }
        }

        [DisplayName("Pier contraction pos")]
        [Description("Pier contraction coefficient Kp flow direction.")]
        public double PierContractionPos
        {
            get { return PierWeirFormula.PierContractionPos; }
            set { PierWeirFormula.PierContractionPos = value; }
        }

        [DisplayName("Pier contraction neg")]
        [Description("Pier contraction coefficient Kp reverse direction.")]
        public double PierContractionNeg
        {
            get { return PierWeirFormula.PierContractionNeg; }
            set { PierWeirFormula.PierContractionNeg = value; }
        }

        [DisplayName("Abutment contraction pos")]
        [Description("Abutment contraction coefficient flow direction Ka.")]
        public double AbutmentContractionPos
        {
            get { return PierWeirFormula.AbutmentContractionPos; }
            set { PierWeirFormula.AbutmentContractionPos = value; }
        }

        [DisplayName("Abutment contraction neg")]
        [Description("Abutment contraction coefficient reverse direction Ka.")]
        public double AbutmentContractionNeg
        {
            get { return PierWeirFormula.AbutmentContractionNeg; }
            set { PierWeirFormula.AbutmentContractionNeg = value; }
        }
    }
}