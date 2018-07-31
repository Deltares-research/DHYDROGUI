using System;
using System.ComponentModel;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Property Grid Decorator class. 
    /// Note that the default values wont be set because an embedded data class is used. Defaults are set in data class constructor
    /// </summary>
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DOutputSettingsProperties_DisplayName")]
    public class WaterFlowModel1DOutputSettingsProperties : ObjectProperties<WaterFlowModel1D>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DOutputSettingsProperties));

        private const string TimeCategoryName = "Output time step";
        private const string LocationsCategoryName = "Grid points";
        private const string BranchCategoryName = "Reach segments";
        private const string StructureCategoryName = "Structures";
        private const string PumpsCategoryName = "Pumps";
        private const string ObservationPointsCategoryName = "Observation points";
        private const string RetentionsCategoryName = "Retentions";
        private const string LateralSourcesCategoryName = "Lateral sources";
        private const string WaqOutputCategoryName = "Waq output";
        private const string SimulationInfoCategoryName = "Simulation info";

        # region Time settings

        [PropertyOrder(1)]
        [TypeConverter(typeof(DeltaShellTimeSpanWithMilliSecondsConverter))]
        [Description("Output time step for grid points and reach segments")]
        [Category(TimeCategoryName)]
        [DisplayName("Gridpoints")]
        public TimeSpan LocationOutputTimeStep
        {
            get { return data.OutputSettings.GridOutputTimeStep; }
            set { data.OutputSettings.GridOutputTimeStep = value; }
        }

        [TypeConverter(typeof(DeltaShellTimeSpanWithMilliSecondsConverter))]
        [Category(TimeCategoryName)]
        [Description("Output time step for structures, lateral sources observation points. Value is ignored for now.")]
        [DisplayName("Structures c.s.")]
        [PropertyOrder(3)]
        public TimeSpan StructureOutputTimeStep
        {
            get { return data.OutputSettings.StructureOutputTimeStep; }
            set { data.OutputSettings.StructureOutputTimeStep = value; }
        }

        # endregion

        # region Computational grid points

        [PropertyOrder(10)]
        [Category(LocationsCategoryName)]
        [Description("Water level at the computational gridpoints")]
        [DisplayName(WaterFlowModelParameterNames.LocationWaterLevel)]
        [DefaultValue(typeof(AggregationOptions), "Current")]
        public AggregationOptions LocationWaterLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [PropertyOrder(11)]
        [Category(LocationsCategoryName)]
        [Description("Water depth at the computational gridpoints")]
        [DisplayName(WaterFlowModelParameterNames.LocationWaterDepth)]
        [DefaultValue(typeof(AggregationOptions), "Current")]
        public AggregationOptions LocationWaterDepth
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Volume at the computational gridpoints")]
        [PropertyOrder(13)]
        [DisplayName(WaterFlowModelParameterNames.LocationVolume)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Total area at the computational gridpoints")]
        [PropertyOrder(14)]
        [DisplayName(WaterFlowModelParameterNames.LocationTotalArea)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationTotalArea
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalArea, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalArea, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Total width at the computational gridpoints")]
        [PropertyOrder(15)]
        [DisplayName(WaterFlowModelParameterNames.LocationTotalWidth)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationTotalwidth
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalWidth, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalWidth, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Salt concentration at the computational gridpoints; salt must be enabled.")]
        [PropertyOrder(16)]
        [DisplayName(WaterFlowModelParameterNames.LocationSaltConcentration)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SaltConcentration
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Density (water & salt) at the computational gridpoints.")]
        [PropertyOrder(18)]
        [DisplayName(WaterFlowModelParameterNames.LocationDensity)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions Density
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Density, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Density, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Lateral at nodes")]
        [DisplayName(WaterFlowModelParameterNames.LocationLateralAtNodes)]
        [PropertyOrder(19)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchLateralAtNodes
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.LateralAtNodes, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.LateralAtNodes, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }


        [Category(LocationsCategoryName)]
        [Description("Lateral discharge from 2d area to the computational gridpoints in 1d.")]
        [PropertyOrder(20)]
        [DisplayName(WaterFlowModelParameterNames.LocationQTotal_1d2d)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions Lateral1D2D
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description("Temperature computation grid points")]
        [DisplayName(WaterFlowModelParameterNames.LocationTemperature)]
        [PropertyOrder(21)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchTemperature
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationTotalHeatFlux)]
        [DisplayName(WaterFlowModelParameterNames.LocationTotalHeatFlux)]
        [PropertyOrder(22)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchLocationTotalHeatFlux
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TotalHeatFlux, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TotalHeatFlux, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationRadFluxClearSky)]
        [DisplayName(WaterFlowModelParameterNames.LocationRadFluxClearSky)]
        [PropertyOrder(23)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationRadFluxClearSky
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.RadFluxClearSky, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.RadFluxClearSky, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossConv)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossConv)]
        [PropertyOrder(24)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossConv
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossConv, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossConv, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationNetSolarRad)]
        [DisplayName(WaterFlowModelParameterNames.LocationNetSolarRad)]
        [PropertyOrder(25)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationNetSolarRad
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NetSolarRad, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NetSolarRad, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationEffectiveBackRad)]
        [DisplayName(WaterFlowModelParameterNames.LocationEffectiveBackRad)]
        [PropertyOrder(26)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationEffectiveBackRad
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.EffectiveBackRad, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.EffectiveBackRad, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossEvap)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossEvap)]
        [PropertyOrder(27)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossEvap
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossEvap, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossEvap, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossForcedEvap)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossForcedEvap)]
        [PropertyOrder(28)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossForcedEvap
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossForcedEvap, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossForcedEvap, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossFreeEvap)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossFreeEvap)]
        [PropertyOrder(29)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossFreeEvap
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossFreeEvap, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossFreeEvap, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossForcedConv)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossForcedConv)]
        [PropertyOrder(30)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossForcedConv
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossForcedConv, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossForcedConv, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(LocationsCategoryName)]
        [Description(WaterFlowModelParameterNames.LocationHeatLossFreeConv)]
        [DisplayName(WaterFlowModelParameterNames.LocationHeatLossFreeConv)]
        [PropertyOrder(31)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LocationHeatLossFreeConv
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.HeatLossFreeConv, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.HeatLossFreeConv, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }
        
        # endregion

        # region Staggered grid points

        [Category(BranchCategoryName)]
        [Description("Discharge at reach segments")]
        [PropertyOrder(21)]
        [DisplayName(WaterFlowModelParameterNames.BranchDischarge)]
        [DefaultValue(typeof(AggregationOptions), "Current")]
        public AggregationOptions BranchDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Velocity at reach segments")]
        [PropertyOrder(22)]
        [DisplayName(WaterFlowModelParameterNames.BranchVelocity)]
        [DefaultValue(typeof(AggregationOptions), "Current")]
        public AggregationOptions BranchVelocity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Flow area at reach segments")]
        [PropertyOrder(23)]
        [DisplayName(WaterFlowModelParameterNames.BranchFlowArea)]
        [DefaultValue(typeof(AggregationOptions), "Current")]
        public AggregationOptions BranchFlowArea
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Hydraulic radius at reach segments")]
        [PropertyOrder(24)]
        [DisplayName(WaterFlowModelParameterNames.BranchHydraulicRadius)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchFlowHydrad
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowHydrad, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowHydrad, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Salt dispersion at the computational gridpoints; salt must be enabled.")]
        [PropertyOrder(25)]
        [DisplayName(WaterFlowModelParameterNames.BranchSaltDispersion)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SaltDispersion
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

//        [Category(BranchCategoryName)]
//        [Description("Energy head level at reach segments")]
//        [PropertyOrder(30)]
//        [DisplayName(WaterFlowModelParameterNames.BranchEnergyHeadLevel)]
//        [DefaultValue(typeof(AggregationOptions), "Current")]
//        public AggregationOptions EnergyHead
//        {
//            get { return data.OutputSettings.EnergyHeadLevel; }
//            set { data.OutputSettings.EnergyHeadLevel = value; }
//        }

        [Category(BranchCategoryName)]
        [Description("Conveyance at reach segments")]
        [PropertyOrder(25)]
        [DisplayName(WaterFlowModelParameterNames.BranchConveyance)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions FlowConv
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Roughness as Chezy at reach segments")]
        [DisplayName(WaterFlowModelParameterNames.BranchRoughness)]
        [PropertyOrder(26)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchRoughness
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowChezy, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowChezy, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [PropertyOrder(27)]
        [Category(BranchCategoryName)]
        [Description("Water level gradient at reach segments")]
        [DisplayName(WaterFlowModelParameterNames.BranchWaterLevelGradient)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchWaterLevelGradient
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevelGradient, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevelGradient, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Froude number at reach segments")]
        [DisplayName(WaterFlowModelParameterNames.BranchFroudeNumber)]
        [PropertyOrder(28)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchFroudeNumber
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(BranchCategoryName)]
        [Description("Results for Discharge, Flow area, Hydraulic radius and Chezy at Main channel, Floodplain1 and Floodplain2")]
        [DisplayName(WaterFlowModelParameterNames.BranchSubsectionParameters)]
        [PropertyOrder(29)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions BranchSubsectionsNumber
        {
            get { return data.OutputSettings.SubSections; }
            set { data.OutputSettings.SubSections = value; }
        }


        # endregion

        # region Structures

        [Category(StructureCategoryName)]
        [Description("Discharge at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureDischarge)]
        [PropertyOrder(41)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Velocity at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureVelocity)]
        [PropertyOrder(42)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureVelocity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Flow area at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureFlowArea)]
        [PropertyOrder(43)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureFlowArea
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.FlowArea, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.FlowArea, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Crest level at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureCrestLevel)]
        [PropertyOrder(44)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureCrestLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CrestLevel, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CrestLevel, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Crest width at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureCrestWidth)]
        [PropertyOrder(45)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureCrestWidth
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.CrestWidth, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.CrestWidth, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Gate level at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureGateLevel)]
        [PropertyOrder(46)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureGateLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GateLowerEdgeLevel, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GateLowerEdgeLevel, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Gate opening at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureOpeningHeight)]
        [PropertyOrder(47)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureOpeningHeight
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.GateOpeningHeight, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.GateOpeningHeight, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Valve opening at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureValveOpening)]
        [PropertyOrder(48)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureValveOpening
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.ValveOpening, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.ValveOpening, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Water level up at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureWaterlevelUp)]
        [PropertyOrder(49)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureWaterLevelUp
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Water level down at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureWaterlevelDown)]
        [PropertyOrder(50)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureWaterLevelDown
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Head difference at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureHeadDifference)]
        [PropertyOrder(51)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureHeadDifference
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Head, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Head, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Pressure difference at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructurePressureDifference)]
        [PropertyOrder(52)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePressureDifference
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PressureDifference, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PressureDifference, ElementSet.Structures).AggregationOptions = value; }
        }

        [Category(StructureCategoryName)]
        [Description("Water level at crest at Structures")]
        [DisplayName(WaterFlowModelParameterNames.StructureWaterLevelAtCrest)]
        [PropertyOrder(53)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructureWaterLevelAtCrest
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevelAtCrest, ElementSet.Structures).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevelAtCrest, ElementSet.Structures).AggregationOptions = value; }
        }
        
        # endregion

        # region Observation points

        [Category(ObservationPointsCategoryName)]
        [Description("Water level at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointWaterLevel)]
        [PropertyOrder(71)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointWaterLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Water depth at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointWaterDepth)]
        [PropertyOrder(72)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointWaterDepth
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Discharge at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointDischarge)]
        [PropertyOrder(74)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Velocity at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointVelocity)]
        [PropertyOrder(75)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointVelocity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Salinity at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointSaltConcentration)]
        [PropertyOrder(76)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointSaltConcentration
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Salt dispersion at observation points; salt must be enabled.")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointSaltDispersion)]
        [PropertyOrder(77)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointSaltDispersion
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Water volume at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointVolume)]
        [PropertyOrder(78)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.Observations).AggregationOptions = value; }
        }

        [Category(ObservationPointsCategoryName)]
        [Description("Temperature at observation points")]
        [DisplayName(WaterFlowModelParameterNames.ObservationPointTemperature)]
        [PropertyOrder(79)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions ObservationPointTemperature
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Temperature, ElementSet.Observations).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Temperature, ElementSet.Observations).AggregationOptions = value; }
        }

        #endregion

        #region Pumps
        
        [Category(PumpsCategoryName)]
        [Description("Pump Suction Side")]
        [DisplayName(WaterFlowModelParameterNames.PumpSuctionSide)]
        [PropertyOrder(55)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpSuctionSide
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.SuctionSideLevel, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.SuctionSideLevel, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Delivery Side")]
        [DisplayName(WaterFlowModelParameterNames.PumpDeliverySide)]
        [PropertyOrder(56)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpDeliverySide
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.DeliverySideLevel, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.DeliverySideLevel, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Head")]
        [DisplayName(WaterFlowModelParameterNames.PumpHead)]
        [PropertyOrder(57)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpHead
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpHead, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpHead, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Stage")]
        [DisplayName(WaterFlowModelParameterNames.PumpStage)]
        [PropertyOrder(58)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpStage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.ActualPumpStage, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.ActualPumpStage, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Capacity")]
        [DisplayName(WaterFlowModelParameterNames.PumpCapacity)]
        [PropertyOrder(59)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpCapacity
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpCapacity, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpCapacity, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Reduction Factor")]
        [DisplayName(WaterFlowModelParameterNames.PumpReductionFactor)]
        [PropertyOrder(60)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpReductionFactor
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.ReductionFactor, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.ReductionFactor, ElementSet.Pumps).AggregationOptions = value; }
        }

        [Category(PumpsCategoryName)]
        [Description("Pump Discharge")]
        [DisplayName(WaterFlowModelParameterNames.PumpDischarge)]
        [PropertyOrder(61)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions StructurePumpDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps).AggregationOptions = value; }
        }

        #endregion

        #region Retentions

        [Category(RetentionsCategoryName)]
        [Description("Water level at retention locations")]
        [DisplayName(WaterFlowModelParameterNames.RetentionWaterLevel)]
        [PropertyOrder(79)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions RetentionWaterLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Retentions).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Retentions).AggregationOptions = value; }
        }

        [Category(RetentionsCategoryName)]
        [Description("Water volume at retention locations")]
        [DisplayName(WaterFlowModelParameterNames.RetentionVolume)]
        [PropertyOrder(80)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions RetentionVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.Retentions).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Volume, ElementSet.Retentions).AggregationOptions = value; }
        }

        # endregion
        
        # region Lateral sources

        [Category(LateralSourcesCategoryName)]
        [Description("Discharge at lateral source.")]
        [DisplayName(WaterFlowModelParameterNames.LateralDischarge)]
        [PropertyOrder(81)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LateralSourceDischarge
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions = value; }
        }

        [Category(LateralSourcesCategoryName)]
        [Description("Water level at lateral source.")]
        [DisplayName(WaterFlowModelParameterNames.LateralWaterLevel)]
        [PropertyOrder(82)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions LateralSourceWaterLevel
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals).AggregationOptions = value; }
        }

        # endregion
        
        [Category(WaqOutputCategoryName)]
        [Description("Write a waterquality hyd file")]
        [DisplayName("\tWrite hyd file")]
        [PropertyOrder(83)]
        public bool WriteHydFile
        {
            get { return data.HydFileOutput; }
            set { data.HydFileOutput = value; }
        }

        #region Simulation Info

        [Category(SimulationInfoCategoryName)]
        [Description("Simulation info negative depth")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoNegativeDepthDisplayName)]
        [PropertyOrder(91)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoNegativeDepth
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NegativeDepth, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NegativeDepth, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Number of iterations")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoNumberOfIterationsDisplayName)]
        [PropertyOrder(92)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoNumberOfIterations
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.NoIteration, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.NoIteration, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Time step estimation")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoTimeStepEstimationDisplayName)]
        [PropertyOrder(93)]
        [DefaultValue(typeof(AggregationOptions), "None")] 
        public AggregationOptions SimulationInfoTimeStepEstimation
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.TimeStepEstimation, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.TimeStepEstimation, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Total Volume")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalVolume)]
        [PropertyOrder(94)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterSystemTotalVolume
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalVolume, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalVolume, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Volume Error")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceVolumeError)]
        [PropertyOrder(95)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterSystemVolumeError
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalError, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalError, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Storage")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalStorage)]
        [PropertyOrder(96)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterSystemTotalStorage
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalStorage, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalStorage, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Boundaries In")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesIn)]
        [PropertyOrder(97)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceBoundariesIn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Boundaries Out")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesOut)]
        [PropertyOrder(98)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceBoundariesOut
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Boundaries Total")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesTotal)]
        [PropertyOrder(99)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceBoundariesTotal
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral Discharge In")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeIn)]
        [PropertyOrder(100)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateralDischargeIn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral Discharge Out")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeOut)]
        [PropertyOrder(101)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateralDischargeOut
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral Discharge Total")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeTotal)]
        [PropertyOrder(102)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateralDischargeTotal
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral 1D2D Discharge In")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeIn)]
        [PropertyOrder(103)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateral1D2DDischargeIn
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral 1D2D Discharge Out")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeOut)]
        [PropertyOrder(104)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateral1D2DDischargeOut
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [Category(SimulationInfoCategoryName)]
        [Description("Waterbalance1D Lateral 1D2D Discharge Total")]
        [DisplayName(WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeTotal)]
        [PropertyOrder(105)]
        [DefaultValue(typeof(AggregationOptions), "None")]
        public AggregationOptions SimulationInfoWaterBalanceLateral1D2DDischargeTotal
        {
            get { return data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide).AggregationOptions; }
            set { data.OutputSettings.GetEngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide).AggregationOptions = value; }
        }

        #endregion
    }
}