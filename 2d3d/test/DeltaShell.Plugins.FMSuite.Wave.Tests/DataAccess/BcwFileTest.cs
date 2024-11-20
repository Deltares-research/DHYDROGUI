using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class BcwFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeVaryingAndSpaceVarying()
        {
            string bcwFilePath = TestHelper.GetTestFilePath(@"bcwTimeseries\timeseries.bcw");
            var bcwFile = new BcwFile();

            IDictionary<string, List<IFunction>> bcwTimeseries = bcwFile.Read(bcwFilePath);

            Assert.AreEqual("Boundary 1", bcwTimeseries.ElementAt(0).Key);
            Assert.AreEqual("Boundary 2", bcwTimeseries.ElementAt(1).Key);

            List<IFunction> series = bcwTimeseries["Boundary 1"];
            Assert.AreEqual(3, series.Count);
            object[] values = series[1].GetAllComponentValues(new DateTime(2006, 1, 5).AddMinutes(120.0));

            Assert.AreEqual(2.0, values[0]);
            Assert.AreEqual(7.0, values[1]);
            Assert.AreEqual(180.0, values[2]);
            Assert.AreEqual(10.0, values[3]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenABcwFileWithATimeVaryingAndUniformBoundaryWithEmptyTimeseries_WhenReadingThisFile_ThenACorrectDictionaryShouldBeBuild()
        {
            // Given
            string bcwFilePath = TestHelper.GetTestFilePath(@"bcwTimeseries\timeseriesuniform.bcw");
            string newBcwFilePath = WaveTestHelper.CreateLocalCopy(bcwFilePath);

            var bcwFile = new BcwFile();

            // When
            IDictionary<string, List<IFunction>> bcwTimeseries = bcwFile.Read(newBcwFilePath);

            // Then
            Assert.AreEqual("BoundaryCondition01", bcwTimeseries.ElementAt(0).Key, "The name of the boundary is different than expected");

            List<IFunction> supportPoints = bcwTimeseries["BoundaryCondition01"];
            Assert.AreEqual(1, supportPoints.Count, "The imported boundary is not uniform");

            IFunction supportPoint = supportPoints[0];

            Assert.AreEqual(3, supportPoint.Attributes.Count, "The number of attributes is different than expected");
            Assert.AreEqual(1, supportPoint.Arguments.Count, "The number of arguments is different than expected");
            Assert.AreEqual(4, supportPoint.Components.Count, "The number of components is different than expected");

            foreach (IVariable component in supportPoint.Components)
            {
                Assert.AreEqual(0, component.Values.Count, "Time series data  has been imported while it was not written in the bcw file.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteAndCompare()
        {
            string bcwFilePath = TestHelper.GetTestFilePath(@"expectedTimeseries.bcw");
            var bcwExportFilePath = "generatedTimeseries.bcw";

            var bcwFile = new BcwFile();
            IDictionary<string, List<IFunction>> result = bcwFile.Read(bcwFilePath);
            bcwFile.Write(result, bcwExportFilePath);

            string[] originalLines = File.ReadAllLines(bcwFilePath);
            string[] exportedLines = File.ReadAllLines(bcwExportFilePath);

            // remove whitespace and empty lines
            List<string> original = originalLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();
            List<string> export = exportedLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();

            Assert.AreEqual(original, export);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenABoundaryConditionNameWithoutAnyFunctions_WhenWriteIsCalled_ThenOnlTheNameIsWrittenToTheFile()
        {
            // Given
            const string boundaryConditionName = "boundary_condition";
            var expectedLine = $"location             '{boundaryConditionName}'";
            const string fileName = "Waves.bcw";
            var bcwFile = new BcwFile();

            var boundaryConditionToFunctionsMappings = new Dictionary<string, List<IFunction>> {{boundaryConditionName, new List<IFunction>()}};

            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                string filePath = Path.Combine(tempDirectory, fileName);
                bcwFile.Write(boundaryConditionToFunctionsMappings, filePath);

                Assert.That(File.Exists(filePath),
                            "The .bcw file should exist after the Write method was called.");
                string[] linesInFile = File.ReadAllLines(filePath);
                Assert.That(linesInFile.Length, Is.EqualTo(1),
                            "When a boundary condition does not have any functions, only one line is expected to be written to the file.");
                Assert.AreEqual(expectedLine, linesInFile.First(),
                                "When a boundary condition does not have any functions, the only line in the file is expected to describe the name of the boundary condition.");
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenABoundaryConditionNameWithTimeSeriesWithoutValues_WhenWriteIsCalled_ThenExceptionIsRisen()
        {
            // Given
            const string boundaryConditionName = "boundary_condition";
            const string fileName = "Waves.bcw";
            var bcwFile = new BcwFile();
            var functions = new List<IFunction>();
            string noValuesComponent = KnownWaveProperties.WaveHeight;
            string expectedErrorMssg = string.Format(Resources.BcwFile_WriteBoundaryData_No_values_given_for__0__, noValuesComponent);
            string expectedLogMssg = string.Format(Resources.BcwFile_Write_While_saving_the_following_error_was_thrown___0___validate_the_model_for_more_information_, expectedErrorMssg);

            // Generate Time series function with no values in one of the components.
            var timeSeriesFunction = new TimeSeries();
            timeSeriesFunction.Attributes.Add("time_function", "dummy");
            timeSeriesFunction.Attributes.Add("reference_date", "20200616");
            timeSeriesFunction.Attributes.Add("time_unit", "days");
            timeSeriesFunction.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            var dummyUnit = new Unit("dummy unit");
            var components = new List<string>
            {
                KnownWaveProperties.WaveHeight,
                KnownWaveProperties.Period,
                KnownWaveProperties.Direction,
                KnownWaveProperties.DirectionalSpreadingValue
            };
            timeSeriesFunction.Arguments[0].Values.Add(new DateTime());
            timeSeriesFunction.Components.AddRange(
                components.Select(c => new Variable<double>(c) {Unit = dummyUnit}));
            timeSeriesFunction.Components
                              .Single(c => c.Name.Equals(noValuesComponent))
                              .Values.Clear();

            functions.Add(timeSeriesFunction);
            var boundaryConditionToFunctionsMappings = new Dictionary<string, List<IFunction>> {{boundaryConditionName, functions}};

            // When
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                string filePath = Path.Combine(tempDirectory, fileName);
                TestDelegate testAction = () => bcwFile.Write(boundaryConditionToFunctionsMappings, filePath);

                // Then
                Assert.That(testAction, Throws.Nothing);
                TestHelper.AssertAtLeastOneLogMessagesContains(testAction.Invoke, expectedLogMssg);
            });
        }



        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenABcwFileWithOnlyABoundaryConditionName_WhenReadIsCalled_ThenADictionaryIsReturnedWithThisNameWithoutFunctions()
        {
            // Given
            const string boundaryConditionName = "boundary_condition";
            const string fileName = "Waves.bcw";
            var bcwFile = new BcwFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                string filePath = Path.Combine(tempDirectory, fileName);
                File.WriteAllLines(filePath, new[]
                {
                    $@"location '{boundaryConditionName}'"
                });

                // When
                IDictionary<string, List<IFunction>> boundaryConditionToFunctionsMappings = bcwFile.Read(filePath);

                // Then
                Assert.That(boundaryConditionToFunctionsMappings.Count, Is.EqualTo(1),
                            "One boundary condition should have been read from the file.");
                KeyValuePair<string, List<IFunction>> boundaryConditionToFunctionsMapping = boundaryConditionToFunctionsMappings.First();
                Assert.That(boundaryConditionToFunctionsMapping.Key, Is.EqualTo(boundaryConditionName),
                            $"The read boundary condition name from the file was expected to be {boundaryConditionName}.");
                Assert.That(boundaryConditionToFunctionsMapping.Value, Is.EqualTo(new List<IFunction>()),
                            "No functions should have been read from the file.");
            });
        }
    }
}