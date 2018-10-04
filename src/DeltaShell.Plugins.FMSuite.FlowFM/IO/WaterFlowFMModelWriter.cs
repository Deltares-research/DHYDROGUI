using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        private static WaterFlowFMModelWriterData WriterData;

        // TODO: get rid of the optional parameters. Solve in a different way.
        public static void Write(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            PrepareModelDefinitionForWriting(model);
            WriterData = CreateWriterData(model);

            WriteMorSedFilesIfNeeded(model);
            WriteMduFile(model, switchTo, writeExtForcings, writeFeatures);
            WriteCrossSectionDefinitions(model);
            WriteCrossSectionLocation(model);
            WriteNodeFile(model);
            WriteBranchesGuiFile(model);
            WriteStructuresFile(model);
            //WriteRoughness(model);
            WriteUGridFile(WriterData);
        }

        //private static void WriteRoughness(WaterFlowFMModel model)
        //{
        //    var writtenRoughessFiles = new List<string>();

        //    foreach (var roughnessSection in model.RoughnessSections)
        //    {
        //        var filename = "roughness-" + roughnessSection.Name + ".ini";
        //        var roughnessFilename = Path.Combine(KnownProperties.RoughnessFile, filename);

        //        RoughnessDataFileWriter.
        //        ThrowIfFileNotExists(roughnessFilename, fileName.TargetPath, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));//Add subPath!!
        //        writtenRoughessFiles.Add(filename);
        //    }
        //    model.ModelDefinition.SetModelProperty(KnownProperties.RoughnessFile, string.Join(" ", writtenRoughessFiles));
        //}

        private static void PrepareModelDefinitionForWriting(IWaterFlowFMModel model)
        {
            var network = model.Network;
            var modelDefinition = model.ModelDefinition;
            if (network.Manholes.Any())
                modelDefinition.SetModelProperty(KnownProperties.NodeFile, "nodeFile.ini");
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, "crsdef.ini");
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, "crsloc.ini");
            }

            if (network.BranchFeatures.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, "structures.ini");
            }
        }

        private static WaterFlowFMModelWriterData CreateWriterData(WaterFlowFMModel model)
        {
            return new WaterFlowFMModelWriterData
            {
                ModelName = model.Name,
                FilePaths = new WaterFlowFMModelWriterData.FileNames
                {
                    NetFilePath = model.NetFilePath,
                    CrossSectionDefinitionFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossDefFile, model),
                    CrossSectionLocationFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossLocFile, model),
                    NodeFilePath = GetAbsoluteFilePathFromModel(KnownProperties.NodeFile, model),
                    StructuresFilePath = GetAbsoluteFilePathFromModel(KnownProperties.StructuresFile, model)
                },
                NetworkDataModel = new NetworkUGridDataModel(model.Network),
                NetworkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(model.NetworkDiscretization)
            };
        }

        private static string GetAbsoluteFilePathFromModel(string key, WaterFlowFMModel model)
        {
            var fileProperty = model.ModelDefinition.GetModelProperty(key);
            var fileName = fileProperty.GetValueAsString();
            if (string.IsNullOrEmpty(fileName)) return string.Empty;

            var absolutePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, fileName);
            return absolutePath;
        }

        private static void WriteMorSedFilesIfNeeded(WaterFlowFMModel model)
        {
            if (!model.UseMorSed) return;

            var morPath = Path.ChangeExtension(model.MduFilePath, "mor");
            MorphologyFile.Save(morPath, model.ModelDefinition);

            var sedPath = Path.ChangeExtension(model.MduFilePath, "sed");
            SedimentFile.Save(sedPath, model);
        }

        private static void WriteMduFile(WaterFlowFMModel model, bool switchTo, bool writeExtForcings, bool writeFeatures)
        {
            var mduFile = new MduFile();
            mduFile.Write(model.MduFilePath, model.ModelDefinition, model.Area, model.FixedWeirsProperties, switchTo, writeExtForcings, writeFeatures, model.DisableFlowNodeRenumbering);
        }

        private static void WriteCrossSectionDefinitions(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.CrossSectionDefinitionFilePath;
            if (string.IsNullOrEmpty(filePath)) return;

            CrossSectionDefinitionFileWriter.WriteFile(filePath, model.Network, model.RoughnessSections);
        }

        private static void WriteCrossSectionLocation(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.CrossSectionLocationFilePath;
            if (!string.IsNullOrEmpty(filePath))
                CrossSectionLocationWriter.WriteFile(filePath, model);
        }

        private static void WriteNodeFile(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.NodeFilePath;
            if (!string.IsNullOrEmpty(filePath))
                NodeFile.Write(filePath, model.Network.Manholes.SelectMany(m => m.Compartments));
        }

        private static void WriteBranchesGuiFile(WaterFlowFMModel model)
        {
            var branchesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(branchesFilePath, model.Network.Branches);
        }

        private static void WriteStructuresFile(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.StructuresFilePath;

            if (!string.IsNullOrEmpty(filePath))
                StructureFileWriter.WriteFile(
                    filePath, 
                    model,
                    GenerateFlow2DStructureCategoriesFromFMModel);
        }

        public static IEnumerable<DelftIniCategory> GenerateFlow2DStructureCategoriesFromFMModel(IModel model)
        {
            var fmModel = model as WaterFlowFMModel;
            if (fmModel?.Network == null) return Enumerable.Empty<DelftIniCategory>();

            var categories1D = NetworkEditor.IO.StructureFile.ExtractFunctionStructuresOfNetworkGenerator(fmModel.Network);
            var categories2D = StructureFile.ExtractFunctionStructuresOfAreaGenerator(fmModel.Area).ToList();

            Action<string, WaterFlowFMModel, IStructure2D> myAction;
            foreach (var category2D in categories2D)
            {
                var structureName = category2D.GetPropertyValue(StructureRegion.Id.Key);
                if(string.IsNullOrEmpty(structureName)) continue;

                foreach (var propertyName in category2D.Properties.Select(property => property.Name))
                {
                    if (StructureWriteActions.TryGetValue(propertyName, out myAction))
                    {
                        var fileNameProperty = category2D.Properties.FirstOrDefault(p => p.Name == propertyName);
                        var structure2D = fmModel.Area.AllHydroObjects.Cast<IStructure2D>().FirstOrDefault(o => o.Name == structureName);
                        if (fileNameProperty != null) myAction(fileNameProperty.Value, fmModel, structure2D);
                    }
                }
            }

            return categories1D.Concat(categories2D);
        }

        private static readonly Dictionary<string, Action<string, WaterFlowFMModel, IStructure2D>> StructureWriteActions = new Dictionary<string, Action<string, WaterFlowFMModel, IStructure2D>>
        {
            {StructureRegion.PolylineFile.Key, WritePolylineFile},
            {StructureRegion.Capacity.Key, WriteTimeSeriesFile},
            {StructureRegion.CrestLevel.Key, WriteTimeSeriesFile }
        };

        private static void WritePolylineFile(string fileName, WaterFlowFMModel fmModel, IStructure2D structure2D)
        {
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(fmModel.MduFilePath, fileName);
            var geometryObjectsToBeWritten = new[]
            {
                new Feature2D
                {
                    Name = structure2D.Name,
                    Geometry = structure2D.Geometry
                }
            };
            new PliFile<Feature2D>().Write(pliFilePath, geometryObjectsToBeWritten);
        }

        private static void WriteTimeSeriesFile(string fileName, WaterFlowFMModel fmModel, IStructure2D structure2D)
        {
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(fmModel.MduFilePath, fileName);
            var pump = structure2D as IPump;
            if (pump != null && pump.HasCapacityTimeSeries())
            {
                new TimFile().Write(timFilePath, pump.CapacityTimeSeries, fmModel.ReferenceTime);
                return;
            }

            var weir = structure2D as IWeir;
            if (weir != null && weir.HasCrestLevelTimeSeries())
            {
                new TimFile().Write(timFilePath, weir.CrestLevelTimeSeries, fmModel.ReferenceTime);
            }
        }

        private static bool HasCapacityTimeSeries(this IPump pump)
        {
            return pump.CanBeTimedependent
                   && pump.UseCapacityTimeSeries
                   && pump.CapacityTimeSeries != null;
        }

        private static bool HasCrestLevelTimeSeries(this IWeir weir)
        {
            return weir.CanBeTimedependent
                   && weir.UseCrestLevelTimeSeries
                   && weir.CrestLevelTimeSeries != null;
        }

        private static void WriteUGridFile(WaterFlowFMModelWriterData writerData)
        {
            var netFilePath = writerData.FilePaths.NetFilePath;

            var metaData = new UGridGlobalMetaData(writerData.ModelName, "1.1", "2.1"); // last two arguments should be retrieved from the FlowFMApplicationPlugin
            UGridToNetworkAdapter.SaveNetwork(netFilePath, writerData.NetworkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, writerData.NetworkDiscretisationDataModel);
        }
    }
}
