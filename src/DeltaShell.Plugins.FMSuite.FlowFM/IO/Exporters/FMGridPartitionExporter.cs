using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class FMGridPartitionExporter : FMPartitionExporterBase
    {
        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }

        #region IFileExporter

        public override bool Export(object item, string path)
        {
            if (PolygonFile == null && NumDomains <= 0) return false;

            var importedNetFile = item as ImportedFMNetFile;
            if (importedNetFile != null)
            {
                ExportPartitionGrid(importedNetFile.Path, path);
                return true;
            }

            var unstructuredGrid = item as UnstructuredGrid;
            if (unstructuredGrid != null)
            {
                if (GetModelForGrid == null || GetModelForGrid(unstructuredGrid) == null)
                {
                    throw new NotImplementedException(
                        "Cannot export unstructured grid to partition without parent FM model");
                }
                var model = GetModelForGrid(unstructuredGrid);
                var netFilePath = Path.Combine(Path.GetDirectoryName(model.MduFilePath), model.NetFilePath);
                ExportPartitionGrid(Path.GetFullPath(netFilePath), path);
                return true;
            }

            return false;
        }

        private void ExportPartitionGrid(string netFilePath, string path)
        {
            var nonzeroPath = FilePath ?? path;
            var filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(nonzeroPath),
                Path.GetFileNameWithoutExtension(nonzeroPath));
            TargetNetFilePath = filePathWithoutExtension + "_net.nc";
            SourceNetFilePath = netFilePath;

            if (PolygonFile == null && NumDomains == 1)
            {
                File.Copy(netFilePath, TargetNetFilePath, true);
                return;
            }
            using (var api = new RemoteFlexibleMeshModelApi())
            {
                WriteNetPartition(api);
            }
        }

        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof (UnstructuredGrid);
            yield return typeof (ImportedFMNetFile);
        }

        public override string FileFilter
        {
            get { return "Flexible Mesh Net File|*_net.nc"; }
        }

        #endregion
    }
}
