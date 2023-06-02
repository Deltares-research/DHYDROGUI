using System.ComponentModel;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity]
    public class SacramentoData: CatchmentModelData
    {
        //nhib
        protected SacramentoData()
            : base(null){ }

        public SacramentoData(Catchment catchment)
            : base(catchment)
        {
            catchment.ModelData = this;
            
            HydrographValues = new EventedList<double>(new double[36]);
            HydrographValues[0] = 1;

            UpperZoneFreeWaterStorageCapacity = 1.0;
            UpperZoneTensionWaterStorageCapacity = 1.0;
            LowerZoneTensionWaterStorageCapacity = 1.0;
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
        [CustomControlHelper("DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.MeteoStationControlHelper",
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui")]
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
        [SubCategory("Percolation")]
        [Description("Proportional increase")]
        public double PercolationIncrease { get; set; }
        
        [Category("Area")]
        [SubCategory("Percolation")]
        [Description("Percolation exponent")]
        public double PercolationExponent { get; set; }

        [Category("Area")]
        [SubCategory("Lower zone")]
        [Description("Percolation water fraction")]
        public double PercolatedWaterFraction { get; set; }

        [Category("Area")]
        [SubCategory("Lower zone")]
        [Description("Free water storage fraction")]
        public double FreeWaterFraction { get; set; }

        [Category("Area")]
        [SubCategory("Lower zone")]
        [Description("Base flow fraction not observed in streams")]
        public double RatioUnobservedToObservedBaseFlow { get; set; }

        [Category("Area")]
        [SubCategory("Lower zone")]
        [Description("Sub-surface outflow")]
        [Unit("mm/day")]
        public double SubSurfaceOutflow { get; set; }

        [Category("Area")]
        [SubCategory("Direct runoff")]
        [Description("Permanently impervious fraction")]
        public double PermanentlyImperviousFraction { get; set; }

        [Category("Area")]
        [SubCategory("Direct runoff")]
        [Description("Additional impervious fraction")]
        public double RainfallImperviousFraction { get; set; }

        [Category("Area")]
        [SubCategory("Direct runoff")]
        [Description("Streams, lakes and vegetation fraction")]
        public double WaterAndVegetationAreaFraction { get; set; }

        [Category("Area")]
        [SubCategory("Internal routing interval")]
        [Description("Upper rainfall threshold")]        
        public double UpperRainfallThreshold { get; set; }

        [Category("Area")]
        [SubCategory("Internal routing interval")]
        [Description("Lower rainfall threshold")]        
        public double LowerRainfallThreshold { get; set; }

        [Category("Area")]
        [SubCategory("Internal routing interval")]
        [Description("Time interval increment parameter")]
        public double TimeIntervalIncrement { get; set; }

        [Category("Capacities")]
        [CustomControlHelper("DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.SacramentoCapacitiesControlHelper",
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui")]
        public double UpperZoneTensionWaterStorageCapacity { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double UpperZoneTensionWaterInitialContent { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double UpperZoneFreeWaterStorageCapacity { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double UpperZoneFreeWaterInitialContent { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double UpperZoneFreeWaterDrainageRate { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZoneTensionWaterStorageCapacity { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZoneTensionWaterInitialContent { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZoneSupplementalFreeWaterStorageCapacity { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZoneSupplementalFreeWaterInitialContent { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZoneSupplementalFreeWaterDrainageRate { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZonePrimaryFreeWaterStorageCapacity { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZonePrimaryFreeWaterInitialContent { get; set; }

        [Category("Capacities")]
        [Hide] //Custom control
        public double LowerZonePrimaryFreeWaterDrainageRate { get; set; }

        [Category("Unit hydrograph")]
        [CustomControlHelper("DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.SacramentoUnitHydrographControlHelper",
            "DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui")]
        public double HydrographStep { get; set; }

        [Hide] //Custom control
        public IEventedList<double> HydrographValues { get; set; }
        
        #endregion
    }

}
