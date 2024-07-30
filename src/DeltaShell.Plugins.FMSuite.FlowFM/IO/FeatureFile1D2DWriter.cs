using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Writer for 1D2D features responsible for writing the feature files and setting the data on the models.
    /// </summary>
    public sealed class FeatureFile1D2DWriter
    {
        private readonly InitialFieldFile initialFieldFile;

        private WaterFlowFMModelDefinition modelDefinition;
        private IHydroNetwork hydroNetwork;
        private HydroArea hydroArea;

        private IEnumerable<RoughnessSection> roughnessSections;
        private IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions;
        private IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions;

        private string targetMduFilePath;
        private bool switchToNewPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureFile1D2DWriter"/> class.
        /// </summary>
        /// <param name="initialFieldFile">Provides methods for reading and writing initial field files (*.ini).</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="initialFieldFile"/> is <c>null</c>.</exception>
        public FeatureFile1D2DWriter(InitialFieldFile initialFieldFile)
        {
            Ensure.NotNull(initialFieldFile, nameof(initialFieldFile));
            this.initialFieldFile = initialFieldFile;
        }

        public void Write1D2DFeatures(
            string mduFilePath,
            WaterFlowFMModelDefinition definition,
            IHydroNetwork network,
            HydroArea area,
            IEnumerable<RoughnessSection> sections,
            IEnumerable<ChannelFrictionDefinition> frictionDefinitions,
            IEnumerable<ChannelInitialConditionDefinition> initialConditionDefinitions,
            bool switchTo = true)
        {
            targetMduFilePath = mduFilePath;
            modelDefinition = definition;
            hydroNetwork = network;
            hydroArea = area;
            roughnessSections = sections;
            channelFrictionDefinitions = frictionDefinitions;
            channelInitialConditionDefinitions = initialConditionDefinitions;
            switchToNewPath = switchTo;

            WriteNodeFile();
            WriteBranchFile();
            WriteRoutesFile();
            WriteCrossSectionFiles();
            WriteObservationPointsFile();
            WriteStructuresFiles();
            WriteRoughnessFiles();
            WriteInitialConditionFiles();
        }

        private void WriteObservationPointsFile()
        {
            List<IObservationPoint> obsPoints = hydroNetwork.ObservationPoints.ToList();
            
            if (obsPoints.Any() || hydroNetwork.Retentions.Any())
            {
                string currentObsFile = modelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
                modelDefinition.SetModelProperty(KnownProperties.ObsFile, currentObsFile + " " + FeatureFile1D2DConstants.DefaultObsFileName);
                
                string obsFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, FeatureFile1D2DConstants.DefaultObsFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(obsFilePath));

                LocationFileWriter.WriteFileObservationPointLocations(obsFilePath, obsPoints);
            }
            
            // Do not clear ObsFile property; observation points are also written in 2D using the same property.
        }

        private void WriteNodeFile()
        {
            List<ICompartment> compartments = hydroNetwork.Manholes.SelectMany(m => m.Compartments).ToList();
            
            if (compartments.Any() || hydroNetwork.Retentions.Any())
            {
                string nodeFilePath = GetFilePropertyValueOrDefault(KnownProperties.StorageNodeFile, NetworkPropertiesHelper.StorageNodeFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(nodeFilePath));

                NodeFile.Write(nodeFilePath, compartments, hydroNetwork.Retentions);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.StorageNodeFile, string.Empty);
            }
        }

        private void WriteBranchFile()
        {
            List<IBranch> branches = hydroNetwork.Branches.ToList();
            
            if (branches.Any())
            {
                string branchFilePath = GetFilePropertyValueOrDefault(KnownProperties.BranchFile, NetworkPropertiesHelper.BranchGuiFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(branchFilePath));

                var branchFile = new BranchFile(new FileSystem());
                branchFile.Write(branchFilePath, branches);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.BranchFile, string.Empty);
            }
        }

        private void WriteRoutesFile()
        {
            string routesFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, RoutesFile.RoutesFileName);
            FileUtils.DeleteIfExists(routesFilePath);

            RoutesFile.Write(routesFilePath, hydroNetwork.Routes);
        }

        private void WriteCrossSectionFiles()
        {
            WriteCrossSectionDefinitions();
            WriteCrossSectionLocations();
        }

        private void WriteCrossSectionLocations()
        {
            if (hydroNetwork.CrossSections.Any() || hydroNetwork.Pipes.Any(p => p.CrossSection?.Definition != null))
            {
                string crossLocFilePath = GetFilePropertyValueOrDefault(KnownProperties.CrossLocFile, FeatureFile1D2DConstants.DefaultCrossLocFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(crossLocFilePath));
                
                IEnumerable<ICrossSection> crossSections = hydroNetwork.CrossSections.Concat(hydroNetwork.SewerConnections.Select(p => p.CrossSection));
                LocationFileWriter.WriteFileCrossSectionLocations(crossLocFilePath, crossSections);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, string.Empty);
            }
        }

        private void WriteCrossSectionDefinitions()
        {
            if (hydroNetwork.ContainsAnyCrossSectionDefinitions() || hydroNetwork.SharedCrossSectionDefinitions.Any())
            {
                string crossDefFilePath = GetFilePropertyValueOrDefault(KnownProperties.CrossDefFile, FeatureFile1D2DConstants.DefaultCrossDefFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(crossDefFilePath));

                var channelFrictionDefinitionPerChannelLookup = channelFrictionDefinitions.ToDictionary(cfd => cfd.Channel, cfd => cfd);
                var crossSectionDefinitionsForChannel = WriteFrictionFromCrossSectionDefinitionsForChannel(channelFrictionDefinitionPerChannelLookup);

                CrossSectionDefinitionFileWriter.WriteFile(crossDefFilePath, hydroNetwork, crossSectionDefinitionsForChannel, RoughnessDataRegion.SectionId.DefaultValue);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, string.Empty);
            }
        }

        private Func<IChannel, bool> WriteFrictionFromCrossSectionDefinitionsForChannel(IReadOnlyDictionary<IChannel, ChannelFrictionDefinition> channelFrictionDefinitionPerChannelLookup)
        {
            return channel =>
            {
                ChannelFrictionDefinition channelFrictionDefinition = channelFrictionDefinitionPerChannelLookup[channel];

                return channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections
                       || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;
            };
        }

        private void WriteStructuresFiles()
        {
            if (hydroNetwork.BranchFeatures.Any() || hydroArea.AllHydroObjects.Any())
            {
                string structuresFilePath = GetFilePropertyValueOrDefault(KnownProperties.StructuresFile, FeatureFile1D2DConstants.DefaultStructuresFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(structuresFilePath));

                var targetMduFilePathPropertyDefinition = new WaterFlowFMPropertyDefinition
                {
                    MduPropertyName = GuiProperties.TargetMduPath,
                    Category = GuiProperties.GUIonly,
                    FileSectionName = GuiProperties.GUIonly,
                    DataType = typeof(string)
                };

                var targetMduFilePathProperty = new WaterFlowFMProperty(targetMduFilePathPropertyDefinition, targetMduFilePath);

                modelDefinition.AddProperty(targetMduFilePathProperty);

                IHydroRegion[] regions = { hydroNetwork, hydroArea };
                DateTime referenceTime = modelDefinition.GetReferenceDateAsDateTime();

                var structureFileWriter = new StructureFileWriter(new FileSystem());
                structureFileWriter.WriteFile(structuresFilePath,
                                              regions,
                                              referenceTime,
                                              StructureFile.GenerateStructureIniSectionsFromFmModel);
                StructureFile.WriteStructureFiles(regions, targetMduFilePath, referenceTime);

                modelDefinition.Properties.Remove(targetMduFilePathProperty);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, string.Empty);
            }
        }

        private void WriteRoughnessFiles()
        {
            string directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null)
            {
                return;
            }

            RoughnessSection[] sections = roughnessSections.ToArray();
            var frictionFiles = new List<string>();
            string frictionFileName = Resources.Roughness_Main_Channels_Filename;
            frictionFiles.Add(frictionFileName);
            frictionFiles.AddRange(sections.Select(GetRoughnessFilename));

            modelDefinition.GetModelProperty(KnownProperties.FrictFile).SetValueFromStrings(frictionFiles, ';');

            var globalFrictionType = (RoughnessType)(int)modelDefinition.GetModelProperty(GuiProperties.UnifFrictTypeChannels).Value;
            var globalFrictionValue = (double)modelDefinition.GetModelProperty(GuiProperties.UnifFrictCoefChannels).Value;

            // write lanes files
            foreach (RoughnessSection roughnessSection in sections)
            {
                string roughnessFileName = GetRoughnessFilename(roughnessSection);
                string roughnessFilePath = Path.Combine(directoryName, roughnessFileName);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(roughnessFilePath));

                var roughnessWriter = new RoughnessDataFileWriter(new FileSystem());

                FileWritingUtils.ThrowIfFileNotExists(roughnessFilePath, directoryName, p => roughnessWriter.WriteFile(p, roughnessSection));
            }

            // write channels roughness
            string frictionFilePath = Path.Combine(directoryName, frictionFileName);
            FileWritingUtils.ThrowIfFileNotExists(frictionFilePath, directoryName, p => ChannelFrictionDefinitionFileWriter.WriteFile(p, channelFrictionDefinitions, globalFrictionType, globalFrictionValue));
        }

        private static string GetRoughnessFilename(RoughnessSection roughnessSection)
        {
            return "roughness-" + roughnessSection.Name + ".ini";
        }

        private void WriteInitialConditionFiles()
        {
            if (!initialFieldFile.ShouldWrite(modelDefinition, hydroNetwork))
            {
                return;
            }

            // write initialFields.ini
            string initialFieldFilePath = GetFilePropertyValueOrDefault(KnownProperties.IniFieldFile, InitialFieldFileConstants.DefaultFileName);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(initialFieldFilePath));

            initialFieldFile.Write(initialFieldFilePath, initialFieldFilePath, switchToNewPath, modelDefinition);

            // write Initial<quantity>.ini
            var globalInitialConditionQuantity1D = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
            var globalInitialConditionValue1D = (double)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
            
            string directoryName = Path.GetDirectoryName(initialFieldFilePath);
            string intialConditionDefinitionFilename = Path.Combine(directoryName, $"Initial{globalInitialConditionQuantity1D}.ini");
            FileWritingUtils.ThrowIfFileNotExists(intialConditionDefinitionFilename, directoryName,
                                                  filename => ChannelInitialConditionDefinitionFileWriter.WriteFile(
                                                      filename, channelInitialConditionDefinitions, globalInitialConditionQuantity1D, globalInitialConditionValue1D));
        }

        private string GetFilePropertyValueOrDefault(string propertyName, string defaultValue)
        {
            WaterFlowFMProperty fileProperty = modelDefinition.GetModelProperty(propertyName);
            
            if (string.IsNullOrWhiteSpace(fileProperty.GetValueAsString()))
            {
                fileProperty.SetValueFromString(defaultValue);
            }
            
            return MduFileHelper.GetSubfilePath(targetMduFilePath, fileProperty);
        }
    }
}