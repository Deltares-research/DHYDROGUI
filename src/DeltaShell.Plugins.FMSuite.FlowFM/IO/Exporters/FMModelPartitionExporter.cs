using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class FMModelPartitionExporter : FMPartitionExporterBase
    {
        public int SolverType { private get; set; }

        public override bool Export(object item, string path)
        {
            if (PolygonFile == null && NumDomains <= 0) return false;

            var waterFlowFMModel = item as WaterFlowFMModel;
            if (waterFlowFMModel != null)
            {
                ExportPartitionMdu(waterFlowFMModel, path);
                return true;
            }

            return false;
        }

        private void ExportPartitionMdu(WaterFlowFMModel waterFlowFMModel, string path)
        {
            using (var api = new RemoteFlexibleMeshModelApi())
            {
                var nonzeroPath = FilePath ?? path;

                var filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(nonzeroPath),
                    Path.GetFileNameWithoutExtension(nonzeroPath));

                var filePath = filePathWithoutExtension + ".mdu";
                var modelDefinition = waterFlowFMModel.ModelDefinition;
                var igcSolverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
                var originalSolverType = igcSolverProperty.GetValueAsString();
                igcSolverProperty.SetValueAsString("2"); //ensure init works

                waterFlowFMModel.ExportTo(filePath, false);
                
                if (PolygonFile == null && NumDomains == 1) return;

                SourceNetFilePath = waterFlowFMModel.NetFilePath;

                var directory = Path.GetDirectoryName(filePath);

                TargetNetFilePath = directory == null
                    ? waterFlowFMModel.NetFilePath
                    : Path.Combine(directory, Path.GetFileName(waterFlowFMModel.NetFilePath));

                var partFileName = Path.GetFileNameWithoutExtension(TargetNetFilePath);
                if (partFileName.EndsWith("_net"))
                {
                    partFileName = partFileName.Substring(0, partFileName.Count() - 4);
                }
                partFileName += "_part.pol";

                WriteNetPartition(api);
                var netFileProperty = modelDefinition.GetModelProperty(KnownProperties.NetFile);
                var partFileProperty = modelDefinition.GetModelProperty(KnownProperties.PartitionFile);

                var originalNetFile = netFileProperty.GetValueAsString();
                var originalPartFile = partFileProperty.GetValueAsString();

                var i = 0;
                foreach (var netFile in FindNetFiles(TargetNetFilePath))
                {
                    filePath = filePathWithoutExtension + "_" + string.Format("{0:0000}", i++) + ".mdu";

                    netFileProperty.SetValueAsString(netFile);
                    igcSolverProperty.SetValueAsString(SolverType > 0 ? SolverType.ToString() : originalSolverType);
                    new MduFile().WriteProperties(filePath, modelDefinition.Properties, true, true, false, modelDefinition.IsPartOf1D2DModel, waterFlowFMModel.DisableFlowNodeRenumbering);
                }
                netFileProperty.SetValueAsString(originalNetFile);
                partFileProperty.SetValueAsString(originalPartFile);
                igcSolverProperty.SetValueAsString(originalSolverType);
            }
        }

        private static IEnumerable<string> FindNetFiles(string netFileName)
        {
            var index = netFileName.LastIndexOf("_net.nc", StringComparison.InvariantCulture);
            if (index < 0)
            {
                yield break;
            }
            var strippedName = netFileName.Substring(0, index);
            bool fileFound;
            var i = 0;
            do
            {
                var fileName = strippedName + "_" + string.Format("{0:0000}", i++) + "_net.nc";
                fileFound = File.Exists(fileName);
                if (fileFound) yield return Path.GetFileName(fileName);
            }
            while (fileFound);
        }

        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof (WaterFlowFMModel);
        }

        public override string FileFilter
        {
            get { return "Flexible Mesh Model Definition|*.mdu"; }
        }
    }
}
