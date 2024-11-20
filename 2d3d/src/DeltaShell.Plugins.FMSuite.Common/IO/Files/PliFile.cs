using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class PliFile<T> : NGHSFileBase, IFeature2DFileBase<T> where T : IFeature, INameable, new()
    {
        public const string Extension = "pli";

        public static readonly string[] NumericColumnAttributesKeys =
        {
            "Column3",
            "Column4",
            "Column5",
            "Column6",
            "Column7",
            "Column8",
            "Column9"
        };

        private const int MaximumAmountOfNumericValuesInPliFile = 9;
        private const int AmountOfDimensionalValuesInPliFile = 2;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PliFile<T>));

        protected readonly string[] StringColumnAttributesKeys =
        {
            "WeirType"
        };

        public Func<List<Coordinate>, string, T> CreateDelegate { private get; set; }

        /// <summary>
        /// Reads a polyline file and creates a collection of features of type <typeparamref name="T"/> and optionally
        /// reports reading progress information to the user.
        /// </summary>
        /// <param name="filePath"> File path to the .pli file that is being read. </param>
        /// <param name="progress">
        /// Action that is invoked when reading a line in the .pli file.
        /// This informs the user about the reading progress.
        /// </param>
        /// <returns> A collection of features of type <typeparamref name="T"/>. </returns>
        public virtual IList<T> Read(string filePath, Action<string, int, int> progress)
        {
            var features = new EventedList<T>();

            OpenInputFile(filePath);
            var lineCount = 0;
            int numberOfLines = File.ReadLines(filePath).Count();
            try
            {
                string line = GetNextLine();
                lineCount++;
                var maxColumns = 0;

                while (line != null)
                {
                    if (lineCount % 100 == 0)
                    {
                        progress?.Invoke("Reading line", lineCount, numberOfLines);
                    }

                    string featureName = line;
                    var subFeatureCounter = 0;

                    line = GetNextLine();
                    lineCount++;
                    if (line == null)
                    {
                        throw new FormatException($"Unexpected end of file; Expected number of points and columns on line {LineNumber} in file {filePath}");
                    }

                    var lineFields = (IList<string>) SplitLine(line).ToList();
                    if (lineFields.Count < 2)
                    {
                        throw new FormatException($"Invalid numpoints/numcolums on line {LineNumber} in file {filePath}");
                    }

                    int numPoints = GetInt(lineFields[0], "value for nr of points");
                    int numColumns = GetInt(lineFields[1], "value for nr of columns");

                    maxColumns = Math.Max(maxColumns, numColumns);

                    var columnNumericalValuesList = new List<IList<double>>(
                        Enumerable.Range(0, numColumns - AmountOfDimensionalValuesInPliFile)
                                  .Select(i => new List<double>(numPoints)));
                    var columnStringValuesList = new List<IList<string>>(
                        Enumerable.Range(0, Math.Max(0, numColumns - MaximumAmountOfNumericValuesInPliFile))
                                  .Select(i => new List<string>(numPoints)));
                    var locationNames = new Dictionary<int, string>(numPoints);
                    var points = new List<Coordinate>(numPoints);

                    for (var i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        lineCount++;
                        if (line == null)
                        {
                            throw new FormatException($"Unexpected end of file; Expected more data (attempting to read point {i + 1} out of {numPoints}) on line {LineNumber} in file {filePath}");
                        }

                        lineFields = SplitLine(line).ToList();

                        if (lineFields.Count < numColumns ||
                            lineFields.Count >= MaximumAmountOfNumericValuesInPliFile &&
                            lineFields.Count > numColumns + 1)
                        {
                            throw new FormatException($"Invalid point row (expected {numColumns} columns, but was {lineFields.Count}) on line {LineNumber} in file {filePath}");
                        }

                        double x = GetDouble(lineFields[0]);
                        double y = GetDouble(lineFields[1]);

                        if (x.Equals(-999.0d) || y.Equals(-999.0d))
                        {
                            // separator between sub-features
                            try
                            {
                                T feature = CreateFeature2D(featureName + "-" + ++subFeatureCounter, points,
                                                            numColumns, columnNumericalValuesList,
                                                            columnStringValuesList, locationNames, filePath);
                                features.Add(feature);
                            }
                            catch (Exception e)
                            {
                                throw new FormatException($"Failed feature construction for {featureName} on line {LineNumber} in file {filePath}: {e.Message}");
                            }

                            points.Clear();
                            foreach (IList<double> columnValues in columnNumericalValuesList)
                            {
                                columnValues.Clear();
                            }

                            locationNames.Clear();
                        }
                        else
                        {
                            points.Add(new Coordinate(x, y));

                            for (var j = 2; j < numColumns; ++j)
                            {
                                try
                                {
                                    if (j != 9)
                                    {
                                        columnNumericalValuesList[j - 2].Add(GetDouble(lineFields[j]));
                                    }
                                    else
                                    {
                                        columnStringValuesList[0].Add(lineFields[j]);
                                    }
                                }
                                catch (Exception e)
                                {
                                    throw new FormatException($"Invalid placement of string value '{lineFields[j]}' on line {LineNumber} in file {filePath}: {e.Message}");
                                }
                            }

                            if (lineFields.Count == numColumns + 1)
                            {
                                locationNames.Add(i, lineFields[numColumns]);
                            }
                        }
                    }

                    if (points.Count > 0)
                    {
                        string actualFeatureName = subFeatureCounter > 0
                                                       ? featureName + "-" + ++subFeatureCounter
                                                       : featureName;

                        try
                        {
                            T feature = CreateFeature2D(actualFeatureName, points, numColumns,
                                                        columnNumericalValuesList, columnStringValuesList,
                                                        locationNames, filePath);
                            features.Add(feature);
                        }
                        catch (Exception e)
                        {
                            throw new FormatException($"Failed feature construction for {actualFeatureName} on line {LineNumber} in file {filePath}: {e.Message}");
                        }

                        points.Clear();
                        foreach (IList<double> columnValues in columnNumericalValuesList)
                        {
                            columnValues.Clear();
                        }

                        locationNames.Clear();
                    }

                    line = GetNextLine();
                    lineCount++;
                }

                if (maxColumns > NumericColumnAttributesKeys.Length + StringColumnAttributesKeys.Length + 2)
                {
                    Log.WarnFormat("In file {0}: columns {1} to {2} will be ignored.", filePath,
                                   NumericColumnAttributesKeys.Length + StringColumnAttributesKeys.Length + 3,
                                   maxColumns);
                }
            }
            finally
            {
                CloseInputFile();
            }

            return features;
        }

        /// <summary>
        /// Writes a polyline file for the collection of features <see cref="features"/>
        /// </summary>
        /// <param name="filePath"> The target file path to write the .pli file to. </param>
        /// <param name="features">
        /// The features of type <typeparamref name="T"/> that are used to write data to file
        /// at path <paramref name="filePath"/>
        /// </param>
        public virtual void Write(string filePath, IEnumerable<T> features)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(filePath);
                try
                {
                    foreach (T feature2D in features)
                    {
                        var numericColumnValues = new List<IList<double>>();
                        var stringColumnValues = new List<IList<string>>();
                        IList<string> locationNames = null;

                        var numColumns = 2; // X, Y
                        if (feature2D.Attributes != null)
                        {
                            BuildColumnValuesFromFeature(feature2D, numericColumnValues, NumericColumnAttributesKeys,
                                                         ref numColumns);
                            BuildColumnValuesFromFeature(feature2D, stringColumnValues, StringColumnAttributesKeys,
                                                         ref numColumns);
                            if (feature2D.Attributes.ContainsKey(Feature2D.LocationKey))
                            {
                                // location names, does not count as a data column
                                locationNames = (IList<string>) feature2D.Attributes[Feature2D.LocationKey];
                            }
                        }

                        WriteLine(feature2D.Name);
                        WriteLine($"    {feature2D.Geometry.NumPoints}    {numColumns}");
                        for (var i = 0; i < feature2D.Geometry.Coordinates.Length; i++)
                        {
                            Coordinate coordinate = feature2D.Geometry.Coordinates[i];
                            var line = $"{coordinate.X:E15}  {coordinate.Y:E15}";
                            ConstructLineContent(i, numericColumnValues, 0.0, NumericColumnAttributesKeys, ref line,
                                                 feature2D.Name);
                            ConstructLineContent(i, stringColumnValues, "T", StringColumnAttributesKeys, ref line,
                                                 feature2D.Name);

                            if (locationNames != null && i < locationNames.Count)
                            {
                                line += $" {locationNames[i]}";
                            }

                            WriteLine(line);
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        /// <summary>
        /// Reads a polyline file and creates a collection of features of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="filePath"> File path to the .pli file that is being read. </param>
        /// <returns> A collection of features of type <typeparamref name="T"/>. </returns>
        public virtual IList<T> Read(string filePath) => Read(filePath, null);

        protected static void AssignDoubleValuesToAttribute(IList<double> columnValues, IFeature feature, string key)
        {
            GeometryPointsSyncedList<double> syncedList;
            if (feature.Attributes.ContainsKey(key) && feature.Attributes[key] is GeometryPointsSyncedList<double>)
            {
                syncedList = (GeometryPointsSyncedList<double>) feature.Attributes[key];
            }
            else
            {
                syncedList = new GeometryPointsSyncedList<double>
                {
                    CreationMethod = (f, i) => 0.0,
                    Feature = feature
                };
                feature.Attributes[key] = syncedList;
            }

            for (var i = 0; i < columnValues.Count; ++i)
            {
                syncedList[i] = columnValues[i];
            }
        }

        protected static void AssignStringValuesToAttribute(IList<string> columnValues, IFeature feature, string key)
        {
            var syncedList = new GeometryPointsSyncedList<string>
            {
                CreationMethod = (f, i) => string.Empty,
                Feature = feature
            };
            feature.Attributes[key] = syncedList;

            for (var i = 0; i < columnValues.Count; ++i)
            {
                syncedList[i] = columnValues[i];
            }
        }

        private T CreateFeature2D(string name,
                                  List<Coordinate> points,
                                  int numColumns,
                                  IEnumerable<IList<double>> columnNumericalValueLists,
                                  IEnumerable<IList<string>> columnStringValueLists,
                                  IDictionary<int, string> locationNames,
                                  string pliFilePath)
        {
            T feature = CreateDelegate != null
                            ? CreateDelegate(points, name)
                            : new T
                            {
                                Name = name,
                                Geometry = points.Count != 1
                                               ? (IGeometry) LineStringCreator.CreateLineString(points)
                                               : new Point(points.FirstOrDefault())
                            };

            feature.TrySetGroupName(pliFilePath);

            SetFeatureAttributes(numColumns, columnNumericalValueLists, columnStringValueLists, locationNames, feature);

            return feature;
        }

        private void SetFeatureAttributes(int numColumns,
                                          IEnumerable<IList<double>> columnNumericalValueLists,
                                          IEnumerable<IList<string>> columnStringValueLists,
                                          IDictionary<int, string> locationNames,
                                          T feature)
        {
            if (numColumns >= 2)
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new DictionaryFeatureAttributeCollection();
                }

                AssignDoubleAttributes(columnNumericalValueLists, feature);
                AssignStringAttributes(columnStringValueLists, feature);
            }

            AssignLocationAttributes(locationNames, feature);
        }

        private static void AssignLocationAttributes(IDictionary<int, string> locationNames, T feature)
        {
            if (!locationNames.Any())
            {
                return;
            }

            if (feature.Attributes == null)
            {
                feature.Attributes = new DictionaryFeatureAttributeCollection();
            }

            var syncedList = new GeometryPointsSyncedList<string>
            {
                CreationMethod = BoundaryConditionSet.DefaultLocationName,
                Feature = feature
            };
            foreach (KeyValuePair<int, string> locationName in locationNames)
            {
                syncedList[locationName.Key] = locationName.Value;
            }

            feature.Attributes[Feature2D.LocationKey] = syncedList;
        }

        private void AssignStringAttributes(IEnumerable<IList<string>> columnStringValueLists, T feature)
        {
            var j = 0;
            foreach (IList<string> stringValueList in columnStringValueLists)
            {
                if (j < StringColumnAttributesKeys.Length)
                {
                    AssignStringValuesToAttribute(stringValueList, feature, StringColumnAttributesKeys[j++]);
                }
            }
        }

        private static void AssignDoubleAttributes(IEnumerable<IList<double>> columnNumericalValueLists, T feature)
        {
            var j = 0;
            foreach (IList<double> columnValueList in columnNumericalValueLists)
            {
                if (j < NumericColumnAttributesKeys.Length)
                {
                    AssignDoubleValuesToAttribute(columnValueList, feature, NumericColumnAttributesKeys[j++]);
                }
            }
        }

        private static void BuildColumnValuesFromFeature<TS>(T feature2D,
                                                             ICollection<IList<TS>> columnValues,
                                                             IEnumerable<string> columnAttributeKeys,
                                                             ref int numColumns)
        {
            foreach (string key in columnAttributeKeys)
            {
                if (!feature2D.Attributes.ContainsKey(key))
                {
                    continue;
                }

                if (feature2D.Attributes[key] is IList<TS> valueList)
                {
                    columnValues.Add(valueList);
                    numColumns++; // add value columns
                }
            }
        }

        private void ConstructLineContent<TS>(int lineNumber,
                                              IList<IList<TS>> columnValues,
                                              TS defaultValue,
                                              IReadOnlyList<string> columnAttributeKeys,
                                              ref string lineContent,
                                              string featureName)
        {
            foreach (IList<TS> columnValueList in columnValues)
            {
                if (lineNumber < columnValueList.Count)
                {
                    lineContent += $"  {columnValueList[lineNumber]:E15}";
                }
                else
                {
                    Log.WarnFormat("Feature {0} has a smaller number of attribute values than geometry points for {1}; filling up remaining entries with zero",
                                   featureName,
                                   columnAttributeKeys[columnValues.IndexOf(columnValueList)]);

                    lineContent += $"  {defaultValue:E15}";
                }
            }
        }
    }
}