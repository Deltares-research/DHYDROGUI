using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class FMModelPartitionExporter : FMPartitionExporterBase
    {
        public override string FileFilter => $"Flexible Mesh Model Definition|*{FileConstants.MduFileExtension}";

        public int SolverType { private get; set; }

        public override bool Export(object item, string path)
        {
            if ((PolygonFile == null && NumDomains <= 0) ||
                !(item is WaterFlowFMModel waterFlowFMModel))
            {
                return false;
            }

            bool originalOutputOutOfSync = waterFlowFMModel.OutputOutOfSync;
            bool success = ExportPartitionMdu(waterFlowFMModel, path);
            waterFlowFMModel.OutputOutOfSync = originalOutputOutOfSync;

            return success;
        }

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
                string nonzeroPath = FilePath ?? path;

                string filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(nonzeroPath),
                                                               Path.GetFileNameWithoutExtension(nonzeroPath));

                string filePath = filePathWithoutExtension + FileConstants.MduFileExtension;
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

                partFileName += $"_part{FileConstants.PolylineFileExtension}";

                WriteNetPartition(api);
                WaterFlowFMProperty netFileProperty = modelDefinition.GetModelProperty(KnownProperties.NetFile);
                WaterFlowFMProperty partFileProperty = modelDefinition.GetModelProperty(KnownProperties.PartitionFile);

                string originalNetFile = netFileProperty.GetValueAsString();
                string originalPartFile = partFileProperty.GetValueAsString();

                var i = 0;
                foreach (string netFile in FindNetFiles(TargetNetFilePath))
                {
                    filePath = filePathWithoutExtension + "_" + string.Format("{0:0000}", i++) + FileConstants.MduFileExtension;

                    netFileProperty.SetValueFromString(netFile);
                    igcSolverProperty.SetValueFromString(SolverType > 0 ? SolverType.ToString() : originalSolverType);

                    var mduWriteConfig = new MduFileWriteConfig
                    {
                        WriteExtForcings = true,
                        WriteFeatures = true,
                        DisableFlowNodeRenumbering = waterFlowFMModel.DisableFlowNodeRenumbering
                    };
                    new MduFile().WriteProperties(filePath,
                                                  modelDefinition.Properties,
                                                  mduWriteConfig,
                                                  false);
                }

                netFileProperty.SetValueFromString(originalNetFile);
                partFileProperty.SetValueFromString(originalPartFile);
                igcSolverProperty.SetValueFromString(originalSolverType);
            }

            return true;
        }

        private static IEnumerable<string> FindNetFiles(string netFileName)
        {
            int index = netFileName.LastIndexOf($"{FileConstants.NetFileExtension}", StringComparison.InvariantCulture);
            if (index < 0)
            {
                yield break;
            }

            string strippedName = netFileName.Substring(0, index);
            bool fileFound;
            var i = 0;
            do
            {
                string fileName = strippedName + "_" + $"{i++:0000}" + $"{FileConstants.NetFileExtension}";
                fileFound = File.Exists(fileName);
                if (fileFound)
                {
                    yield return Path.GetFileName(fileName);
                }
            } while (fileFound);
        }
    }
}