using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelMerge
{
    /// <summary>
    /// Class for merging two models together.
    /// </summary>
    public static class ModelMerger
    {
        private const string importedSuffix = "-imported";
        
        private static readonly ILog log = LogManager.GetLogger(typeof(ModelMerger));
        
        /// <summary>
        /// Merges a <see cref="WaterFlowFMModel"/> into an existing <see cref="WaterFlowFMModel"/>.
        /// </summary>
        /// <remarks>
        /// This method assumes there are no duplicate names in the models. 
        /// </remarks>
        /// <param name="originalModel">The original <see cref="WaterFlowFMModel"/>.</param>
        /// <param name="newModel">The new <see cref="WaterFlowFMModel"/> to be merged in the original model.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public static void Merge(WaterFlowFMModel originalModel, WaterFlowFMModel newModel)
        {
            Ensure.NotNull(originalModel, nameof(originalModel));
            Ensure.NotNull(newModel, nameof(newModel));

            try
            {
                IHydroNetwork originalNetwork = originalModel.Network;
                IHydroNetwork newNetwork = newModel.Network;
                
                PrepareCrossSectionsForMerge(originalNetwork.CrossSectionSectionTypes, newNetwork);
                MergeNetworks(originalNetwork, newNetwork);
                MergeNetworkDiscretizations(originalModel, newModel);
                
                WaterFlowFMModel mergedModel = originalModel;
                UpdateChannelFrictionDefinitions(mergedModel, newModel);
                UpdateChannelInitialConditionDefinitions(mergedModel, newModel);
                Update1DBoundaryConditions(mergedModel, newModel);
            }
            catch(Exception e)
            {
                if (e is ArgumentException
                    || e is NullReferenceException)
                {
                    log.Error("Could not merge the provided models.");                    
                }
            }
        }
        
        private static void PrepareCrossSectionsForMerge(IEnumerable<CrossSectionSectionType> originalNetworkCrossSectionSectionTypes, IHydroNetwork newNetwork)
        {
            var originalNetworkCrossSectionSectionTypesLookup = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (CrossSectionSectionType originalNetworkCrossSectionSectionType in originalNetworkCrossSectionSectionTypes)
            {
                originalNetworkCrossSectionSectionTypesLookup.Add(originalNetworkCrossSectionSectionType.Name);
            }
            
            foreach (CrossSectionSectionType newCrossSectionSectionType in newNetwork.CrossSectionSectionTypes)
            {
                if (originalNetworkCrossSectionSectionTypesLookup.Contains(newCrossSectionSectionType.Name))
                {
                    newCrossSectionSectionType.Name += importedSuffix;
                }
            }
        }

        private static void MergeNetworks(IHydroNetwork originalNetwork, IHydroNetwork newNetwork)
        {
            originalNetwork.CrossSectionSectionTypes.AddRange(newNetwork.CrossSectionSectionTypes);
            originalNetwork.Branches.AddRange(newNetwork.Branches);
            originalNetwork.Nodes.AddRange(newNetwork.Nodes);
            originalNetwork.SharedCrossSectionDefinitions.AddRange(newNetwork.SharedCrossSectionDefinitions);
        }
        
        private static void MergeNetworkDiscretizations(IModelWithNetwork originalModel, IModelWithNetwork newModel)
        {
            originalModel.NetworkDiscretization.UpdateNetworkLocations(newModel.NetworkDiscretization.Locations.Values);
        }
        
        private static void UpdateChannelFrictionDefinitions(WaterFlowFMModel mergedModel, WaterFlowFMModel newModel)
        {
            Dictionary<string, ChannelFrictionDefinition> mergedChannelFrictionDefinitionLookup =
                mergedModel.ChannelFrictionDefinitions.ToDictionary(c => c.Channel.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (ChannelFrictionDefinition newModelChannelFrictionDefinition in newModel.ChannelFrictionDefinitions)
            {
                if (!mergedChannelFrictionDefinitionLookup.TryGetValue(newModelChannelFrictionDefinition.Channel.Name, out ChannelFrictionDefinition originalChannelFrictionDefinition))
                {
                    continue;
                }

                mergedModel.ChannelFrictionDefinitions.Remove(originalChannelFrictionDefinition);
                mergedModel.ChannelFrictionDefinitions.Add(newModelChannelFrictionDefinition);
            }
        }
        
        private static void UpdateChannelInitialConditionDefinitions(WaterFlowFMModel mergedModel, WaterFlowFMModel newModel)
        {
            Dictionary<string, ChannelInitialConditionDefinition> mergedChannelInitialConditionDefinitionLookup =
                mergedModel.ChannelInitialConditionDefinitions.ToDictionary(c => c.Channel.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (ChannelInitialConditionDefinition newModelChannelInitialConditionDefinition in newModel.ChannelInitialConditionDefinitions)
            {
                if (!mergedChannelInitialConditionDefinitionLookup.TryGetValue(newModelChannelInitialConditionDefinition.Channel.Name, out ChannelInitialConditionDefinition originalChannelInitialConditionDefinition))
                {
                    continue;
                }

                mergedModel.ChannelInitialConditionDefinitions.Remove(originalChannelInitialConditionDefinition);
                mergedModel.ChannelInitialConditionDefinitions.Add(newModelChannelInitialConditionDefinition);
            }
        }
        
        private static void Update1DBoundaryConditions(IWaterFlowFMModel mergedModel, IWaterFlowFMModel newModel)
        {
            Dictionary<string, Model1DBoundaryNodeData> merged1DBoundaryConditionLookup =
                mergedModel.BoundaryConditions1D.ToDictionary(bc => bc.Node.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (Model1DBoundaryNodeData newBoundaryCondition in newModel.BoundaryConditions1D)
            {
                if (!merged1DBoundaryConditionLookup.TryGetValue(newBoundaryCondition.Node.Name, out Model1DBoundaryNodeData originalBoundaryCondition))
                {
                    mergedModel.BoundaryConditions1D.Add(newBoundaryCondition);
                    continue;
                }

                mergedModel.BoundaryConditions1D.Remove(originalBoundaryCondition);
                mergedModel.BoundaryConditions1D.Add(newBoundaryCondition);
            }
        }
    }
}