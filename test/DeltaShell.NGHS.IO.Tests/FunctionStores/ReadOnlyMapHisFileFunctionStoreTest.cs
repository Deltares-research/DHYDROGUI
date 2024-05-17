using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FunctionStores;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FunctionStores
{
    [TestFixture]
    public class ReadOnlyMapHisFileFunctionStoreTest
    {
        [Test]
        public void CopyTo_IsSuccessfulWhenCopyingToNonExistantDirectory()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var store = new ReadOnlyMapHisFileFunctionStore {Path = mapFilePath};

            string directoryPath = Path.Combine(TestHelper.GetTestDataDirectory(), "DirectoryDoesNotExist");
            FileUtils.DeleteIfExists(directoryPath);

            string filePath = Path.Combine(directoryPath, "deltashell.map");
            store.CopyTo(filePath);
        }

        [Test]
        public void ReadTimesFromMapFileStore()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var firstTimeStep = new DateTime(2010, 1, 1, 0, 0, 0);
            var lastTimeStep = new DateTime(2010, 1, 2, 0, 0, 0);
            var dtVariable = new Variable<DateTime>();

            var store = new ReadOnlyMapHisFileFunctionStore {Path = mapFilePath};
            IMultiDimensionalArray<DateTime> times = store.GetVariableValues<DateTime>(dtVariable);

            Assert.AreEqual(25, times.Count);
            DateTime startTime = firstTimeStep;
            DateTime endTime = lastTimeStep;

            Assert.IsTrue(times[0].CompareTo(startTime) == 0);
            Assert.IsTrue(times[24].CompareTo(endTime) == 0);

            Assert.AreEqual(endTime, store.GetMaxValue<DateTime>(dtVariable));
            Assert.AreEqual(startTime, store.GetMinValue<DateTime>(dtVariable));
        }

        [Test]
        public void ReadTimeStepValuesFromMapFileStore()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var timeStep7 = new DateTime(2010, 1, 1, 6, 0, 0);
            var lastTimeStep = new DateTime(2010, 1, 2, 0, 0, 0);

            var store = new ReadOnlyMapHisFileFunctionStore {Path = mapFilePath};
            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            var timeVariable = new Variable<DateTime>("datetime");
            funtion.Arguments.Add(timeVariable);
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Variable = timeVariable,
                Values = new[]
                {
                    timeStep7
                }
            };

            IMultiDimensionalArray values = store.GetVariableValues(component, timeFilter);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(19.767536163330078, values[0]);
            Assert.AreEqual(27.733558654785156, values[1]);

            timeFilter.Values = new[]
            {
                lastTimeStep
            };
            values = store.GetVariableValues(component, timeFilter);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(5.6551790237426758, values[0]);
            Assert.AreEqual(14.770989418029785, values[1]);

            Assert.AreEqual(5.6551790237426758, store.GetMinValue<double>(component));
            Assert.AreEqual(27.733558654785156, store.GetMaxValue<double>(component));
        }

        [Test]
        public void ReadTimeSeriesValuesFromMapFileStore()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var store = new ReadOnlyMapHisFileFunctionStore {Path = mapFilePath};

            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            funtion.Arguments.Add(new Variable<DateTime>("datetime"));
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

            var locationFilter = new VariableValueFilter<int>
            {
                Variable = funtion.Arguments[1],
                Values = new[]
                {
                    1
                }
            };
            IMultiDimensionalArray<double> values = store.GetVariableValues<double>(component, locationFilter);

            Assert.AreEqual(30.0, values[0]);                   // first timestep
            Assert.AreEqual(27.733558654785156, values[6]);     // timestep 7
            Assert.AreEqual(14.770989418029785, values.Last()); // last timestep
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeStepsFromStoreUsingCoverageFromMapFileStore()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var store = new ReadOnlyMapHisFileFunctionStore {Path = mapFilePath};
            var unstrCellCoverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), true) {Store = store};

            Assert.AreEqual(25, unstrCellCoverage.Time.Values.Count);
        }

        [Test]
        public void ReadTimeSeriesFromHisFileStore()
        {
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "rrbalans.his");
            var store = new ReadOnlyMapHisFileFunctionStore
            {
                Path = path,
                GetParameterName = s => "DWF Paved"
            };

            var timeseries = new TimeSeries {Name = "DWF paved (bm)"};
            timeseries.Components.Add(new Variable<double>("DWF paved (bm)"));
            timeseries.Store = store;

            Assert.AreEqual(11713, timeseries.Time.Values.Count);
            Assert.AreEqual(11713, timeseries.GetValues<double>().Count);
        }

        [Test]
        public void ReadFeatureCoverageTimeSliceFromHisFileStore()
        {
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "rrrunoff.his");

            var weir = new Structure() {Name = "Catchment1"};
            var featureCoverage = new FeatureCoverage("Outflow(hbv)")
            {
                IsTimeDependent = true,
                Features = new EventedList<IFeature>(new IFeature[]
                {
                    weir
                })
            };

            featureCoverage.Arguments.Add(new Variable<IFeature>("Feature"));
            featureCoverage.Components.Add(new Variable<double>("Outflow (hbv)"));

            var store = new ReadOnlyMapHisFileFunctionStore
            {
                Path = path,
                GetParameterName = s => "Total Outflow [m3/s]",
                LocationFromObjectToString = f => ((INameable) f).Name,
                LocationsFromStringToObject = n => featureCoverage.Features.OfType<INameable>().FirstOrDefault(f => f.Name == n)
            };

            featureCoverage.Store = store;

            Assert.AreEqual(25, featureCoverage.Time.Values.Count);
            Assert.AreEqual(1, featureCoverage.FeatureVariable.Values.Count);

            var timeFilter = new VariableValueFilter<DateTime>(featureCoverage.Time, featureCoverage.Time.Values[1]);
            Assert.AreEqual(1, featureCoverage.GetValues<double>(timeFilter).Count);

            IFunction timeSeries = featureCoverage.GetTimeSeries(weir);
            Assert.AreEqual(25, timeSeries.Arguments[0].Values.Count);
            Assert.AreEqual(25, timeSeries.GetValues().Count);

            // filtered coverage is used during rendering of featurecoverage (renderable coverage)
            var filteredCoverage = (IFeatureCoverage) featureCoverage.FilterTime(featureCoverage.Time.Values[1]);

            Assert.AreEqual(1, filteredCoverage.FeatureVariable.Values.Count);
        }
    }
}