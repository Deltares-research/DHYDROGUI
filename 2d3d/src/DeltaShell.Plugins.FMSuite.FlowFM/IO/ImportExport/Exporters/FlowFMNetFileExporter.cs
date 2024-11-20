using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class FlowFMNetFileExporter : IFileExporter
    {
        private ILog Log = LogManager.GetLogger(typeof(FlowFMNetFileExporter));

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { private get; set; }

        #region IFileExporter

        [ExcludeFromCodeCoverage]
        public string Name => "Grid exporter";

        [ExcludeFromCodeCoverage]
        public string Category => "General";

        public string Description => string.Empty;

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
                    NetFile.Write(path, grid);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Failed to export unstructured grid: {0}", e.Message);
                    return false;
                }

                return true;
            }

            WaterFlowFMModel model = GetModelForGrid(grid);

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
                        NetFile.Write(path, grid);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Failed to export unstructured grid: {0}", e.Message);
                        return false;
                    }
                }
            }

            if (!grid.IsEmpty && model.MduFile != null)
            {
                model.MduFile.WriteBathymetry(model.ModelDefinition, path);
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(UnstructuredGrid);
            yield return typeof(ImportedFMNetFile);
            yield return typeof(UnstructuredGridCoverage);
        }

        [ExcludeFromCodeCoverage]
        public string FileFilter => $"Net file|*{FileConstants.NetCdfFileExtension}";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.unstruc;

        /// <summary>
        /// Checks if the item can be exported.
        /// For UnstructuredGridCoverages only the bathymetry coverage of the model can be exported [TOOLS-22932].
        /// </summary>
        /// <param name="item">Item to export</param>
        /// <returns>A boolean indicating whether this exporter can export the provided <paramref name="item"/></returns>
        public bool CanExportFor(object item)
        {
            if (item is UnstructuredGrid || item is ImportedFMNetFile)
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