using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FM1DFileFunctionStoreGetVariableValuesTest
    {
        private const string fileName = "output_mapfiles\\CustomLengthFlowFM_map.nc";
        private static readonly string filePath = TestHelper.GetTestFilePath(fileName);
        private const string dictionaryData = "ncName";
        private readonly MultiDimensionalArray<double> emptyMultiDimensionalArray = new MultiDimensionalArray<double>(new List<double>(), new [] { 0, 0 });

        private static (FM1DFileFunctionStore, ICoverage, IVariable) GetDefaultFunctionStore(bool withCoverage = true, 
                                                                                             bool withDictionary = true, 
                                                                                             bool withPath = true)
        {
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            IVariable variable = GetVariableSubstitute();

            IEventedList<IFunction> functions;
            ICoverage coverage;

            if (withCoverage)
            {
                coverage = GetCoverage(variable, withDictionary);
                functions = GetEventedFuntionList(coverage);
            }
            else
            {
                coverage = null;
                functions = new EventedList<IFunction>(); 
            }

            var functionStore = new FM1DFileFunctionStore(hydroNetwork)
            { 
                Functions = functions,
                Path = withPath ? filePath : null,
            };

            return (functionStore, coverage, variable);
        }

        [Test]
        [TestCaseSource(nameof(GetSimulatedInvalidData))]
        public void WhenGetVariableValues_WithInvalidData_ThenReturnEmptyMultiDimensionalArray(FM1DFileFunctionStore testFM1D, 
                                                                                               IVariable variable, 
                                                                                               IVariableFilter[] filters)
        {
            //Act
            IMultiDimensionalArray<double> actual = testFM1D.GetVariableValues<double>(variable, filters);

            //Assert
            Assert.That(actual, Is.EqualTo(emptyMultiDimensionalArray));
        }

        private static IEnumerable<TestCaseData> GetSimulatedInvalidData()
        {
            TestCaseData ToTestCase(IVariableFilter filter,
                                    string name,
                                    bool withCoverage = true,
                                    bool withDictionary = true,
                                    bool withPath = true)
            {
                (FM1DFileFunctionStore functionStore, ICoverage _, IVariable variable) = GetDefaultFunctionStore(withCoverage, withDictionary, withPath);
                IVariableFilter[] filters = filter != null ? new[] { filter } : null;
                return new TestCaseData(functionStore, variable, filters).SetName(name);
            }

            yield return ToTestCase(null, "Filters null");

            yield return ToTestCase(Substitute.For<IVariableFilter>(), "No valid file", withPath: false);
            yield return ToTestCase(Substitute.For<IVariableFilter>(), "No coverage", withCoverage: false);
            yield return ToTestCase(Substitute.For<IVariableFilter>(), "No valid data in coverage", withDictionary: false);
        }

        [Test]
        public void WhenGetVariableValues_WithHasNoShapeOrTimeseries_ThenUseBaseGetVariableValuesAsReturn()
        {
            //Arrange
            (FM1DFileFunctionStore testFM1D, ICoverage coverage, IVariable variable) = GetDefaultFunctionStore();
            var filter = Substitute.For<IVariableValueFilter>();

            var baseReadOnlyNetCdfFunctionStore = Substitute.ForPartsOf<ReadOnlyNetCdfFunctionStoreBase>();
            baseReadOnlyNetCdfFunctionStore.Path = filePath;
            baseReadOnlyNetCdfFunctionStore.Functions = GetEventedFuntionList(coverage);

            //Act
            IMultiDimensionalArray<double> actualOfTestClass = testFM1D.GetVariableValues<double>(variable, filter);

            //Assert
            IMultiDimensionalArray<double> expectedOfBaseClass = baseReadOnlyNetCdfFunctionStore.GetVariableValues<double>(variable, filter);
            Assert.That(actualOfTestClass, Is.Not.Null);
            Assert.That(actualOfTestClass.GetType(), Is.EqualTo(expectedOfBaseClass.GetType()));
        }

        private static IEventedList<IFunction> GetEventedFuntionList(ICoverage givenCoverage)
        {
            IEventedList<IFunction> function = new EventedList<IFunction>();
            function.Add(givenCoverage);
            return function;
        }

        private static ICoverage GetCoverage(IVariable givenVariable, bool withDictionary = true)
        {
            var substitute = Substitute.For<ICoverage>();
            IEventedList<IVariable> eventedList = new EventedList<IVariable>();
            eventedList.Add(givenVariable);

            substitute.Arguments.Returns(eventedList);
            IVariable variable2 = GetVariableSubstitute(withDictionary);
            substitute.Components[0].Returns(variable2);
            return substitute;
        }

        private static IVariable GetVariableSubstitute(bool withDictionary = true)
        {
            var substitute = Substitute.For<IVariable>();

            if (withDictionary)
            {
                substitute.Attributes = GetDictionaryFake();
            }

            substitute.ValueType.Returns(typeof(double));
            substitute.IsIndependent.Returns(false);

            return substitute;
        }

        private static IDictionary<string, string> GetDictionaryFake()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(dictionaryData, dictionaryData);
            return dictionary;
        }
    }
}