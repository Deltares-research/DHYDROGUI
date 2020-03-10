using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Networks;

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

        public static void Write1D2DFeatures(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, HydroArea area, IEnumerable<RoughnessSection> roughnessSections)
        {
            WriteNodeFile(targetMduFilePath, modelDefinition, network);
            WriteBranchFile(targetMduFilePath, modelDefinition, network.Branches);
            WriteCrossSectionFiles(targetMduFilePath, modelDefinition, network);
            WriteObservationPointsFiles(targetMduFilePath, modelDefinition, network);
            WriteStructuresFiles(targetMduFilePath, modelDefinition, network, area);
            WriteRoughnessFiles(targetMduFilePath, modelDefinition, roughnessSections);
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
                //var currentObsCrs = modelDefinition.GetModelProperty(KnownProperties.ObsCrsFile).GetValueAsString();
                //modelDefinition.SetModelProperty(KnownProperties.ObsCrsFile, string.IsNullOrEmpty(currentObsCrs) ? OBS_CRS_FILE_NAME : currentObsCrs + " " + OBS_CRS_FILE_NAME);
                //LocationFileWriter.WriteFileObservationPointLocations(obsCrsFilePath, obsPoints, true);
            }
            else
            {
                ///empty?
                modelDefinition.SetModelProperty(KnownProperties.ObsFile, string.Empty);
                modelDefinition.SetModelProperty(KnownProperties.ObsCrsFile, string.Empty);
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

            var branchesFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            FileUtils.DeleteIfExists(branchesFilePath);
            if (!branches.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.BranchFile, string.Empty);
            }
            BranchFile.Write(branchesFilePath, branches);
            modelDefinition.SetModelProperty(KnownProperties.BranchFile, UGridToNetworkAdapter.BranchGuiFileName);
        }
        private static void WriteCrossSectionFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            WriteCrossSectionDefinitions(targetMduFilePath, modelDefinition, network);
            WriteCrossSectionLocations(targetMduFilePath, modelDefinition, network);
        }

        private static void WriteCrossSectionLocations(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_LOCATION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionLocationFilePath);

            
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, CROSS_SECTION_LOCATION_FILE_NAME);

                var crossSections = network.CrossSections.Concat(network.Pipes.Select(p=>p.CrossSection));
                LocationFileWriter.WriteFileCrossSectionLocations(crossSectionLocationFilePath, crossSections);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, string.Empty);
            }
        }

        private static void WriteCrossSectionDefinitions(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_DEFINITION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionDefinitionFilePath);

            if (network.ContainsAnyCrossSectionDefinitions())
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, CROSS_SECTION_DEFINITION_FILE_NAME);
                CrossSectionDefinitionFileWriter.WriteFile(crossSectionDefinitionFilePath, network);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, string.Empty);
            }
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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion> {network, area}, (DateTime)modelDefinition.GetModelProperty(KnownProperties.RefDate).Value, targetMduFilePath, StructureFile.Generate2DStructureCategoriesFromFmModel);
                modelDefinition.Properties.Remove(targetMduFilePathProperty);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, string.Empty);
            }
        }

        private static void WriteRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IEnumerable<RoughnessSection> roughnessSections)
        {
            var directoryName = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var sections = roughnessSections.ToArray();
            var roughnessFileNames = sections.Select(GetRoughnessFilename);
            modelDefinition.SetModelProperty(KnownProperties.FrictFile, string.Join(";", roughnessFileNames));

            foreach (var roughnessSection in sections)
            {
                var roughnessFileName = GetRoughnessFilename(roughnessSection);
                var roughnessFilePath = System.IO.Path.Combine(directoryName, roughnessFileName);

                FileWritingUtils.ThrowIfFileNotExists(roughnessFilePath, directoryName, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));
            }
        }

        private static string GetRoughnessFilename(RoughnessSection roughnessSection)
        {
            return "roughness-" + roughnessSection.Name + ".ini";
        }
    }
}
