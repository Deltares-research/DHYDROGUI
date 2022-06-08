using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DWriter
    {
        public const string NODE_FILE_NAME = "nodeFile.ini";
        public const string OBS_FILE_NAME = "obsFile1D_obs.ini";
        public const string OBS_CRS_FILE_NAME = "obsCrsFile1D_crs.ini";
        public const string CROSS_SECTION_DEFINITION_FILE_NAME = "crsdef.ini";
        public const string CROSS_SECTION_LOCATION_FILE_NAME = "crsloc.ini";
        public const string STRUCTURES_FILE_NAME = "structures.ini";
        public const string INITIAL_CONDITIONS_FILE_NAME = "initialFields.ini";

        public static void Write1D2DFeatures(
            string targetMduFilePath, 
            WaterFlowFMModelDefinition modelDefinition, 
            IHydroNetwork network, 
            HydroArea area, 
            IEnumerable<RoughnessSection> roughnessSections, 
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions,
            IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            WriteNodeFile(targetMduFilePath, modelDefinition, network);
            WriteBranchFile(targetMduFilePath, modelDefinition, network.Branches);
            WriteRoutesFile(targetMduFilePath, network.Routes);
            WriteCrossSectionFiles(targetMduFilePath, modelDefinition, network, channelFrictionDefinitions);
            WriteObservationPointsFiles(targetMduFilePath, modelDefinition, network);
            WriteStructuresFiles(targetMduFilePath, modelDefinition, network, area);
            WriteRoughnessFiles(targetMduFilePath, modelDefinition, roughnessSections, channelFrictionDefinitions);
            WriteInitialConditionFiles(targetMduFilePath, modelDefinition, channelInitialConditionDefinitions, network);
        }

        private static void WriteObservationPointsFiles(string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var obsFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, OBS_FILE_NAME);
            FileUtils.DeleteIfExists(obsFilePath);

            var obsCrsFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, OBS_CRS_FILE_NAME);
            FileUtils.DeleteIfExists(obsCrsFilePath);

            var obsPoints = network.ObservationPoints.ToList();
            if (obsPoints.Any() || network.Retentions.Any())
            {
                var currentObs = modelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
                modelDefinition.SetModelProperty(KnownProperties.ObsFile, string.IsNullOrEmpty(currentObs) ? OBS_FILE_NAME : currentObs + " " + OBS_FILE_NAME);
                LocationFileWriter.WriteFileObservationPointLocations(obsFilePath, obsPoints);
            }
            else
            {
                ///empty? dont set to empty string!! because you suck and i have to debug everything again. obspoints can also be in 2d!!
            }
        }

        private static void WriteNodeFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, NODE_FILE_NAME);
            FileUtils.DeleteIfExists(nodeFilePath);

            var compartments = network.Manholes.SelectMany(m => m.Compartments).ToList();
            if (compartments.Any() || network.Retentions.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.StorageNodeFile, NODE_FILE_NAME);
                NodeFile.Write(nodeFilePath, compartments, network.Retentions);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.StorageNodeFile, string.Empty);
            }
        }

        private static void WriteBranchFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IEnumerable<IBranch> branches)
        {
            var branchesFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, NetworkPropertiesHelper.BranchGuiFileName);
            FileUtils.DeleteIfExists(branchesFilePath);
            if (!branches.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.BranchFile, string.Empty);
            }
            BranchFile.Write(branchesFilePath, branches);
            modelDefinition.SetModelProperty(KnownProperties.BranchFile, NetworkPropertiesHelper.BranchGuiFileName);
        }

        private static void WriteRoutesFile(string targetMduFilePath, IEnumerable<Route> routes)
        {
            var routesFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, RoutesFile.RoutesFileName);
            FileUtils.DeleteIfExists(routesFilePath);

            RoutesFile.Write(routesFilePath, routes);
        }

        private static void WriteCrossSectionFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network, IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            WriteCrossSectionDefinitions(targetMduFilePath, modelDefinition, network, channelFrictionDefinitions);
            WriteCrossSectionLocations(targetMduFilePath, modelDefinition, network);
        }

        private static void WriteCrossSectionLocations(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_LOCATION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionLocationFilePath);

            
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSection?.Definition != null))
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, CROSS_SECTION_LOCATION_FILE_NAME);

                var crossSections = network.CrossSections.Concat(network.SewerConnections.Select(p=>p.CrossSection));
                LocationFileWriter.WriteFileCrossSectionLocations(crossSectionLocationFilePath, crossSections);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, string.Empty);
            }
        }

        private static void WriteCrossSectionDefinitions(
            string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_DEFINITION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionDefinitionFilePath);

            var channelFrictionDefinitionPerChannelLookup = channelFrictionDefinitions.ToDictionary(cfd => cfd.Channel, cfd => cfd);

            if (network.ContainsAnyCrossSectionDefinitions() || network.SharedCrossSectionDefinitions.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, CROSS_SECTION_DEFINITION_FILE_NAME);
                CrossSectionDefinitionFileWriter.WriteFile(crossSectionDefinitionFilePath, network,
                    WriteFrictionFromCrossSectionDefinitionsForChannel(channelFrictionDefinitionPerChannelLookup),
                    RoughnessDataRegion.SectionId.DefaultValue);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, string.Empty);
            }
        }

        private static Func<IChannel, bool> WriteFrictionFromCrossSectionDefinitionsForChannel(IReadOnlyDictionary<IChannel, ChannelFrictionDefinition> channelFrictionDefinitionPerChannelLookup)
        {
            return channel =>
            {
                var channelFrictionDefinition = channelFrictionDefinitionPerChannelLookup[channel];

                return channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections
                       || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;
            };
        }

        private static void WriteStructuresFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, HydroArea area)
        {
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, STRUCTURES_FILE_NAME);
            if (network.BranchFeatures.Any() || area.AllHydroObjects.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, STRUCTURES_FILE_NAME);

                var targetMduFilePathPropertyDefinition = new WaterFlowFMPropertyDefinition
                {
                    MduPropertyName = GuiProperties.TargetMduPath,
                    Category = GuiProperties.GUIonly,
                    FileCategoryName = GuiProperties.GUIonly,
                    DataType = typeof(string)
                };

                var targetMduFilePathProperty = new WaterFlowFMProperty(targetMduFilePathPropertyDefinition, targetMduFilePath);

                modelDefinition.AddProperty(targetMduFilePathProperty);

                IHydroRegion[] regions = { network, area };
                var referenceTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
                
                StructureFileWriter.WriteFile(structuresFilePath, 
                                              regions, 
                                              referenceTime,
                                              StructureFile.GenerateStructureCategoriesFromFmModel);
                StructureFile.WriteStructureTimFiles(regions, targetMduFilePath, referenceTime);

                modelDefinition.Properties.Remove(targetMduFilePathProperty);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, string.Empty);
            }
        }

        private static void WriteRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IEnumerable<RoughnessSection> roughnessSections, IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var directoryName = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var sections = roughnessSections.ToArray();
            var frictionFileName = Properties.Resources.Roughness_Main_Channels_Filename;
            var roughnessFileNames = sections.Select(GetRoughnessFilename);
            var roughnessFileNamesString = string.Join(";", roughnessFileNames);

            modelDefinition.SetModelProperty(KnownProperties.FrictFile, 
                string.Join(";", frictionFileName, roughnessFileNamesString));

            RoughnessType globalFrictionType = (RoughnessType)(int)modelDefinition.GetModelProperty(GuiProperties.UnifFrictTypeChannels).Value;
            double globalFrictionValue = (double)modelDefinition.GetModelProperty(GuiProperties.UnifFrictCoefChannels).Value;

            // write lanes files
            foreach (var roughnessSection in sections)
            {
                var roughnessFileName = GetRoughnessFilename(roughnessSection);
                var roughnessFilePath = System.IO.Path.Combine(directoryName, roughnessFileName);

                FileWritingUtils.ThrowIfFileNotExists(roughnessFilePath, directoryName, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));
            }

            // write channels roughness
            var frictionFilePath = System.IO.Path.Combine(directoryName, frictionFileName);
            FileWritingUtils.ThrowIfFileNotExists(frictionFilePath, directoryName, p => ChannelFrictionDefinitionFileWriter.WriteFile(p, channelFrictionDefinitions, globalFrictionType, globalFrictionValue));
            
        }

        private static void WriteInitialConditionFiles(string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions, IHydroNetwork network)
        {
            var networkIsEmpty = network == null
                                         || network.IsEdgesEmpty
                                         || network.IsVerticesEmpty;

            var directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            modelDefinition.SetModelProperty(KnownProperties.IniFieldFile, INITIAL_CONDITIONS_FILE_NAME);

            var globalInitialConditionQuantity1D = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
            double globalInitialConditionValue1D = (double)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;

            // write initialFields.ini
            var initialConditionFilePath = Path.Combine(directoryName, INITIAL_CONDITIONS_FILE_NAME);
            FileWritingUtils.ThrowIfFileNotExists(initialConditionFilePath, directoryName,
                filename => InitialConditionInitialFieldsFileWriter.WriteFile(filename, modelDefinition, networkIsEmpty));

            if (networkIsEmpty) return;
            // write Initial<quantity>.ini
            var intialConditionDefinitionFilename =
                Path.Combine(directoryName, GetInitialConditionDefinitionFilename(globalInitialConditionQuantity1D));
            FileWritingUtils.ThrowIfFileNotExists(intialConditionDefinitionFilename, directoryName,
                filename => ChannelInitialConditionDefinitionFileWriter.WriteFile(
                    filename, channelInitialConditionDefinitions, globalInitialConditionQuantity1D, globalInitialConditionValue1D));
        }

        private static string GetRoughnessFilename(RoughnessSection roughnessSection)
        {
            return "roughness-" + roughnessSection.Name + ".ini";
        }

        private static string GetInitialConditionDefinitionFilename(InitialConditionQuantity quantity)
        {
            return $"Initial{quantity}.ini";
        }
    }
}
