using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class SacramentoDataProperties : ObjectProperties<SacramentoData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double Area
        {
            get { return data.Area; }
            set { data.Area = value; }
        }

        [Category("Percolation")]
        [DisplayName("Proportional increase")]
        public double PercolationIncrease
        {
            get { return data.PercolationIncrease; }
            set { data.PercolationIncrease = value; }
        }
        
        [Category("Percolation")]
        [DisplayName("Percolation exponent")]
        public double PercolationExponent
        {
            get { return data.PercolationExponent; }
            set { data.PercolationExponent = value; }
        }

        [Category("Lower zone")]
        [DisplayName("Percolation water fraction")]
        public double PercolatedWaterFraction
        {
            get { return data.PercolatedWaterFraction; }
            set { data.PercolatedWaterFraction = value; }
        }

        [Category("Lower zone")]
        [DisplayName("Free water storage fraction")]
        public double FreeWaterFraction
        {
            get { return data.FreeWaterFraction; }
            set { data.FreeWaterFraction = value; }
        }

        [Category("Lower zone")]
        [DisplayName("Unobserved base flow fraction")]
        public double RatioUnobservedToObservedBaseFlow
        {
            get { return data.RatioUnobservedToObservedBaseFlow; }
            set { data.RatioUnobservedToObservedBaseFlow = value; }
        }

        [Category("Lower zone")]
        [DisplayName("Sub-surface outflow [mm/day]")]
        public double SubSurfaceOutflow
        {
            get { return data.SubSurfaceOutflow; }
            set { data.SubSurfaceOutflow = value; }
        }

        [Category("Direct runoff")]
        [DisplayName("Permanently impervious fraction")]
        public double PermanentlyImperviousFraction
        {
            get { return data.PermanentlyImperviousFraction; }
            set { data.PermanentlyImperviousFraction = value; }
        }

        [Category("Direct runoff")]
        [DisplayName("Additional impervious fraction")]
        public double RainfallImperviousFraction
        {
            get { return data.RainfallImperviousFraction; }
            set { data.RainfallImperviousFraction = value; }
        }

        [Category("Direct runoff")]
        [DisplayName("Streams, lakes and vegetation fraction")]
        public double WaterAndVegetationAreaFraction
        {
            get { return data.WaterAndVegetationAreaFraction; }
            set { data.WaterAndVegetationAreaFraction = value; }
        }

        [Category("Internal routing interval")]
        [DisplayName("Upper rainfall threshold")]
        public double UpperRainfallThreshold
        {
            get { return data.UpperRainfallThreshold; }
            set { data.UpperRainfallThreshold = value; }
        }

        [Category("Internal routing interval")]
        [DisplayName("Lower rainfall threshold")]
        public double LowerRainfallThreshold
        {
            get { return data.LowerRainfallThreshold; }
            set { data.LowerRainfallThreshold = value; }
        }

        [Category("Internal routing interval")]
        [DisplayName("Time interval increment parameter")]
        public double TimeIntervalIncrement
        {
            get { return data.TimeIntervalIncrement; }
            set { data.TimeIntervalIncrement = value; }
        }

        [Category("Capacities")]
        [DisplayName("UZTW storage capacity [mm]")]
        [Description("Upper zone tension water storage capacity")]
        public double UpperZoneTensionWaterStorageCapacity
        {
            get { return data.UpperZoneTensionWaterStorageCapacity; }
            set { data.UpperZoneTensionWaterStorageCapacity = value; }
        }

        [Category("Capacities")]
        [DisplayName("UZTW initial content [mm]")]
        [Description("Upper zone tension water storage capacity")]
        public double UpperZoneTensionWaterInitialContent
        {
            get { return data.UpperZoneTensionWaterInitialContent; }
            set { data.UpperZoneTensionWaterInitialContent = value; }
        }

        [Category("Capacities")]
        [DisplayName("UZFW storage capacity [mm]")]
        [Description("Upper zone free water storage capacity")]
        public double UpperZoneFreeWaterStorageCapacity
        {
            get { return data.UpperZoneFreeWaterStorageCapacity; }
            set { data.UpperZoneFreeWaterStorageCapacity = value; }
        }

        [Category("Capacities")]
        [DisplayName("UZFW initial content [mm]")]
        [Description("Upper zone free water storage capacity")]
        public double UpperZoneFreeWaterInitialContent
        {
            get { return data.UpperZoneFreeWaterInitialContent; }
            set { data.UpperZoneFreeWaterInitialContent = value; }
        }

        [Category("Capacities")]
        [DisplayName("UZFW drainage rate [1/day]")]
        [Description("Upper zone free water drainage rate")]
        public double UpperZoneFreeWaterDrainageRate
        {
            get { return data.UpperZoneFreeWaterDrainageRate; }
            set { data.UpperZoneFreeWaterDrainageRate = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZTW storage capacity [mm]")]
        [Description("Lower zone tension water storage capacity")]
        public double LowerZoneTensionWaterStorageCapacity
        {
            get { return data.LowerZoneTensionWaterStorageCapacity; }
            set { data.LowerZoneTensionWaterStorageCapacity = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZTW initial content [mm]")]
        [Description("Lower zone tension water initial content")]
        public double LowerZoneTensionWaterInitialContent
        {
            get { return data.LowerZoneTensionWaterInitialContent; }
            set { data.LowerZoneTensionWaterInitialContent = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZSFW storage capacity [mm]")]
        [Description("Lower zone supplemental free water storage capacity")]
        public double LowerZoneSupplementalFreeWaterStorageCapacity
        {
            get { return data.LowerZoneSupplementalFreeWaterStorageCapacity; }
            set { data.LowerZoneSupplementalFreeWaterStorageCapacity = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZSFW initial content [mm]")]
        [Description("Lower zone supplemental free water initial content")]
        public double LowerZoneSupplementalFreeWaterInitialContent
        {
            get { return data.LowerZoneSupplementalFreeWaterInitialContent; }
            set { data.LowerZoneSupplementalFreeWaterInitialContent = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZSFW drainage rate [1/day]")]
        [Description("Lower zone supplemental free water drainage rate")]
        public double LowerZoneSupplementalFreeWaterDrainageRate
        {
            get { return data.LowerZoneSupplementalFreeWaterDrainageRate; }
            set { data.LowerZoneSupplementalFreeWaterDrainageRate = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZPFW storage capacity [mm]")]
        [Description("Lower zone primary free water storage capacity")]
        public double LowerZonePrimaryFreeWaterStorageCapacity
        {
            get { return data.LowerZonePrimaryFreeWaterStorageCapacity; }
            set { data.LowerZonePrimaryFreeWaterStorageCapacity = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZPFW initial content [mm]")]
        [Description("Lower zone primary free water initial content")]
        public double LowerZonePrimaryFreeWaterInitialContent
        {
            get { return data.LowerZonePrimaryFreeWaterInitialContent; }
            set { data.LowerZonePrimaryFreeWaterInitialContent = value; }
        }

        [Category("Capacities")]
        [DisplayName("LZPFW drainage rate [1/day]")]
        [Description("Lower zone primary free water drainage rate")]
        public double LowerZonePrimaryFreeWaterDrainageRate
        {
            get { return data.LowerZonePrimaryFreeWaterDrainageRate; }
            set { data.LowerZonePrimaryFreeWaterDrainageRate = value; }
        }

        [Category("Unit hydrograph")]
        [DisplayName("Time step")]
        public double HydrographStep
        {
            get { return data.HydrographStep; }
            set { data.HydrographStep = value; }
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
    }
}
