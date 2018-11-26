using DelftTools.Functions;
using DelftTools.Functions.Generic;
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

        public readonly string Name;
        public BoundaryConditionWater WaterComponent { get; set; }
        public BoundaryConditionSalt SaltComponent { get; set; }
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
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionWater.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionWater.</param>
        /// <pre-condition>type == FlowConstant || type == WaterLevelConstant. </pre-condition>
        public BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                      InterpolationType interpolationType,
                                      bool isPeriodic,
                                      double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionWater.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionWater. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionWater.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionWater.</param>
        /// <pre-condition> type != FlowConstant && type != WaterLevelConstant. </pre-condition>
        public BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                      InterpolationType interpolationType,
                                      bool isPeriodic,
                                      IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType type,
                                       InterpolationType interpolationType,
                                       bool isPeriodic,
                                       double constantBoundaryValue,
                                       IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
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
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionSalt.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionSalt.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public BoundaryConditionSalt(SaltBoundaryConditionType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionSalt.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionSalt. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionSalt.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionSalt.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public BoundaryConditionSalt(SaltBoundaryConditionType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionSalt(SaltBoundaryConditionType type,
                                      InterpolationType interpolationType,
                                      bool isPeriodic,
                                      double constantBoundaryValue,
            IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
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
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionTemperature.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this BoundaryConditionTemperature.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                            InterpolationType interpolationType,
                                            bool isPeriodic,
                                            double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent BoundaryConditionTemperature.
        /// </summary>
        /// <param name="type"> The type of this BoundaryConditionTemperature. </param>
        /// <param name="interpolationType"> The type of interpolation for this BoundaryConditionTemperature.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this BoundaryConditionTemperature.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                            InterpolationType interpolationType,
                                            bool isPeriodic,
                                            IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private BoundaryConditionTemperature(TemperatureBoundaryConditionType type,
                                             InterpolationType interpolationType,
                                             bool isPeriodic,
                                             double constantBoundaryValue,
                                             IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of temperature boundary of this BoundaryConditionSalt. </summary>
        public readonly TemperatureBoundaryConditionType BoundaryType;
    }


}
