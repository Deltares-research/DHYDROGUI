using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.DelftTools.Utils.Tuples;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DNetCdfFunctionStoreTest
    {
        private const double Delta = 0.0000000001;

        #region Mock Classes

        internal class MockNetworkLocationTypeConverter : NetworkLocationTypeConverter
        {
            public override INetworkLocation ConvertFromStore(object source)
            {
                return new NetworkLocation();
            }
        }

        internal class MockFeatureTypeConverter : FeatureTypeConverter
        {
            public MockFeatureTypeConverter()
            {
                SpecificType = typeof(IBranchFeature);
            }

            public override IFeature ConvertFromStore(object source)
            {
                return new ObservationPoint();
            }
        }

        internal class MockWaterFlowModel1DNetCdfFunctionStore : WaterFlowModel1DNetCdfFunctionStore
        {
            public MockWaterFlowModel1DNetCdfFunctionStore()
            {
                networkLocationTypeConverter = new MockNetworkLocationTypeConverter();
                featureTypeConverter = new MockFeatureTypeConverter();
            }
        }

        #endregion

        [Test]
        public void TestGetMetaData_ForExistingMetaData()
        {
            var testFile = TestHelper.GetTestFilePath(@"FileWriters\output\gridpoints.nc");
            var store = new WaterFlowModel1DNetCdfFunctionStore(){ Path = testFile };

            var metaData = TypeUtils.GetPropertyValue(store, "MetaData") as WaterFlowModel1DOutputFileMetaData;
            Assert.NotNull(metaData);
            Assert.IsTrue(metaData.Locations.Any());
        }

        [Test]
        public void TestGetMetaData_ForNonExistingMetaData()
        {
            var testFile = TestHelper.GetTestFilePath(@"thisFileDoesNotExist.nc");
            var store = new WaterFlowModel1DNetCdfFunctionStore() { Path = testFile };

            var metaData = TypeUtils.GetPropertyValue(store, "MetaData") as WaterFlowModel1DOutputFileMetaData;
            Assert.NotNull(metaData);
            Assert.IsFalse(metaData.Locations.Any());
        }

        [Test]
        public void Clone_CopiesExistingFunctions()
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + "gridpoints.nc");
            
            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var branch2 = new Branch("branch2", new HydroNode("node3"), new HydroNode("node4"), 200.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var coverage = new NetworkCoverage("Water level", true) { Network = network };
            var store = new WaterFlowModel1DNetCdfFunctionStore{ Path = filePath };
            store.Functions.AddRange(coverage.Arguments);
            store.Functions.AddRange(coverage.Components);
            store.Functions.Add(coverage);

            var clone = (WaterFlowModel1DNetCdfFunctionStore)store.Clone();
            Assert.AreEqual(store.Functions.Count, clone.Functions.Count);

            var variable = store.Functions.OfType<Variable<double>>().First();
            var storeValues = store.GetVariableValues(variable);
            Assert.Greater(storeValues.Count, 0);

            var clonedVariable = clone.Functions.OfType<Variable<double>>().First();
            var cloneValues = clone.GetVariableValues(clonedVariable);
            Assert.Greater(cloneValues.Count, 0);
            Assert.AreEqual(storeValues.Count, cloneValues.Count);

            for (var i = 0; i < storeValues.Count; i++)
            {
                Assert.AreEqual(storeValues[i], cloneValues[i]);
            }
        }

        [TestCase("gridpoints.nc", 27)]
        [TestCase("reachsegments.nc", 25)]
        public void GetVariableValues_ReturnsAllLocationsForVariableOfTypeINetworkLocation(string path, int numLocations)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<INetworkLocation>("test");
            var store = new MockWaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { variable }
            };
            var values = store.GetVariableValues(variable);

            Assert.AreEqual(numLocations, values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetVariableValues_LogsErrorMessage_IfLocationCouldNotBeFound()
        {
            /*Test created after issue SOBEK3-1376*/
            var filePath = TestHelper.GetTestFilePath(@"CompositeStructures/structures.nc");
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var featureName = "stuw_Linn_zom";
                var coverageName = "Crest level (s)";
                Weir expectedLocation;
                IVariableFilter[] filters;
                WaterFlowModel1DNetCdfFunctionStore store;
                var variable = CreateVariableAndFilter(coverageName, featureName, filePath, out expectedLocation, out filters, out store);

                var logMessage =
                    string.Format(
                        Resources
                            .WaterFlowModel1DNetCdfFunctionStore_GetLocationIndex_Values_for__0__feature_type__1__could_not_be_found_,
                        featureName, expectedLocation.GetType().Name);
                TestHelper.AssertAtLeastOneLogMessagesContains( () => store.GetVariableValues(variable, filters), logMessage);

                Assert.NotNull(store.GetVariableValues(variable, filters));
            }
            finally 
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetVariableValues_ReturnsEmptyValueCollection_IfLocationCouldNotBeFound()
        {
            /*Test created after issue SOBEK3-1376*/
            var filePath = TestHelper.GetTestFilePath(@"CompositeStructures/structures.nc");
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var featureName = "stuw_Linn_zom";
                var coverageName = "Crest level (s)";//;
                Weir expectedLocation;
                IVariableFilter[] filters;
                WaterFlowModel1DNetCdfFunctionStore store;
                var variable = CreateVariableAndFilter(coverageName, featureName, filePath, out expectedLocation, out filters, out store);

                var values = store.GetVariableValues(variable, filters);
                Assert.IsNotNull(values);
                Assert.AreEqual(new MultiDimensionalArray<double>(), store.GetVariableValues(variable, filters));
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        private static Variable<double> CreateVariableAndFilter(string coverageName, string featureName, string filePath,
            out Weir expectedLocation, out IVariableFilter[] filters, out WaterFlowModel1DNetCdfFunctionStore store)
        {
            var variable = new Variable<double>(coverageName) {NoDataValue = -999.9};
            var coverage = new FeatureCoverage(coverageName);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);

            var dateTimeVariable = new Variable<DateTime>();
            dateTimeVariable.Values.Add(expectedTime);
            coverage.Arguments.Add(dateTimeVariable);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            expectedLocation = new Weir { Name = featureName };
            var composite = new CompositeBranchStructure("Composite", 50.0);
            expectedLocation.ParentStructure = composite;
            composite.Structures.Add(expectedLocation);
            
            branch.BranchFeatures.Add(expectedLocation);

            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.Add(expectedLocation);
            coverage.Arguments.Add(locationVariable);

            filters = new IVariableFilter[]
            {
                new VariableValueFilter<IBranchFeature>(locationVariable, expectedLocation),
            };

            coverage.Components.Add(variable);
            store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction> {coverage, variable}
            };
            return variable;
        }

        [TestCase("laterals.nc", 2)]
        [TestCase("observations.nc", 3)]
        public void GetVariableValues_ReturnsAllLocationsForVariableOfTypeIBranchFeature(string path, int numLocations)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);
            
            var variable = new Variable<IBranchFeature>("feature");
            var store = new MockWaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { variable }
            };
            var values = store.GetVariableValues(variable);

            Assert.AreEqual(numLocations, values.Count);
        }

        [TestCase("gridpoints.nc", 2, "")]
        [TestCase("laterals.nc", 2, "")]
        [TestCase("observations.nc", 2, "")]
        [TestCase("reachsegments.nc", 2, "")]
        public void GetVariableValues_ReturnsTimeSeriesForVariableOfTypeDateTimeWithNoFilters(string path, int numTimes, string coverageName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<DateTime>("feature");
            var store = new MockWaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { variable }
            };
            var values = store.GetVariableValues(variable);

            Assert.AreEqual(numTimes, values.Count);
        }

        [TestCase("gridpoints.nc", 1, "")]
        [TestCase("laterals.nc", 1, "")]
        [TestCase("observations.nc", 1, "")]
        [TestCase("reachsegments.nc", 1, "")]
        public void GetVariableValues_ReturnsSingleTimeForVariableOfTypeDateTimeWithOneFilterWithOneValue(string path, int numTimes, string coverageName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<DateTime>("feature");
            var expectedTime = DateTime.Now;
            var filter = new IVariableFilter[] { new VariableValueFilter<DateTime>(variable, expectedTime) };
            var store = new MockWaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { variable }
            };
            var values = store.GetVariableValues(variable, filter);

            Assert.AreEqual(numTimes, values.Count);
            Assert.AreEqual(expectedTime, values[0]);
        }

        [TestCase("gridpoints.nc", 54, "Water level", 0.1, 0.1000000146683305, 1.0, 0.0)]
        [TestCase("laterals.nc", 4, "Discharge (l)", 5.0, 3.0, 1.0, 0.0)]
        [TestCase("observations.nc", 6, "Water level (op)", 0.1, 0.14595623763293833, 1.0, 0.0)]
        [TestCase("reachsegments.nc", 50, "Discharge", 1.0, 0.099558974458171884, 1.0, 0.0)]
        public void GetVariableValues_ReturnsAllValuesForVariableOfTypeDoubleWithNoFilters(
            string path, int numValues, string coverageName, double firstValue, double lastValue,
            double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName);
            var coverage = new NetworkCoverage(coverageName, true);

            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };
            
            var values = store.GetVariableValues(variable);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("gridpoints.nc", 27, "Water level", 0.14595623763293833, 0.1000000146683305, 0.16450473553176148, 0.1000000146683305)]
        [TestCase("laterals.nc", 2, "Discharge (l)", 5.0, 3.0, 5.0, 3.0)]
        [TestCase("observations.nc", 3, "Water level (op)", 0.14595623763293833, 0.14595623763293833, 0.14595623763293833, 0.14595623763293833)]
        [TestCase("reachsegments.nc", 25, "Discharge", 1.0000000733416525, 0.099558974458171884, 1.0000000733416525, 0.09955897445817187)]
        public void GetVariableValues_ReturnsTimeStepValuesForAllLocationsForVariableOfTypeDoubleWithTimeFilters(
            string path, int numValues, string coverageName, double firstValue, double lastValue,
            double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName){ NoDataValue = -999.9 };
            var coverage = new NetworkCoverage(coverageName, true);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);
            var dateTimeVariable = coverage.Arguments[0];
            dateTimeVariable.Values.Add(expectedTime);
            
            var filters = new IVariableFilter[] { new VariableValueFilter<DateTime>(dateTimeVariable, expectedTime) };

            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filters);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("gridpoints.nc", 1, "Water level", 0.0, 0.14595623763293833, 0.14595623763293833, 0.14595623763293833)]
        [TestCase("reachsegments.nc", 1, "Discharge", 5.0, 1.0000000733416525, 1.0000000733416525, 1.0000000733416525)]
        public void GetVariableValues_ReturnsTimeStepValueForOneNetworkLocationForVariableOfTypeDoubleWithTimeFiltersAndLocationFilters(
            string path, int numValues, string coverageName, double locationChainage, double value,
            double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new NetworkCoverage(coverageName, true);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);
            var dateTimeVariable = coverage.Arguments[0];
            dateTimeVariable.Values.Add(expectedTime);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            var expectedLocation = new NetworkLocation(branch, locationChainage);
            var locationVariable = coverage.Arguments[1];
            locationVariable.Values.Add(expectedLocation);

            var filters = new IVariableFilter[]
            {
                new VariableValueFilter<DateTime>(dateTimeVariable, expectedTime),
                new VariableValueFilter<INetworkLocation>(locationVariable, expectedLocation), 
            };

            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filters);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(value, (double)values[0], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("laterals.nc", 1, "Discharge (l)", 0.0, "LateralSource1", 5.0, 5.0, 5.0)]
        [TestCase("observations.nc", 1, "Water level (op)", 0.0, "observationPoint1", 0.14595623763293833, 0.14595623763293833, 0.14595623763293833)]
        public void GetVariableValues_ReturnsTimeStepValueForOneBranchFeatureForVariableOfTypeDoubleWithTimeFiltersAndLocationFilters(
            string path, int numValues, string coverageName, double locationChainage, string featureName, double value,
            double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new FeatureCoverage(coverageName);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);

            var dateTimeVariable = new Variable<DateTime>();
            dateTimeVariable.Values.Add(expectedTime);
            coverage.Arguments.Add(dateTimeVariable);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            var expectedLocation = new ObservationPoint() {Name = featureName};
            branch.BranchFeatures.Add(expectedLocation);
            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.Add(expectedLocation);
            coverage.Arguments.Add(locationVariable);

            var filters = new IVariableFilter[]
            {
                new VariableValueFilter<DateTime>(dateTimeVariable, expectedTime),
                new VariableValueFilter<IBranchFeature>(locationVariable, expectedLocation), 
            };

            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filters);
        
            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(value, (double)values[0], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("gridpoints.nc", 4, "Water level", 0.0, 50.0, 0.14595623763293833, 0.14154141606236365, 0.16450473553176148, 0.14154141606236365)]
        [TestCase("reachsegments.nc", 4, "Discharge", 5.0, 45.0, 1.0000000733416525, 0.44888475948599238, 1.0000000733416525, 0.44888475948599238)]
        public void GetVariableValues_ReturnsTimeStepValuesForRangeOfNetworkLocationForVariableOfTypeDoubleWithTimeFiltersAndLocationRangeFilters(
            string path, int numValues, string coverageName, double firstLocationChainage, double secondLocationChainage, double firstValue,
            double lastValue, double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new NetworkCoverage(coverageName, true);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);
            var dateTimeVariable = coverage.Arguments[0];
            dateTimeVariable.Values.Add(expectedTime);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            var expectedLocation1 = new NetworkLocation(branch, firstLocationChainage);
            var expectedLocation2 = new NetworkLocation(branch, secondLocationChainage);
            var locationVariable = coverage.Arguments[1];
            locationVariable.Values.AddRange(new List<INetworkLocation>()
            {
                expectedLocation1, 
                new NetworkLocation(branch, 10.0), 
                new NetworkLocation(branch, 20.0), 
                expectedLocation2
            });

            var filters = new IVariableFilter[]
            {
                new VariableValueFilter<DateTime>(dateTimeVariable, expectedTime),
                new VariableValueRangesFilter<INetworkLocation>(locationVariable,
                    new List<Pair<INetworkLocation, INetworkLocation>>()
                    {
                        new Pair<INetworkLocation, INetworkLocation>(expectedLocation1, expectedLocation2)
                    })
            };
            
            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filters);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("laterals.nc", 2, "Discharge (l)", 0.0, "lateralSource1", 1.0, "lateralSource2", 5.0, 3.0, 5.0, 3.0)]
        [TestCase("observations.nc", 2, "Water level (op)", 0.0, "observationPoint1", 1.0, "observationPoint2", 0.14595623763293833, 0.14595623763293833, 0.14595623763293833, 0.14595623763293833)]
        public void GetVariableValues_ReturnsTimeStepValuesForRangeOfBranchFeatureForVariableOfTypeDoubleWithTimeFiltersAndLocationRangeFilters(
            string path, int numValues, string coverageName, double firstLocationChainage, string firstFeatureName, double secondLocationChainage, 
            string secondFeatureName, double firstValue, double lastValue, double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new FeatureCoverage(coverageName);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);

            var dateTimeVariable = new Variable<DateTime>();
            dateTimeVariable.Values.Add(expectedTime);
            coverage.Arguments.Add(dateTimeVariable);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);
            
            var expectedLocation1 = new ObservationPoint() { Name = firstFeatureName };
            var expectedLocation2 = new ObservationPoint() { Name = secondFeatureName };
            branch.BranchFeatures.AddRange(new List<IBranchFeature>() { expectedLocation1, expectedLocation2 });
            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.AddRange(new List<IBranchFeature>()
            {
                expectedLocation1, 
                expectedLocation2
            });
            coverage.Arguments.Add(locationVariable);

            var filters = new IVariableFilter[]
            {
                new VariableValueFilter<DateTime>(dateTimeVariable, expectedTime),
                new VariableValueRangesFilter<IBranchFeature>(locationVariable,
                    new List<Pair<IBranchFeature, IBranchFeature>>()
                    {
                        new Pair<IBranchFeature, IBranchFeature>(expectedLocation1, expectedLocation2)
                    })
            };
            
            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filters);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("gridpoints.nc", 2, "Water level", 0.0, 0.1, 0.14595623763293833, 0.14595623763293833, 0.1)]
        [TestCase("reachsegments.nc", 2, "Discharge", 5.0, 1.0, 1.0000000733416525, 1.0000000733416525, 1.0)]
        public void GetVariableValues_ReturnsTimeSeriesValuesForOneNetworkLocationForVariableOfTypeDoubleWithLocationFilterOnly(
            string path, int numValues, string coverageName, double locationChainage, double firstValue, double lastValue, 
            double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new NetworkCoverage(coverageName, true);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);
            var dateTimeVariable = coverage.Arguments[0];
            dateTimeVariable.Values.Add(expectedTime);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            var expectedLocation = new NetworkLocation(branch, locationChainage);
            var locationVariable = coverage.Arguments[1];
            locationVariable.Values.Add(expectedLocation);

            var filter = new IVariableFilter[]
            {
                new VariableValueFilter<INetworkLocation>(locationVariable, expectedLocation)
            };
            
            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filter);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }

        [TestCase("laterals.nc", 2, "Discharge (l)", 0.0, "LateralSource1", 5.0, 5.0, 5.0, 5.0)]
        [TestCase("observations.nc", 2, "Water level (op)", 0.0, "observationPoint1", 0.1, 0.14595623763293833, 0.14595623763293833, 0.1)]
        public void GetVariableValues_ReturnsTimeSeriesValuesForOneBranchFeatureForVariableOfTypeDoubleWithLocationFilterOnly(
            string path, int numValues, string coverageName, double locationChainage, string featureName, double firstValue,
            double lastValue, double expectedMaxValue, double expectedMinValue)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var variable = new Variable<double>(coverageName) { NoDataValue = -999.9 };
            var coverage = new FeatureCoverage(coverageName);

            //{08/10/2016 10:40:00, 08/10/2016 10:40:30}
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);

            var dateTimeVariable = new Variable<DateTime>();
            dateTimeVariable.Values.Add(expectedTime);
            coverage.Arguments.Add(dateTimeVariable);

            var branch = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 100.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch);

            var expectedLocation = new ObservationPoint() { Name = featureName };
            branch.BranchFeatures.Add(expectedLocation);
            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.Add(expectedLocation);
            coverage.Arguments.Add(locationVariable);

            var filter = new IVariableFilter[]
            {
                new VariableValueFilter<IBranchFeature>(locationVariable, expectedLocation), 
            };
            
            coverage.Components.Add(variable);
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, variable }
            };

            var values = store.GetVariableValues(variable, filter);

            Assert.AreEqual(numValues, values.Count);
            Assert.AreEqual(firstValue, (double)values[0], Delta);
            Assert.AreEqual(lastValue, (double)values[values.Count - 1], Delta);

            var maxValue = store.GetMaxValue<double>(variable);
            var minValue = store.GetMinValue<double>(variable);

            Assert.AreEqual(expectedMaxValue, maxValue, Delta);
            Assert.AreEqual(expectedMinValue, minValue, Delta);
        }
        
        [TestCase("gridpoints.nc")]
        [TestCase("laterals.nc")]
        [TestCase("observations.nc")]
        [TestCase("reachsegments.nc")]
        public void GetMaxValue_ReturnsLastTimeForTypeOfDateTime(string path)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 30);

            var variable = new Variable<DateTime>();
            var store = new WaterFlowModel1DNetCdfFunctionStore{ Path = filePath };

            var maxValue = store.GetMaxValue<DateTime>(variable);
            Assert.AreEqual(expectedTime, maxValue);
        }

        [TestCase("gridpoints.nc", "Discharge (l)", 150.0)]
        [TestCase("reachsegments.nc", "Water level (op)", 145.0)]
        public void GetMaxValue_ReturnsLastLocationForTypeOfNetworkLocation(string path, string coverageName, double locationChainage)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var branch2 = new Branch("branch2", new HydroNode("node3"), new HydroNode("node4"), 200.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var coverage = new NetworkCoverage(coverageName, true) {Network = network};
            var locationVariable = coverage.Arguments[1];

            var expectedLocation = new NetworkLocation(branch2, locationChainage);
            locationVariable.Values.Add(expectedLocation);
            locationVariable.Values.Add(new NetworkLocation(branch2, locationChainage-1));
            locationVariable.Values.Add(new NetworkLocation(branch2, locationChainage-2));
            
            var store = new WaterFlowModel1DNetCdfFunctionStore()
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, locationVariable }
            };

            var maxValue = store.GetMaxValue<INetworkLocation>(locationVariable);
            Assert.AreEqual(expectedLocation, maxValue);
        }
        
        [TestCase("laterals.nc", "Discharge (l)", 2, "lateralSource2")]
        [TestCase("observations.nc", "Water level (op)", 3, "observationPoint3")]
        public void GetMaxValue_ReturnsLastLocationForTypeOfBranchFeature(string path, string coverageName, int numFeatures, string featureName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);
            var coverage = new FeatureCoverage(coverageName);

            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var branch2 = new Branch("branch2", new HydroNode("node3"), new HydroNode("node4"), 200.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var expectedLocation = new ObservationPoint() { Name = featureName, Chainage = 100.0};
            branch2.BranchFeatures.Add(expectedLocation);

            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.Add(expectedLocation);
            coverage.Arguments.Add(locationVariable);

            for(var i = 1; i < numFeatures; i++) coverage.Features.Add(new ObservationPoint(){Chainage = 0.0});
            coverage.Features.Add(expectedLocation);
            
            var store = new WaterFlowModel1DNetCdfFunctionStore()
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, locationVariable }
            };

            var maxValue = store.GetMaxValue<IBranchFeature>(locationVariable);
            Assert.AreEqual(expectedLocation, maxValue);
        }
        
        [TestCase("gridpoints.nc")]
        [TestCase("laterals.nc")]
        [TestCase("observations.nc")]
        [TestCase("reachsegments.nc")]
        public void GetMinValue_ReturnsFirstTimeForTypeOfDateTime(string path)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);
            var expectedTime = new DateTime(2016, 08, 10, 10, 40, 00);

            var variable = new Variable<DateTime>();
            var store = new WaterFlowModel1DNetCdfFunctionStore { Path = filePath };

            var maxValue = store.GetMinValue<DateTime>(variable);
            Assert.AreEqual(expectedTime, maxValue);
        }

        [TestCase("gridpoints.nc", "Discharge (l)", 0.0)]
        [TestCase("reachsegments.nc", "Water level (op)", 5.0)]
        public void GetMinValue_ReturnsFirstLocationForTypeOfNetworkLocation(string path, string coverageName, double locationChainage)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);

            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            
            var coverage = new NetworkCoverage(coverageName, true) { Network = network };
            var locationVariable = coverage.Arguments[1];

            var expectedLocation = new NetworkLocation(branch1, locationChainage);
            locationVariable.Values.Add(expectedLocation);
            locationVariable.Values.Add(new NetworkLocation(branch1, locationChainage+1));
            locationVariable.Values.Add(new NetworkLocation(branch1, locationChainage+2));

            var store = new WaterFlowModel1DNetCdfFunctionStore()
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, locationVariable }
            };

            var minValue = store.GetMinValue<INetworkLocation>(locationVariable);
            Assert.AreEqual(expectedLocation, minValue);
        }

        [TestCase("laterals.nc", "Discharge (l)", 2, "lateralSource1")]
        [TestCase("observations.nc", "Water level (op)", 3, "observationPoint1")]
        public void GetMinValue_ReturnsFirstLocationForTypeOfBranchFeature(string path, string coverageName, int numFeatures, string featureName)
        {
            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/" + path);
            var coverage = new FeatureCoverage(coverageName);

            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var network = new HydroNetwork();
            network.Branches.Add(branch1);

            var expectedLocation = new ObservationPoint() { Name = featureName, Chainage = 0.0};
            branch1.BranchFeatures.Add(expectedLocation);
            
            var locationVariable = new Variable<IBranchFeature>();
            locationVariable.Values.Add(expectedLocation);

            coverage.Arguments.Add(locationVariable);
            coverage.Features.Add(expectedLocation);
            for (var i = 1; i < numFeatures; i++) coverage.Features.Add(new ObservationPoint() {Chainage = 1.0});

            var store = new WaterFlowModel1DNetCdfFunctionStore()
            {
                Path = filePath,
                Functions = new EventedList<IFunction>() { coverage, locationVariable }
            };

            var minValue = store.GetMinValue<IBranchFeature>(locationVariable);
            Assert.AreEqual(expectedLocation, minValue);
        }

        [Test, Category(TestCategory.Performance)]
        public void GettingArgumentValuesWithCachingShouldBeFast()
        {
            var branch1 = new Branch("branch1", new HydroNode("node1"), new HydroNode("node2"), 200.0);
            var branch2 = new Branch("branch2", new HydroNode("node3"), new HydroNode("node4"), 200.0);

            var network = new HydroNetwork();
            network.Branches.AddRange(new []{branch1, branch2});

            var coverage = new NetworkCoverage("Discharge (l)", true) { Network = network };
            var locationVariable = coverage.Arguments[1];

            var filePath = TestHelper.GetTestFilePath(@"FileWriters/output/gridpoints.nc");
            var store = new WaterFlowModel1DNetCdfFunctionStore
            {
                Path = filePath,
                Functions = new EventedList<IFunction> (new[] { coverage })
            };

            var getValuesCount = 10000;

            store.DisableCaching = true;

            TestHelper.AssertIsFasterThan(4000, () =>
            {
                for (int i = 0; i < getValuesCount; i++)
                {
                    var values = new List<INetworkLocation>(store.GetVariableValues<INetworkLocation>(locationVariable));
                }
            });

            store.DisableCaching = false;

            // should be much faster with caching -> gets better with more values in .nc file
            TestHelper.AssertIsFasterThan(1000, () =>
            {
                for (int i = 0; i < getValuesCount; i++)
                {
                    var values = new List<INetworkLocation>(store.GetVariableValues<INetworkLocation>(locationVariable));
                }
            });
        }
    }
}
