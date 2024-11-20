using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    class SacramentoDataRow : RainfallRunoffDataRow<SacramentoData>
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

        [Description("Proportional increase")]
        public double PercolationIncrease
        {
            get { return data.PercolationIncrease; }
            set { data.PercolationIncrease = value; }
        }

        [Description("Percolation exponent")]
        public double PercolationExponent
        {
            get { return data.PercolationExponent; }
            set { data.PercolationExponent = value; }
        }

        [Description("Percolation water fraction")]
        public double PercolatedWaterFraction
        {
            get { return data.PercolatedWaterFraction; }
            set { data.PercolatedWaterFraction = value; }
        }

        [Description("Free water storage fraction")]
        public double FreeWaterFraction
        {
            get { return data.FreeWaterFraction; }
            set { data.FreeWaterFraction = value; }
        }

        [Description("Base flow fraction not observed in streams")]
        public double RatioUnobservedToObservedBaseFlow
        {
            get { return data.RatioUnobservedToObservedBaseFlow; }
            set { data.RatioUnobservedToObservedBaseFlow = value; }
        }

        [Description("Sub-surface outflow (mm/day)")]
        public double SubSurfaceOutflow
        {
            get { return data.SubSurfaceOutflow; }
            set { data.SubSurfaceOutflow = value; }
        }

        [Description("Permanently impervious fraction")]
        public double PermanentlyImperviousFraction
        {
            get { return data.PermanentlyImperviousFraction; }
            set { data.PermanentlyImperviousFraction = value; }
        }

        [Description("Additional impervious fraction")]
        public double RainfallImperviousFraction
        {
            get { return data.RainfallImperviousFraction; }
            set { data.RainfallImperviousFraction = value; }
        }

        [Description("Streams, lakes and vegetation fraction")]
        public double WaterAndVegetationAreaFraction
        {
            get { return data.WaterAndVegetationAreaFraction; }
            set { data.WaterAndVegetationAreaFraction = value; }
        }

        [Description("Upper rainfall threshold")]
        public double UpperRainfallThreshold
        {
            get { return data.UpperRainfallThreshold; }
            set { data.UpperRainfallThreshold = value; }
        }

        [Description("Lower rainfall threshold")]
        public double LowerRainfallThreshold
        {
            get { return data.LowerRainfallThreshold; }
            set { data.LowerRainfallThreshold = value; }
        }

        [Description("Time interval increment parameter")]
        public double TimeIntervalIncrement
        {
            get { return data.TimeIntervalIncrement; }
            set { data.TimeIntervalIncrement = value; }
        }

        [Description("Upper zone tension water storage capacity (mm)")]
        public double UpperZoneTensionWaterStorageCapacity
        {
            get { return data.UpperZoneTensionWaterStorageCapacity; }
            set { data.UpperZoneTensionWaterStorageCapacity = value; }
        }

        [Description("Upper zone tension water initial content (mm)")]
        public double UpperZoneTensionWaterInitialContent
        {
            get { return data.UpperZoneTensionWaterInitialContent; }
            set { data.UpperZoneTensionWaterInitialContent = value; }
        }

        [Description("Upper zone free water storage capacity (mm)")]
        public double UpperZoneFreeWaterStorageCapacity
        {
            get { return data.UpperZoneFreeWaterStorageCapacity; }
            set { data.UpperZoneFreeWaterStorageCapacity = value; }
        }

        [Description("Upper zone free water initial content (mm)")]
        public double UpperZoneFreeWaterInitialContent
        {
            get { return data.UpperZoneFreeWaterInitialContent; }
            set { data.UpperZoneFreeWaterInitialContent = value; }
        }

        [Description("Upper zone free water drainage rate (1/day)")]
        public double UpperZoneFreeWaterDrainageRate
        {
            get { return data.UpperZoneFreeWaterDrainageRate; }
            set { data.UpperZoneFreeWaterDrainageRate = value; }
        }

        [Description("Lower zone tension water storage capacity (mm)")]
        public double LowerZoneTensionWaterStorageCapacity
        {
            get { return data.LowerZoneTensionWaterStorageCapacity; }
            set { data.LowerZoneTensionWaterStorageCapacity = value; }
        }

        [Description("Lower zone tension water initial content (mm)")]
        public double LowerZoneTensionWaterInitialContent
        {
            get { return data.LowerZoneTensionWaterInitialContent; }
            set { data.LowerZoneTensionWaterInitialContent = value; }
        }

        [Description("Lower zone suppl. water storage capacity (mm)")]
        public double LowerZoneSupplementalFreeWaterStorageCapacity
        {
            get { return data.LowerZoneSupplementalFreeWaterStorageCapacity; }
            set { data.LowerZoneSupplementalFreeWaterStorageCapacity = value; }
        }

        [Description("Lower zone suppl. water initial content (mm)")]
        public double LowerZoneSupplementalFreeWaterInitialContent
        {
            get { return data.LowerZoneSupplementalFreeWaterInitialContent; }
            set { data.LowerZoneSupplementalFreeWaterInitialContent = value; }
        }

        [Description("Lower zone suppl. water drainage rate (1/day)")]
        public double LowerZoneSupplementalFreeWaterDrainageRate
        {
            get { return data.LowerZoneSupplementalFreeWaterDrainageRate; }
            set { data.LowerZoneSupplementalFreeWaterDrainageRate = value; }
        }

        [Description("Lower zone primary water storage capacity (mm)")]
        public double LowerZonePrimaryFreeWaterStorageCapacity
        {
            get { return data.LowerZonePrimaryFreeWaterStorageCapacity; }
            set { data.LowerZonePrimaryFreeWaterStorageCapacity = value; }
        }

        [Description("Lower zone primary water initial content (mm)")]
        public double LowerZonePrimaryFreeWaterInitialContent
        {
            get { return data.LowerZonePrimaryFreeWaterInitialContent; }
            set { data.LowerZonePrimaryFreeWaterInitialContent = value; }
        }

        [Description("Lower zone primary water drainage rate (1/day)")]
        public double LowerZonePrimaryFreeWaterDrainageRate
        {
            get { return data.LowerZonePrimaryFreeWaterDrainageRate; }
            set { data.LowerZonePrimaryFreeWaterDrainageRate = value; }
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

        [Description("Unit Hydrograph step size")]
        public double HydrographStep
        {
            get { return data.HydrographStep; }
            set { data.HydrographStep = value; }
        }

        [Description("H(1)")]
        public double H1
        {
            get { return data.HydrographValues[0]; }
            set { data.HydrographValues[0] = value; }
        }

        [Description("H(2)")]
        public double H2
        {
            get { return data.HydrographValues[1]; }
            set { data.HydrographValues[1] = value; }
        }

        [Description("H(3)")]
        public double H3
        {
            get { return data.HydrographValues[2]; }
            set { data.HydrographValues[2] = value; }
        }

        [Description("H(4)")]
        public double H4
        {
            get { return data.HydrographValues[3]; }
            set { data.HydrographValues[3] = value; }
        }

        [Description("H(5)")]
        public double H5
        {
            get { return data.HydrographValues[4]; }
            set { data.HydrographValues[4] = value; }
        }

        [Description("H(6)")]
        public double H6
        {
            get { return data.HydrographValues[5]; }
            set { data.HydrographValues[5] = value; }
        }

        [Description("H(7)")]
        public double H7
        {
            get { return data.HydrographValues[6]; }
            set { data.HydrographValues[6] = value; }
        }

        [Description("H(8)")]
        public double H8
        {
            get { return data.HydrographValues[7]; }
            set { data.HydrographValues[7] = value; }
        }

        [Description("H(9)")]
        public double H9
        {
            get { return data.HydrographValues[8]; }
            set { data.HydrographValues[8] = value; }
        }

        [Description("H(10)")]
        public double H10
        {
            get { return data.HydrographValues[9]; }
            set { data.HydrographValues[9] = value; }
        }

        [Description("H(11)")]
        public double H11
        {
            get { return data.HydrographValues[10]; }
            set { data.HydrographValues[10] = value; }
        }

        [Description("H(12)")]
        public double H12
        {
            get { return data.HydrographValues[11]; }
            set { data.HydrographValues[11] = value; }
        }

        [Description("H(13)")]
        public double H13
        {
            get { return data.HydrographValues[12]; }
            set { data.HydrographValues[12] = value; }
        }

        [Description("H(14)")]
        public double H14
        {
            get { return data.HydrographValues[13]; }
            set { data.HydrographValues[13] = value; }
        }

        [Description("H(15)")]
        public double H15
        {
            get { return data.HydrographValues[14]; }
            set { data.HydrographValues[14] = value; }
        }

        [Description("H(16)")]
        public double H16
        {
            get { return data.HydrographValues[15]; }
            set { data.HydrographValues[15] = value; }
        }

        [Description("H(17)")]
        public double H17
        {
            get { return data.HydrographValues[16]; }
            set { data.HydrographValues[16] = value; }
        }

        [Description("H(18)")]
        public double H18
        {
            get { return data.HydrographValues[17]; }
            set { data.HydrographValues[17] = value; }
        }

        [Description("H(19)")]
        public double H19
        {
            get { return data.HydrographValues[18]; }
            set { data.HydrographValues[18] = value; }
        }

        [Description("H(20)")]
        public double H20
        {
            get { return data.HydrographValues[19]; }
            set { data.HydrographValues[19] = value; }
        }

        [Description("H(21)")]
        public double H21
        {
            get { return data.HydrographValues[20]; }
            set { data.HydrographValues[20] = value; }
        }

        [Description("H(22)")]
        public double H22
        {
            get { return data.HydrographValues[21]; }
            set { data.HydrographValues[21] = value; }
        }

        [Description("H(23)")]
        public double H23
        {
            get { return data.HydrographValues[22]; }
            set { data.HydrographValues[22] = value; }
        }

        [Description("H(24)")]
        public double H24
        {
            get { return data.HydrographValues[23]; }
            set { data.HydrographValues[23] = value; }
        }

        [Description("H(25)")]
        public double H25
        {
            get { return data.HydrographValues[24]; }
            set { data.HydrographValues[24] = value; }
        }

        [Description("H(26)")]
        public double H26
        {
            get { return data.HydrographValues[25]; }
            set { data.HydrographValues[25] = value; }
        }

        [Description("H(27)")]
        public double H27
        {
            get { return data.HydrographValues[26]; }
            set { data.HydrographValues[26] = value; }
        }

        [Description("H(28)")]
        public double H28
        {
            get { return data.HydrographValues[27]; }
            set { data.HydrographValues[27] = value; }
        }

        [Description("H(29)")]
        public double H29
        {
            get { return data.HydrographValues[28]; }
            set { data.HydrographValues[28] = value; }
        }

        [Description("H(30)")]
        public double H30
        {
            get { return data.HydrographValues[29]; }
            set { data.HydrographValues[29] = value; }
        }

        [Description("H(31)")]
        public double H31
        {
            get { return data.HydrographValues[30]; }
            set { data.HydrographValues[30] = value; }
        }

        [Description("H(32)")]
        public double H32
        {
            get { return data.HydrographValues[31]; }
            set { data.HydrographValues[31] = value; }
        }

        [Description("H(33)")]
        public double H33
        {
            get { return data.HydrographValues[32]; }
            set { data.HydrographValues[32] = value; }
        }

        [Description("H(34)")]
        public double H34
        {
            get { return data.HydrographValues[33]; }
            set { data.HydrographValues[33] = value; }
        }

        [Description("H(35)")]
        public double H35
        {
            get { return data.HydrographValues[34]; }
            set { data.HydrographValues[34] = value; }
        }

        [Description("H(36)")]
        public double H36
        {
            get { return data.HydrographValues[35]; }
            set { data.HydrographValues[35] = value; }
        }
    }
}
