using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class RainfallRunoffOutputSettingsProperties : ObjectProperties<RainfallRunoffModel>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RainfallRunoffOutputSettingsProperties));

        //tabs (\t) are there to force category ordering...
        private const string GeneralCategory = "\t\t\t\t\t\t\t\t\t\t\tGeneral";
        private const string UnpavedCategoryName = "\t\t\t\t\t\t\t\t\t\tUnpaved";
        private const string PavedCategoryName = "\t\t\t\t\t\t\t\t\tPaved";
        private const string GreenhouseCategoryName = "\t\t\t\t\t\t\t\tGreenhouse";
        private const string OpenWaterCategoryName = "\t\t\t\t\t\t\tOpen water";
        private const string SacramentoCategoryName = "\t\t\t\t\t\tSacramento";
        private const string HbvCategoryName = "\t\t\t\t\tHBV";
        private const string WWTPCategoryName = "\t\t\t\tWaste water treatment plant";
        private const string BalancePerNodeCategoryName = "\t\t\tWater balance per node";
        private const string BalanceTotalCategoryName = "\t\tWater balance total";
        private const string LinkCategoryName = "\tLink";
        private const string BoundaryCategoryName = "Boundary";
        private const string NWRWCategoryName = "NWRW";

        [PropertyOrder(0)]
        [Category(GeneralCategory)]
        [DisplayName("Output timestep")]
        [TypeConverter(typeof (DeltaShellTimeSpanConverter))]
        public TimeSpan OutputTimestep
        {
            get { return data.OutputSettings.OutputTimeStep; }
            set { data.OutputSettings.OutputTimeStep = value; }
        }

        #region Unpaved

        [PropertyOrder(99)]
        [Category(UnpavedCategoryName)]
        [DisplayName("    All unpaved output")] //tab to make sure it shows on top of list
        [Description("Toggles all unpaved aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? Unpaved
        {
            get { return GetCommonAggregationOption(ElementSet.UnpavedElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.UnpavedElmSet); }
        }

        [PropertyOrder(100)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Surface runoff [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedSurfaceRunoff)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedSurfaceRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SurfRunoff, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SurfRunoff, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(101)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Groundwater outflow [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedGwOutflow)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedGroundwaterOutflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GwOutflow, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GwOutflow, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(102)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Rainfall [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedRainfall)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(103)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Evaporation surface [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedEvaporationSurface)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedEvaporationSurface
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(104)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Infiltration [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedInfiltration)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedInfiltration
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Infiltration, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Infiltration, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(105)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Net seepage [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedSeepage)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedSeepage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Seepage, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Seepage, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(106)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Actual evaporation [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedEvaporationActual)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedEvaporationActual
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationActual, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationActual, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(107)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Potential evaporation [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedEvaporationPotential)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedEvaporationPotential
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationPotential, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationPotential, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(108)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Percolation [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedPercolation)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedPercolation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Percolation, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Percolation, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(109)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Capillary rise [m³/s] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedCapillaryRise)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedCapillaryRise
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CapillaryRise, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CapillaryRise, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(110)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Groundwater level [m] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedGroundwaterLevel)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedGroundwaterLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(111)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Storage land [mm] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedStorageLandmm)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedStorageLandmm
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Storage_mm, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Storage_mm, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(112)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Groundwater volume [m³] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedGroundwaterVolume)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedGroundwaterVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterVolume, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterVolume, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(113)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Storage land [m³] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedStorageLandm3)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedStorageLandm3
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Storage_m3, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Storage_m3, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(114)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Groundwater level threshold [hour] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelThreshold)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedGroundwaterLevelThreshold
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevelThreshold, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevelThreshold, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(115)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Groundwater level surface [m] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelSurface)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedGoundwaterLevelSurface
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevelSurface, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevelSurface, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(116)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Storage coefficient (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedStorageCoeff)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedStorageCoefficient
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageCoeff, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageCoeff, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(117)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Unsaturated zone [mm] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedUnsaturatedZone)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedUnsaturatedZoneContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.UnsatZoneContent, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.UnsatZoneContent, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(118)]
        [Category(UnpavedCategoryName)]
        [DisplayName("Volume unsaturated zone [m³] (unp)")]
        [Description(RainfallRunoffModelParameterNames.UnpavedUnsaturatedZoneVolume)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions UnpavedUnsaturatedZoneVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.UnsaturatedZoneVolume, ElementSet.UnpavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.UnsaturatedZoneVolume, ElementSet.UnpavedElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Paved

        [PropertyOrder(0)]
        [Category(PavedCategoryName)]
        [DisplayName("    All paved output")] //tab to make sure it shows on top of list
        [Description("Toggles all paved aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? Paved
        {
            get { return GetCommonAggregationOption(ElementSet.PavedElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.PavedElmSet); }
        }

        [PropertyOrder(0)]
        [Category(PavedCategoryName)]
        [DisplayName("Storage RWA [mm] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedStorageRWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedStorageRWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageRWA_mm, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageRWA_mm, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(1)]
        [Category(PavedCategoryName)]
        [DisplayName("Storage DWA [mm] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedStorageDWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedStorageDWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageDWA_mm, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageDWA_mm, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(2)]
        [Category(PavedCategoryName)]
        [DisplayName("Storage street [mm] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedStorageStreet)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedStorageStreet
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageStreet_mm, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageStreet_mm, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(3)]
        [Category(PavedCategoryName)]
        [DisplayName("Spilling [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedSpillingTotal)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedSpillingTotal
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SpillingTotal, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SpillingTotal, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(4)]
        [Category(PavedCategoryName)]
        [DisplayName("Pumped flow [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedPumpedTotal)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedPumpedTotal
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpedTotal, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpedTotal, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(5)]
        [Category(PavedCategoryName)]
        [DisplayName("Rainfall [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedRainfall)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(6)]
        [Category(PavedCategoryName)]
        [DisplayName("DWA infl-RWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedDWAToRWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedDwaToRwa
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.DWA2RWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.DWA2RWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(7)]
        [Category(PavedCategoryName)]
        [DisplayName("DWA infl-DWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedDWAToDWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedDwaToDwa
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.DWA2DWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.DWA2DWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(8)]
        [Category(PavedCategoryName)]
        [DisplayName("Surface RWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedSurface_RWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedSurfaceRwa
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SurfaceRWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SurfaceRWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(9)]
        [Category(PavedCategoryName)]
        [DisplayName("RWA to DWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedRWAToDWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedRwaToDwa
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RWA2DWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RWA2DWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(10)]
        [Category(PavedCategoryName)]
        [DisplayName("Spilling RWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedSpillingRWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedSpillingRWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SpillingRWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SpillingRWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(11)]
        [Category(PavedCategoryName)]
        [DisplayName("Pumped RWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedPumpedRWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedPumpedRWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpedRWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpedRWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(12)]
        [Category(PavedCategoryName)]
        [DisplayName("Spilling DWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedSpillingDWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedSpillingDWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SpillingDWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SpillingDWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(13)]
        [Category(PavedCategoryName)]
        [DisplayName("Pumped DWA [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedPumpedDWA)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedPumpedDWA
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpedDWA, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpedDWA, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(14)]
        [Category(PavedCategoryName)]
        [DisplayName("Evaporation surface [m³/s] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedEvapSurface)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedEvaporationSurface
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(15)]
        [Category(PavedCategoryName)]
        [DisplayName("Volume dynamic storage [mm] (p)")]
        [Description(RainfallRunoffModelParameterNames.PavedVolDynStorage)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions PavedStorageVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageVolDyn, ElementSet.PavedElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageVolDyn, ElementSet.PavedElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Greenhouse

        [PropertyOrder(199)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("    All greenhouse output")] //tab to make sure it shows on top of list
        [Description("Toggles all greenhouse aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? Greenhouse
        {
            get { return GetCommonAggregationOption(ElementSet.GreenhouseElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.GreenhouseElmSet); }
        }

        [PropertyOrder(200)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("Storage basins [m³] (g)")]
        [Description(RainfallRunoffModelParameterNames.GreenhouseStorageBasins)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions GreenhouseStorageBasisns
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Storage_m3, ElementSet.GreenhouseElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Storage_m3, ElementSet.GreenhouseElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(201)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("Flow basins [m³/s] (g)")]
        [Description(RainfallRunoffModelParameterNames.GreenhouseFlowBasins)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions GreenhouseFlowBasins
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.GreenhouseElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.GreenhouseElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(202)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("Rainfall [m³/s] (g)")]
        [Description(RainfallRunoffModelParameterNames.GreenhouseRainfall)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions GreenhouseRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.GreenhouseElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.GreenhouseElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(203)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("Evaporation [m³/s] (g)")]
        [Description(RainfallRunoffModelParameterNames.GreenhouseEvaporation)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions GreenhouseEvaoration
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.GreenhouseElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.GreenhouseElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(204)]
        [Category(GreenhouseCategoryName)]
        [DisplayName("Water use [m³/s] (g)")]
        [Description(RainfallRunoffModelParameterNames.GreenhouseWaterUse)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions GreenhouseWaterUse
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterUse, ElementSet.GreenhouseElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterUse, ElementSet.GreenhouseElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Open Water

        [PropertyOrder(299)]
        [Category(OpenWaterCategoryName)]
        [DisplayName("    All open water output")] //tab to make sure it shows on top of list
        [Description("Toggles all open water aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? Openwater
        {
            get { return GetCommonAggregationOption(ElementSet.OpenWaterElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.OpenWaterElmSet); }
        }

        [PropertyOrder(300)]
        [Category(OpenWaterCategoryName)]
        [DisplayName("Rainfall [m³/s] (ow)")]
        [Description(RainfallRunoffModelParameterNames.OpenWaterRainfall)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions OpenWaterRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.OpenWaterElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.OpenWaterElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(301)]
        [Category(OpenWaterCategoryName)]
        [DisplayName("Evaporation [m³/s] (ow)")]
        [Description(RainfallRunoffModelParameterNames.OpenWaterEvaporation)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions OpenWaterEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Sacramento
        
        [PropertyOrder(339)]
        [Category(SacramentoCategoryName)]
        [DisplayName("    All Sacramento output")] //tab to make sure it shows on top of list
        [Description("Toggles all Sacramento aggregation options")]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions? Sacramento
        {
            get { return GetCommonAggregationOption(ElementSet.SacramentoElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.SacramentoElmSet); }
        }

        [PropertyOrder(340)]
        [Category(SacramentoCategoryName)]
        [DisplayName("UZTW capacity [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoUpperZoneTensionWaterCapacity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoUpperZoneTensionWaterCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrUZTWC, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrUZTWC, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(341)]
        [Category(SacramentoCategoryName)]
        [DisplayName("UZFW capacity [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoUpperZoneFreeWaterCapacity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoUpperZoneFreeWaterCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrUZFWC, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrUZFWC, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(342)]
        [Category(SacramentoCategoryName)]
        [DisplayName("LZTW capacity [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoLowerZoneTensionWaterCapacity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoLowerZoneTensionWaterCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrLZTWC, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrLZTWC, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(343)]
        [Category(SacramentoCategoryName)]
        [DisplayName("LZFSW capacity [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoLowerZoneFreeSupplementalWaterCapacity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoLowerZoneFreeSupplementalWaterCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrLZFSC, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrLZFSC, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(344)]
        [Category(SacramentoCategoryName)]
        [DisplayName("LZFPW capacity [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoLowerZoneFreePrimaryWaterCapacity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoLowerZoneFreePrimaryWaterCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrLZFPC, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrLZFPC, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(345)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Rainfall [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoRainfall)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrPrecip, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrPrecip, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(346)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Potential evaporation [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoPotEvap)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoPotentialEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrPotEvap, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrPotEvap, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(347)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Actual evaporation [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoActEvap)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoActualEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrActEvap, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrActEvap, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(348)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Base flow [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoBaseFlow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoBaseFlow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrBaseFlow, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrBaseFlow, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(349)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Surface runoff [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoSurfaceFlow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoSurfaceRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrSurfFlow, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrSurfFlow, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(350)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Impervious area runoff [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoRunoffImpArea)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoRunoffImperviousArea
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrRunoffImpArea, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrRunoffImpArea, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(351)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Total runoff [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoTotalRunoff)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoTotalRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrTotalRunoff, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrTotalRunoff, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(352)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Channel inflow [m³/s] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoChannelInflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoChannelInflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrChannelInflow, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrChannelInflow, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(352)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Side + subsurface outflow [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoSideSubsurfaceOutflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoSideSubsurfaceOutflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrSideSubSurfaceOutflow, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrSideSubSurfaceOutflow, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(353)]
        [Category(SacramentoCategoryName)]
        [DisplayName("Additional impervious area content [mm] (sac)")]
        [Description(RainfallRunoffModelParameterNames.SacramentoAdditionalImpAreaContent)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SacramentoSideSubsurfaceAdditionalImperviousAreaContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SacrAddImpAreaContent, ElementSet.SacramentoElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SacrAddImpAreaContent, ElementSet.SacramentoElmSet).AggregationOptions = value; }
        }

        #endregion

        #region HBV

        [PropertyOrder(369)]
        [Category(HbvCategoryName)]
        [DisplayName("    All HBV output")] //tab to make sure it shows on top of list
        [Description("Toggles all HBV aggregation options")]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions? HBV
        {
            get { return GetCommonAggregationOption(ElementSet.HbvElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.HbvElmSet); }
        }

        [PropertyOrder(370)]
        [Category(HbvCategoryName)]
        [DisplayName("Outflow [m³/s] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvOutflow)]
        [DefaultValue(typeof(AggregationOptions),"None")]
        public AggregationOptions HbvOutflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffOutflow, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffOutflow, ElementSet.HbvElmSet).AggregationOptions = value; }            
        }

        [PropertyOrder(371)]
        [Category(HbvCategoryName)]
        [DisplayName("Rainfall [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvRainfall)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffRainfall, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffRainfall, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(372)]
        [Category(HbvCategoryName)]
        [DisplayName("Snowfall [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvSnowfall)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvSnowfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffSnowfall, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffSnowfall, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(373)]
        [Category(HbvCategoryName)]
        [DisplayName("Potential evaporation [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvPotEvap)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvPotentialEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffPotEvap, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffPotEvap, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(374)]
        [Category(HbvCategoryName)]
        [DisplayName("Actual evaporation [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvActEvap)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvActualEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffActEvap, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffActEvap, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(375)]
        [Category(HbvCategoryName)]
        [DisplayName("Base flow [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvBaseflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvBaseFlow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffBaseflow, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffBaseflow, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(376)]
        [Category(HbvCategoryName)]
        [DisplayName("Interflow [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvInterflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvInterflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffInterflow, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffInterflow, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(377)]
        [Category(HbvCategoryName)]
        [DisplayName("Quickflow [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvQuickflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvQuickflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffQuickflow, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffQuickflow, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(378)]
        [Category(HbvCategoryName)]
        [DisplayName("Dry snow content [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvDrySnowContent)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvDrySnowContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffDrySnowContent, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffDrySnowContent, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(379)]
        [Category(HbvCategoryName)]
        [DisplayName("Free water content [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvFreeWaterContent)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvFreeWaterContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffFreeWaterContent, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffFreeWaterContent, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(380)]
        [Category(HbvCategoryName)]
        [DisplayName("Soil moisture [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvSoilMoisture)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvSoilMoisture
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffSoilMoisture, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffSoilMoisture, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(381)]
        [Category(HbvCategoryName)]
        [DisplayName("Upper zone content [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvUpperZoneContent)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvUpperZoneContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffUpperZoneContent, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffUpperZoneContent, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(382)]
        [Category(HbvCategoryName)]
        [DisplayName("Lower zone content [mm] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvLowerZoneContent)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvLowerZoneContent
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffLowerZoneContent, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffLowerZoneContent, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(383)]
        [Category(HbvCategoryName)]
        [DisplayName("Temperature [°C] (hbv)")]
        [Description(RainfallRunoffModelParameterNames.HbvTemperature)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions HbvTemperature
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffTemperature, ElementSet.HbvElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffTemperature, ElementSet.HbvElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Waste Water Treatment Plant

        [PropertyOrder(400)]
        [Category(WWTPCategoryName)]
        [DisplayName("Inflow [m³/s] (wwtp)")]
        [Description(RainfallRunoffModelParameterNames.WWTPInFlow)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions WWTPFlowIn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowIn, ElementSet.WWTPElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowIn, ElementSet.WWTPElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(401)]
        [Category(WWTPCategoryName)]
        [DisplayName("Outflow [m³/s] (wwtp)")]
        [Description(RainfallRunoffModelParameterNames.WWTPOutFlow)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions WWTPFlowOut
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.WWTPElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.WWTPElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Balance Per Node

        [PropertyOrder(699)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("    All balance per node output")] //tab to make sure it shows on top of list
        [Description("Toggles all water balance per node aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? BalancePerNode
        {
            get { return GetCommonAggregationOption(ElementSet.BalanceNodeElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.BalanceNodeElmSet); }
        }

        [PropertyOrder(700)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Total in non-links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceFlowInNonLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceFlowInAtNode
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalInNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalInNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(701)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Total in via links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceFlowInViaLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceFlowInViaLinks
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalInViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalInViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(702)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Total out non-links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceFlowOutNonLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceFlowOutAtNode
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalOutNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalOutNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(703)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Total out via links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceFlowOutViaLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceFlowOutViaLinks
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalOutViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalOutViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(704)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Delta storage [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceDeltaStorage)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceDeltaStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.DeltaStorage_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.DeltaStorage_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(705)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Balance error [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceBalanceError)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceBalanceError
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(706)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative in non-links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumFlowInNonLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeFlowInAtNode
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumInNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumInNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(707)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative in via links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumFlowInViaLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeFlowInViaLinks
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumInViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumInViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(708)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative out non-links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutNonLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeFlowOutAtNode
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumOutNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumOutNonLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(709)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative out via links [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutViaLinks)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeFlowOutViaLinks
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumOutViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumOutViaLinks_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(710)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative delta storage [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumDeltaStorage)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeDeltaStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumDeltaStorage_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumDeltaStorage_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(711)]
        [Category(BalancePerNodeCategoryName)]
        [DisplayName("Cumulative balance error [m³] (bn)")]
        [Description(RainfallRunoffModelParameterNames.NodeBalanceCumBalanceError)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions NodeBalanceCumulativeBalanceError
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CumBalanceError_m3, ElementSet.BalanceNodeElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CumBalanceError_m3, ElementSet.BalanceNodeElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Balance Total

        [PropertyOrder(699)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("    All model balance output")] //tab to make sure it shows on top of list
        [Description("Toggles all model water balance aggregation options")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions? ModelBalance
        {
            get { return GetCommonAggregationOption(ElementSet.BalanceModelElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.BalanceModelElmSet); }
        }

        [PropertyOrder(800)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Rainfall  [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceRainfall)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(801)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Evaporation paved [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceEvaporationPaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceEvaporationPaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationPaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationPaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(802)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Evaporation unpaved [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceEvaporationUnpaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceEvaporationUnpaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EvaporationUnpaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EvaporationUnpaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(803)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Use greenhouses (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceUseGreenhouses)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceUseGreenhouses
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterUse, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterUse, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(804)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("DWF paved [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceDWFPaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceDWFPaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.DWFPaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.DWFPaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(805)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Net seepage unpaved [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceNetSeepageUnpaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceNetSeepageUnpaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NetSeepageUnpaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NetSeepageUnpaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(806)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage paved [mm] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceStoragePaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceStoragePaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StoragePaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StoragePaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(807)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage unpaved [mm] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceStorageUnpaved)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceStorageUnpaved
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StoragePaved, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StoragePaved, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(808)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage greenhouses [mm] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceStorageGreenhouses)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceStorageGreenhouses
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageGreenhouses, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageGreenhouses, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(809)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage wwtp [mm] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceStorageWWTP)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceStorageWWTP
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageWWTP, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageWWTP, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(810)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Boundaries out (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceBoundariesOut)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceBoundariesOut
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BoundariesOut, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BoundariesOut, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(811)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Boundaries in (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceBoundariesIn)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceBoundariesIn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BoundariesIn, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BoundariesIn, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(812)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("External inflow RRRunoff [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceExternalInFlowRRRunoff)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceExternalInFlowRRRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.ExternalInflowRRRunoff, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.ExternalInflowRRRunoff, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(813)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage change RRRunoff [m³/s] (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceStorageChangeRRRunoff)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceStorageChangeRRRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.StorageChangeRRRunoff, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.StorageChangeRRRunoff, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(813)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Balance error RR Rural (bm)")]
        [Description(RainfallRunoffModelParameterNames.ModelBalanceError)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions ModelBalanceError
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceModelElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(814)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Rainfall NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceRainfall)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRainfall, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRainfall, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(815)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Evaporation NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceEvaporation)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceEvaporation, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceEvaporation, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(816)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Infiltration Storage NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceInfiltrStorage)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceInfiltrationStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceInfiltrStorage, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceInfiltrStorage, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(817)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Infiltration Runoff NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceInfiltrRunOff)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceInfiltrationRunOff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceInfiltrRunOff, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceInfiltrRunOff, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(818)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Storage NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceStorage)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceStorage, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceStorage, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(819)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("RWF NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceRWF)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceRWF
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRWF, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRWF, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(820)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("DWF People NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceDWFPeople)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceDWFPeople
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceDWFPeople, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceDWFPeople, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(821)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("DWF Companies NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceDWFCompanies)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceDWFCompanies
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceDWFCompanies, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceDWFCompanies, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(822)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("RWF + DWF NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceRWFAndDWF)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceRWFAndDWF
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRWFAndDWF, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceRWFAndDWF, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(823)]
        [Category(BalanceTotalCategoryName)]
        [DisplayName("Balance error NWRW [m³] (bm)")]
        [Description(RainfallRunoffModelParameterNames.NwrwModelBalanceError)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwModelBalanceError
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceError, ElementSet.BalanceModelElmSet)?.AggregationOptions ?? AggregationOptions.None; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwModelBalanceError, ElementSet.BalanceModelElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Link

        [PropertyOrder(500)]
        [Category(LinkCategoryName)]
        [DisplayName("Link flow [m³/s] (l)")]
        [Description(RainfallRunoffModelParameterNames.LinkFlowOut)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions LinkDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.LinkElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.LinkElmSet).AggregationOptions = value; }
        }

        #endregion

        #region Boundaries

        [PropertyOrder(600)]
        [Category(BoundaryCategoryName)]
        [DisplayName("Discharge [m³/s] (bnd)")]
        [Description(RainfallRunoffModelParameterNames.BoundaryDischarge)]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions BoundaryDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).AggregationOptions; }
            set
            {
                if (data.GetDataItemByValue(data.BoundaryDischarge).LinkedBy.Any() && value != AggregationOptions.Current)
                {
                    Log.WarnFormat("Can't change output type from current to {0} since it is used.", value);

                    return;
                }

                data.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).AggregationOptions = value;
            }
        }

        #endregion
        #region NWRW


        [PropertyOrder(899)]
        [Category(NWRWCategoryName)]
        [DisplayName("    All NWRW output")] //tab to make sure it shows on top of list
        [Description("Toggles all NWRW aggregation options")]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions? NWRW
        {
            get { return GetCommonAggregationOption(ElementSet.NWRWElmSet); }
            set { SetAggregationOptionForElementSet(value, ElementSet.NWRWElmSet); }
        }

        [PropertyOrder(900)]
        [Category(NWRWCategoryName)]
        [DisplayName("Rainfall [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwRainfall)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwRainfall
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(901)]
        [Category(NWRWCategoryName)]
        [DisplayName("Evaporation [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwEvaporation)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwEvaporation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RunoffActEvap, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RunoffActEvap, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(902)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow sewer [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflowSewer)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflowSewer
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflowSewer, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflowSewer, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(903)]
        [Category(NWRWCategoryName)]
        [DisplayName("Infiltration Depression [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInfilDepress)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInfilDepress
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfilDepress, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfilDepress, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(904)]
        [Category(NWRWCategoryName)]
        [DisplayName("Infiltration Runoff [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInfilRunoff)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInfilRunoff
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfilRunoff, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfilRunoff, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(905)]
        [Category(NWRWCategoryName)]
        [DisplayName("RWF [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwRWF)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwRWF
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwRWF, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwRWF, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(906)]
        [Category(NWRWCategoryName)]
        [DisplayName("DWF People [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwDWFPeople)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwDWFPeople
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwDWFPeople, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwDWFPeople, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(907)]
        [Category(NWRWCategoryName)]
        [DisplayName("DWF Companies [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwDWFCompanies)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwDWFCompanies
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwDWFCompanies, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwDWFCompanies, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(908)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Depression [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageDepress)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageDepress
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageDepress, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageDepress, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(909)]
        [Category(NWRWCategoryName)]
        [DisplayName("Dynamic Storage [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwDynamicStorage)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwDynamicStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwDynamicStorage, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwDynamicStorage, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(910)]
        [Category(NWRWCategoryName)]
        [DisplayName("Infiltration capacity surface [mm/hr] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInfCapSurf)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInfCapSurf
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfCapSurf, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfCapSurf, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(911)]
        [Category(NWRWCategoryName)]
        [DisplayName("Infiltration capacity dynamic [mm/hr] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInfCapDyn)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInfCapDyn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfCapDyn, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInfCapDyn, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(912)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 1 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer1)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer1
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer1, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer1, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(913)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 2 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer2)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer2
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer2, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer2, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(914)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 3 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer3)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer3
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer3, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer3, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(914)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 4 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer4)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer4
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer4, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer4, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(915)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 5 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer5)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer5
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer5, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer5, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(916)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 6 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer6)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer6
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer6, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer6, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(917)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 7 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer7)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer7
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer7, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer7, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(918)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 8 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer8)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer8
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer8, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer8, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(919)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 9 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer9)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer9
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer9, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer9, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(920)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 10 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer10)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer10
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer10, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer10, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(921)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 11 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer11)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer11
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer11, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer11, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(922)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer 12 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewer12)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewer12
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer12, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewer12, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(923)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 1 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp1)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp1
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp1, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp1, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(924)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 2 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp2)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp2
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp2, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp2, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(925)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 3 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp3)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp3
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp3, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp3, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(926)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 4 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp4)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp4
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp4, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp4, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(927)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 5 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp5)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp5
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp5, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp5, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(928)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 6 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp6)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp6
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp6, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp6, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(929)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 7 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp7)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp7
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp7, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp7, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(930)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 8 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp8)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp8
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp8, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp8, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(931)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 9 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp9)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp9
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp9, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp9, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(932)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 10 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp10)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp10
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp10, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp10, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(933)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 11 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp11)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp11
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp11, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp11, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(934)]
        [Category(NWRWCategoryName)]
        [DisplayName("Inflow Sewer Special 12 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwInflSewerSp12)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwInflSewerSp12
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp12, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwInflSewerSp12, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(935)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 1 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp1)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp1
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp1, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp1, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(936)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 2 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp2)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp2
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp2, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp2, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(937)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 3 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp3)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp3
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp3, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp3, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(938)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 4 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp4)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp4
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp4, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp4, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(939)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 5 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp5)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp5
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp5, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp5, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(940)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 6 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp6)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp6
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp6, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp6, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        [PropertyOrder(941)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 7 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp7)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp7
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp7, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp7, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        [PropertyOrder(942)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 8 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp8)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp8
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp8, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp8, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        [PropertyOrder(943)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 9 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp9)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp9
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp9, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp9, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        [PropertyOrder(944)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 10 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp10)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp10
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp10, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp10, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        [PropertyOrder(945)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 11 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp11)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp11
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp11, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp11, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(946)]
        [Category(NWRWCategoryName)]
        [DisplayName("Storage Special 12 [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwStorageSp12)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwStorageSp12
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp12, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwStorageSp12, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(947)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi inflow [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiInflow)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiInflow
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiInflow, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiInflow, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }
        
        [PropertyOrder(948)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi infiltration [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiInfiltr)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiInfiltr
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiInfiltr, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiInfiltr, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(949)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi spill [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiSpill)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiSpill
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiSpill, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiSpill, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(950)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi drain [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiDrain)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiDrain
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiDrain, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiDrain, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(951)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi storage [m³] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiStorage)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiStorage, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiStorage, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(952)]
        [Category(NWRWCategoryName)]
        [DisplayName("Wadi level [m] (nwrw)")]
        [Description(RainfallRunoffModelParameterNames.NwrwWadiLevel)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions NwrwWadiLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiLevel, ElementSet.NWRWElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NwrwWadiLevel, ElementSet.NWRWElmSet).AggregationOptions = value; }
        }

        #endregion

        private void SetAggregationOptionForElementSet(AggregationOptions? value, ElementSet elementSet)
        {
            if (value == null) return;

            data.OutputSettings.BeginEdit(new DefaultEditAction(string.Format("Setting all aggregation options to {0}.", value)));
            data.OutputSettings.SetAggregationOptionForElementSet(value, elementSet); 
            data.OutputSettings.EndEdit();
        }

        private AggregationOptions? GetCommonAggregationOption(ElementSet elementSet)
        {
            return data.OutputSettings.GetCommonAggregationOption(elementSet);
        }
    }
}