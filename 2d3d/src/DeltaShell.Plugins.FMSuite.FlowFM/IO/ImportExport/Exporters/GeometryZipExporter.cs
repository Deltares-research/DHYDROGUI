using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class GeometryZipExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GeometryZipExporter));

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { get; set; }

        #region IFileExporter

        public string Name => "Net-geometry exporter";

        public string Category => "General";

        public string Description => string.Empty;

        public bool Export(object item, string path)
        {
            var grid = item as UnstructuredGrid;
            if (grid != null)
            {
                return ExportGrid(path, grid);
            }

            var coverage = item as UnstructuredGridCoverage;
            if (coverage != null)
            {
                return ExportGrid(path, coverage.Grid);
            }

            return false;
        }

        private bool ExportGrid(string filePath, UnstructuredGrid grid)
        {
            if (grid == null || grid.IsEmpty)
            {
                Log.Warn(Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct);
                return false;
            }

            if (GetModelForGrid == null || GetModelForGrid(grid) == null)
            {
                throw new NotImplementedException("Exporting netcdf file of non-filebased grid not supported yet");
            }

            WaterFlowFMModel model = GetModelForGrid(grid);

            string targetDirectory = Path.GetDirectoryName(filePath);
            string netFileName = Path.GetFileName(model.NetFilePath);
            string targetNetFilePath = Path.Combine(targetDirectory, netFileName);

            var k = 2;
            while (File.Exists(targetNetFilePath))
            {
                string newNetFileName =
                    string.Concat(Path.GetFileNameWithoutExtension(netFileName), "(", k++, ")", FileConstants.NetCdfFileExtension);
                targetNetFilePath = Path.Combine(targetDirectory, newNetFileName);
            }

            string geomFileName = string.Concat(Path.GetFileNameWithoutExtension(netFileName), FileConstants.GeomFileExtension);
            string targetGeomFilePath = Path.Combine(targetDirectory, geomFileName);
            k = 2;
            while (File.Exists(targetGeomFilePath))
            {
                string newGeomFileName =
                    string.Concat(Path.GetFileNameWithoutExtension(geomFileName), "(", k++, ")", FileConstants.NetCdfFileExtension);
                targetGeomFilePath = Path.Combine(targetDirectory, newGeomFileName);
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                WriteNetGeomApi.WriteNetGeometryFile(model, targetGeomFilePath);
                if (!File.Exists(targetGeomFilePath))
                {
                    return false;
                }

                File.Copy(model.NetFilePath, targetNetFilePath, true);

                if (model.MduFile != null)
                {
                    model.MduFile.WriteBathymetry(model.ModelDefinition, targetGeomFilePath);
                }

                Directory.SetCurrentDirectory(targetDirectory);
                ZipFileUtils.Create(filePath,
                                    new List<string>
                                    {
                                        Path.GetFileName(targetNetFilePath),
                                        Path.GetFileName(targetGeomFilePath)
                                    });
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Export to geometry net-file failed: {0}", e.Message);
                return false;
            }
            finally
            {
                if (File.Exists(targetNetFilePath))
                {
                    File.Delete(targetNetFilePath);
                }

                if (File.Exists(targetGeomFilePath))
                {
                    File.Delete(targetGeomFilePath);
                }

                Directory.SetCurrentDirectory(currentDirectory);
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(UnstructuredGrid);
            yield return typeof(UnstructuredGridCoverage);
        }

        public string FileFilter => "Zip file|*.zip";

        public Bitmap Icon => Resources.unstruc;

        /// <summary>
        /// Checks if the item can be exported.
        /// For UnstructuredGridCoverages only the bathymetry coverage of the model can be exported [TOOLS-22932].
        /// </summary>
        /// <param name="item">Item to export</param>
        /// <returns>A boolean indicating whether this exporter can export the provided <paramref name="item"/></returns>
        public bool CanExportFor(object item)
        {
            if (item is UnstructuredGrid)
            {
                return true;
            }

            if (!(item is UnstructuredGridCoverage unstructuredGridCoverage))
            {
                return false;
            }

            return GetModelForGrid?.Invoke(unstructuredGridCoverage.Grid)?.SpatialData.Bathymetry == unstructuredGridCoverage;
        }

        #endregion
    }
}