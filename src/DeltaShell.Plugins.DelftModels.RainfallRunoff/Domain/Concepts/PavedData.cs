using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange = false)]
    public class PavedData : CatchmentModelData
    {
        /// <summary>
        /// The sewer pump capacity unit.
        /// </summary>
        public const PavedEnums.SewerPumpCapacityUnit PumpCapacityUnit = PavedEnums.SewerPumpCapacityUnit.m3_s;

        /// <summary>
        /// The storage unit.
        /// </summary>
        public const RainfallRunoffEnums.StorageUnit StorageUnit = RainfallRunoffEnums.StorageUnit.mm;

        /// <summary>
        /// The water use unit.
        /// </summary>
        public const PavedEnums.WaterUseUnit WaterUseUnit = PavedEnums.WaterUseUnit.l_day;

        public PavedData(Catchment catchment) : base(catchment)
        {
            CalculationArea = catchment.AreaSize;
            double defaultPerc = 100.0 / 24.0;
            VariableWaterUseFunction = new Function
            {
                Arguments = {new Variable<int>("Hour") {FixedSize = 24}},
                Components = {new Variable<double>("Percentage")}
            };
            for (var i = 0; i < 24; i++)
            {
                VariableWaterUseFunction[i] = defaultPerc;
            }

            VariableWaterUseFunction.Arguments[0].IsEditable = false;
            isSewerPumpCapacityFixed = true;
            SurfaceLevel = 1.5;
            SewerType = PavedEnums.SewerType.MixedSystem;
            IsSewerPumpCapacityFixed = true;
            BoundaryData = new RainfallRunoffBoundaryData();
            DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
        }

        protected PavedData()
            : base(null) {}

        public override object Clone()
        {
            var clone = (PavedData) base.Clone();
            clone.MixedSewerPumpVariableCapacitySeries = MixedSewerPumpVariableCapacitySeries != null
                                                             ? (TimeSeries) MixedSewerPumpVariableCapacitySeries.Clone()
                                                             : null;
            clone.DwfSewerPumpVariableCapacitySeries = DwfSewerPumpVariableCapacitySeries != null
                                                           ? (TimeSeries) DwfSewerPumpVariableCapacitySeries.Clone()
                                                           : null;
            clone.VariableWaterUseFunction = VariableWaterUseFunction != null
                                                 ? (Function) VariableWaterUseFunction.Clone()
                                                 : null;
            clone.BoundaryData = BoundaryData != null ? (RainfallRunoffBoundaryData) BoundaryData.Clone() : null;
            return clone;
        }
        
        public RainfallRunoffBoundaryData BoundaryData { get; set; }

        private void CreateSewerPumpCapacityFunctionsIfNeeded()
        {
            if (!IsSewerPumpCapacityFixed)
            {
                if (MixedSewerPumpVariableCapacitySeries == null)
                {
                    MixedSewerPumpVariableCapacitySeries = new TimeSeries {Name = "Mixed Sewer Pump Capacity"};
                    MixedSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>
                    {
                        Name = "Pump Capacity",
                        Unit = new Unit("m³/s", "m³/s")
                    });
                }

                if (DwfSewerPumpVariableCapacitySeries == null)
                {
                    DwfSewerPumpVariableCapacitySeries = new TimeSeries {Name = "Dwf Sewer Pump Capacity"};
                    DwfSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>
                    {
                        Name = "Pump Capacity",
                        Unit = new Unit("m³/s", "m³/s")
                    });
                }
            }
        }

        #region properties

        private bool isSewerPumpCapacityFixed;

        public double SurfaceLevel { get; set; } // m AD

        [Description("Spilling")]
        public PavedEnums.SpillingDefinition SpillingDefinition { get; set; }

        public double RunoffCoefficient { get; set; }

        [Description("Management")]
        public PavedEnums.SewerType SewerType { get; set; }

        public bool IsSewerPumpCapacityFixed
        {
            get => isSewerPumpCapacityFixed;
            set
            {
                isSewerPumpCapacityFixed = value;
                CreateSewerPumpCapacityFunctionsIfNeeded();
            }
        }

        /// <summary>
        /// The fixed capacity of a mixed/rainfall sewer pump (m³/s).
        /// </summary>
        public double CapacityMixedAndOrRainfall { get; set; }

        /// <summary>
        /// The fixed capacity of a dry weather flow sewer pump (m³/s).
        /// </summary>
        public double CapacityDryWeatherFlow { get; set; }

        public TimeSeries DwfSewerPumpVariableCapacitySeries { get; set; }
        public TimeSeries MixedSewerPumpVariableCapacitySeries { get; set; }

        /// <summary>
        /// The maximum street storage (mm) of the area (m²).
        /// </summary>
        [Description("Storage")]
        public double MaximumStreetStorage { get; set; }

        /// <summary>
        /// The initial street storage (mm) of the area (m²).
        /// </summary>
        public double InitialStreetStorage { get; set; }

        /// <summary>
        /// The maximum mixed/rainfall sewer storage (mm) of the area (m²).
        /// </summary>
        public double MaximumSewerMixedAndOrRainfallStorage { get; set; }

        /// <summary>
        /// The initial mixed/rainfall sewer storage (mm) of the area (m²).
        /// </summary>
        public double InitialSewerMixedAndOrRainfallStorage { get; set; }

        /// <summary>
        /// The maximum dry weather flow sewer storage (mm) of the area (m²).
        /// </summary>
        public double MaximumSewerDryWeatherFlowStorage { get; set; }

        /// <summary>
        /// The initial dry weather flow sewer storage (mm) of the area (m²).
        /// </summary>
        public double InitialSewerDryWeatherFlowStorage { get; set; }

        [Description("DryWeatherFlow")]
        public int NumberOfInhabitants { get; set; }

        public PavedEnums.DryWeatherFlowOptions DryWeatherFlowOptions { get; set; }

        /// <summary>
        /// The water use per capita (l/day).
        /// </summary>
        public double WaterUse { get; set; }

        public Function VariableWaterUseFunction { get; set; } // distribution per hour

        public PavedEnums.SewerPumpDischargeTarget MixedAndOrRainfallSewerPumpDischarge { get; set; }
        public PavedEnums.SewerPumpDischargeTarget DryWeatherFlowSewerPumpDischarge { get; set; }

        public IHydroObject MixedSewerTarget => GetTargetHydroObject(MixedAndOrRainfallSewerPumpDischarge);

        public IHydroObject DwfSewerTarget => GetTargetHydroObject(DryWeatherFlowSewerPumpDischarge);

        private IHydroObject GetTargetHydroObject(PavedEnums.SewerPumpDischargeTarget dischargeTarget)
        {
            IEnumerable<IHydroObject> targets = Catchment.Links.Select(l => l.Target);

            switch (dischargeTarget)
            {
                case PavedEnums.SewerPumpDischargeTarget.BoundaryNode:
                    return targets.FirstOrDefault(f => !(f is WasteWaterTreatmentPlant));
                case PavedEnums.SewerPumpDischargeTarget.WWTP:
                    return targets.OfType<WasteWaterTreatmentPlant>().FirstOrDefault();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}