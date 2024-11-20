using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class PavedDataRow : RainfallRunoffDataRow<PavedData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Runoff area (m²)")]
        public double RunoffArea
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Description("Spilling definition")]
        public PavedEnums.SpillingDefinition SpillingDefinition
        {
            get { return data.SpillingDefinition; }
            set { data.SpillingDefinition = value; }
        }

        private bool UseRunoffCoefficient
        {
            get { return data.SpillingDefinition == PavedEnums.SpillingDefinition.UseRunoffCoefficient; }
        }

        [Description("Coefficient")]
        public double? Coefficient
        {
            get { return UseRunoffCoefficient ? data.RunoffCoefficient : (double?) null; }
            set
            {
                if (value != null && UseRunoffCoefficient)
                {
                    data.RunoffCoefficient = value.Value;
                }
            }
        }

        [Description("Surface level (m AD)")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Description("Sewer type")]
        public PavedEnums.SewerType SewerType
        {
            get { return data.SewerType; }
            set { data.SewerType = value; }
        }

        private bool SewerTypeIsMixed
        {
            get { return SewerType == PavedEnums.SewerType.MixedSystem; }
        }

        [Description("Mixed/rainfall capacity (m³/s)")]
        public double? MixedAndOrRainfallCapacity
        {
            get
            {
                return data.IsSewerPumpCapacityFixed
                           ? data.CapacityMixedAndOrRainfall
                           : (double?) null;
            }
            set
            {
                if (value != null && data.IsSewerPumpCapacityFixed)
                {
                    data.CapacityMixedAndOrRainfall = value.Value;
                }
            }
        }

        [Description("Dry weather flow capacity (m³/s)")]
        public double? DWFCapacity
        {
            get
            {
                return (data.IsSewerPumpCapacityFixed && !SewerTypeIsMixed)
                           ? data.CapacityDryWeatherFlow
                           : (double?) null;
            }
            set
            {
                if (value != null && (data.IsSewerPumpCapacityFixed && !SewerTypeIsMixed))
                {
                    data.CapacityDryWeatherFlow = value.Value;
                }
            }
        }

        [Description("Mixed/rainfall discharge")]
        public PavedEnums.SewerPumpDischargeTarget MixedAndOrRainfallDischarge
        {
            get { return data.MixedAndOrRainfallSewerPumpDischarge; }
            set { data.MixedAndOrRainfallSewerPumpDischarge = value; }
        }

        [Description("Dry weather flow discharge")]
        public PavedEnums.SewerPumpDischargeTarget SewerPumpDischarge
        {
            get
            {
                return SewerTypeIsMixed
                           ? (PavedEnums.SewerPumpDischargeTarget) (-1)
                           : data.DryWeatherFlowSewerPumpDischarge;
            }
            set
            {
                if (!SewerTypeIsMixed)
                {
                    data.DryWeatherFlowSewerPumpDischarge = value;
                }
            }
        }

        [Description("Street maximum (mm (x Area))")]
        public double StreetMaximum
        {
            get { return data.MaximumStreetStorage; }
            set { data.MaximumStreetStorage = value; }
        }

        [Description("Street initial (mm (x Area))")]
        public double StreetInitial
        {
            get { return data.InitialStreetStorage; }
            set { data.InitialStreetStorage = value; }
        }

        [Description("Sewer DWF maximum (mm (x Area))")]
        public double SewerDWFMaximum
        {
            get { return data.MaximumSewerDryWeatherFlowStorage; }
            set { data.MaximumSewerDryWeatherFlowStorage = value; }
        }

        [Description("Sewer DWF initial (mm (x Area))")]
        public double SewerDWFInitial
        {
            get { return data.InitialSewerDryWeatherFlowStorage; }
            set { data.InitialSewerDryWeatherFlowStorage = value; }
        }

        [Description("Sewer Mixed/rainfall maximum (mm (x Area))")]
        public double SewerMixedAndOrRainfallMaximum
        {
            get { return data.MaximumSewerMixedAndOrRainfallStorage; }
            set { data.MaximumSewerMixedAndOrRainfallStorage = value; }
        }

        [Description("Sewer Mixed/rainfall initial (mm (x Area))")]
        public double SewerMixedAndOrRainfallInitial
        {
            get { return data.InitialSewerMixedAndOrRainfallStorage; }
            set { data.InitialSewerMixedAndOrRainfallStorage = value; }
        }

        [Description("# inhabitants")]
        public int NumberOfInhabitants
        {
            get { return data.NumberOfInhabitants; }
            set { data.NumberOfInhabitants = value; }
        }

        [Description("Type")]
        public PavedEnums.DryWeatherFlowOptions DryWeatherFlowOptions
        {
            get { return data.DryWeatherFlowOptions; }
            set { data.DryWeatherFlowOptions = value; }
        }

        [Description("Water use (l/day)")]
        public double WaterUse
        {
            get { return data.WaterUse; }
            set { data.WaterUse = value; }
        }

        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Description("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}