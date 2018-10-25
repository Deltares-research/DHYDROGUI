using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetCDF.Builders;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class RasterFile
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RasterFile));

        private static double FileSizeWarningLimitInBytes = 1e9;

        private static double FileSizeErrorLimitInBytes = 2.0e9;

        public const string AscExtension = ".asc";

        public const string TiffExtension = ".tif";

        /// <summary>
        /// Read the Asc from the provided <param name="ascFilePath"/>
        /// </summary>
        /// <param name="ascFilePath">Path to the Asc file</param>
        /// <param name="checkForUnsupportedSize">Check if the file size is not to big (> 2.0 gb)</param>
        /// <returns>List of points with a value, or null if file is too large</returns>
        /// <exception cref="FormatException">When less then 3 values are found or the values are invalid</exception>
        public static IEnumerable<IPointValue> Read(string ascFilePath, bool checkForUnsupportedSize = false)
        {
            if (checkForUnsupportedSize)
            {
                LogFileSizeMessageIfNeeded(ascFilePath);

                if (!IsFileSizeAccepted(ascFilePath))
                {
                    return null;
                }
            }

            var regularGridCoverage = ReadAscFileToRegularGridCoverage(ascFilePath);
            var pointValuesList = ConvertRegularGridToBedLevelValues(regularGridCoverage);

            return pointValuesList;
        }

        /// <summary>
        /// Write the xyValuePoints to the filePath provided
        /// </summary>
        /// <param name="filePath">The FilePath to write the xyValuePoints to</param>
        /// <param name="xyValuePoints">The xyValues to write to the FilePath</param>
        public void Write(string filePath, IEnumerable<IPointValue> xyValuePoints)
        {

            var importer = new GdalFileExporter();
            var builder = new RegularGridCoverageBuilder();

            IList<IVariable> variables = new List<IVariable>();

            var variable = new Variable<int>();
            variable.Attributes["coordinates"] = "y x";
            var pointValues = xyValuePoints.ToList();
            variable.SetValues(pointValues.Select(xyValuePoint => xyValuePoint.Value));

            var x = new Variable<double>();
            x.Attributes["standard_name"] = "projection_x_coordinate";
            x.SetValues(pointValues.Select(xyValuePoint => xyValuePoint.X));

            var y = new Variable<double>();
            y.Attributes["standard_name"] = "projection_y_coordinate";
            y.SetValues(pointValues.Select(xyValuePoint => xyValuePoint.Y));

            variable.Arguments.AddRange(new[] { x, y });

            variables.Add(variable);
            variables.Add(x);
            variables.Add(y);


            var grid = builder.CreateFunction(variables);

            importer.Export(grid, filePath) ;
        }

        /// <summary>
        /// Check on whether the file provided is within acceptable size limits
        /// </summary>
        /// <param name="filePath">The path of the file to check</param>
        /// <returns>True or False</returns>
        public static bool IsFileSizeAccepted(string filePath)
        {
            var fileSizeInBytes = GetFileSize(filePath);
            if (!(fileSizeInBytes > FileSizeWarningLimitInBytes)) return true;

            return fileSizeInBytes <= FileSizeErrorLimitInBytes;
        }

        /// <summary>
        /// Logs messsage (warning or error) if filesize is over acceptable limits
        /// </summary>
        /// <param name="filePath">The filepath to check</param>
        public static void LogFileSizeMessageIfNeeded(string filePath)
        {
            var fileSizeInBytes = GetFileSize(filePath);

            if (fileSizeInBytes <= FileSizeWarningLimitInBytes) return;

            var fileSizeGreaterThanErrorLimit = fileSizeInBytes > FileSizeErrorLimitInBytes;

            var limitInGb = FileUtils.GetReadableFileSize(fileSizeGreaterThanErrorLimit ? FileSizeErrorLimitInBytes : FileSizeWarningLimitInBytes);
            var message = string.Format("The file '{0}' is greater than {1} in size, ", Path.GetFileName(filePath), limitInGb);

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

            var f = new FileInfo(filePath);

            var fileSizeInBytes = f.Length;
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

            var values = gridCoverage.GetValues<float>();

            var pointValueList = new List<IPointValue>();

            try
            {
                for (var i = 0; i < yValues.Count; i++)
                {
                    for (var j = 0; j < xValues.Count; j++)
                    {
                        var pointValue = new PointValue
                        {
                            X = xValues[j] + deltaX,
                            Y = yValues[i] + deltaY,
                            Value = values[j+i*xValues.Count]
                        };
                        pointValueList.Add(pointValue);
                    }
                }
            }
            catch (Exception)
            {
                Log.Error(Resources.RasterBedLevelFileImporter_ConvertRegularGridToBedLevelValues_The_file_you_are_trying_to_import_only_contains_integers__This_is_not_yet_supported__Please_change_a_minimum_of_one_value_to_a_decimal_number_in_the_import_file);
            }

            return pointValueList;
        }

    }
}