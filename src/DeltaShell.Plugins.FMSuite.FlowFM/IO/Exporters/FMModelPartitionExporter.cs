using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    /// <summary>
    /// Provides a D-FlowFM model file partition exporter.
    /// </summary>
    public class FMModelPartitionExporter : FMPartitionExporterBase
    {
        /// <inheritdoc/>
        public override string FileFilter => "Flexible Mesh Model Definition|*.mdu";

        /// <summary>
        /// Gets or sets the directory to export the D-FLowFM model file to.
        /// </summary>
        public string ExportDirectory { get; set; }

        public int SolverType { private get; set; }

        /// <inheritdoc/>
        public override bool Export(object item, string path)
        {
            if (PolygonFile == null && NumDomains <= 0)
            {
                return false;
            }

            var waterFlowFMModel = item as WaterFlowFMModel;
            return waterFlowFMModel != null && ExportPartitionMdu(waterFlowFMModel, path);
        }
        
        /// <inheritdoc/>
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaterFlowFMModel);
        }

        private bool ExportPartitionMdu(WaterFlowFMModel waterFlowFMModel, string path)
        {
            IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
            if (api == null)
            {
                return false;
            }

            using (api)
            {
                string exportPath = ExportDirectory ?? path;
                if (Directory.Exists(exportPath))
                {
                    exportPath = waterFlowFMModel.GetMduExportPath(exportPath);
                }
                
                string filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(exportPath),
                                                               Path.GetFileNameWithoutExtension(exportPath));

                string filePath = filePathWithoutExtension + ".mdu";
                
                WaterFlowFMModelDefinition modelDefinition = waterFlowFMModel.ModelDefinition;
                WaterFlowFMProperty igcSolverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
                
                string originalSolverType = igcSolverProperty.GetValueAsString();
                igcSolverProperty.SetValueFromString("2"); //ensure init works

                waterFlowFMModel.ExportTo(filePath, false);

                if (PolygonFile == null && NumDomains == 1)
                {
                    return true;
                }

                SourceNetFilePath = waterFlowFMModel.NetFilePath;

                string directory = Path.GetDirectoryName(filePath);

                TargetNetFilePath = directory == null
                                        ? waterFlowFMModel.NetFilePath
                                        : Path.Combine(directory, Path.GetFileName(waterFlowFMModel.NetFilePath));

                string partFileName = Path.GetFileNameWithoutExtension(TargetNetFilePath);
                if (partFileName.EndsWith("_net"))
                {
                    partFileName = partFileName.Substring(0, partFileName.Count() - 4);
                }

                partFileName += "_part.pol";

                WriteNetPartition(api);
                WaterFlowFMProperty netFileProperty = modelDefinition.GetModelProperty(KnownProperties.NetFile);
                WaterFlowFMProperty partFileProperty = modelDefinition.GetModelProperty(KnownProperties.PartitionFile);

                string originalNetFile = netFileProperty.GetValueAsString();
                string originalPartFile = partFileProperty.GetValueAsString();

                var i = 0;
                foreach (string netFile in FindNetFiles(TargetNetFilePath))
                {
                    filePath = filePathWithoutExtension + "_" + $"{i++:0000}" + ".mdu";

                    netFileProperty.SetValueFromString(netFile);
                    igcSolverProperty.SetValueFromString(SolverType > 0 ? SolverType.ToString() : originalSolverType);
                    new MduFile().WriteProperties(filePath, modelDefinition.Properties, true, true, false, false, waterFlowFMModel.DisableFlowNodeRenumbering);
                }

                netFileProperty.SetValueFromString(originalNetFile);
                partFileProperty.SetValueFromString(originalPartFile);
                igcSolverProperty.SetValueFromString(originalSolverType);
            }

            return true;
        }

        private static IEnumerable<string> FindNetFiles(string netFileName)
        {
            int index = netFileName.LastIndexOf("_net.nc", StringComparison.InvariantCulture);
            if (index < 0)
            {
                yield break;
            }

            string strippedName = netFileName.Substring(0, index);
            bool fileFound;
            var i = 0;
            do
            {
                string fileName = strippedName + "_" + $"{i++:0000}" + "_net.nc";
                fileFound = File.Exists(fileName);
                if (fileFound)
                {
                    yield return Path.GetFileName(fileName);
                }
            } while (fileFound);
        }
    }
}