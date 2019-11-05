using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
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
                if (IsOutDatedParameterList(value, engineParameters))
                {
                    FixMissing070Role(value, engineParameters);
                    RemoveNoLongerExistingParametersFrom(value, engineParameters);
                    MergeMissingParametersInto(value, engineParameters);
                }
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

        public WaterFlowModel1DOutputSettingData()
        {
            engineParameters = NGHS.IO.DataObjects.Model1D.EngineParameters.EngineMapping();
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