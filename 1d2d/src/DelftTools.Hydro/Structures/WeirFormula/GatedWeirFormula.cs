using DelftTools.Functions;
using DelftTools.Hydro.Structures.SteerableProperties;
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
        public GatedWeirFormula() : this(false) { }

        public GatedWeirFormula(bool canBeTimeDependent)
        {
            GateOpening = 1.0;
            GateHeight = 0.0;
            ContractionCoefficient = 0.63;
            LateralContraction = 1.0;
            CanBeTimeDependent = canBeTimeDependent;
            UseVelocityHeight = true;

            LowerEdgeLevelProperty = ConstructLowerEdgeLevelSteerableProperty();
        }

        private SteerableProperty ConstructLowerEdgeLevelSteerableProperty()
        {
            const double defaultValue = 11.0;
            return CanBeTimeDependent 
                       ? new SteerableProperty(defaultValue, 
                                               "Gate opening", 
                                               "Gate opening", 
                                               "m AD")
                       : new SteerableProperty(defaultValue);
        }

        /// <summary>
        /// Indicates if time dependent parameters can be used.
        /// </summary>
        public virtual bool CanBeTimeDependent { get; protected set; }

        /// <summary>
        /// When true, use <see cref="LowerEdgeLevelTimeSeries"/>, else use <see cref="GateOpening"/>.
        /// </summary>
        public virtual bool UseLowerEdgeLevelTimeSeries
        {
            get => LowerEdgeLevelProperty.CurrentDriver == SteerablePropertyDriver.TimeSeries;
            set => LowerEdgeLevelProperty.CurrentDriver = 
                       value ? SteerablePropertyDriver.TimeSeries
                             : SteerablePropertyDriver.Constant;
        }

        /// <inheritdoc />
        public virtual double LowerEdgeLevel
        {
            get => LowerEdgeLevelProperty.Constant; 
            set => LowerEdgeLevelProperty.Constant = value;
        }

        /// <summary>
        /// Time dependent Lower edge level
        /// </summary>
        public virtual TimeSeries LowerEdgeLevelTimeSeries
        {
            get => LowerEdgeLevelProperty.TimeSeries; 
            protected set => LowerEdgeLevelProperty.TimeSeries = value;
        }

        /// <summary>
        /// The <see cref="SteerableProperty"/> describing the lower edge level.
        /// </summary>
        public virtual SteerableProperty LowerEdgeLevelProperty { get; set; }

        /// <inheritdoc />
        public virtual string Name => "Gated weir (Orifice)";

        /// <inheritdoc />
        public virtual bool IsRectangle => true;

        /// <inheritdoc />
        public virtual bool HasFlowDirection => true;

        /// <summary>
        /// Contraction coefficient μ
        /// </summary>
        public virtual double ContractionCoefficient { get; set; }

        /// <summary>
        /// Lateral contraction Cw
        /// </summary>
        public virtual double LateralContraction { get; set; }

        /// <inheritdoc />
        public virtual double GateOpening { get; set; }

        /// <inheritdoc />
        public virtual double GateHeight { get; set; }

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
        
        /// <summary>
        /// Determine whether a time series is being used for the lower edge level.
        /// </summary>
        /// <returns><c>true</c> when using a time series for the lower edge level; otherwise <c>false</c>.</returns>
        public virtual bool IsUsingTimeSeriesForLowerEdgeLevel() 
            => CanBeTimeDependent && UseLowerEdgeLevelTimeSeries;

        /// <inheritdoc />
        public virtual object Clone()
        {
            var lowerEdgeLevelProperty = new SteerableProperty(LowerEdgeLevelProperty);
            var gatedWeirFormula = new GatedWeirFormula(CanBeTimeDependent) 
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
                LowerEdgeLevelProperty = lowerEdgeLevelProperty
            };

            return gatedWeirFormula;
        }
    }
}