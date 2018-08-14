using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DDataAccessListener : DataAccessListenerBase
    {
        private bool firstNetwork = true;
        private bool firstFlowModel = true;

        public override object Clone()
        {
            return new WaterFlowModel1DDataAccessListener {ProjectRepository = ProjectRepository};
        }

        public override void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            // nhibernate performance optimizations:
            if (entity is Project)
            {
                firstNetwork = true;
                firstFlowModel = true;
            }
            else if (firstFlowModel && entity is WaterFlowModel1D)
            {
                ProjectRepository.PreLoad<Parameter>(fp => fp.Value);

                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.Feature);
                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.SeriesDataItem);
                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.FlowConstantDataItem);

                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.Feature);
                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.SeriesDataItem);
                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.FlowConstantDataItem);

                firstFlowModel = false;
            }
            else if (firstNetwork && entity is HydroNetwork)
            {
                ProjectRepository.PreLoad<HydroNode>(n => n.Links);
                ProjectRepository.PreLoad<LateralSource>(n => n.Links);
                ProjectRepository.PreLoad<ICompositeBranchStructure>(cbs => cbs.Structures);
                firstNetwork = false;
            }
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var hydroNetwork = entity as HydroNetwork;
            if (hydroNetwork != null)
            {
                BackwardCompatibilityFix_SOBEK3_1392(hydroNetwork);
            }

            if (entity is WaterFlowModel1D)
            {
                // do not access WaterFlowModel1D properties directly! it can influence loading order (lazy loading)
                // instead we use the 'state'

                var propertyNamesList = propertyNames.ToList();
                var dataItemsIndex = propertyNamesList.IndexOf("DataItems");
                var outputSettingsIndex = propertyNamesList.IndexOf("OutputSettings");

                if (dataItemsIndex < 0 || outputSettingsIndex < 0) return;

                // state and propertyNames will always be the same length
                var stateDataItems = state[dataItemsIndex] as IEnumerable<IDataItem>;
                var stateOutputSettings = state[outputSettingsIndex] as WaterFlowModel1DOutputSettingData;

                if (stateDataItems == null || stateOutputSettings == null) return;

                SyncAggregationOptionsForExistingOutputCoverages(stateDataItems, stateOutputSettings.EngineParameters);
            }
        }

        /// <summary>
        /// As part of Issue: SOBEK3-1392, we found that some models exist with CrossSections that have CrossSecitonDefinitions with zero Sections
        /// This fix ensures that all CrossSectionDefinitions have at least one Section ('Main' if there is no other)
        /// </summary>
        /// <param name="hydroNetwork"></param>
        private static void BackwardCompatibilityFix_SOBEK3_1392(IHydroNetwork hydroNetwork)
        {
            // SOBEK3-1392: CrossSectionDefinitions without any sections must have at least 'Main'
            var crossSectionDefinitionsWithoutSections = hydroNetwork.CrossSections
                .Select(cs => cs.Definition)
                .Union(hydroNetwork.SharedCrossSectionDefinitions)
                .Where(csd => csd != null && !csd.Sections.Any())
                .ToList();

            if (!crossSectionDefinitionsWithoutSections.Any()) return;
            
            var mainSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(cst => cst.Name == CrossSectionDefinition.MainSectionName);
            if (mainSectionType == null)
            {
                mainSectionType = new CrossSectionSectionType { Name = CrossSectionDefinition.MainSectionName };
                hydroNetwork.CrossSectionSectionTypes.Add(mainSectionType);
            }

            foreach (var definition in crossSectionDefinitionsWithoutSections)
            {
                definition.Sections.Add(new CrossSectionSection()
                {
                    SectionType = mainSectionType
                });
            }
        }

        /// <summary>
        /// As part of Issue: SOBEK3-1438, we found that some models exist with output coverages that are now out-of-sync with the corresponding aggregation option
        /// This fix ensures that all aggregation options for existing output coverages are synchronised
        /// </summary>
        /// <param name="dataItems"></param>
        /// <param name="engineParameters"></param>
        private static void SyncAggregationOptionsForExistingOutputCoverages(IEnumerable<IDataItem> dataItems, IEnumerable<EngineParameter> engineParameters)
        {
            var existingOutputCoverageDataItems = dataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction);

            var parametersList = engineParameters.ToList();

            foreach (var dataItem in existingOutputCoverageDataItems)
            {
                var matchingEngineParameter = parametersList.FirstOrDefault(ep => ep.Name == dataItem.Tag);
                var existingOutputCoverage = dataItem.Value as IFunction; // should always be the case

                if (matchingEngineParameter == null || existingOutputCoverage == null) continue;

                var firstComponent = existingOutputCoverage.Components.FirstOrDefault();
                if (firstComponent == null) continue;

                string aggregationType;
                if (!firstComponent.Attributes.TryGetValue(FunctionAttributes.AggregationType, out aggregationType))
                    continue;

                switch (aggregationType)
                {
                    case FunctionAttributes.AggregationTypes.None: // Current
                        if (matchingEngineParameter.AggregationOptions != AggregationOptions.Current)
                            matchingEngineParameter.AggregationOptions = AggregationOptions.Current;
                        break;
                    case FunctionAttributes.AggregationTypes.Average:
                        if (matchingEngineParameter.AggregationOptions != AggregationOptions.Average)
                            matchingEngineParameter.AggregationOptions = AggregationOptions.Average;
                        break;
                    case FunctionAttributes.AggregationTypes.Maximum:
                        if (matchingEngineParameter.AggregationOptions != AggregationOptions.Maximum)
                            matchingEngineParameter.AggregationOptions = AggregationOptions.Maximum;
                        break;
                    case FunctionAttributes.AggregationTypes.Minimum:
                        if (matchingEngineParameter.AggregationOptions != AggregationOptions.Minimum)
                            matchingEngineParameter.AggregationOptions = AggregationOptions.Minimum;
                        break;
                    default:
                        continue;
                }
            }
        }

    }
}