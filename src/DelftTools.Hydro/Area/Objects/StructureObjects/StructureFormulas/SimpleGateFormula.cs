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
        private bool useLowerEdgeLevelTimeSeries;
        private bool useHorizontalDoorOpeningWidthTimeSeries;

        public SimpleGateFormula() : this(false) {}

        public SimpleGateFormula(bool canBeTimeDependent)
        {
            DoorHeight = 0.0;

            HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric;

            HorizontalDoorOpeningWidth = 0.0;
            UseHorizontalDoorOpeningWidthTimeSeries = false;
            HorizontalDoorOpeningWidthTimeSeries = null;

            LowerEdgeLevel = 0.0;
            UseLowerEdgeLevelTimeSeries = false;
            LowerEdgeLevelTimeSeries = null;

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
            GateOpening { get; set; } // LowerEdgeLevel, this should be replaced with LowerEdgeLevel value, see LowerEdgeLevelTimeSeries documentation

        public virtual double DoorHeight { get; set; }

        /// <summary>
        /// The direction in which the door will open.
        /// Left and right are defined by the flow direction of the gate,
        /// indicated in the gui by a small arrow.
        /// </summary>
        public virtual GateOpeningDirection HorizontalDoorOpeningDirection { get; set; }

        public virtual double HorizontalDoorOpeningWidth { get; set; }

        public virtual bool UseHorizontalDoorOpeningWidthTimeSeries
        {
            get => useHorizontalDoorOpeningWidthTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for gate horizontal door opening width when time varying data is not allowed.");
                }

                useHorizontalDoorOpeningWidthTimeSeries = value;
            }
        }

        public virtual TimeSeries HorizontalDoorOpeningWidthTimeSeries { get; set; }

        public virtual double LowerEdgeLevel { get; set; }

        /// <summary>
        /// When true, use <see cref="LowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseLowerEdgeLevelTimeSeries
        {
            get => useLowerEdgeLevelTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for gate opening when time varying data is not allowed.");
                }

                useLowerEdgeLevelTimeSeries = value;
            }
        }

        /// <summary>
        /// Time dependent Lower edge level
        /// </summary>
        public virtual TimeSeries LowerEdgeLevelTimeSeries { get; set; }

        public virtual object Clone()
        {
            var gatedWeirFormula = new SimpleGateFormula(CanBeTimedependent)
            {
                ContractionCoefficient = ContractionCoefficient,
                GateOpening = GateOpening,
                LateralContraction = LateralContraction,
                MaxFlowNeg = MaxFlowNeg,
                MaxFlowPos = MaxFlowPos,
                UseMaxFlowNeg = UseMaxFlowNeg,
                UseMaxFlowPos = UseMaxFlowPos,
                DoorHeight = DoorHeight,
                HorizontalDoorOpeningDirection = HorizontalDoorOpeningDirection,
                HorizontalDoorOpeningWidth = HorizontalDoorOpeningWidth,
                UseHorizontalDoorOpeningWidthTimeSeries = UseHorizontalDoorOpeningWidthTimeSeries,
                LowerEdgeLevel = LowerEdgeLevel,
                UseLowerEdgeLevelTimeSeries = UseLowerEdgeLevelTimeSeries
            };

            if (gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
            {
                gatedWeirFormula.LowerEdgeLevelTimeSeries = (TimeSeries) LowerEdgeLevelTimeSeries.Clone(true);
            }

            if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries =
                    (TimeSeries) HorizontalDoorOpeningWidthTimeSeries.Clone(true);
            }

            return gatedWeirFormula;
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                // For Performance: initialize lazy
                if (LowerEdgeLevelTimeSeries == null)
                {
                    LowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.GateLowerEdgeLevel, GuiParameterNames.GateLowerEdgeLevel, "m AD");
                }

                if (HorizontalDoorOpeningWidthTimeSeries == null)
                {
                    HorizontalDoorOpeningWidthTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries(
                        GuiParameterNames.HorizontalOpeningWidth, GuiParameterNames.HorizontalOpeningWidth, "m AD");
                }
            }
            else
            {
                UseLowerEdgeLevelTimeSeries = false;
                LowerEdgeLevelTimeSeries = null;

                UseHorizontalDoorOpeningWidthTimeSeries = false;
                HorizontalDoorOpeningWidthTimeSeries = null;
            }
        }
    }
}