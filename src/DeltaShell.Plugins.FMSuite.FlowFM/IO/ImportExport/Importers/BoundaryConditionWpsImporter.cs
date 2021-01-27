using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Web;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class BoundaryConditionWpsImporter : IFileImporter
    {
        public enum SupportPointImportMode
        {
            Selected,
            Active,
            Inactive,
            All
        }

        private TimeSpan timeStep;

        public BoundaryConditionWpsImporter()
        {
            Process = "tidal_predict";
            SupportPointIndex = -1;
        }

        public DateTime StartDate { private get; set; }

        public DateTime EndDate { private get; set; }

        public ICoordinateSystem InputCoordinateSystem { private get; set; }

        public bool CreateNewBoundaryConditions { private get; set; }

        public SupportPointImportMode ImportMode { private get; set; }

        public int SupportPointIndex { private get; set; }

        public TimeSpan TimeStep
        {
            get => timeStep;
            set
            {
                timeStep = value;
                UpdateFrequency();
            }
        }

        public string Frequency { get; set; }

        public WpsClient Client { get; private set; }

        public string Process { get; }

        public void InitializeClient()
        {
            if (Client == null)
            {
                Client = new WpsClient(new Uri("http://wps.openearth.nl/wps"));
            }
        }

        public void Import(BoundaryConditionSet boundaryConditionSet)
        {
            var index = 0;
            Import(boundaryConditionSet, ref index, 0);
        }

        private void UpdateFrequency()
        {
            if (TimeStep.TotalMinutes < 1)
            {
                Frequency = "SECONDLY";
                return;
            }

            if (TimeStep.TotalHours < 1)
            {
                Frequency = "MINUTELY";
                return;
            }

            if (TimeStep.TotalDays < 1)
            {
                Frequency = "HOURLY";
                return;
            }

            Frequency = "DAILY";
        }

        private string GetLocation(Coordinate coordinate)
        {
            Coordinate newCoordinate;
            if (InputCoordinateSystem != null)
            {
                var coordinateSystemFactory = new OgrCoordinateSystemFactory();
                ICoordinateSystem targetCoordinateSystem = coordinateSystemFactory.CreateFromEPSG(4326);
                ICoordinateTransformation transformation = coordinateSystemFactory.CreateTransformation(
                    InputCoordinateSystem,
                    targetCoordinateSystem);
                IGeometry newPoint =
                    GeometryTransform.TransformGeometry(new Point(coordinate), transformation.MathTransform);
                newCoordinate = newPoint.Coordinate;
                newCoordinate.Z = double.NaN;
            }
            else
            {
                throw new ArgumentException("No input coordinate system defined.");
            }

            return WKTWriter.ToPoint(newCoordinate);
        }

        private int CountDataSetsToImport(BoundaryConditionSet boundaryConditionSet, int pointIndex)
        {
            FlowBoundaryCondition boundaryCondition =
                boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                    .FirstOrDefault(
                                        bc =>
                                            bc.DataType == BoundaryConditionDataType.TimeSeries &&
                                            bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel);
            int pointCount = boundaryConditionSet.Feature.Geometry.Coordinates.Count();

            switch (ImportMode)
            {
                case SupportPointImportMode.Selected:
                    return pointIndex == -1 ? 0 : 1;
                case SupportPointImportMode.Active:
                    if (CreateNewBoundaryConditions)
                    {
                        return 0;
                    }

                    return boundaryCondition == null ? 0 : boundaryCondition.DataPointIndices.Count;
                case SupportPointImportMode.Inactive:
                    if (CreateNewBoundaryConditions)
                    {
                        return pointCount;
                    }

                    return boundaryCondition == null
                               ? 0
                               : Enumerable.Range(0, pointCount).Except(boundaryCondition.DataPointIndices).Count();
                case SupportPointImportMode.All:
                    return pointCount;
                default:
                    throw new NotImplementedException(
                        string.Format("Boundary import mode {0} not supported", ImportMode));
            }
        }

        private void Import(IList<BoundaryConditionSet> boundaryConditionSets)
        {
            if (InputCoordinateSystem == null)
            {
                throw new ArgumentException("No input coordinate system defined.");
            }

            int count = boundaryConditionSets.Select(bcs => CountDataSetsToImport(bcs, -1)).Sum();
            var index = 0;
            foreach (BoundaryConditionSet boundaryConditionSet in boundaryConditionSets)
            {
                if (ShouldCancel)
                {
                    return;
                }

                Import(boundaryConditionSet, ref index, count);
            }
        }

        private void Import(BoundaryConditionSet boundaryConditionSet, ref int fullIndex,
                            int totalSteps)
        {
            if (ImportMode == SupportPointImportMode.Selected && SupportPointIndex == -1)
            {
                return;
            }

            FlowBoundaryCondition boundaryCondition;
            var indices = new List<int>();

            if (CreateNewBoundaryConditions)
            {
                boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                              BoundaryConditionDataType.TimeSeries) {Feature = boundaryConditionSet.Feature};
                boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

                if (ImportMode == SupportPointImportMode.Selected)
                {
                    indices = new List<int>(new[]
                    {
                        SupportPointIndex
                    });
                }
                else if (ImportMode == SupportPointImportMode.Inactive || ImportMode == SupportPointImportMode.All)
                {
                    indices = Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count()).ToList();
                }
            }
            else
            {
                boundaryCondition =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                        .FirstOrDefault(
                                            bc =>
                                                bc.DataType == BoundaryConditionDataType.TimeSeries &&
                                                bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel);
                indices = new List<int>();

                if (boundaryCondition == null)
                {
                    return;
                }

                switch (ImportMode)
                {
                    case SupportPointImportMode.Selected:
                        indices = new List<int>(new[]
                        {
                            SupportPointIndex
                        });
                        break;
                    case SupportPointImportMode.Active:
                        indices = boundaryCondition.DataPointIndices.ToList();
                        break;
                    case SupportPointImportMode.Inactive:
                        indices = Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count())
                                            .Except(boundaryCondition.DataPointIndices).ToList();
                        break;
                    case SupportPointImportMode.All:
                        indices =
                            Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count()).ToList();
                        break;
                }
            }

            int n = totalSteps == 0 ? indices.Count : totalSteps;
            foreach (int index in indices)
            {
                if (ShouldCancel)
                {
                    return;
                }

                if (ProgressChanged != null)
                {
                    ProgressChanged(string.Format("Importing time series at point {0}", index), fullIndex++, n);
                }

                boundaryCondition.BeginEdit(new DefaultEditAction("Importing data from WPS"));
                try
                {
                    if (!boundaryCondition.DataPointIndices.Contains(index))
                    {
                        boundaryCondition.AddPoint(index);
                    }

                    Import(boundaryCondition.GetDataAtPoint(index),
                           boundaryCondition.Feature.Geometry.Coordinates[index]);
                }
                finally
                {
                    boundaryCondition.EndEdit();
                }
            }
        }

        private void Import(IFunction boundaryData, Coordinate coordinate)
        {
            if (!boundaryData.Arguments.Any())
            {
                return;
            }

            InitializeClient();
            if (boundaryData.Arguments[0].ValueType == typeof(DateTime))
            {
                using (CultureUtils.SwitchToInvariantCulture())
                {
                    WpsProcessData locationInput = Client.CreateInputForProcess("tidal_predict", "location",
                                                                                GetLocation(coordinate));
                    WpsProcessData startDateInput =
                        Client.CreateInputForProcess("tidal_predict", "startdate", StartDate);
                    WpsProcessData endDateInput = Client.CreateInputForProcess("tidal_predict", "enddate", EndDate);
                    WpsProcessData frequencyInput =
                        Client.CreateInputForProcess("tidal_predict", "frequency", Frequency);

                    string output =
                        Client.Execute("tidal_predict", locationInput, startDateInput, endDateInput, frequencyInput)
                              .FirstOrDefault();

                    if (output == null)
                    {
                        return;
                    }

                    IList<DateTime> times;
                    IList<double> levels;

                    ParseCsv(output, out times, out levels);

                    boundaryData.BeginEdit(new DefaultEditAction("Setting imported values"));
                    boundaryData.Arguments[0].Values.Clear();
                    boundaryData.Arguments[0].SetValues(times);
                    boundaryData.Components[0].SetValues(levels);
                    boundaryData.EndEdit();
                }
            }
        }

        private static void ParseCsv(string output, out IList<DateTime> times, out IList<double> levels)
        {
            times = new List<DateTime>();
            levels = new List<double>();
            foreach (string line in Regex.Split(output, "\n").ToList())
            {
                string[] values = Regex.Split(line, ",");
                DateTime dateTime;
                double waterLevel;
                if (DateTime.TryParse(values[0].Trim('\"'), out dateTime) &&
                    double.TryParse(values[1].Trim('\"'), out waterLevel))
                {
                    times.Add(dateTime);
                    levels.Add(waterLevel);
                }
            }
        }

        #region IFileImporter

        public string Name => "Boundary data from WPS";

        public string Category => "Boundary data";

        public string Description => string.Empty;

        public Bitmap Image => Resources.down;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<BoundaryConditionSet>);
                yield return typeof(BoundaryConditionSet);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => null;

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public object ImportItem(string path, object target = null)
        {
            var boundaryConditionSets = target as IList<BoundaryConditionSet>;
            if (boundaryConditionSets != null)
            {
                Import(boundaryConditionSets);
                OpenViewAfterImport = false;
                return boundaryConditionSets;
            }

            var boundaryConditionSet = target as BoundaryConditionSet;
            if (boundaryConditionSet != null)
            {
                Import(boundaryConditionSet);
                OpenViewAfterImport = true;
                return boundaryConditionSet;
            }

            return null;
        }

        #endregion
    }
}