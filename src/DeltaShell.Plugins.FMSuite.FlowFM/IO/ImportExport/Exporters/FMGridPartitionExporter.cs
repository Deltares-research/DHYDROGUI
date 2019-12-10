using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class FMGridPartitionExporter : FMPartitionExporterBase
    {
        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }

        #region IFileExporter

        public override bool Export(object item, string path)
        {
            if (PolygonFile == null && NumDomains <= 0)
            {
                return false;
            }

            var importedNetFile = item as ImportedFMNetFile;
            if (importedNetFile != null)
            {
                return ExportPartitionGrid(importedNetFile.Path, path);
            }

            var unstructuredGrid = item as UnstructuredGrid;
            if (unstructuredGrid != null)
            {
                if (GetModelForGrid == null || GetModelForGrid(unstructuredGrid) == null)
                {
                    throw new NotImplementedException(
                        "Cannot export unstructured grid to partition without parent FM model");
                }

                WaterFlowFMModel model = GetModelForGrid(unstructuredGrid);
                string netFilePath = Path.Combine(Path.GetDirectoryName(model.MduFilePath), model.NetFilePath);
                return ExportPartitionGrid(Path.GetFullPath(netFilePath), path);
            }

            return false;
        }

        private bool ExportPartitionGrid(string netFilePath, string path)
        {
            string nonzeroPath = FilePath ?? path;
            string filePathWithoutExtension = Path.Combine(Path.GetDirectoryName(nonzeroPath),
                                                           Path.GetFileNameWithoutExtension(nonzeroPath));
            TargetNetFilePath = filePathWithoutExtension + FileConstants.NetFileExtension;
            SourceNetFilePath = netFilePath;

            if (PolygonFile == null && NumDomains == 1)
            {
                File.Copy(netFilePath, TargetNetFilePath, true);
                return true;
            }

            IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
            if (api == null)
            {
                return false;
            }

            using (api)
            {
                WriteNetPartition(api);
            }

            return true;
        }

        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(UnstructuredGrid);
            yield return typeof(ImportedFMNetFile);
        }

        public override string FileFilter => $"Flexible Mesh Net File|*{FileConstants.NetFileExtension}";

        #endregion
    }
}