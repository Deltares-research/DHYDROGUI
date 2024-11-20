using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    internal class GatedWeirFormulaProperties : WeirFormulaProperties
    {
        private const string timeSeriesString = "Time series";
        
        public GatedWeirFormulaProperties(GatedWeirFormula gatedWeirFormula, IWeir weir) 
            : base(gatedWeirFormula, weir)
        {
        }

        private GatedWeirFormula GatedWeirFormula
        {
            get { return (GatedWeirFormula)weirFormula; }
        }

        [DisplayName("Contraction coefficient")]
        [Description("Contraction coefficient")]
        public double ContractionCoefficient
        {
            get { return GatedWeirFormula.ContractionCoefficient; }
            set { GatedWeirFormula.ContractionCoefficient = value; }
        }

        [DisplayName("Lateral contraction")]
        [Description("Lateral contraction Cw")]
        public double LateralContraction
        {
            get { return GatedWeirFormula.LateralContraction; }
            set { GatedWeirFormula.LateralContraction = value; }
        }

        [DisplayName("Gate Opening")]
        [Description("Gate opening (open level)")]
        [ReadOnly(true)]
        public string GateOpening
        {
            get
            {
                if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                {
                    return "Time series";
                }
                return (GatedWeirFormula.LowerEdgeLevel - weir.CrestLevel).ToString("0.00", CultureInfo.CurrentCulture);
            }
        }
        
        [DisplayName("Lower edge level")]
        [Description("Gate lower edge level")]
        [DynamicReadOnly]
        public string LowerEdgeLevel
        {
            get
            {
                if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                {
                    return timeSeriesString;
                }
                return GatedWeirFormula.LowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                {
                    throw new InvalidOperationException("Cannot set value using time dependent gate opening.");
                }
                GatedWeirFormula.LowerEdgeLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DisplayName("Max Flow Pos")]
        [Description("Limitation flow direction")]
        [DynamicReadOnly]
        public double MaxFlowPos
        {
            get { return GatedWeirFormula.MaxFlowPos; }
            set { GatedWeirFormula.MaxFlowPos = value; }
        }

        [DisplayName("Max Flow Neg")]
        [Description("Limitation reverse direction")]
        [DynamicReadOnly]
        public double MaxFlowNeg
        {
            get { return GatedWeirFormula.MaxFlowNeg; }
            set { GatedWeirFormula.MaxFlowNeg = value; }
        }

        [DisplayName("Use Max Flow Pos")]
        [Description("Use max flow limitation flow direction")]
        [DynamicReadOnly]
        public bool UseMaxFlowPos
        {
            get { return GatedWeirFormula.UseMaxFlowPos; }
            set { GatedWeirFormula.UseMaxFlowPos = value; }
        }

        [DisplayName("Use Max Flow Neg")]
        [Description("Use max flow limitation reverse direction")]
        [DynamicReadOnly]
        public bool UseMaxFlowNeg
        {
            get { return GatedWeirFormula.UseMaxFlowNeg; }
            set { GatedWeirFormula.UseMaxFlowNeg = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(LowerEdgeLevel))
            {
                return GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel();
            }

            if (propertyName == nameof(MaxFlowNeg) || propertyName == nameof(UseMaxFlowNeg))
            {
                return !weir.AllowNegativeFlow;
            }

            if (propertyName == nameof(MaxFlowPos) || propertyName == nameof(UseMaxFlowPos))
            {
                return !weir.AllowPositiveFlow;
            }

            return false;
        }
    }
}