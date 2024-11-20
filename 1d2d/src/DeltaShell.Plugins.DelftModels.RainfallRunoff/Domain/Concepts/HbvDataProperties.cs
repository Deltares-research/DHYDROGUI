using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class HbvDataProperties : ObjectProperties<HbvData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double Area
        {
            get { return data.Area; }
            set { data.Area = value; }
        }

        [Category("Area")]
        [DisplayName("Surface level [m AD]")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Category("Snow")]
        [DisplayName("Snowfall temperature [°C]")]
        public double SnowFallTemperature
        {
            get { return data.SnowFallTemperature; }
            set { data.SnowFallTemperature = value; }
        }

        [Category("Snow")]
        [DisplayName("Snowmelt temperature [°C]")]
        public double SnowMeltTemperature
        {
            get { return data.SnowMeltTemperature; }
            set { data.SnowMeltTemperature = value; }
        }

        [Category("Snow")]
        [DisplayName("Snow melting constant [mm/day°C]")]
        public double SnowMeltingConstant
        {
            get { return data.SnowMeltingConstant; }
            set { data.SnowMeltingConstant = value; }
        }

        [Category("Snow")]
        [DisplayName("Temperature altitude constant [°C/km]")]
        public double TemperatureAltitudeConstant
        {
            get { return data.TemperatureAltitudeConstant; }
            set { data.TemperatureAltitudeConstant = value; }
        }

        [Category("Snow")]
        [DisplayName("Freezing efficiency")]
        public double FreezingEfficiency
        {
            get { return data.FreezingEfficiency; }
            set { data.FreezingEfficiency = value; }
        }

        [Category("Snow")]
        [DisplayName("Free water fraction")]
        public double FreeWaterFraction
        {
            get { return data.FreeWaterFraction; }
            set { data.FreeWaterFraction = value; }
        }

        [Category("Soil")]
        [DisplayName("Beta")]
        public double Beta
        {
            get { return data.Beta; }
            set { data.Beta = value; }
        }

        [Category("Soil")]
        [DisplayName("Field capacity [mm]")]
        public double FieldCapacity
        {
            get { return data.FieldCapacity; }
            set { data.FieldCapacity = value; }
        }

        [Category("Soil")]
        [DisplayName("Field capacity fraction threshold")]
        public double FieldCapacityThreshold
        {
            get { return data.FieldCapacityThreshold; }
            set { data.FieldCapacityThreshold = value; }
        }

        [Category("Flow")]
        [DisplayName("Base flow reservoir coefficient")]
        public double BaseFlowReservoirConstant
        {
            get { return data.BaseFlowReservoirConstant; }
            set { data.BaseFlowReservoirConstant = value; }
        }

        [Category("Flow")]
        [DisplayName("Interflow reservoir coefficient")]
        public double InterflowReservoirConstant
        {
            get { return data.InterflowReservoirConstant; }
            set { data.InterflowReservoirConstant = value; }
        }

        [Category("Flow")]
        [DisplayName("Quickflow reservoir coefficient")]
        public double QuickFlowReservoirConstant
        {
            get { return data.QuickFlowReservoirConstant; }
            set { data.QuickFlowReservoirConstant = value; }
        }

        [Category("Flow")]
        [DisplayName("Upper zone reservoir content threshold [mm]")]
        public double UpperZoneThreshold
        {
            get { return data.UpperZoneThreshold; }
            set { data.UpperZoneThreshold = value; }
        }

        [Category("Flow")]
        [DisplayName("Maximum percolation [mm/day]")]
        public double MaximumPercolation
        {
            get { return data.MaximumPercolation; }
            set { data.MaximumPercolation = value; }
        }

        [Category("Hini")]
        [DisplayName("Initial dry snow content [mm]")]
        public double InitialDrySnowContent
        {
            get { return data.InitialDrySnowContent; }
            set { data.InitialDrySnowContent = value; }
        }

        [Category("Hini")]
        [DisplayName("Initial free water content [mm]")]
        public double InitialFreeWaterContent
        {
            get { return data.InitialFreeWaterContent; }
            set { data.InitialFreeWaterContent = value; }
        }

        [Category("Hini")]
        [DisplayName("Initial soil moisture contents")]
        public double InitialSoilMoistureContents
        {
            get { return data.InitialSoilMoistureContents; }
            set { data.InitialSoilMoistureContents = value; }
        }

        [Category("Hini")]
        [DisplayName("Initial upper zone content [mm]")]
        public double InitialUpperZoneContent
        {
            get { return data.InitialUpperZoneContent; }
            set { data.InitialUpperZoneContent = value; }
        }

        [Category("Hini")]
        [DisplayName("Initial lower zone content [mm]")]
        public double InitialLowerZoneContent
        {
            get { return data.InitialLowerZoneContent; }
            set { data.InitialLowerZoneContent = value; }
        }

        [Category("Meteo")]
        [DisplayName("Meteo station")]
        public string StationName
        {
            get { return data.StationName; }
            set { data.StationName = value; }
        }

        [Category("Meteo")]
        [DisplayName("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }

        [Category("Meteo")]
        [DisplayName("Temperature station")]
        public string TemperatureStationName
        {
            get { return data.TemperatureStationName; }
            set { data.TemperatureStationName = value; }
        }
    }
}
