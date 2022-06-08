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
            CanBeTimedependent = canBeTimeDependent;
            UseVelocityHeight = true;

            LowerEdgeLevelProperty = ConstructLowerEdgeLevelSteerableProperty();
        }

        private SteerableProperty ConstructLowerEdgeLevelSteerableProperty()
        {
            const double defaultValue = 11.0;
            return CanBeTimedependent 
                       ? new SteerableProperty(defaultValue, 
                                               "Gate opening", 
                                               "Gate opening", 
                                               "m AD")
                       : new SteerableProperty(defaultValue);
        }

        /// <summary>
        /// Indicates if time dependent parameters can be used.
        /// </summary>
        public virtual bool CanBeTimedependent { get; protected set; }

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

        /// <summary>
        /// LowerEdgeLevel
        /// </summary>
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

        public virtual string Name => "Gated weir (Orifice)";

        public virtual bool IsRectangle => true;

        public virtual bool HasFlowDirection => true;

        /// <summary>
        /// Contraction coefficient μ
        /// </summary>
        public virtual double ContractionCoefficient { get; set; }

        /// <summary>
        /// Lateral contraction Cw
        /// </summary>
        public virtual double LateralContraction { get; set; }

        /// <summary>
        /// Gate opening (openlevel)
        /// </summary>
        public virtual double GateOpening { get; set; }

        /// <summary>
        /// GateHeight
        /// </summary>
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
            };

            gatedWeirFormula.LowerEdgeLevelProperty = new SteerableProperty(LowerEdgeLevelProperty);
            return gatedWeirFormula;
        }
    }
}