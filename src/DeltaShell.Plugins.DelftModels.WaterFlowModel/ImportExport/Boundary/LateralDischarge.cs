using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    public class LateralDischarge
    {
        /// <summary>
        /// Construct a new empty LateralDischarge with the given name.
        /// </summary>
        public LateralDischarge(string name)
        {
            this.Name = name;
            WaterComponent = null;
            SaltComponent = null;
            TemperatureComponent = null;
        }

        /// <summary> Name of the boundary location (node id). </summary>
        public readonly string Name;
        public LateralDischargeWater WaterComponent { get; set; }
        public LateralDischargeSalt SaltComponent { get; set; }
        public LateralDischargeTemperature TemperatureComponent { get; set; }
    }

    /// <summary>
    /// LateralDischargeWater is a read only class containing the relevant
    /// lateral discharge information of either the WaterLevel or Flow of a single node.
    /// </summary>
    public class LateralDischargeWater : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant LateralDischargeWater.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeWater. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeWater.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this LateralDischargeWater.</param>
        /// <pre-condition>type == FlowConstant || type == WaterLevelConstant. </pre-condition>
        public LateralDischargeWater(WaterFlowModel1DLateralDataType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent LateralDischargeWater.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeWater. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeWater.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this LateralDischargeWater.</param>
        /// <pre-condition> type != FlowConstant && type != WaterLevelConstant. </pre-condition>
        public LateralDischargeWater(WaterFlowModel1DLateralDataType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private LateralDischargeWater(WaterFlowModel1DLateralDataType type,
                                      InterpolationType interpolationType,
                                      bool isPeriodic,
                                      double constantBoundaryValue,
                                      IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of boundary of this LateralDischargeWater. </summary>
        public readonly WaterFlowModel1DLateralDataType BoundaryType;
    }

    /// <summary>
    /// LateralDischargeSalt is a read only class containing the relevant
    /// lateral discharge information of the salinity of a BoundaryNode.
    /// </summary>
    public class LateralDischargeSalt : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant LateralDischargeSalt.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeSalt. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeSalt.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this LateralDischargeSalt.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public LateralDischargeSalt(SaltLateralDischargeType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent LateralDischargeSalt.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeSalt. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeSalt.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this LateralDischargeSalt.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public LateralDischargeSalt(SaltLateralDischargeType type,
                                     InterpolationType interpolationType,
                                     bool isPeriodic,
                                     IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private LateralDischargeSalt(SaltLateralDischargeType type,
                                      InterpolationType interpolationType,
                                      bool isPeriodic,
                                      double constantBoundaryValue,
            IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of salt boundary of this LateralDischargeSalt. </summary>
        public readonly SaltLateralDischargeType BoundaryType;
    }

    /// <summary>
    /// LateralDischargeTemperature is a read only class containing the relevant
    /// lateral discharge information of the temperature of a BoundaryNode.
    /// </summary>
    public class LateralDischargeTemperature : BoundaryComponent
    {
        /// <summary>
        /// Construct a new Constant LateralDischargeTemperature.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeTemperature. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeTemperature.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="constantBoundaryValue"> The constant value of this LateralDischargeTemperature.</param>
        /// <pre-condition>type == Constant. </pre-condition>
        public LateralDischargeTemperature(TemperatureLateralDischargeType type,
                                            InterpolationType interpolationType,
                                            bool isPeriodic,
                                            double constantBoundaryValue) : this(type, interpolationType, isPeriodic, constantBoundaryValue, null) { }
        /// <summary>
        /// Construct a new TimeDependent LateralDischargeTemperature.
        /// </summary>
        /// <param name="type"> The type of this LateralDischargeTemperature. </param>
        /// <param name="interpolationType"> The type of interpolation for this LateralDischargeTemperature.</param>
        /// <param name="isPeriodic">Whether the timeseries repeats or not.</param>
        /// <param name="timeDependentBoundaryValue">The TimeDependentFunction of this LateralDischargeTemperature.</param>
        /// <pre-condition> type == TimeDependent. </pre-condition>
        public LateralDischargeTemperature(TemperatureLateralDischargeType type,
                                            InterpolationType interpolationType,
                                            bool isPeriodic,
                                            IFunction timeDependentBoundaryValue) : this(type, interpolationType, isPeriodic, 0.0, timeDependentBoundaryValue) { }

        private LateralDischargeTemperature(TemperatureLateralDischargeType type,
                                             InterpolationType interpolationType,
                                             bool isPeriodic,
                                             double constantBoundaryValue,
                                             IFunction timeDependentBoundaryValue) : base(interpolationType, isPeriodic, constantBoundaryValue, timeDependentBoundaryValue)
        {
            this.BoundaryType = type;
        }

        /// <summary> The Type of temperature boundary of this LateralDischargeSalt. </summary>
        public readonly TemperatureLateralDischargeType BoundaryType;
    }
}
