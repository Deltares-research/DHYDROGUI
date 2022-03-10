using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.NetCDF.Builders;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public static class RasterFile
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RasterFile));

        private static double FileSizeWarningLimitInBytes = 1e9;

        private static double FileSizeErrorLimitInBytes = 2.0e9;

        public const string AscExtension = ".asc";

        public const string TiffExtension = ".tif";

        /// <summary>
        /// Read the raster from the provided <param name="rasterFilePath"/>
        /// </summary>
        /// <param name="rasterFilePath">Path to the Asc file</param>
        /// <param name="checkForUnsupportedSize">Check if the file size is not to big (> 2.0 gb)</param>
        /// <returns>List of points with a value, or null if file is too large</returns>
        /// <exception cref="FormatException">When less then 3 values are found or the values are invalid</exception>
        public static IEnumerable<IPointValue> ReadPointValues(string rasterFilePath, IRegularGridCoverage gridCoverage = null, bool checkForUnsupportedSize = false)
        {
            if (checkForUnsupportedSize)
            {
                LogFileSizeMessageIfNeeded(rasterFilePath);

                if (!IsFileSizeAccepted(rasterFilePath))
                {
                    return null;
                }
            }

            if(gridCoverage == null) gridCoverage = ReadAscFileToRegularGridCoverage(rasterFilePath);
            var pointValuesList = ConvertRegularGridToBedLevelValues(gridCoverage);

            return pointValuesList;
        }

        /// <summary>
        /// Read the raster from the provided <param name="rasterFilePath"/>
        /// </summary>
        /// <param name="rasterFilePath">Path to the Asc file</param>
        /// <param name="regularGridCoverage"></param>
        /// <param name="checkForUnsupportedSize">Check if the file size is not to big (> 2.0 gb)</param>
        /// <returns>List of points with a value, or null if file is too large</returns>
        /// <exception cref="FormatException">When less then 3 values are found or the values are invalid</exception>
        public static UnstructuredGrid ReadUnstructuredGrid(string rasterFilePath, out IRegularGridCoverage regularGridCoverage, bool checkForUnsupportedSize = false)
        {
            if (checkForUnsupportedSize)
            {
                LogFileSizeMessageIfNeeded(rasterFilePath);

                if (!IsFileSizeAccepted(rasterFilePath))
                {
                    regularGridCoverage = null;
                    return null;
                }
            }

            regularGridCoverage = ReadAscFileToRegularGridCoverage(rasterFilePath);

            return ConvertRegularGridCoverageToUnstructuredGrid(regularGridCoverage);
        }

        private static UnstructuredGrid ConvertRegularGridCoverageToUnstructuredGrid(IRegularGridCoverage gridCoverage)
        {
            var vertices = new List<Coordinate>();
            var edges = new List<Edge>();
            var cellToVertex = new List<IList<int>>();
            var previousHorizontalEdges = new List<Edge>();
            Edge previousVerticalEdge = null;

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
                }
            }

            var grid = new UnstructuredGrid
            {
                Vertices = vertices,
                Edges = edges
            };

            grid.Cells = cellToVertex.Select(c => new Cell(c.ToArray(), vertices)).ToList();
            return grid;
        }

        /// <summary>
        /// Check on whether the file provided is within acceptable size limits
        /// </summary>
        /// <param name="filePath">The path of the file to check</param>
        /// <returns>True or False</returns>
        private static bool IsFileSizeAccepted(string filePath)
        {
            var fileSizeInBytes = GetFileSize(filePath);
            if (!(fileSizeInBytes > FileSizeWarningLimitInBytes)) return true;

            return fileSizeInBytes <= FileSizeErrorLimitInBytes;
        }

        /// <summary>
        /// Logs messsage (warning or error) if filesize is over acceptable limits
        /// </summary>
        /// <param name="filePath">The filepath to check</param>
        private static void LogFileSizeMessageIfNeeded(string filePath)
        {
            var fileSizeInBytes = GetFileSize(filePath);

            if (fileSizeInBytes <= FileSizeWarningLimitInBytes) return;

            var fileSizeGreaterThanErrorLimit = fileSizeInBytes > FileSizeErrorLimitInBytes;

            var limitInGb = FileUtils.GetReadableFileSize(fileSizeGreaterThanErrorLimit ? FileSizeErrorLimitInBytes : FileSizeWarningLimitInBytes);
            var message = $"The file '{Path.GetFileName(filePath)}' is greater than {limitInGb} in size, ";

            if (fileSizeGreaterThanErrorLimit)
            {
                Log.Error(message + "the application can\'t handle these file sizes. Try importing a smaller file.");
                return;
            }

            Log.Warn(message + "loading the file and rendering the data might take a while");
        }

        private static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath)) return 0;

            var fileInfo = new FileInfo(filePath);

            var fileSizeInBytes = fileInfo.Length;
            return fileSizeInBytes;
        }

        private static IRegularGridCoverage ReadAscFileToRegularGridCoverage(string ascFilePath)
        {
            var importer = new GdalFileImporter();
            var regularGrid = importer.ImportItem(ascFilePath) as IRegularGridCoverage;

            return regularGrid;
        }

        private static IEnumerable<IPointValue> ConvertRegularGridToBedLevelValues(IRegularGridCoverage gridCoverage)
        {
            var xValues = gridCoverage.X.Values;
            var yValues = gridCoverage.Y.Values;

            //Insert values at the center of the cell
            var deltaX = gridCoverage.DeltaX / 2.0;
            var deltaY = gridCoverage.DeltaY / 2.0;

            var values = gridCoverage.Components[0].ValueType == typeof(float)
                ? new ConvertedArray<double, float>(gridCoverage.GetValues<float>(), Convert.ToSingle, Convert.ToDouble)
                : gridCoverage.Components[0].ValueType == typeof(int)
                    ? new ConvertedArray<double, int>(gridCoverage.GetValues<int>(), Convert.ToInt32, Convert.ToDouble)
                    : gridCoverage.GetValues<double>();
            
            for (var i = 0; i < yValues.Count; i++)
            {
                for (var j = 0; j < xValues.Count; j++)
                {
                    var pointValue = new PointValue
                    {
                        X = xValues[j] + deltaX,
                        Y = yValues[i] + deltaY,
                        Value = values[j + i * xValues.Count]
                    };

                    if(gridCoverage.Components[0].NoDataValues.Contains(pointValue.Value) || Math.Abs(pointValue.Value - (-999.0)) < 0.0001) continue;

                    yield return pointValue;
                }
            }
        }

    }
}