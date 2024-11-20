using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class GeometryZipExporter: IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (GeometryZipExporter));

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { get; set; }

        #region IFileExporter

        public string Name { get { return "Net-geometry exporter"; } }
        public string Description { get { return Name; } }

        public string Category { get { return "General"; } }

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

            var model = GetModelForGrid(grid);

            var targetDirectory = Path.GetDirectoryName(filePath);
            var netFileName = Path.GetFileName(model.NetFilePath);
            var targetNetFilePath = Path.Combine(targetDirectory, netFileName);

            var k = 2;
            while (File.Exists(targetNetFilePath))
            {
                var newNetFileName = string.Concat(Path.GetFileNameWithoutExtension(netFileName), "(", k++, ")", ".nc");
                targetNetFilePath = Path.Combine(targetDirectory, newNetFileName);
            }

            var geomFileName = string.Concat(Path.GetFileNameWithoutExtension(netFileName), "geom.nc");
            var targetGeomFilePath = Path.Combine(targetDirectory, geomFileName);
            k = 2;
            while (File.Exists(targetGeomFilePath))
            {
                var newGeomFileName = string.Concat(Path.GetFileNameWithoutExtension(geomFileName), "(", k++, ")", ".nc");
                targetGeomFilePath = Path.Combine(targetDirectory, newGeomFileName);
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                WriteNetGeomApi.WriteNetGeometryFile(model, targetGeomFilePath);
                if (!File.Exists(targetGeomFilePath))
                {
                    return false;
                }
                File.Copy(model.NetFilePath, targetNetFilePath, true);
                
                BathymetryFileWriter.Write(targetGeomFilePath, model.ModelDefinition);
                
                Directory.SetCurrentDirectory(targetDirectory);
                ZipFileUtils.Create(filePath,
                    new List<string> {Path.GetFileName(targetNetFilePath), Path.GetFileName(targetGeomFilePath)});
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

        public string FileFilter { get { return "Zip file|*.zip"; } }

        public Bitmap Icon { get { return Properties.Resources.unstruc; } }

        public bool CanExportFor(object item)
        {
            // only support model bathymetry for UnstructuredGridCoverages (TOOLS-22932)
            var unstructuredGridCoverage = item as UnstructuredGridCoverage;

            return unstructuredGridCoverage == null ||
                   (GetModelForGrid != null && (GetModelForGrid(unstructuredGridCoverage.Grid) == null 
                   || GetModelForGrid(unstructuredGridCoverage.Grid).Bathymetry == unstructuredGridCoverage));
        }

        #endregion
    }
}
