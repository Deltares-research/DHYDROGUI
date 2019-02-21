using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// BoundaryCondition describes a valid state of a single BoundaryCondition.
    /// It contains the Water, salt, and temperature components if defined.
    /// If not defined these values are null.
    /// </summary>
    public class BoundaryCondition
    {
        /// <summary>
        /// Construct a new empty BoundaryCondition with the given name.
        /// </summary>
        public BoundaryCondition(string name)
        {
            this.Name = name;
            WaterComponent = null;
            SaltComponent = null;
            TemperatureComponent = null;
        }

        /// <summary>The name of this BoundaryCondition.</summary>
        public readonly string Name;
        /// <summary>Gets or sets the water component of this BoundaryCondition.</summary>
        /// <value>The BoundaryComponent which describes the water component of this BoundaryCondition.</value>
        public BoundaryConditionWater WaterComponent { get; set; }
        /// <summary>Get or set the salt component of this BoundaryCondition.</summary>
        /// <value>The BoundaryComponent which describes the salt component of this BoundaryCondition.</value>
        public BoundaryConditionSalt SaltComponent { get; set; }
        /// <summary>Get or set the temperature component of this BoundaryCondition.</summary>
        /// <value>The BoundaryComponent which describes the temperature component of this BoundaryCondition.</value>
        public BoundaryConditionTemperature TemperatureComponent { get; set; }
    }

    /// <summary>
    /// BoundaryConditionWater is a read only class containing the relevant
    /// information of either the WaterLevel or Flow of a single node.
    /// </summary>
    public class BoundaryConditionWater : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant BoundaryConditionWater.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionWater. </param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionWater.</param>
        /// <pre-condition>type == FlowConstant || type == WaterLevelConstant. </pre-condition>
        public BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                      double constantBoundaryValue) : this(type, null, null, false, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionWater.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionWater. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionWater.</param>
        /// <param name="extrapolationType"> The type of extrapolation for this BoundaryConditionWater.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionWater.</param>
        /// <pre-condition> type != FlowConstant && type != WaterLevelConstant. </pre-condition>
        public BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                      Flow1DInterpolationType interpolationType,
                                      Flow1DExtrapolationType extrapolationType,
                                      bool isPeriodic,
                                      IFunction timeDependentBoundaryValue) : this(type, interpolationType, extrapolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                       Flow1DInterpolationType? interpolationType,
                                       Flow1DExtrapolationType? extrapolationType,
                                       bool isPeriodic,
                                       double constantBoundaryValue,
                                       IFunction timeDependentBoundaryValue) : base(interpolationType, extrapolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of boundary of this BoundaryConditionWater. </summary>
        public readonly WaterFlowModel1DBoundaryNodeDataType BoundaryType;
    }

    /// <summary>
    /// BoundaryConditionSalt is a read only class containing the relevant
    /// information of the salinity of a BoundaryNode.
    /// </summary>
    public class BoundaryConditionSalt : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant BoundaryConditionSalt.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionSalt. </param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionSalt.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public BoundaryConditionSalt(SaltBoundaryConditionType type,
                                     double constantBoundaryValue) : this(type, null, null, false, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionSalt.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionSalt. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionSalt.</param>
        /// <param name="extrapolationType"> The type of extrapolation for this BoundaryConditionSalt.</param
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionSalt.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public BoundaryConditionSalt(SaltBoundaryConditionType type,
                                     Flow1DInterpolationType interpolationType,
                                     Flow1DExtrapolationType extrapolationType,
                                     bool isPeriodic,
                                     IFunction timeDependentBoundaryValue) : this(type, interpolationType, extrapolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionSalt(SaltBoundaryConditionType type,
                                      Flow1DInterpolationType? interpolationType,
                                      Flow1DExtrapolationType? extrapolationType,
                                      bool isPeriodic,
                                      double constantBoundaryValue,
                                      IFunction timeDependentBoundaryValue) : base(interpolationType, extrapolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of salt boundary of this BoundaryConditionSalt. </summary>
        public readonly SaltBoundaryConditionType BoundaryType;
    }

    /// <summary>
    /// BoundaryConditionTemperature is a read only class containing the relevant
    /// information of the temperature of a BoundaryNode.
    /// </summary>
    public class BoundaryConditionTemperature : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant BoundaryConditionTemperature.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionTemperature. </param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionTemperature.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                            double constantBoundaryValue) : this(type, null, null, false, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionTemperature.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionTemperature. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionTemperature.</param>
        /// <param name="extrapolationType"> The type of extrapolation for this BoundaryConditionTemperature.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionTemperature.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                            Flow1DInterpolationType interpolationType,
                                            Flow1DExtrapolationType extrapolationType,
                                            bool isPeriodic,
                                            IFunction timeDependentBoundaryValue) : this(type, interpolationType, extrapolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                             Flow1DInterpolationType? interpolationType,
                                             Flow1DExtrapolationType? extrapolationType,
                                             bool isPeriodic,
                                             double constantBoundaryValue,
                                             IFunction timeDependentBoundaryValue) : base(interpolationType, extrapolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of temperature boundary of this BoundaryConditionSalt. </summary>
        public readonly TemperatureBoundaryConditionType BoundaryType;
    }
}
