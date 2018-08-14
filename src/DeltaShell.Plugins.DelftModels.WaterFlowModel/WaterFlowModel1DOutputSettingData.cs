using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public static class WaterFlowModelParameterNames
    {

        /// <summary>
        /// Location parameter names
        /// </summary>        
        public const string LocationWaterLevel = "Water level";
        public const string LocationWaterDepth = "Water depth";
        public const string LocationSurfaceArea = "Surface area";
        //public const string LocationFreeboard = "Freeboard";
        public const string LocationVolume = "Water volume";
        public const string LocationTotalArea = "Total area";
        public const string LocationTotalWidth = "Total width";
        public const string LocationSaltConcentration = "Salt concentration";
        public const string LocationTemperature = "Temperature";
        public const string LocationDensity = "Density";
        public const string LocationQTotal_1d2d = "Lateral Discharge from 2d to 1d";
        public const string LocationLateralAtNodes = "Lateral at nodes";
        public const string LocationTotalHeatFlux = "Total heat flux";
        public const string LocationRadFluxClearSky = "Radiation flux for clear sky condition";
        public const string LocationHeatLossConv = "Heat loss due to convection";
        public const string LocationNetSolarRad = "Net incident solar radiation";
        public const string LocationEffectiveBackRad = "Effective back radiation";
        public const string LocationHeatLossEvap = "Heat loss due to evaporation";
        public const string LocationHeatLossForcedEvap = "Heat loss due to forced evaporation";
        public const string LocationHeatLossFreeEvap = "Heat loss due to free evaporation";
        public const string LocationHeatLossForcedConv = "Heat loss due to forced convection";
        public const string LocationHeatLossFreeConv = "Heat loss due to free convection";

        /// <summary>
        /// Branch parameter names
        /// </summary>        
        public const string BranchDischarge = "Discharge";
        public const string BranchVelocity = "Velocity";
        public const string BranchSaltDispersion = "Salt dispersion";
        public const string BranchEnergyHeadLevel = "Energy head";
        public const string BranchFlowArea = "Flow area";
        public const string BranchHydraulicRadius = "Hydraulic radius";
        public const string BranchConveyance = "Conveyance";
        public const string BranchRoughness = "Chezy values";
        public const string BranchWaterLevelGradient = "Water level gradient";
        public const string BranchFroudeNumber = "Froude number";
        public const string BranchSubsectionParameters = "Subsection parameters";

        /// <summary>
        /// subsectie parameters; only for coverages; in propertygrid BranchSubsectionParameters is used
        /// </summary>
        public const string MainChannel = "Main ";
        public const string FloodPlain1 = "FloodPlain1 ";
        public const string FloodPlain2 = "FloodPlain2 ";
        public const string SubSectionDischarge = "Discharge";
        public const string SubSectionFlowArea = "Flow area";
        public const string SubSectionFlowWidth = "Flow width";
        public const string SubSectionHydraulicRadius = "Hydraulic radius";
        public const string SubSectionRoughness = "Chezy values";

        /// <summary>
        /// Structure parameter names
        /// </summary>        
        public const string StructureDischarge = "Discharge (s)";
        public const string StructureVelocity = "Velocity (s)";
        public const string StructureFlowArea = "Flow area (s)";
        public const string StructurePressureDifference = "Pressure difference (s)";
        public const string StructureCrestLevel = "Crest level (s)";
        public const string StructureCrestWidth = "Crest width (s)";
        public const string StructureGateLevel = "Gate lower edge level (s)";
        public const string StructureOpeningHeight = "Opening height (s)";
        public const string StructureValveOpening = "Valve opening (s)";
        public const string StructureWaterlevelUp = "Water level up (s)";
        public const string StructureWaterlevelDown = "Water level down (s)";
        public const string StructureHeadDifference = "Head Difference (s)";
        public const string StructureWaterLevelAtCrest = "Water level at crest (s)";
        public const string StructureSetPoint = "Setpoint (s)";

        /// <summary>
        /// Pumps parameter names
        /// </summary>
        public const string PumpOutput = "All output (p)";
        public const string PumpSuctionSide = "Suction side (p)";
        public const string PumpDeliverySide = "Delivery side (p)";
        public const string PumpHead = "Pump head (p)";
        public const string PumpStage = "Pump stage (p)";
        public const string PumpReductionFactor = "Reduction factor (p)";
        public const string PumpCapacity = "Capacity (p)";
        public const string PumpDischarge = "Discharge (p)";

        /// <summary>
        /// Observation Point parameter names
        /// </summary>        
        public const string ObservationPointWaterLevel = "Water level (op)";
        public const string ObservationPointWaterDepth = "Water depth (op)";
        public const string ObservationPointSurfaceArea = "Surface area (op)";
        public const string ObservationPointDischarge = "Discharge (op)";
        public const string ObservationPointVelocity = "Velocity (op)";
        public const string ObservationPointSaltConcentration = "Salt concentration (op)";
        public const string ObservationPointSaltDispersion = "Salt dispersion (op)";
        public const string ObservationPointVolume = "Water volume (op)";
        public const string ObservationPointTemperature = "Temperature (op)";

        /// <summary>
        /// Retention parameter names
        /// </summary>
        public const string RetentionWaterLevel = "Water level (rt)";
        public const string RetentionVolume = "Volume (rt)";

        /// <summary>
        /// Lateral Source parameter names
        /// </summary>        
        public const string LateralDischarge = "Discharge (l)";
        public const string LateralActualDischarge = "Actual discharge (l)";
        public const string LateralDefinedDischarge = "Defined discharge (l)";
        public const string LateralDifference = "Lateral difference (l)";
        public const string LateralWaterLevel = "Water level (l)";

        /// <summary>
        /// Finite volume (Delwaq) parameter names
        /// </summary>
        public const string FiniteVolumeGridType = "Grid type (finite volume)";
        public const string FiniteVolumeDischarge = "Discharge (finite volume)";
        public const string FiniteVolumeVolume = "Volume (finite volume)";
        public const string FiniteVolumeVelocity = "Velocity (finite volume)";
        public const string FiniteVolumeSurface = "Surface (finite volume)";
        public const string FiniteVolumeChezy = "Chezy (finite volume)";
        public const string FiniteVolumeQLats = "Discharge lateral sources (finite volume)";

        /// <summary>
        /// SimulationInfo string constants
        /// </summary>        
        public const string SimulationInfoNegativeDepthDisplayName = "Negative depth";
        public const string SimulationInfoNumberOfIterationsDisplayName = "Number of iterations";
        public const string SimulationInfoTimeStepEstimationDisplayName = "Time step estimation";

        public const string SimulationInfoWaterBalanceTotalVolume = "Waterbalance1D_TotalVolume"; 
        public const string SimulationInfoWaterBalanceVolumeError = "Waterbalance1D_VolumeError";
        public const string SimulationInfoWaterBalanceTotalStorage = "Waterbalance1D_Storage";

        public const string SimulationInfoWaterBalanceBoundariesIn = "Waterbalance1D_Boundaries_In";
        public const string SimulationInfoWaterBalanceBoundariesOut = "Waterbalance1D_Boundaries_Out";
        public const string SimulationInfoWaterBalanceBoundariesTotal = "Waterbalance1D_Boundaries_Total";

        public const string SimulationInfoWaterBalanceLateralDischargeIn = "Waterbalance1D_LateralDischarge_In";
        public const string SimulationInfoWaterBalanceLateralDischargeOut = "Waterbalance1D_LateralDischarge_Out";
        public const string SimulationInfoWaterBalanceLateralDischargeTotal = "Waterbalance1D_LateralDischarge_Total";

        public const string SimulationInfoWaterBalanceLateral1D2DDischargeIn    = "Waterbalance1D_Lateral1D2DDischarge_In";
        public const string SimulationInfoWaterBalanceLateral1D2DDischargeOut   = "Waterbalance1D_Lateral1D2DDischarge_Out";
        public const string SimulationInfoWaterBalanceLateral1D2DDischargeTotal = "Waterbalance1D_Lateral1D2DDischarge_Total";
    }

    public static class WaterFlowParametersCategories
    {
        public const string Weirs = "weirs";
        public const string ObservationPoints = "observations";
        public const string Culverts = "culverts";
        public const string Pumps = "pumps";
        public const string BoundaryConditions = "boundaries";
        public const string Laterals = "laterals";
        public const string Retentions = "retentions";
    }

    /// <summary>
    /// Output settings of the WaterFlowModel1D. 
    /// WaterFlowModel1DOutputSettingData gets initialized with a hardcoded array of engineparameters. These hold 
    /// the available parameters supported by the current version of ModelApi.
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class WaterFlowModel1DOutputSettingData : Unique<long>, ICopyFrom
    {
        private IEventedList<EngineParameter> engineParameters;
        private bool rebuildRequired;
        public virtual IEventedList<EngineParameter> EngineParameters
        {
            get
            {
                return engineParameters;
            }
            private set // mapped to NHibernate storage!. 
            {
                CheckInputAndOutputLateralSources(value, engineParameters);
                if (IsOutDatedParameterList(value, engineParameters))
                {
                    FixMissing070Role(value, engineParameters);
                    RemoveNoLongerExistingParametersFrom(value, engineParameters);
                    MergeMissingParametersInto(value, engineParameters);
                }
                CheckInAndOutDischarges(value, engineParameters);
                engineParameters = value;
            } 
        }

        public static readonly Dictionary<string, string> CategoryMap = new Dictionary<string, string>()
        {
            {"GridpointsOnBranches", ModelDefinitionsRegion.ResultsNodesHeader},
            {"ReachSegElmSet", ModelDefinitionsRegion.ResultsBranchesHeader},
            {"Structures", ModelDefinitionsRegion.ResultsStructuresHeader},
            {"Pumps", ModelDefinitionsRegion.ResultsPumpsHeader},
            {"Observations", ModelDefinitionsRegion.ResultsObservationsPointsHeader},
            {"Laterals", ModelDefinitionsRegion.ResultsLateralsHeader},
            {"Retentions", ModelDefinitionsRegion.ResultsRetentionsHeader},
            {"ModelWide", ModelDefinitionsRegion.ResultsWaterBalanceHeader}
        };

        /// <summary>
        /// backward compatiblity 0.7..bleh
        /// </summary>
        /// <param name="nhValues"></param>
        /// <param name="currentParameters"></param>
        private static void FixMissing070Role(IEventedList<EngineParameter> nhValues, IEventedList<EngineParameter> currentParameters)
        {
            foreach(var value in nhValues)
            {
                if (value.Role != 0)
                    continue;

                var currentEquivalent = currentParameters.FirstOrDefault(p => p.Name == value.Name);

                if (currentEquivalent != null)
                {
                    TypeUtils.SetPrivatePropertyValue(value, "Role", currentEquivalent.Role);
                }
            }
        }

        /// <summary>
        /// This method exists for backward compatibility reasons: any parameters later added but not in the old file format are added 
        /// (but not on set)
        /// </summary>
        private static bool IsOutDatedParameterList(IEventedList<EngineParameter> nhValues, IEnumerable<EngineParameter> currentParameters)
        {
            if (nhValues != null && nhValues.Count > 0) //something real was set
            {
                if (currentParameters.Any(cp => nhValues.All(nhcp => nhcp.Name != cp.Name))) //any missing parameters?
                {
                    return true;
                }
                if (nhValues.Any(nhcp => currentParameters.All(cp => cp.Name != nhcp.Name))) //any no longer existing parameters?
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckInAndOutDischarges(IEventedList<EngineParameter> nhValues, IEventedList<EngineParameter> currentParameters)
        {
            List<EngineParameter> copyOfNhValues = nhValues.ToList();
            IEnumerable<EngineParameter> inOutDischarges = copyOfNhValues.Where(nhV =>
                (nhV.ElementSet == ElementSet.Laterals) &&
                (nhV.QuantityType == QuantityType.Discharge) &&
                (nhV.Role == (DataItemRole.Input | DataItemRole.Output)));

            foreach (EngineParameter inOutDischarge in inOutDischarges)
            {
                nhValues.Remove(inOutDischarge);
                EngineParameter inDischarge = currentParameters.First(ep =>
                                    (ep.ElementSet == ElementSet.Laterals) &&
                                    (ep.QuantityType == QuantityType.Discharge) &&
                                    (ep.Role == (DataItemRole.Input)));
                nhValues.Add(inDischarge);
                EngineParameter outDischarge = currentParameters.First(ep =>
                                    (ep.ElementSet == ElementSet.Laterals) &&
                                    (ep.QuantityType == QuantityType.Discharge) &&
                                    (ep.Role == (DataItemRole.Output)));
                outDischarge.AggregationOptions = inOutDischarge.AggregationOptions;
                nhValues.Add(outDischarge);
            }
        }

        private static void MergeMissingParametersInto(IEventedList<EngineParameter> nhValues, IEnumerable<EngineParameter> completeList)
        {
            //changes were detected: add missing items
            foreach (var parameter in completeList)
            {
                var legacyEquivalent = nhValues.FirstOrDefault(nhp => nhp.Name == parameter.Name);
                if (legacyEquivalent == null)
                {
                    nhValues.Add(parameter);
                }
            }
        }

        private static void RemoveNoLongerExistingParametersFrom(IEventedList<EngineParameter> nhValues, IEnumerable<EngineParameter> completeList)
        {
            //changes were detected: remove no longer existing items

            List<EngineParameter> copyOfNhValues = nhValues.ToList();
            foreach (var parameter in copyOfNhValues)
            {
                var completeListEquivalent = completeList.FirstOrDefault(nhp => nhp.Name == parameter.Name);
                if (completeListEquivalent == null)
                {
                    nhValues.Remove(parameter);
                }
            }
        }

        private static void CheckInputAndOutputLateralSources(IEventedList<EngineParameter> nhValues, IEnumerable<EngineParameter> completeList)
        {
            //changes were detected: remove no longer existing items

            IEnumerable<EngineParameter> inOutDischarges = nhValues.Where(nhV =>
                nhV.QuantityType == QuantityType.Discharge &&
                nhV.ElementSet == ElementSet.Laterals &&
                (nhV.Role == (DataItemRole.Output | DataItemRole.Input))).ToList();
            foreach (EngineParameter inOutDischarge in inOutDischarges)
            {
                nhValues.Remove(inOutDischarge);
                EngineParameter inDischarge = completeList.First(nhV =>
                    nhV.QuantityType == QuantityType.Discharge &&
                    nhV.ElementSet == ElementSet.Laterals &&
                    nhV.Role == DataItemRole.Input);
                nhValues.Add(inDischarge);
                EngineParameter outDischarge = completeList.First(nhV =>
                    nhV.QuantityType == QuantityType.Discharge &&
                    nhV.ElementSet == ElementSet.Laterals &&
                    nhV.Role == DataItemRole.Output);
                outDischarge.AggregationOptions = inOutDischarge.AggregationOptions;
                nhValues.Add(outDischarge);
            }
        }

        public WaterFlowModel1DOutputSettingData()
        {
            engineParameters = WaterFlowModel.EngineParameters.EngineMapping();
            var defaultTimeStep = new TimeSpan(0, 1, 0, 0);
            
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            GridOutputTimeStep = defaultTimeStep;
            StructureOutputTimeStep = defaultTimeStep;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }
        
        public virtual TimeSpan GridOutputTimeStep { get; set; }
        public virtual TimeSpan StructureOutputTimeStep { get; set; }

        public virtual IList<DelftIniCategory> GenerateAdvancedOptionsValues()
        {
            IList<DelftIniCategory> settingsGroups = new List<DelftIniCategory>();
            var groupedKey = engineParameters.GroupBy(ep => ep.ElementSet);
            foreach (var gEp in groupedKey)
            {
                DelftIniCategory category = new DelftIniCategory(getCategoryName(gEp.Key.ToString()));
                foreach (var ep in gEp)
                {
                    getParameterCategory(ep, category);
                }
                var orderedCats = category.Properties.OrderBy(p => p.Name).ToList();
                category.Properties = orderedCats;
                settingsGroups.Add(category);
            }
            return settingsGroups;
        }

        private string getCategoryName(string categoryName)
        {
            string newName = "";
            if (!CategoryMap.TryGetValue(categoryName, out newName))
            {
                return categoryName;
            }
            return newName;
        }

        private void getParameterCategory(EngineParameter ep, DelftIniCategory category)
        {
            if ((ep.Role & DataItemRole.Output) == DataItemRole.Output)
            {
                category.AddProperty(ep.QuantityType.ToString(), Enum.GetName(typeof(AggregationOptions), ep.AggregationOptions), ep.ElementSet + " " + ep.Role + " " + ep.Name);
            }
        }

        public virtual EngineParameter GetEngineParameter(QuantityType quantityType, ElementSet elementSet, DataItemRole role)
        {
            return EngineParameters.FirstOrDefault(m => m.QuantityType == quantityType && m.ElementSet == elementSet && (m.Role & role) == role);
        }

        public virtual EngineParameter GetEngineParameter(QuantityType quantityType, ElementSet elementSet)
        {
            return EngineParameters.FirstOrDefault(m => m.QuantityType == quantityType && m.ElementSet == elementSet);
        }

        [NoNotifyPropertyChange]
        public virtual AggregationOptions LocationWaterLevel
        {
            get { return GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions LocationWaterDepth
        {
            get { return GetEngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { GetEngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions LocationTemperature
        {
            get { return GetEngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions ObservationPointTemperature
        {
            get { return GetEngineParameter(QuantityType.Temperature, ElementSet.Observations).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Temperature, ElementSet.Observations).AggregationOptions = value; }
        }

        [NoNotifyPropertyChange]
        public virtual AggregationOptions BranchDischarge
        {
            get { return GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions BranchVelocity
        {
            get { return GetEngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions BranchFlowArea
        {
            get { return GetEngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet).AggregationOptions; }
            set { GetEngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions LocationSaltConcentration
        {
            get { return GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceTotalVolume
        {
            get { return GetEngineParameter(QuantityType.BalVolume, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalVolume, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceVolumeError
        {
            get { return GetEngineParameter(QuantityType.BalError, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalError, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceStorage
        {
            get { return GetEngineParameter(QuantityType.BalStorage, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalStorage, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceBoundariesIn
        {
            get { return GetEngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceBoundariesOut
        {
            get { return GetEngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceBoundariesTotal
        {
            get { return GetEngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateralDischargeIn
        {
            get { return GetEngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateralDischargeOut
        {
            get { return GetEngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateralDischargeTotal
        {
            get { return GetEngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateral1D2DDischargeIn
        {
            get { return GetEngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateral1D2DDischargeOut
        {
            get { return GetEngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide).AggregationOptions = value; }
        }
        [NoNotifyPropertyChange]
        public virtual AggregationOptions WaterBalanceLateral1D2DDischargeTotal
        {
            get { return GetEngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide).AggregationOptions; }
            set { GetEngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide).AggregationOptions = value; }
        }

        [NoNotifyPropertyChange]
        public virtual AggregationOptions SubSections
        {
            get
            {
                // all 15 settings are equal
                return GetEngineParameter(QuantityType.DischargeMain, ElementSet.ReachSegElmSet).AggregationOptions;
            }
            set
            {
                // Discharge, Flow area, Hydraulic radius and Chezy
                // Main Channel
                GetEngineParameter(QuantityType.DischargeMain, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.ChezyMain, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.AreaMain, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.WidthMain, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.HydradMain, ElementSet.ReachSegElmSet).AggregationOptions = value; 

                // Floodplain1 
                GetEngineParameter(QuantityType.DischargeFP1, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.ChezyFP1, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.AreaFP1, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.WidthFP1, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.HydradFP1, ElementSet.ReachSegElmSet).AggregationOptions = value;

                // Floodplain2
                GetEngineParameter(QuantityType.DischargeFP2, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.ChezyFP2, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.AreaFP2, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.WidthFP2, ElementSet.ReachSegElmSet).AggregationOptions = value;
                GetEngineParameter(QuantityType.HydradFP2, ElementSet.ReachSegElmSet).AggregationOptions = value;
            }
        }

        public virtual void CopyFrom(object source)
        {
            var sourceSettings = (WaterFlowModel1DOutputSettingData)source;

            // merge parameters (required during backward compatibility)
            for (int i=0; i<EngineParameters.Count; i++)
            {
                var sourceParameter = sourceSettings.EngineParameters.FirstOrDefault(
                        s =>
                        s.Name == EngineParameters[i].Name && s.Role == EngineParameters[i].Role &&
                        s.QuantityType == EngineParameters[i].QuantityType);

                if (source != null)
                {
                    EngineParameters[i] = (EngineParameter) sourceParameter.Clone();
                }
            }

            GridOutputTimeStep = sourceSettings.GridOutputTimeStep;
            StructureOutputTimeStep = sourceSettings.StructureOutputTimeStep;
        }
    }
}