using System.ComponentModel;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange = false)]
    public class HbvData : CatchmentModelData
    {
        //nhib
        protected HbvData() : base(null) { }

        public HbvData(Catchment catchment)
            : base(catchment)
        {
            catchment.ModelData = this;
            TemperatureStationName = "";

            // quasi-arbitrary defaults: should be (0,1)
            BaseFlowReservoirConstant = 0.1;
            InterflowReservoirConstant = 0.1;
            QuickFlowReservoirConstant = 0.1;
        }

        #region properties

        [Category("Area")]
        [Description("Runoff area")]
        [Unit("m²")]
        public double Area
        {
            get { return base.CalculationArea; }
            set { SetCalculationArea(value); }
        }

        [EditAction]
        private void SetCalculationArea(double area)
        {
            base.CalculationArea = area;
        }

        [Category("Meteo")]
        [Hide]
        public string StationName
        {
            get { return base.MeteoStationName; }
            set { SetMeteoStationName(value); }
        }

        [EditAction]
        private void SetMeteoStationName(string name)
        {
            base.MeteoStationName = name;
        }

        [Category("Area")]
        [Description("Surface level (altitude)")]
        [Unit("m AD")]
        public double SurfaceLevel { get; set; }

        [Category("Snow")]
        [Description("Snowfall temperature")]
        [Unit("°C")]
        public double SnowFallTemperature { get; set; }

        [Category("Snow")]
        [Description("Snowmelt temperature")]
        [Unit("°C")]
        public double SnowMeltTemperature { get; set; }

        [Category("Snow")]
        [Description("Snow melting constant")]
        [Unit("mm/day°C")]
        public double SnowMeltingConstant { get; set; }
        
        [Category("Snow")]
        [Description("Temperature altitude constant")]
        [Unit("°C/km")]
        public double TemperatureAltitudeConstant { get; set; }

        [Category("Snow")]
        [Description("Freezing efficiency")]
        public double FreezingEfficiency { get; set; }

        [Category("Snow")]
        [Description("Free water fraction")]
        public double FreeWaterFraction { get; set; }

        [Category("Soil")]
        [Description("Beta")]
        public double Beta { get; set; }

        [Category("Soil")]
        [Description("Field capacity")]
        [Unit("mm")]
        public double FieldCapacity { get; set; }
        
        [Category("Soil")]
        [Description("Field capacity fraction threshold")]
        public double FieldCapacityThreshold { get; set; }

        [Category("Flow")]
        [Description("Base flow reservoir coefficient")]
        public double BaseFlowReservoirConstant { get; set; }

        [Category("Flow")]
        [Description("Interflow reservoir coefficient")]
        public double InterflowReservoirConstant { get; set; }

        [Category("Flow")]
        [Description("Quickflow reservoir coefficient")]
        public double QuickFlowReservoirConstant { get; set; }

        [Category("Flow")]
        [Description("Upper zone reservoir content threshold")]
        [Unit("mm")]
        public double UpperZoneThreshold { get; set; }

        [Category("Flow")]
        [Description("Maximum percolation")]
        [Unit("mm/day")]
        public double MaximumPercolation { get; set; }
    
        [Category("Meteo")]
        [CustomControlHelper("DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.TemperatureStationControlHelper",
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui")]
        public string TemperatureStationName { get; set; }

        [Category("Hini")]
        [Description("Initial dry snow content")]
        [Unit("mm")]
        public double InitialDrySnowContent { get; set; }

        [Category("Hini")]
        [Description("Initial free water content")]
        [Unit("mm")]
        public double InitialFreeWaterContent { get; set; }

        [Category("Hini")]
        [Description("Initial soil moisture contents")]
        public double InitialSoilMoistureContents { get; set; }

        [Category("Hini")]
        [Description("Initial upper zone content")]
        [Unit("mm")]
        public double InitialUpperZoneContent { get; set; }

        [Category("Hini")]
        [Description("Initial lower zone content")]
        [Unit("mm")]
        public double InitialLowerZoneContent { get; set; }

        #endregion
    }
}
