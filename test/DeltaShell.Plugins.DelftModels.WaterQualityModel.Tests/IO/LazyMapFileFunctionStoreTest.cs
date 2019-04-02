using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class LazyMapFileFunctionStoreTest
    {
        private readonly DateTime firstTimeStep = new DateTime(2010, 1, 1, 0, 0, 0);
        private readonly DateTime timeStep7 = new DateTime(2010, 1, 1, 6, 0, 0);
        private readonly DateTime lastTimeStep = new DateTime(2010, 1, 2, 0, 0, 0);
        private string mapFilePath;

        [TestFixtureSetUp]
        public void SetUpTests()
        {
            mapFilePath = Path.Combine(TestHelper.GetDataDir(), "IO", "deltashell.map");
        }

        [Test]
        public void CopyTo_IsSuccessfulWhenCopyingToNonExistantDirectory()
        {
            var mapFilePath = Path.Combine(TestHelper.GetDataDir(), "FunctionStores", "deltashell.map");
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var directoryPath = Path.Combine(TestHelper.GetDataDir(), "DirectoryDoesNotExist");
            FileUtils.DeleteIfExists(directoryPath);

            var filePath = Path.Combine(directoryPath, "deltashell.map");
            store.CopyTo(filePath);
        }

        [Test]
        public void ReadTimesFromStore()
        {
            var dtVariable = new Variable<DateTime>();

            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var times = store.GetVariableValues<DateTime>(dtVariable);

            Assert.AreEqual(25, times.Count);
            var startTime = firstTimeStep;
            var endTime = lastTimeStep;

            Assert.IsTrue(times[0].CompareTo(startTime) == 0);
            Assert.IsTrue(times[24].CompareTo(endTime) == 0);

            Assert.AreEqual(endTime, store.GetMaxValue<DateTime>(dtVariable));
            Assert.AreEqual(startTime, store.GetMinValue<DateTime>(dtVariable));
        }

        [Test]
        public void ReadTimeStepValuesFromStore()
        {
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7}
            };

            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            funtion.Arguments.Add(new Variable<DateTime>("datetime"));
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

            var values = store.GetVariableValues(component, timeFilter);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(19.767536163330078, values[0]);
            Assert.AreEqual(27.733558654785156, values[1]);

            timeFilter.Values = new[] {lastTimeStep};
            values = store.GetVariableValues(component, timeFilter);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(5.6551790237426758, values[0]);
            Assert.AreEqual(14.770989418029785, values[1]);

            Assert.AreEqual(5.6551790237426758, store.GetMinValue<double>(component));
            Assert.AreEqual(27.733558654785156, store.GetMaxValue<double>(component));
        }

        [Test]
        public void ReadTimeSeriesValuesFromStore()
        {
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var segmentFilter = new VariableValueFilter<int> {Values = new[] {1}};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[]
                {
                    timeStep7,
                    lastTimeStep
                }
            };

            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            funtion.Arguments.Add(new Variable<DateTime>("datetime"));
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

            var values = store.GetVariableValues<double>(component, segmentFilter);

            Assert.AreEqual(30.0, values[0]); // first timestep
            Assert.AreEqual(27.733558654785156, values[6]); // timestep 7
            Assert.AreEqual(14.770989418029785, values.Last()); // last timestep
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeStepsFromStoreUsingCoverage()
        {
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var unstrCellCoverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), true) {Store = store};

            Assert.AreEqual(25, unstrCellCoverage.Time.Values.Count);
        }

        [Test]
        public void GivenAFunctionStoreCall_WhenDelWaqFilePathIsEmpty_ThenAnEmptyMultiDimensionalArrayShouldBeReturned()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = ""};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7}
            };

            var component = new Variable<double>("Salinity");

            //When
            var values = store.GetVariableValues(component, timeFilter);

            //Then
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void
            GivenAFunctionStoreCall_WhenTheFunctionIsIndependentAndNotDateTimeAsType_ThenANotImplementedExceptionShouldBeThrown()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7}
            };

            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");

            function.Components.Add(component);

            //When
            var values = store.GetVariableValues(component, timeFilter);

            //Then NotImplementedException
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void
            GivenAFunctionStoreCall_WhenTheFilterContainsMultipleValuesAndTheFunctionIsNotADoubleAndTheFunctionIsIndependentOfTime_ThenANotImplementedExceptionShouldBeThrown()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7, timeStep7}
            };

            var component = new Variable<int>("Salinity");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            //When
            var values = store.GetVariableValues(component, timeFilter);

            //Then NotImplementedException
        }

        [Test]
        public void
            GivenAFunctionStoreCall_WhenTheFunctionNameIsMissing_ThenAnEmptyMultiDimensionalArrayShouldBeReturned()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7}
            };

            var component = new Variable<double>("");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            //When
            var values = store.GetVariableValues(component, timeFilter);

            //Then
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        public void
            GivenAFunctionStoreCall_WhenTheFunctionIsIndependentOfLocation_ThenAnEmptyMultiDimensionalArrayShouldBeReturned()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[] {timeStep7}
            };

            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Components.Add(component);
            
            //When
            var values = store.GetVariableValues(component, timeFilter);
            
            //Then
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void
            GivenAFunctionStoreCall_WhenFiltersAreMissingForLocationAndTime_ThenANotImplementedExceptionShouldBeThrown()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<double>
            {
                Values = new[] {1.2}
            };

            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            //When
            var values = store.GetVariableValues(component, timeFilter);

            //Then NotImplementedException
        }
    }
}