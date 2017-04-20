using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using Resources = DeltaShell.Plugins.FMSuite.FlowFM.Properties.Resources;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class FlowFMNetFileExporter: IFileExporter
    {
        private ILog Log = LogManager.GetLogger(typeof (FlowFMNetFileExporter));

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }

        #region IFileExporter

        public string Name { get { return "Grid exporter"; } }
        
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

        private bool ExportGrid(string path, UnstructuredGrid grid)
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
                    NetFile.Write(path,grid);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Failed to export unstructured grid: {0}", e.Message);
                    return false;
                }
                return true;
            }

            var model = GetModelForGrid(grid);

            if (path != model.NetFilePath)
            {
                if (File.Exists(model.NetFilePath))
                {
                    File.Copy(model.NetFilePath, path, true);
                }
                else
                {
                    try
                    {
                        NetFile.Write(path,grid);
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
                UnstructuredGridFileHelper.WriteZValues(path, model.Bathymetry.Components[0].GetValues<double>().ToArray());
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (UnstructuredGrid);
            yield return typeof (ImportedFMNetFile);
            yield return typeof (UnstructuredGridCoverage);
        }

        public string FileFilter { get { return "Net file|*.nc"; } }
        
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
