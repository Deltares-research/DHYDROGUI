using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class PavedData : CatchmentModelData
    {
        /// <summary>
        /// The default sewer pump capacity unit. 
        /// </summary>
        public const PavedEnums.SewerPumpCapacityUnit PumpCapacityUnit = PavedEnums.SewerPumpCapacityUnit.m3_s;

        /// <summary>
        /// The default storage unit.
        /// </summary>
        public const RainfallRunoffEnums.StorageUnit StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
        
        protected PavedData()
            : base(null)
        {
        }

        public PavedData(Catchment catchment) : base (catchment)
        {
            CalculationArea = catchment.AreaSize;
            double defaultPerc = 100.0/24.0;
            VariableWaterUseFunction = new Function
                {
                    Arguments = {new Variable<int>("Hour") {FixedSize = 24}},
                    Components = {new Variable<double>("Percentage")}
                };
            for (int i = 0; i < 24; i++)
            {
                VariableWaterUseFunction[i] = defaultPerc;
            }
            
            WaterUseUnit = PavedEnums.WaterUseUnit.l_day;

            VariableWaterUseFunction.Arguments[0].IsEditable = false;
            isSewerPumpCapacityFixed = true;
            SurfaceLevel = 1.5;
            SewerType = PavedEnums.SewerType.MixedSystem;
            IsSewerPumpCapacityFixed = true;
            DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
        }
        
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
                            Unit = new Unit("m³/min", "m³/min")
                        });
                }
                if (DwfSewerPumpVariableCapacitySeries == null)
                {
                    DwfSewerPumpVariableCapacitySeries = new TimeSeries {Name = "Dwf Sewer Pump Capacity"};
                    DwfSewerPumpVariableCapacitySeries.Components.Add(new Variable<double>
                        {
                            Name = "Pump Capacity",
                            Unit = new Unit("m³/min", "m³/min")
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
            get { return isSewerPumpCapacityFixed; }
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
        public double WaterUse { get; set; } // l/day
        public PavedEnums.WaterUseUnit WaterUseUnit { get; set; }
        public Function VariableWaterUseFunction { get; set; } // distribution per hour

        public PavedEnums.SewerPumpDischargeTarget MixedAndOrRainfallSewerPumpDischarge { get; set; }
        public PavedEnums.SewerPumpDischargeTarget DryWeatherFlowSewerPumpDischarge { get; set; }

        public IHydroObject MixedSewerTarget
        {
            get
            {
                switch(MixedAndOrRainfallSewerPumpDischarge)
                {
                    case PavedEnums.SewerPumpDischargeTarget.BoundaryNode:
                        return Catchment.Links.Select(l => l.Target).FirstOrDefault(f => !(f is WasteWaterTreatmentPlant));
                    case PavedEnums.SewerPumpDischargeTarget.WWTP:
                        return Catchment.Links.Select(l => l.Target).OfType<WasteWaterTreatmentPlant>().FirstOrDefault();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IHydroObject DwfSewerTarget
        {
            get
            {
                switch (DryWeatherFlowSewerPumpDischarge)
                {
                    case PavedEnums.SewerPumpDischargeTarget.BoundaryNode:
                        return Catchment.Links.Select(l => l.Target).FirstOrDefault(f => !(f is WasteWaterTreatmentPlant));
                    case PavedEnums.SewerPumpDischargeTarget.WWTP:
                        return Catchment.Links.Select(l => l.Target).OfType<WasteWaterTreatmentPlant>().FirstOrDefault();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        public override object Clone()
        {
            var clone = (PavedData)base.Clone();
            clone.MixedSewerPumpVariableCapacitySeries = MixedSewerPumpVariableCapacitySeries != null
                                                             ? (TimeSeries) MixedSewerPumpVariableCapacitySeries.Clone()
                                                             : null;
            clone.DwfSewerPumpVariableCapacitySeries = DwfSewerPumpVariableCapacitySeries != null
                                                           ? (TimeSeries) DwfSewerPumpVariableCapacitySeries.Clone()
                                                           : null;
            clone.VariableWaterUseFunction = VariableWaterUseFunction != null
                                                 ? (Function) VariableWaterUseFunction.Clone()
                                                 : null;
            return clone;
        }
    }
}