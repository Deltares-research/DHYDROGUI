using System;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    /// <summary>
    /// Sharp crested gated weir (Orifice)
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class SimpleGateFormula : Unique<long>, IGatedStructureFormula
    {
        private bool canBeTimedependent;
        private bool useGateLowerEdgeLevelTimeSeries;
        private bool useHorizontalGateOpeningWidthTimeSeries;

        public SimpleGateFormula() : this(false) {}

        public SimpleGateFormula(bool canBeTimeDependent)
        {
            GateHeight = 0.0;

            GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric;

            HorizontalGateOpeningWidth = 0.0;
            UseHorizontalGateOpeningWidthTimeSeries = false;
            HorizontalGateOpeningWidthTimeSeries = null;

            GateLowerEdgeLevel = 0.0;
            UseGateLowerEdgeLevelTimeSeries = false;
            GateLowerEdgeLevelTimeSeries = null;

            GateOpening = 0.0;
            ContractionCoefficient = 0.63;
            LateralContraction = 1.0;
            CanBeTimedependent = canBeTimeDependent;
        }

        /// <summary>
        /// Indicates if time dependent parameters can be used.
        /// </summary>
        public virtual bool CanBeTimedependent
        {
            get => canBeTimedependent;
            protected set
            {
                canBeTimedependent = value;

                OnCanBeTimeDependentSet();
            }
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

        public virtual string Name => "Simple Gate";

        /// <summary>
        /// Gate opening (openlevel)
        /// </summary>
        public virtual double
            GateOpening { get; set; } // GateLowerEdgeLevel, this should be replaced with GateLowerEdgeLevel value, see GateLowerEdgeLevelTimeSeries documentation

        public virtual double GateHeight { get; set; }

        /// <summary>
        /// The direction in which the gate will open.
        /// Left and right are defined by the flow direction of the gate,
        /// indicated in the gui by a small arrow.
        /// </summary>
        public virtual GateOpeningDirection GateOpeningHorizontalDirection { get; set; }

        public virtual double HorizontalGateOpeningWidth { get; set; }

        public virtual bool UseHorizontalGateOpeningWidthTimeSeries
        {
            get => useHorizontalGateOpeningWidthTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for gate horizontal gate opening width when time varying data is not allowed.");
                }

                useHorizontalGateOpeningWidthTimeSeries = value;
            }
        }

        public virtual TimeSeries HorizontalGateOpeningWidthTimeSeries { get; set; }

        public virtual double GateLowerEdgeLevel { get; set; }

        /// <summary>
        /// When true, use <see cref="GateLowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseGateLowerEdgeLevelTimeSeries
        {
            get => useGateLowerEdgeLevelTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for gate opening when time varying data is not allowed.");
                }

                useGateLowerEdgeLevelTimeSeries = value;
            }
        }

        /// <summary>
        /// Time dependent gate lower edge level
        /// </summary>
        public virtual TimeSeries GateLowerEdgeLevelTimeSeries { get; set; }

        public virtual object Clone()
        {
            var gateFormula = new SimpleGateFormula(CanBeTimedependent)
            {
                ContractionCoefficient = ContractionCoefficient,
                GateOpening = GateOpening,
                LateralContraction = LateralContraction,
                MaxFlowNeg = MaxFlowNeg,
                MaxFlowPos = MaxFlowPos,
                UseMaxFlowNeg = UseMaxFlowNeg,
                UseMaxFlowPos = UseMaxFlowPos,
                GateHeight = GateHeight,
                GateOpeningHorizontalDirection = GateOpeningHorizontalDirection,
                HorizontalGateOpeningWidth = HorizontalGateOpeningWidth,
                UseHorizontalGateOpeningWidthTimeSeries = UseHorizontalGateOpeningWidthTimeSeries,
                GateLowerEdgeLevel = GateLowerEdgeLevel,
                UseGateLowerEdgeLevelTimeSeries = UseGateLowerEdgeLevelTimeSeries
            };

            if (gateFormula.UseGateLowerEdgeLevelTimeSeries)
            {
                gateFormula.GateLowerEdgeLevelTimeSeries = (TimeSeries) GateLowerEdgeLevelTimeSeries.Clone(true);
            }

            if (gateFormula.UseHorizontalGateOpeningWidthTimeSeries)
            {
                gateFormula.HorizontalGateOpeningWidthTimeSeries =
                    (TimeSeries) HorizontalGateOpeningWidthTimeSeries.Clone(true);
            }

            return gateFormula;
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                // For Performance: initialize lazy
                if (GateLowerEdgeLevelTimeSeries == null)
                {
                    GateLowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.GateLowerEdgeLevel, GuiParameterNames.GateLowerEdgeLevel, "m AD");
                }

                if (HorizontalGateOpeningWidthTimeSeries == null)
                {
                    HorizontalGateOpeningWidthTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.HorizontalGateOpeningWidth, GuiParameterNames.HorizontalGateOpeningWidth, "m AD");
                }
            }
            else
            {
                UseGateLowerEdgeLevelTimeSeries = false;
                GateLowerEdgeLevelTimeSeries = null;

                UseHorizontalGateOpeningWidthTimeSeries = false;
                HorizontalGateOpeningWidthTimeSeries = null;
            }
        }
    }
}