using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DWriter
    {
        public const string NODE_FILE_NAME = "nodeFile.ini";
        public const string CROSS_SECTION_DEFINITION_FILE_NAME = "crsdef.ini";
        public const string CROSS_SECTION_LOCATION_FILE_NAME = "crsloc.ini";
        public const string STRUCTURES_FILE_NAME = "structures.ini";

        public static void Write1D2DFeatures(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            WriteNodeFile(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network);
            WriteCrossSections(targetMduFilePath, fmModel);
            WriteStructuresFiles(targetMduFilePath, fmModel);
            WriteRoughness(targetMduFilePath, fmModel);
        }

        private static void WriteNodeFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, NODE_FILE_NAME);
            FileUtils.DeleteIfExists(nodeFilePath);

            var compartments = network.Manholes.SelectMany(m => m.Compartments).ToList();
            if (compartments.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.NodeFile, NODE_FILE_NAME);
                NodeFile.Write(nodeFilePath, compartments);
            }
            else
            {
                modelDefinition.SetModelProperty(KnownProperties.NodeFile, string.Empty);
            }
        }

        private static void WriteCrossSections(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            WriteCrossSectionDefinitions(targetMduFilePath, fmModel);
            WriteCrossSectionLocations(targetMduFilePath, fmModel);
        }

        private static void WriteCrossSectionLocations(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_LOCATION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionLocationFilePath);

            if (fmModel.Network.CrossSections.Any() || fmModel.Network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.CrossLocFile, CROSS_SECTION_LOCATION_FILE_NAME);
                CrossSectionLocationWriter.WriteFile(crossSectionLocationFilePath, fmModel);
            }
            else
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.CrossLocFile, string.Empty);
            }
        }

        private static void WriteCrossSectionDefinitions(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, CROSS_SECTION_DEFINITION_FILE_NAME);
            FileUtils.DeleteIfExists(crossSectionDefinitionFilePath);

            if (fmModel.Network.CrossSections.Any() || fmModel.Network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.CrossDefFile, CROSS_SECTION_DEFINITION_FILE_NAME);
                CrossSectionDefinitionFileWriter.WriteFile(crossSectionDefinitionFilePath, fmModel.Network, fmModel.RoughnessSections);
            }
            else
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.CrossDefFile, string.Empty);
            }
        }

        private static void WriteStructuresFiles(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, STRUCTURES_FILE_NAME);
            if (fmModel.Network.BranchFeatures.Any() || fmModel.Area.AllHydroObjects.Any())
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.StructuresFile, STRUCTURES_FILE_NAME);

                var targetMduFilePathPropertyDefinition = new WaterFlowFMPropertyDefinition
                {
                    MduPropertyName = GuiProperties.TargetMduPath,
                    Category = GuiProperties.GUIonly,
                    FileCategoryName = GuiProperties.GUIonly,
                    DataType = typeof(string)
                };
                var targetMduFilePathProperty = new WaterFlowFMProperty(targetMduFilePathPropertyDefinition, targetMduFilePath);

                fmModel.ModelDefinition.AddProperty(targetMduFilePathProperty);
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFmModel);
                fmModel.ModelDefinition.Properties.Remove(targetMduFilePathProperty);
            }
            else
            {
                fmModel.ModelDefinition.SetModelProperty(KnownProperties.StructuresFile, string.Empty);
            }
        }

        private static void WriteRoughness(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var directoryName = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var roughnessFileNames = fmModel.RoughnessSections.Select(GetRoughnessFilename);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RoughnessFile, string.Join(" ", roughnessFileNames));

            foreach (var roughnessSection in fmModel.RoughnessSections)
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
