using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.NetCdf;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// This class contains helpers but should be removed when the kernels are compliant with the manual.
    /// </summary>
    public static class FMHisFileFunctionStoreHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMHisFileFunctionStoreHelper));

        public static IDictionary<string, Func<string[], int, double[], double[], IEnumerable<IFeature>>> OutputStructuresGenerators =
            new Dictionary<string, Func<string[], int, double[], double[], IEnumerable<IFeature>>>
            {
                {"stations", GenerateOutputStations},
                {"cross_section", GenerateGenerateOutputCrossSections},
                {"dambreaks", GenerateGenerateOutputDamBreaks}
            };

        public static IDictionary<string, Func<NetCdfFile, CoordinateLocation, Array>> OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute = new Dictionary<string, Func<NetCdfFile, CoordinateLocation, Array>>
        {
            {"cross_section", GetXOrYCoordinateBecauseOfStupidKernelMistakeForCrossSections }
        };

        public enum CoordinateLocation
        {
            X,
            Y,
            Name,
            Pts
        }

        private static Array GetXOrYCoordinateBecauseOfStupidKernelMistakeForCrossSections(NetCdfFile netCdfFile, CoordinateLocation xOrY)
        {
            NetCdfVariable crossSectionCoordinateNetCdfVariable = null;
            switch (xOrY)
            {
                case CoordinateLocation.X:
                    crossSectionCoordinateNetCdfVariable = netCdfFile.GetVariableByName("cross_section_x_coordinate");
                    break;
                    
                case CoordinateLocation.Y:
                    crossSectionCoordinateNetCdfVariable = netCdfFile.GetVariableByName("cross_section_y_coordinate");
                    break;
                case CoordinateLocation.Name:
                    crossSectionCoordinateNetCdfVariable = netCdfFile.GetVariableByName("cross_section_name");
                    break;
                case CoordinateLocation.Pts:
                    var netCdfDimension = netCdfFile.GetDimension("cross_section_pts");
                    if (netCdfDimension == null) return new[]{1};
                    return new[] {netCdfFile.GetDimensionLength(netCdfDimension)};
                default:
                    throw new ArgumentOutOfRangeException(nameof(xOrY), xOrY, null);
            }
            if (crossSectionCoordinateNetCdfVariable == null) return new object[0];
            return netCdfFile.Read(crossSectionCoordinateNetCdfVariable);
        }

        private static IEnumerable<IFeature> GenerateOutputStations(string[] ids, int length, double[] xCoordinates, double[] yCoordinates)
        {
            if (ids == null)
            {
                Log.Warn("Cannot generate output station without ids");
                yield break;
            }
            if (xCoordinates == null)
            {
                Log.Warn("Cannot generate output station without xCoordinates");
                yield break;
            }

            if (yCoordinates == null)
            {
                Log.Warn("Cannot generate output station without yCoordinates");
                yield break;
            }
            if (ids.Length != xCoordinates.Length || xCoordinates.Length != yCoordinates.Length)
            {
                Log.Warn("Length of ids, x and y coordinate list are not equal");
                yield break;
            }
            for (int i = 0; i < xCoordinates.Length; i++)
            {
                yield return new Feature2D{Name = ids[i], Geometry = new Point(xCoordinates[i], yCoordinates[i])};
            }
        }

        private static IEnumerable<IFeature> GenerateGenerateOutputCrossSections(string[] ids, int length, double[] xCoordinates, double[] yCoordinates)
        {
            if (ids == null)
            {
                Log.Warn("Cannot generate output cs without ids");
                yield break;
            }
            if (xCoordinates == null)
            {
                Log.Warn("Cannot generate output cs without xCoordinates");
                yield break;
            }

            if (yCoordinates == null)
            {
                Log.Warn("Cannot generate output cs without yCoordinates");
                yield break;
            }
            for (int i = 0; i < ids.Length; i++)
            {
                var coordinates = new List<Coordinate>();
                for (int j = 0; j < length; j++)
                {
                    if(xCoordinates[i * length + j] < NetCdfConstants.FillValues.NcFillFloat)
                        coordinates.Add(new Coordinate(xCoordinates[i * length + j], yCoordinates[i * length + j]));
                }
                yield return new Feature2D{Name = ids[i], Geometry = new LineString(coordinates.ToArray())};
            }
        }

        private static IEnumerable<IFeature> GenerateGenerateOutputDamBreaks(string[] ids, int length, double[] xCoordinates, double[] yCoordinates)
        {
            if (ids == null)
            {
                Log.Warn("Cannot generate output leveeBreach without ids");
                yield break;
            }

            for (int i = 0; i < ids.Length; i++)
            {
                yield return new LeveeBreach
                {
                    Name = ids[i]
                };
            }
        }

        public static string CharArrayToString(char[] chars)
        {
            return new string(chars).TrimEnd('\0', ' ');
        }

        public static void CheckAndResolveInputBecauseKernelIsNotGeneratingOutputCorrectly(NetCdfFile netCdfFile, ref string[] ids, ref int maxNumberOfCoordinatesTheGeometryOfTheObjectConsistOf, ref double[] xCoordinates, ref double[] yCoordinates, string featureName)
        {
            if (OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute.ContainsKey(featureName))
            {
                maxNumberOfCoordinatesTheGeometryOfTheObjectConsistOf = OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute[featureName](netCdfFile, CoordinateLocation.Pts).Cast<int>().ToArray()[0];
                ids = OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute[featureName](netCdfFile, CoordinateLocation.Name)?.Cast<char[]>().Select(CharArrayToString).ToArray();
                xCoordinates = OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute[featureName](netCdfFile, CoordinateLocation.X)?.Cast<double>().ToArray();
                yCoordinates = OutputCoordinateExtractorsBecauseKernelSucksAndSometimesForgetToWriteCfRoleAndXAndYCoordinatesInTheCoordinateAttribute[featureName](netCdfFile, CoordinateLocation.Y)?.Cast<double>().ToArray();
            }
        }
    }
}