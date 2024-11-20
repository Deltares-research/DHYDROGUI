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
    /// <summary>
    /// Provides a grid file partition exporter.
    /// </summary>
    public class FMGridPartitionExporter : FMPartitionExporterBase
    {
        /// <inheritdoc/>
        public override string FileFilter => $"Flexible Mesh Net File|*{FileConstants.NetFileExtension}";

        /// <summary>
        /// Gets or sets the path to export the grid file to.
        /// </summary>
        public string FilePath { get; set; }

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }
        
        /// <inheritdoc/>
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(UnstructuredGrid);
            yield return typeof(ImportedFMNetFile);
        }

        /// <inheritdoc/>
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
    }
}