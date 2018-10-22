using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;


namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public static class RegularGridCoverageConvertExtensions
    {
        public static UnstructuredGrid ConvertRegularGridCoverage(this IRegularGridCoverage gridCoverage)
        {
            var vertices = new List<Coordinate>();
            var edges = new List<Edge>();
            var cellToVertex = new List<IList<int>>();
            var previousHorizontalEdges = new List<Edge>();
            Edge previousVerticalEdge = null;
            var currentCellIndex = 0;

            var xValues = gridCoverage.X.Values;
            var yValues = gridCoverage.Y.Values;

            var numbOfPointsHorizontal = xValues.Count + 1;
            var numbOfPointsVertical = yValues.Count + 1;


            for (int verticalIndex = 0; verticalIndex < numbOfPointsVertical; verticalIndex++)
            {
                var horizontalOffset = numbOfPointsHorizontal * verticalIndex;
                for (int horizontalIndex = 0; horizontalIndex < numbOfPointsHorizontal; horizontalIndex++)
                {
                    var x = gridCoverage.Origin.X + (horizontalIndex * gridCoverage.DeltaX);
                    var y = gridCoverage.Origin.Y + (verticalIndex * gridCoverage.DeltaY);

                    vertices.Add(new Coordinate(x, y, 0));

                    var currentPointIndex = horizontalOffset + horizontalIndex;

                    if (horizontalIndex != 0)
                    {
                        if (previousHorizontalEdges.Count == xValues.Count)
                        {
                            previousHorizontalEdges.RemoveAt(0); // only cache the last row of horizontal edges
                        }

                        // create horizontal edge
                        var horizontalEdge = new Edge(currentPointIndex - 1, currentPointIndex);
                        previousHorizontalEdges.Add(horizontalEdge);
                        edges.Add(horizontalEdge);
                    }

                    if (verticalIndex != 0)
                    {
                        // create vertical edge
                        previousVerticalEdge = new Edge(currentPointIndex - numbOfPointsHorizontal,
                            currentPointIndex);
                        edges.Add(previousVerticalEdge);
                    }

                    if (horizontalIndex == 0 || verticalIndex == 0)
                        continue;

                    cellToVertex.Add(new[]
                    {
                        currentPointIndex - numbOfPointsHorizontal - 1,
                        currentPointIndex - numbOfPointsHorizontal,
                        currentPointIndex,
                        currentPointIndex - 1,
                    });

                    currentCellIndex++;
                }
            }

            var grid = new UnstructuredGrid
            {
                Vertices = vertices,
                Edges = edges,
            };

            grid.Cells = cellToVertex.Select(c => new Cell(c.ToArray(), grid)).ToList();
            return grid;
        }
    }

    public class RasterFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RasterFileImporter));
        //TODO: Implement file size limit of 2GB.
        //private static double AscFileSizeErrorLimitInBytes = 2.0e9;

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { get; set; }

        private static IRegularGridCoverage ImportAscFileToRegularGridCoverage(string ascFilePath)
        {
            var importer = new GdalFileImporter();
            var regularGrid = importer.ImportItem(ascFilePath) as IRegularGridCoverage;

            return regularGrid;
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Could not find file {0}", path));
            }

            if (target == null)
            {
                return new DataItem { Value = new ImportedFMNetFile(path), Name = Path.GetFileName(path) };
            }

            var flowModel = target as WaterFlowFMModel;
            if (flowModel == null)
            {
                flowModel = GetModelForGrid(target as UnstructuredGrid);
            }

            if (flowModel.Grid.Cells.Any())
            {
                Log.Error(Resources.RasterFileImporter_ImportItem_There_is_already_a_grid_present__Remove_the_current_grid_before_importing_a_new_one_);
                return null;
            }

            var regularGridCoverage = ImportAscFileToRegularGridCoverage(path);
            var grid = regularGridCoverage.ConvertRegularGridCoverage();
            if (grid != null)
            {
//                try
//                {
//                  flowModel.BeginEdit(new DefaultEditAction("Reset grid"));
                    flowModel.Grid = grid;
//                }
//                finally
//                {
//                    flowModel.EndEdit();
//                }

                flowModel.ReloadGrid();

                return flowModel.Grid;
            }
            return null;
        }


        public string Name
        {
            get { return "Raster Grid Importer"; }
        }

        public string Category
        {
            get {return "2D / 3D";}
        }

        public Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(UnstructuredGrid); }
        }
        public bool CanImportOnRootLevel { get; }

        public string FileFilter
        {
            get { string fileFilter = "";
                fileFilter += "All supported raster formats|*.asc;*.bil;*.tif;*.tiff;*.map";
                fileFilter += "|" + "Arc/Info ASCII Grid (*.asc)|*.asc";
                fileFilter += "|" + "ESRI .hdr Labelled (*.bil)|*.bil";
                fileFilter += "|" + "TIF Tagget Image File Format (*.tif)|*.tif;*.tiff";
                fileFilter += "|" + "PCRaster raster file format (*.map)|*.map"; ;
                return fileFilter;
            }
           
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; }
    }
}
