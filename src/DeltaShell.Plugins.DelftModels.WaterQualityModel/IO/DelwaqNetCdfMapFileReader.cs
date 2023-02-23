using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// A map file reader designed for NetCdf files (_map.nc) created by D-Water Quality.
    /// </summary>
    public static class DelwaqNetCdfMapFileReader
    {
        private const string mesh2dVariableName = "mesh2d";
        private static readonly ILog log = LogManager.GetLogger(typeof(DelwaqNetCdfMapFileReader));

        /// <summary>
        /// Reads the meta data of the provided <paramref name="path"/>
        /// </summary>
        /// <param name="path"> Path to the *_map.nc file </param>
        /// <remarks>If <paramref name="path"/> does not exist, an 'empty' <see cref="MapFileMetaData"/> is returned.</remarks>
        public static MapFileMetaData ReadMetaData(string path)
        {
            try
            {
                return NetCdfFileReaderHelper.DoWithNetCdfFile(path, ReadMetaData);
            }
            catch (FileNotFoundException)
            {
                log.Error(string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path));
                return new MapFileMetaData();
            }
        }

        /// <summary>
        /// Gets the values for <paramref name="substanceName"/> at specified segment and time indices.
        /// </summary>
        /// <param name="path"> Path of the *_map.nc file. </param>
        /// <param name="mapFileMeta"> Metadata for the map file (use <see cref="ReadMetaData"/> to get it initially) </param>
        /// <param name="timeStepIndex"> Time index (zero based) at which to get the values </param>
        /// <param name="substanceName"> Substance name </param>
        /// <param name="segmentIndex"> Segment index (zero based) at which to get the values (default -1: no filtering) </param>
        /// <returns>
        /// A list of double values at the specified <paramref name="timeStepIndex"/> and
        /// <paramref name="segmentIndex"/>
        /// </returns>
        /// <remarks>If <paramref name="path"/> does not exist, an empty list is returned.</remarks>
        public static List<double> GetTimeStepData(string path, MapFileMetaData mapFileMeta, int timeStepIndex, string substanceName, int segmentIndex = -1)
        {
            try
            {
                return NetCdfFileReaderHelper.DoWithNetCdfFile(
                    path, file => GetDataAtIndices(substanceName, timeStepIndex, segmentIndex, mapFileMeta, file));
            }
            catch (FileNotFoundException)
            {
                log.Error(string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path));
                return new List<double>();
            }
        }

        /// <summary>
        /// Gets the values for <paramref name="substanceName"/> at the specified segment index for all time steps.
        /// </summary>
        /// <param name="path"> Path of the *_map.nc file. </param>
        /// <param name="mapFileMeta"> Metadata for the map file (use <see cref="ReadMetaData"/> to get it initially) </param>
        /// <param name="substanceName"> Substance name </param>
        /// <param name="segmentIndex"> Segment index (zero based) at which to get the values (default -1: no filtering) </param>
        /// <returns> A list of double values at the specified  <paramref name="segmentIndex"/> for all time steps. </returns>
        /// <remarks>If <paramref name="path"/> does not exist, an empty list is returned.</remarks>
        public static List<double> GetTimeSeriesData(string path, MapFileMetaData mapFileMeta, string substanceName, int segmentIndex)
        {
            try
            {
                return NetCdfFileReaderHelper.DoWithNetCdfFile(path, file => GetDataAtIndices(substanceName, -1, segmentIndex, mapFileMeta, file));
            }
            catch (FileNotFoundException)
            {
                log.Error(string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path));
                return new List<double>();
            }
        }

        private static MapFileMetaData ReadMetaData(NetCdfFile file)
        {
            List<DateTime> times = NetCdfFileReaderHelper.GetDateTimes(file).ToList();
            int nFaces = file.GetDimensionLength(GetFaceDimensionNameForMesh2D(file));
            Dictionary<string, string> substanceToVariableMapping = SubstanceToVariableMapping(file);

            return new MapFileMetaData
            {
                Times = times,
                SubstancesMapping = substanceToVariableMapping,
                Substances = substanceToVariableMapping.Keys.ToList(),
                NumberOfTimeSteps = times.Count,
                NumberOfSegments = nFaces,
                NumberOfSubstances = substanceToVariableMapping.Count
            };
        }

        private static List<double> GetDataAtIndices(string substanceName, int timeStepIndex, int segmentIndex,
                                                     MapFileMetaData mapFileMeta, NetCdfFile file)
        {
            if (!mapFileMeta.SubstancesMapping.TryGetValue(substanceName, out string netCdfVariableName))
            {
                return new List<double>();
            }

            if (!NetCdfFileReaderHelper.TryGetVariableByStandardName(file, NetCdfConventions.StandardNames.Time, out NetCdfVariable timeVariable))
            {
                log.ErrorFormat(Resources.NetCdfFileReaderHelper_GetDateTimes_Time_variable_not_found, NetCdfConventions.StandardNames.Time, file.Path);
                return new List<double>();
            }

            string timeDimensionName = file.GetVariableName(timeVariable);

            NetCdfVariable variable = file.GetVariableByName(netCdfVariableName);
            List<NetCdfDimension> dimensions = file.GetDimensions(variable).ToList();

            int nDimensions = dimensions.Count;
            var shapes = new int[nDimensions];
            var origins = new int[nDimensions];

            for (var i = 0; i < nDimensions; i++)
            {
                string dimensionName = file.GetDimensionName(dimensions[i]);

                var shape = 1;
                var origin = 0;

                if (dimensionName == timeDimensionName)
                {
                    if (timeStepIndex == -1)
                    {
                        shape = mapFileMeta.NumberOfTimeSteps;
                    }
                    else
                    {
                        origin = timeStepIndex;
                    }
                }
                else if (dimensionName == GetFaceDimensionNameForMesh2D(file))
                {
                    if (segmentIndex == -1)
                    {
                        shape = mapFileMeta.NumberOfSegments;
                    }
                    else
                    {
                        origin = segmentIndex;
                    }
                }

                shapes[i] = shape;
                origins[i] = origin;
            }

            return ReadFromFile(file, variable, origins, shapes);
        }

        private static Dictionary<string, string> SubstanceToVariableMapping(NetCdfFile file)
        {
            IEnumerable<NetCdfVariable> substanceVariables = file.GetVariables()
                                                                 .Where(v => file.GetAttributes(v)
                                                                                 .ContainsKey(NetCdfConventions.Attributes.DelwaqName));
            Dictionary<string, string> mapping = substanceVariables
                .ToDictionary(v => GetSubstanceName(file, v), file.GetVariableName);

            return mapping;
        }

        /// <summary>
        /// Gets the substance name by retrieving the `delwaq_name` attribute value from the variable.
        /// This value may or may not contain the `_avg` postfix.
        /// If the attribute value contains the postfix, it needs to be removed to retrieve the substance name. 
        /// </summary>
        /// <param name="file"> The NetCDF file. </param>
        /// <param name="variable"> The substance NetCDF variable. </param>
        /// <returns></returns>
        private static string GetSubstanceName(INetCdfFile file, NetCdfVariable variable)
        {
            string delwaqName = file.GetAttributeValue(variable, NetCdfConventions.Attributes.DelwaqName);
            string substanceName = delwaqName.Replace("_avg", "");

            return substanceName;
        }

        private static List<double> ReadFromFile(NetCdfFile file, NetCdfVariable variable, int[] origins, int[] shapes)
        {
            Array floatArray = file.Read(variable, origins, shapes);

            var doubleValues = new List<double>();
            foreach (float floatValue in floatArray)
            {
                doubleValues.Add(floatValue);
            }

            return doubleValues;
        }

        private static string GetFaceDimensionNameForMesh2D(NetCdfFile file)
        {
            NetCdfVariable mesh2dVariable = file.GetVariableByName(mesh2dVariableName);
            return file.GetAttributeValue(mesh2dVariable, NetCdfConventions.Attributes.FaceDimension);
        }
    }
}