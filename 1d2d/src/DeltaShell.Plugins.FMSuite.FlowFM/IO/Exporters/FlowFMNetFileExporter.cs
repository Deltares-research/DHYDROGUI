using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class FlowFMNetFileExporter: IFileExporter
    {
        private ILog Log = LogManager.GetLogger(typeof (FlowFMNetFileExporter));

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }

        #region IFileExporter

        [ExcludeFromCodeCoverage]
        public string Name { get { return "Grid exporter"; } }
        public string Description { get { return Name; } }

        [ExcludeFromCodeCoverage]
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var importedGrid = item as ImportedFMNetFile;
            if (importedGrid != null)
            {
                if (Path.GetFullPath(importedGrid.Path) != Path.GetFullPath(path))
                {
                    File.Copy(importedGrid.Path, path, true);
                }
                return true;
            }

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
                try
                {
                    NetFile.Write(filePath, grid);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Failed to export unstructured grid: {0}", e.Message);
                    return false;
                }
                return true;
            }

            var model = GetModelForGrid(grid);

            if (filePath != model.NetFilePath)
            {
                if (File.Exists(model.NetFilePath))
                {
                    File.Copy(model.NetFilePath, filePath, true);
                }
                else
                {
                    try
                    {
                        NetFile.Write(filePath, grid);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Failed to export unstructured grid: {0}", e.Message);
                        return false;
                    }
                }
            }

            if (!grid.IsEmpty)
            {
                BathymetryFileWriter.Write(filePath, model.ModelDefinition);
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (UnstructuredGrid);
            yield return typeof (ImportedFMNetFile);
            yield return typeof (UnstructuredGridCoverage);
        }

        [ExcludeFromCodeCoverage]
        public string FileFilter { get { return "Net file|*.nc"; } }

        [ExcludeFromCodeCoverage]
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
