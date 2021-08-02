using System;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Sharp crested gated weir (Orifice)
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class GatedWeirFormula : Unique<long>, IGatedWeirFormula
    {
        private bool canBeTimedependent;
        private bool useLowerEdgeLevelTimeSeries;
        public GatedWeirFormula() : this(false) { }

        public GatedWeirFormula(bool canBeTimeDependent)
        {
            GateOpening = 1.0;
            LowerEdgeLevel = 11.0;
            GateHeight = 0.0;
            ContractionCoefficient = 0.63;
            LateralContraction = 1.0;
            CanBeTimedependent = canBeTimeDependent;
            UseVelocityHeight = true;
        }

        /// <summary>
        /// Indicates if time dependent parameters can be used.
        /// </summary>
        public virtual bool CanBeTimedependent
        {
            get { return canBeTimedependent; }
            protected set
            {
                canBeTimedependent = value;

                OnCanBeTimeDependentSet();
            }
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                // For Performance: initialize lazy
                if (LowerEdgeLevelTimeSeries == null)
                    LowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Gate opening", "Gate opening", "m AD");
            }
            else
            {
                UseLowerEdgeLevelTimeSeries = false;
                LowerEdgeLevelTimeSeries = null;
            }
        }

        public virtual string Name
        {
            get { return "Gated weir (Orifice)"; }
        }

        public virtual bool IsRectangle
        {
            get { return true; }
        }

        public virtual bool HasFlowDirection
        {
            get { return true; }
        }
        
        /// <summary>
        /// Contraction coefficient μ
        /// </summary>
        public virtual double ContractionCoefficient { get; set; }

        /// <summary>
        /// Lateral contraction Cw
        /// </summary>
        public virtual double LateralContraction { get; set; }

        /// <summary>
        /// When true, use <see cref="LowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseLowerEdgeLevelTimeSeries
        {
            get { return useLowerEdgeLevelTimeSeries; }
            set
            {
                if (!canBeTimedependent && value)
                    throw new InvalidOperationException("Cannot use time series for gate opening when time varying data is not allowed.");
                useLowerEdgeLevelTimeSeries = value;
            }
        }

        /// <summary>
        /// Gate opening (openlevel)
        /// </summary>
        public virtual double GateOpening { get; set; }

        /// <summary>
        /// GateHeight
        /// </summary>
        public virtual double GateHeight { get; set; }

        /// <summary>
        /// LowerEdgeLevel
        /// </summary>
        public virtual double LowerEdgeLevel { get; set; }

        /// <summary>
        /// Time dependent Lower edge level
        /// </summary>
        public virtual TimeSeries LowerEdgeLevelTimeSeries { get; protected set; }

        /// <summary>
        /// Limitation flow direction (limitflowpos)
        /// </summary>
        public virtual double MaxFlowPos { get; set; }

        /// <summary>
        /// limitation reverse direction (limitflowneg)
        /// </summary>
        public virtual double MaxFlowNeg { get; set; }

        /// <summary>
        /// Use max flow limitation flow direction (uselimitflowpos)
        /// </summary>
        public virtual bool UseMaxFlowPos { get; set; }

        /// <summary>
        /// Use max flow limitation reverse direction (uselimitflowneg)
        /// </summary>
        public virtual bool UseMaxFlowNeg { get; set; }

        public virtual bool UseVelocityHeight { get; set; }

        public virtual object Clone()
        {
            var gatedWeirFormula = new GatedWeirFormula(CanBeTimedependent)
                {
                    ContractionCoefficient = ContractionCoefficient, 
                    GateOpening = GateOpening, 
                    LateralContraction = LateralContraction, 
                    MaxFlowNeg = MaxFlowNeg, 
                    MaxFlowPos = MaxFlowPos, 
                    UseMaxFlowNeg = UseMaxFlowNeg, 
                    UseMaxFlowPos = UseMaxFlowPos,
                    UseVelocityHeight = UseVelocityHeight,
                    GateHeight = GateHeight,
                    LowerEdgeLevel = LowerEdgeLevel
            };
            if (gatedWeirFormula.CanBeTimedependent)
            {
                gatedWeirFormula.UseLowerEdgeLevelTimeSeries = UseLowerEdgeLevelTimeSeries;
                gatedWeirFormula.LowerEdgeLevelTimeSeries = (TimeSeries)LowerEdgeLevelTimeSeries.Clone(true);
            }
            return gatedWeirFormula;
        }
    }
}