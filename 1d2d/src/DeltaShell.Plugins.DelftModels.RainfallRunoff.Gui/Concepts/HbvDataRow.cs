using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class HbvDataRow : RainfallRunoffDataRow<HbvData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Runoff area (m²)")]
        public double RunoffArea
        {
            get { return data.Area; }
            set { data.Area = value; }
        }

        [Description("Surface level (m AD)")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Description("Snowfall temperature (°C)")]
        public double SnowFallTemperature
        {
            get { return data.SnowFallTemperature; }
            set { data.SnowFallTemperature = value; }
        }

        [Description("Snowmelt temperature (°C)")]        
        public double SnowMeltTemperature
        {
            get { return data.SnowMeltTemperature; }
            set { data.SnowMeltTemperature = value; }
        }

        [Description("Snow melting constant (mm/day°C)")]
        public double SnowMeltingConstant
        {
            get { return data.SnowMeltingConstant; }
            set { data.SnowMeltingConstant = value; }
        }

        [Description("Temperature altitude constant (°C/km)")]       
        public double TemperatureAltitudeConstant
        {
            get { return data.TemperatureAltitudeConstant; }
            set { data.TemperatureAltitudeConstant = value; }
        }

        [Description("Freezing efficiency")]
        public double FreezingEfficiency
        {
            get { return data.FreezingEfficiency; }
            set { data.FreezingEfficiency = value; }
        }
        
        [Description("Free water fraction")]
        public double FreeWaterFraction
        {
            get { return data.FreeWaterFraction; }
            set { data.FreeWaterFraction = value; }
        }

        [Description("Beta")]
        public double Beta
        {
            get { return data.Beta; }
            set { data.Beta = value; }
        }

        [Description("Field capacity (mm)")]
        public double FieldCapacity
        {
            get { return data.FieldCapacity; }
            set { data.FieldCapacity = value; }
        }

        [Description("Field capacity fraction threshold")]
        public double FieldCapacityThreshold
        {
            get { return data.FieldCapacityThreshold; }
            set { data.FieldCapacityThreshold = value; }
        }

        [Description("Base flow reservoir coefficient")]
        public double BaseFlowReservoirConstant
        {
            get { return data.BaseFlowReservoirConstant; }
            set { data.BaseFlowReservoirConstant = value; }
        }

        [Description("Interflow reservoir coefficient")]
        public double InterflowReservoirConstant
        {
            get { return data.InterflowReservoirConstant; }
            set { data.InterflowReservoirConstant = value; }
        }

        [Description("Quickflow reservoir coefficient")]
        public double QuickFlowReservoirConstant
        {
            get { return data.QuickFlowReservoirConstant; }
            set { data.QuickFlowReservoirConstant = value; }
        }

        [Description("Upper zone reservoir content threshold")]
        public double UpperZoneThreshold
        {
            get { return data.UpperZoneThreshold; }
            set { data.UpperZoneThreshold = value; }
        }

        
        [Description("Maximum percolation (mm/day)")]
        public double MaximumPercolation
        {
            get { return data.MaximumPercolation; }
            set { data.MaximumPercolation = value; }
        }

        [Description("Initial dry snow content")]
        public double InitialDrySnowContent
        {
            get { return data.InitialDrySnowContent; }
            set { data.InitialDrySnowContent = value; }
        }

        [Description("Initial free water content (mm)")]
        public double InitialFreeWaterContent
        {
            get { return data.InitialFreeWaterContent; }
            set { data.InitialFreeWaterContent = value; }
        }

        [Description("Initial soil moisture contents")]
        public double InitialSoilMoistureContents
        {
            get { return data.InitialSoilMoistureContents; }
            set { data.InitialSoilMoistureContents = value; }
        }

        [Description("Initial upper zone content (mm)")]
        public double InitialUpperZoneContent
        {
            get { return data.InitialUpperZoneContent; }
            set { data.InitialUpperZoneContent = value; }
        }

        [Description("Initial lower zone content (mm)")]
        public double InitialLowerZoneContent
        {
            get { return data.InitialLowerZoneContent; }
            set { data.InitialLowerZoneContent = value; }
        }

        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Description("Temperature station")]
        public string TemperatureStationName
        {
            get { return data.TemperatureStationName; }
            set { data.TemperatureStationName = value; }
        }

        [Description("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}
