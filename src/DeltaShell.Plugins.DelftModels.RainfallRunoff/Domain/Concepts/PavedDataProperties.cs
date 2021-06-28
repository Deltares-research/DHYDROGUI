using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class PavedDataProperties : ObjectProperties<PavedData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double RunoffArea
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Category("Area")]
        [DisplayName("Surface level [m AD]")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Category("Storage")]
        [DisplayName("Street maximum [mm]")]
        public double StreetMaximum
        {
            get { return data.MaximumStreetStorage; }
            set { data.MaximumStreetStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Street initial [mm]")]
        public double StreetInitial
        {
            get { return data.InitialStreetStorage; }
            set { data.InitialStreetStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Sewer DWF initial [mm]")]
        public double SewerDWFInitial
        {
            get { return data.InitialSewerDryWeatherFlowStorage; }
            set { data.InitialSewerDryWeatherFlowStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Sewer DWF maximum [mm]")]
        public double SewerDWFMaximum
        {
            get { return data.MaximumSewerDryWeatherFlowStorage; }
            set { data.MaximumSewerDryWeatherFlowStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Sewer Mixed/rainfall initial [mm]")]
        public double SewerMixedAndOrRainfallInitial
        {
            get { return data.InitialSewerMixedAndOrRainfallStorage; }
            set { data.InitialSewerMixedAndOrRainfallStorage = value; }
        }

        [Category("Storage")]
        [DisplayName("Sewer Mixed/rainfall maximum [mm]")]
        public double SewerMixedAndOrRainfallMaximum
        {
            get { return data.MaximumSewerMixedAndOrRainfallStorage; }
            set { data.MaximumSewerMixedAndOrRainfallStorage = value; }
        }

        [Category("Capacity")]
        [DisplayName("Sewer type")]
        public PavedEnums.SewerType SewerType
        {
            get { return data.SewerType; }
            set { data.SewerType = value; }
        }

        [Category("Capacity")]
        [DisplayName("Dry weather flow capacity [m³/s]")]
        public double DWFCapacity
        {
            get { return data.CapacityDryWeatherFlow; }
            set { data.CapacityMixedAndOrRainfall = value; }
        }

        [Category("Capacity")]
        [DisplayName("Mixed/rainfall capacity [m³/s]")]
        public double MixedAndOrRainfallCapacity
        {
            get { return data.CapacityMixedAndOrRainfall; }
            set { data.CapacityMixedAndOrRainfall = value; }
        }

        [Category("Capacity")]
        [DisplayName("Fixed sewer pump capacity")]
        public bool DwfSewerPumpCapacityFixed
        {
            get { return data.IsSewerPumpCapacityFixed; }
            set { data.IsSewerPumpCapacityFixed = value; }
        }

        [Category("Capacity")]
        [DisplayName("Mixed/rainfall discharge")]
        public PavedEnums.SewerPumpDischargeTarget MixedAndOrRainfallDischarge
        {
            get { return data.MixedAndOrRainfallSewerPumpDischarge; }
            set { data.MixedAndOrRainfallSewerPumpDischarge = value; }
        }

        [Category("Capacity")]
        [DisplayName("DWF discharge")]
        public PavedEnums.SewerPumpDischargeTarget SewerPumpDischarge
        {
            get { return data.DryWeatherFlowSewerPumpDischarge; }
            set { data.DryWeatherFlowSewerPumpDischarge = value; }
        }

        [Category("Dry weather flow")]
        [DisplayName("Nr. of inhabitants")]
        public int NumberOfInhabitants
        {
            get { return data.NumberOfInhabitants; }
            set { data.NumberOfInhabitants = value; }
        }

        [Category("Dry weather flow")]
        [DisplayName("Type")]
        public PavedEnums.DryWeatherFlowOptions DryWeatherFlowOptions
        {
            get { return data.DryWeatherFlowOptions; }
            set { data.DryWeatherFlowOptions = value; }
        }

        [Category("Dry weather flow")]
        [DisplayName("Water use [l/day]")]
        public double WaterUse
        {
            get { return data.WaterUse; }
            set { data.WaterUse = value; }
        }

        
        [Category("Runoff")]
        [DisplayName("No delay")]
        public bool NoDelay
        {
            get { return data.SpillingDefinition == PavedEnums.SpillingDefinition.NoDelay; }
        }

        [Category("Runoff")]
        [DisplayName("Coefficient")]
        public double Coefficient
        {
            get { return data.RunoffCoefficient; }
            set { data.RunoffCoefficient = value; }
        }

        [Category("Meteo")]
        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Category("Meteo")]
        [DisplayName("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}