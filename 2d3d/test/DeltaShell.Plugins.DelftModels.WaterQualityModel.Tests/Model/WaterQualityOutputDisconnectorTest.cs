using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaterQualityOutputDisconnectorTest
    {
        [Test]
        public void Disconnect_WhenModelHasFeatureCoverageDataItem_ThenFeatureCoverageIsCleared()
        {
            const string outputFeatureCoverageTag = "OutputFeatureCoverage";
            FeatureCoverage featureCoverage = CreateFeatureCoverage();

            // Pre-condition
            Assert.That(featureCoverage.GetValues().Count, Is.EqualTo(1));
            Assert.That(featureCoverage.Filters.Count, Is.EqualTo(1));

            IDataItem featureCoverageDataItem;
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.DataItems.Add(new DataItem(featureCoverage, DataItemRole.Output, outputFeatureCoverageTag));

                // Call
                WaterQualityOutputDisconnector.Disconnect(waqModel);

                // Assert
                featureCoverageDataItem = waqModel.GetDataItemByTag(outputFeatureCoverageTag);
            }

            Assert.That(featureCoverageDataItem, Is.Not.Null);

            var coverage = (FeatureCoverage) featureCoverageDataItem.Value;
            Assert.That(coverage.GetValues().Count, Is.EqualTo(0));
            Assert.That(coverage.Filters.Count, Is.EqualTo(0));
        }

        [Test]
        public void Disconnect_WhenModelHashTextDocumentOutputDataItem_ThenDataItemIsRemoved()
        {
            const string outputTextDocumentTag = "OutputTextDocument";

            // Setup
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, outputTextDocumentTag));

                // Call
                WaterQualityOutputDisconnector.Disconnect(waqModel);

                // Assert
                Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Null);
            }
        }

        [Test]
        public void Disconnect_WhenModelHasTextDocumentOutputDataItem_ThenDataItemIsRemovedFromModel()
        {
            const string outputTextDocumentTag = "OutputTextDocument";
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string filePath = Path.Combine(tempDirectory.Path, "myTextFile.txt");
                File.WriteAllText(filePath, @"This is a test file.");

                using (var waqModel = new WaterQualityModel())
                {
                    waqModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, outputTextDocumentTag));

                    // Call
                    WaterQualityOutputDisconnector.Disconnect(waqModel);

                    // Assert
                    Assert.That(File.Exists(filePath), Is.True);
                    Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Null);
                }
            }
        }

        [Test]
        public void Disconnect_WhenModelHasUnstructuredGridCellCoverageOutputDataItem_ThenCoverageIsCleared()
        {
            const int numberOfHorizontalCells = 2;
            const int numberOfVerticalCells = 2;
            const int numberOfCells = numberOfHorizontalCells * numberOfVerticalCells;
            const double noDataValue = -999.0;
            const string outputGridCellCoverageTag = "OutputGridCellCoverage";

            // Setup
            UnstructuredGridCellCoverage unstructuredGridCellCoverage = CreateUnstructuredGridCellCoverage(numberOfHorizontalCells,
                                                                                                           numberOfVerticalCells,
                                                                                                           noDataValue,
                                                                                                           numberOfCells);

            // Pre-condition
            Assert.That(unstructuredGridCellCoverage.GetValues().Count, Is.EqualTo(numberOfCells));
            Assert.That(unstructuredGridCellCoverage.GetValues<double>().All(v => Math.Abs(v - noDataValue) > 10e-6), Is.True);

            IDataItem coverageDataItem;
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.DataItems.Add(new DataItem(unstructuredGridCellCoverage, DataItemRole.Output, outputGridCellCoverageTag));

                // Call
                WaterQualityOutputDisconnector.Disconnect(waqModel);

                // Assert
                coverageDataItem = waqModel.GetDataItemByTag(outputGridCellCoverageTag);
            }

            Assert.That(coverageDataItem, Is.Not.Null);

            var coverage = (UnstructuredGridCellCoverage) coverageDataItem.Value;
            Assert.That(coverage.GetValues().Count, Is.EqualTo(numberOfCells));
            Assert.That(coverage.GetValues<double>().All(v => Math.Abs(v - noDataValue) < 10e-6), Is.True);
        }

        [Test]
        public void Disconnect_WhenModelHasUnstructuredGridCellCoverageOutputDataItemThatIsConnectedToLazyMapFileFunctionStore_ThenPathOfThisStoreIsSetToNull()
        {
            const string outputGridCellCoverageTag = "OutputGridCellCoverage";

            // Setup
            UnstructuredGridCellCoverage unstructuredGridCellCoverage = CreateUnstructuredGridCellCoverage(2, 2, -999.0, 4);
            unstructuredGridCellCoverage.Store = new LazyMapFileFunctionStore {Path = "not_null"};

            // Pre-condition
            Assert.That(((LazyMapFileFunctionStore) unstructuredGridCellCoverage.Store).Path, Is.Not.Null);

            IDataItem coverageDataItem;
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.DataItems.Add(new DataItem(unstructuredGridCellCoverage, DataItemRole.Output, outputGridCellCoverageTag));

                // Call
                WaterQualityOutputDisconnector.Disconnect(waqModel);

                // Assert
                coverageDataItem = waqModel.GetDataItemByTag(outputGridCellCoverageTag);
            }

            Assert.That(coverageDataItem, Is.Not.Null);
            var coverage = (UnstructuredGridCellCoverage) coverageDataItem.Value;
            Assert.That(((LazyMapFileFunctionStore) coverage.Store).Path, Is.Null,
                        $"When disconnecting output of model, Path of {coverage.Store} should be set to Null.");
        }

        [Test]
        public void Disconnect_WhenModelHasObservationVariableOutputDataItemWithTimeSeries_ThenAllTimeSeriesAreCleared()
        {
            // Setup
            WaterQualityObservationVariableOutput waterQualityObservationVariableOutput = CreateObservationVariableOutput();
            List<TimeSeries> timeSeriesList = waterQualityObservationVariableOutput.TimeSeriesList.ToList();

            // Preconditions
            Assert.That(timeSeriesList.Count, Is.EqualTo(2), "Precondition violated.");
            Assert.That(!IsEmptyTimeSeries(timeSeriesList), "Precondition violated.");

            using (var model = new WaterQualityModel())
            {
                model.DataItems.Add(new DataItem(waterQualityObservationVariableOutput, DataItemRole.Output));

                // Call
                WaterQualityOutputDisconnector.Disconnect(model);
            }

            // Assert
            timeSeriesList = waterQualityObservationVariableOutput.TimeSeriesList.ToList();
            Assert.That(timeSeriesList.Count, Is.EqualTo(2));
            Assert.That(IsEmptyTimeSeries(timeSeriesList),
                        "After disconnecting the output, the time series list of the observation variable output should be empty.");
        }

        private static UnstructuredGridCellCoverage CreateUnstructuredGridCellCoverage(
            int numberOfHorizontalCells, int numberOfVerticalCells, double noDataValue, int numberOfCells)
        {
            UnstructuredGrid grid =
                UnstructuredGridTestHelper.GenerateRegularGrid(numberOfHorizontalCells, numberOfVerticalCells, 10, 10);

            var random = new Random();
            var unstructuredGridCellCoverage = new UnstructuredGridCellCoverage(grid, false);
            unstructuredGridCellCoverage.Components[0].NoDataValue = noDataValue;
            unstructuredGridCellCoverage.SetValues(Enumerable.Range(0, numberOfCells).Select(i => random.NextDouble()));
            return unstructuredGridCellCoverage;
        }

        private static FeatureCoverage CreateFeatureCoverage()
        {
            var featureCoverage = new FeatureCoverage("Test coverage");
            featureCoverage.Arguments.Add(new Variable<IFeature>("Feature argument"));
            featureCoverage.Components.Add(new Variable<int>("Test component"));
            featureCoverage[new Feature()] = 2;
            featureCoverage.Filters = new List<IVariableFilter> {new ComponentFilter(featureCoverage.Components[0])};
            return featureCoverage;
        }

        private static WaterQualityObservationVariableOutput CreateObservationVariableOutput()
        {
            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(
                new List<DelftTools.Utils.Tuple<string, string>>
                {
                    new DelftTools.Utils.Tuple<string, string>("Substance 1", "mg/l"),
                    new DelftTools.Utils.Tuple<string, string>("Substance 2", "kg/l")
                });

            // Add a time-value pair to the first time series
            TimeSeries sub1 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(0);
            sub1.Arguments[0].AddValues(new[]
            {
                DateTime.Now
            });
            sub1.Components[0].SetValues(new[]
            {
                1.0
            });

            // Add a time-value pair to the second time series
            TimeSeries sub2 = waterQualityObservationVariableOutput.TimeSeriesList.ElementAt(1);
            sub2.Arguments[0].AddValues(new[]
            {
                DateTime.Now
            });
            sub2.Components[0].SetValues(new[]
            {
                2.0
            });
            return waterQualityObservationVariableOutput;
        }

        private static bool IsEmptyTimeSeries(IEnumerable<TimeSeries> timeSeriesList)
        {
            return timeSeriesList.All(timeSeries =>
                                          !timeSeries.Time.Values.Any() &&
                                          timeSeries.GetValues().Count == 0);
        }
    }
}