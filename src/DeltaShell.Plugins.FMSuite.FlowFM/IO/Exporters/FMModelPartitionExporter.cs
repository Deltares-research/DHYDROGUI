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
            return waterFlowFMModel != null && ExportPartitionMdu(waterFlowFMModel, path);
        }

        private bool ExportPartitionMdu(WaterFlowFMModel waterFlowFMModel, string path)
        {
            var api = FlexibleMeshModelApiFactory.CreateNew();
            if (api == null)
            {
                return false;
            }

            using (api)
            {
                var nonzeroPath = FilePath ?? path;

                var filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(nonzeroPath),
                    Path.GetFileNameWithoutExtension(nonzeroPath));

                var filePath = filePathWithoutExtension + ".mdu";
                var modelDefinition = waterFlowFMModel.ModelDefinition;
                var igcSolverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
                var originalSolverType = igcSolverProperty.GetValueAsString();
                igcSolverProperty.SetValueFromString("2"); //ensure init works

                waterFlowFMModel.ExportTo(filePath, false);
                
                if (PolygonFile == null && NumDomains == 1) return true;

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
