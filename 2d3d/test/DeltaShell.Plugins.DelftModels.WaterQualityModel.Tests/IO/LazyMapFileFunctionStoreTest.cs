using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class LazyMapFileFunctionStoreTest
    {
        private readonly DateTime firstTimeStep = new DateTime(2010, 1, 1, 0, 0, 0);
        private readonly DateTime timeStep7 = new DateTime(2010, 1, 1, 6, 0, 0);
        private readonly DateTime lastTimeStep = new DateTime(2010, 1, 2, 0, 0, 0);
        private string mapFilePath;

        [OneTimeSetUp]
        public void SetUpTests()
        {
            mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.map");
        }

        [Test]
        public void CopyTo_IsSuccessfulWhenCopyingToNonExistantDirectory()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "FunctionStores", "deltashell.map");
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            string directoryPath = Path.Combine(TestHelper.GetTestDataDirectory(), "DirectoryDoesNotExist");
            FileUtils.DeleteIfExists(directoryPath);

            string filePath = Path.Combine(directoryPath, "deltashell.map");
            store.CopyTo(filePath);
        }

        [Test]
        public void ReadTimesFromStore()
        {
            var dtVariable = new Variable<DateTime>();

            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
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
        public void ReadTimeStepValuesFromStore()
        {
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[]
                {
                    timeStep7
                }
            };

            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            funtion.Arguments.Add(new Variable<DateTime>("datetime"));
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

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
        public void ReadTimeSeriesValuesFromStore()
        {
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var segmentFilter = new VariableValueFilter<int>
            {
                Values = new[]
                {
                    1
                }
            };

            var component = new Variable<double>("Salinity");
            var funtion = new Function("Salinity");

            funtion.Arguments.Add(new Variable<DateTime>("datetime"));
            funtion.Arguments.Add(new Variable<int>("cell_index"));
            funtion.Components.Add(component);

            IMultiDimensionalArray<double> values = store.GetVariableValues<double>(component, segmentFilter);

            Assert.AreEqual(30.0, values[0]);                   // first timestep
            Assert.AreEqual(27.733558654785156, values[6]);     // timestep 7
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
                Values = new[]
                {
                    timeStep7
                }
            };

            var component = new Variable<double>("Salinity");

            //When
            IMultiDimensionalArray values = store.GetVariableValues(component, timeFilter);

            //Then
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        public void
            GivenAFunctionStoreCall_WhenTheFunctionIsIndependentAndNotDateTimeAsType_ThenANotImplementedExceptionShouldBeThrown()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");

            function.Components.Add(component);

            Assert.That(
                // When
                () => store.GetVariableValues(component),
                // Then
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo(
                    string.Format(Resources.LazyMapFileFunctionStore_GetArgumentValues_Filters_of_type___0___can_only_filter_on_functions_with_value_type___1___, typeof(VariableValueFilter<DateTime>), typeof(DateTime))));
        }

        [Test]
        public void GivenAFunctionStoreCall_WhenTheFilterContainsMultipleValuesAndTheFunctionIsNotADoubleAndTheFunctionIsIndependentOfTime_ThenANotImplementedExceptionShouldBeThrown()
        {
            // Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[]
                {
                    timeStep7,
                    timeStep7
                }
            };

            var component = new Variable<int>("Salinity");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            // Then
            Assert.That(() => store.GetVariableValues(component, timeFilter), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void
            GivenAFunctionStoreCall_WhenTheFunctionNameIsMissing_ThenAnEmptyMultiDimensionalArrayShouldBeReturned()
        {
            //Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};

            var timeFilter = new VariableValueFilter<DateTime>
            {
                Values = new[]
                {
                    timeStep7
                }
            };

            var component = new Variable<double>("");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            //When
            IMultiDimensionalArray values = store.GetVariableValues(component, timeFilter);

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
                Values = new[]
                {
                    timeStep7
                }
            };

            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");

            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Components.Add(component);

            //When
            IMultiDimensionalArray values = store.GetVariableValues(component, timeFilter);

            //Then
            Assert.AreEqual(0, values.Count);
        }

        [Test]
        public void GivenAFunctionStoreCall_WhenFiltersAreMissingForLocationAndTime_ThenAnEmptyListShouldBeReturned()
        {
            // Given
            var store = new LazyMapFileFunctionStore {Path = mapFilePath};
            var component = new Variable<double>("Salinity");
            var function = new Function("Salinity");
            function.Arguments.Add(new Variable<DateTime>("datetime"));
            function.Arguments.Add(new Variable<int>("cell_index"));
            function.Components.Add(component);

            // When
            IMultiDimensionalArray values = store.GetVariableValues(component);

            // Then
            Assert.That(values, Is.Empty,
                        "When values are queried from the LazyMapFileFunctionStore without filters an empty list should be returned.");
        }

        [TestCase("CBOD5", "EColi")]
        [TestCase("DO", "EColi")]
        [TestCase("NH4", "EColi")]
        [TestCase("OXY", "EColi")]
        [TestCase("SOD", "EColi")]
        [TestCase("CBOD5", "Salinity")]
        [TestCase("DO", "Salinity")]
        [TestCase("NH4", "Salinity")]
        [TestCase("OXY", "Salinity")]
        [TestCase("SOD", "Salinity")]
        public void GivenALazyMapFileFunctionStore_WhenNewPathIsSet_ThenValuesCanBeRetrieved(
            string functionNameFirstFile, string functionNameSecondFile)
        {
            string tesDataDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

            // Given
            var store = new LazyMapFileFunctionStore();
            var filter = new VariableValueFilter<int> {Values = new List<int> {0}};
            IVariable function = CreateDependentFunction();

            store.Path = Path.Combine(tesDataDirectory, "oxygen_map.nc");
            function.Name = functionNameFirstFile;

            IMultiDimensionalArray firstValues = store.GetVariableValues(function, filter);
            Assert.That(firstValues, Is.Not.Empty,
                        "Retrieved values from file should not be empty.");

            // When
            store.Path = Path.Combine(tesDataDirectory, "bacteria_map.nc");
            function.Name = functionNameSecondFile;

            // Then
            IMultiDimensionalArray secondValues = store.GetVariableValues(function, filter);
            Assert.That(secondValues, Is.Not.Empty,
                        "Retrieved values from file should not be empty after setting the path of the function store to a new file with other substances.");

            Assert.That(firstValues, Is.Not.EqualTo(secondValues),
                        "Values should not be the same.");
        }

        private static IVariable CreateDependentFunction()
        {
            var function = MockRepository.GenerateStub<IVariable>();
            function.ValueType = typeof(double);
            function.Stub(f => f.IsIndependent).Return(false);
            function.Arguments = new EventedList<IVariable>(CreateFunctionArguments());
            return function;
        }

        private static IEnumerable<IVariable> CreateFunctionArguments()
        {
            var timeArgument = MockRepository.GenerateStub<IVariable>();
            timeArgument.ValueType = typeof(DateTime);
            yield return timeArgument;

            var segmentArgument = MockRepository.GenerateStub<IVariable>();
            segmentArgument.ValueType = typeof(int);
            yield return segmentArgument;
        }

        [Test]
        public void GetMinValue_Double_WithoutData_ReturnsZero()
        {
            // Setup
            var variable = new Variable<double>();
            var store = new LazyMapFileFunctionStore();

            // Call
            var result = store.GetMinValue<double>(variable);

            // Assert
            Assert.That(result, Is.EqualTo(0D));
        }

        [Test]
        public void GetMaxValue_Double_WithoutData_ReturnsZero()
        {
            // Setup
            var variable = new Variable<double>();
            var store = new LazyMapFileFunctionStore();

            // Call
            var result = store.GetMaxValue<double>(variable);

            // Assert
            Assert.That(result, Is.EqualTo(0D));
        }
    }
}