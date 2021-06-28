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
            
            StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
            SewerPumpCapacityUnit = PavedEnums.SewerPumpCapacityUnit.m3_min;
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

        // fixed or variable
        public double CapacityMixedAndOrRainfall { get; set; } // m3_min
        public double CapacityDryWeatherFlow { get; set; } // m3_min
        public PavedEnums.SewerPumpCapacityUnit SewerPumpCapacityUnit { get; } = PavedEnums.SewerPumpCapacityUnit.m3_min;
        public TimeSeries DwfSewerPumpVariableCapacitySeries { get; set; }
        public TimeSeries MixedSewerPumpVariableCapacitySeries { get; set; }

        [Description("Storage")]
        public double MaximumStreetStorage { get; set; }

        //mm (x Area)
        public double InitialStreetStorage { get; set; } //mm (x Area)
        public double MaximumSewerMixedAndOrRainfallStorage { get; set; } //mm (x Area)
        public double InitialSewerMixedAndOrRainfallStorage { get; set; } //mm (x Area)
        public double MaximumSewerDryWeatherFlowStorage { get; set; } //mm (x Area)
        public double InitialSewerDryWeatherFlowStorage { get; set; } //mm (x Area)
        public RainfallRunoffEnums.StorageUnit StorageUnit { get; set; }

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