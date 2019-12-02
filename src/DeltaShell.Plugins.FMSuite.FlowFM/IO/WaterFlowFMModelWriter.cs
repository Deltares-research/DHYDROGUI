using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelWriter));

        // TODO: get rid of the optional parameters. Solve in a different way.
        public static bool Write(string mduFilePath, WaterFlowFMModel model)
        {

            PrepareModelDefinitionForWriting(model);
            try
            {
                var writerData = CreateWriterData(mduFilePath, model);
                WriteMorSedFilesIfNeeded(mduFilePath, model);
                WriteMduFile(mduFilePath, model, false, true, true);
                WriteCrossSectionDefinitions(writerData, model);
                WriteCrossSectionLocations(writerData, model);
                WriteNodeFile(writerData, model);
                WriteBranchesGuiFile(writerData, model);
                WriteStructuresFile(writerData, model);
                WriteRoughness(writerData, model);
                WriteUGridFile(writerData, model);
            }
            catch (Exception e)
            {
                Log.WarnFormat("While writing FM with 1D data an exception occured : {0}", e.Message);
                return false;
            }
            return true;
        }

        private static void WriteRoughness(WaterFlowFMModelWriterData writerData, WaterFlowFMModel fmModel)
        {   
            var directoryName = Path.GetDirectoryName(fmModel.MduFilePath);
            if (directoryName == null) return;
            
            foreach (var roughnessSection in fmModel.RoughnessSections)
            {
                var filename = GetRoughnessFilename(roughnessSection);
                var roughnessFilePath = Path.Combine(directoryName, filename);

                FileWritingUtils.ThrowIfFileNotExists(roughnessFilePath, directoryName, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));//Add subPath!!
            }
            //ok.. and now? how do you want the roughness from the 2d model? It's a UnstructuredGridFlowLinkCoverage and has some physical parameters, if set it's a spatial operation
            //fmModel.Roughness.
        }

        private static string GetRoughnessFilename(RoughnessSection roughnessSection)
        {
            return "roughness-" + roughnessSection.Name + ".ini";
        }

        private static void PrepareModelDefinitionForWriting(WaterFlowFMModel fmModel)
        {
            var network = fmModel.Network;

            var modelDefinition = fmModel.ModelDefinition;
            if (network.Manholes.Any())
                modelDefinition.SetModelProperty(KnownProperties.NodeFile, FeatureFile1D2DWriter.NODE_FILE_NAME);
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, FeatureFile1D2DWriter.CROSS_SECTION_DEFINITION_FILE_NAME);
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, FeatureFile1D2DWriter.CROSS_SECTION_LOCATION_FILE_NAME);
            }

            if (network.BranchFeatures.Any() || fmModel.Area.AllHydroObjects.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, FeatureFile1D2DWriter.STRUCTURES_FILE_NAME);
            }

            var roughnessFileNames = fmModel.RoughnessSections.Select(GetRoughnessFilename);
            modelDefinition.SetModelProperty(KnownProperties.FrictFile, string.Join(";", roughnessFileNames));
        }

        private static WaterFlowFMModelWriterData CreateWriterData(string mduFilePath, WaterFlowFMModel model)
        {
            return new WaterFlowFMModelWriterData
            {
                ModelName = model.Name,
                FilePaths = new WaterFlowFMModelWriterData.FileNames
                {
                    NetFilePath = GetAbsoluteFilePathFromModel(KnownProperties.NetFile, model, mduFilePath),
                    CrossSectionDefinitionFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossDefFile, model, mduFilePath),
                    CrossSectionLocationFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossLocFile, model, mduFilePath),
                    NodeFilePath = GetAbsoluteFilePathFromModel(KnownProperties.NodeFile, model, mduFilePath),
                    StructuresFilePath = GetAbsoluteFilePathFromModel(KnownProperties.StructuresFile, model, mduFilePath)
                },
                NetworkDataModel = new NetworkUGridDataModel(model.Network),
                NetworkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(model.NetworkDiscretization)
            };
        }

        private static string GetAbsoluteFilePathFromModel(string key, WaterFlowFMModel model, string mduFilePath)
        {
            var fileProperty = model.ModelDefinition.GetModelProperty(key);
            var fileName = fileProperty?.GetValueAsString();
            if (string.IsNullOrEmpty(fileName)) return string.Empty;

            var absolutePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, fileName);
            return absolutePath;
        }

        private static void WriteMorSedFilesIfNeeded(string mduFilePath, WaterFlowFMModel model)
        {
            if (!model.UseMorSed) return;

            var morPath = Path.ChangeExtension(mduFilePath, "mor");
            MorphologyFile.Save(morPath, model.ModelDefinition);

            var sedPath = Path.ChangeExtension(mduFilePath, "sed");
            SedimentFile.Save(sedPath, model.ModelDefinition, model);
        }

        private static void WriteMduFile(string mduFilePath, WaterFlowFMModel model, bool switchTo, bool writeExtForcings, bool writeFeatures)
        {
            var mduFile = new MduFile();
            mduFile.Write(mduFilePath, model.ModelDefinition, model.Area, model.FixedWeirsProperties, switchTo, writeExtForcings, writeFeatures, model.DisableFlowNodeRenumbering, null, false);
        }

        private static void WriteCrossSectionDefinitions(WaterFlowFMModelWriterData writerData, WaterFlowFMModel model)
        {
            var filePath = writerData.FilePaths.CrossSectionDefinitionFilePath;
            if (string.IsNullOrEmpty(filePath)) return;

            CrossSectionDefinitionFileWriter.WriteFile(filePath, model.Network);
        }

        private static void WriteCrossSectionLocations(WaterFlowFMModelWriterData writerData, IWaterFlowFMModel model)
        {
            var filePath = writerData.FilePaths.CrossSectionLocationFilePath;
            
            if (!string.IsNullOrEmpty(filePath))
            {
                var pipeCrossSections = HydroNetworkHelper.GeneratePipeCrossSections(model.Network);
                var crossSections = model.Network.CrossSections.Concat(pipeCrossSections);
                LocationFileWriter.WriteFileCrossSectionLocations(filePath, crossSections);
            }
        }

        private static void WriteNodeFile(WaterFlowFMModelWriterData writerData, IWaterFlowFMModel model)
        {
            var filePath = writerData.FilePaths.NodeFilePath;
            if (!string.IsNullOrEmpty(filePath))
                NodeFile.Write(filePath, model.Network.Manholes.SelectMany(m => m.Compartments), model.Network.Retentions);
        }

        private static void WriteBranchesGuiFile(WaterFlowFMModelWriterData writerData, WaterFlowFMModel model)
        {
            var branchesFilePath = IoHelper.GetFilePathToLocationInSameDirectory(writerData.FilePaths.NetFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(branchesFilePath, model.Network.Branches);
        }

        private static void WriteStructuresFile(WaterFlowFMModelWriterData writerData, IModel model)
        {
            var filePath = writerData.FilePaths.StructuresFilePath;

            if (!string.IsNullOrEmpty(filePath))
                StructureFileWriter.WriteFile(
                    filePath, 
                    model,
                    StructureFile.Generate2DStructureCategoriesFromFmModel);
        }

        private static void WriteUGridFile(WaterFlowFMModelWriterData writerData, WaterFlowFMModel model)
        {
            if (writerData.FilePaths.NetFilePath == null) return;
            UnstructuredGridFileHelper.WriteGridToFile(writerData.FilePaths.NetFilePath, model.Grid, model.Network, model.NetworkDiscretization, model.Links, model.Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion, model.BedLevelLocation, model.BedLevelZValues);
            // if needed, adjust coordinate system in netfile
            if (File.Exists(writerData.FilePaths.NetFilePath) && model.Grid.CoordinateSystem != null && !model.Grid.CoordinateSystem.IsNetfileCoordinateSystemUpToDate(writerData.FilePaths.NetFilePath))
                UnstructuredGridFileHelper.SetCoordinateSystem(writerData.FilePaths.NetFilePath, model.Grid.CoordinateSystem);
        }
    }
}
