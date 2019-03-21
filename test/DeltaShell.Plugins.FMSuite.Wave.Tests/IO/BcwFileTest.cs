using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class BcwFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeVaryingAndSpaceVarying()
        {
            var bcwFilePath = TestHelper.GetTestFilePath(@"bcwTimeseries\timeseries.bcw");
            var bcwFile = new BcwFile();

            var bcwTimeseries = bcwFile.Read(bcwFilePath);

            Assert.AreEqual("Boundary 1", bcwTimeseries.ElementAt(0).Key);
            Assert.AreEqual("Boundary 2", bcwTimeseries.ElementAt(1).Key);

            var series = bcwTimeseries["Boundary 1"];
            Assert.AreEqual(3, series.Count);
            var values = series[1].GetAllComponentValues(new DateTime(2006, 1, 5).AddMinutes(120.0));

            Assert.AreEqual(2.0, values[0]);
            Assert.AreEqual(7.0, values[1]);
            Assert.AreEqual(180.0, values[2]);
            Assert.AreEqual(10.0, values[3]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteAndCompare()
        {
            var bcwFilePath = TestHelper.GetTestFilePath(@"expectedTimeseries.bcw");
            var bcwExportFilePath = "generatedTimeseries.bcw";

            var bcwFile = new BcwFile();
            var result = bcwFile.Read(bcwFilePath);
            bcwFile.Write(result, bcwExportFilePath);

            var originalLines = File.ReadAllLines(bcwFilePath);
            var exportedLines = File.ReadAllLines(bcwExportFilePath);

            // remove whitespace and empty lines
            var original = originalLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();
            var export = exportedLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();

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

            var boundaryConditionToFunctionsMappings = new Dictionary<string, List<IFunction>>
            {
                {boundaryConditionName, new List<IFunction>()}
            };

            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                var filePath = Path.Combine(tempDirectory, fileName);
                bcwFile.Write(boundaryConditionToFunctionsMappings, filePath);

                Assert.That(File.Exists(filePath),
                    "The .bcw file should exist after the Write method was called.");
                var linesInFile = File.ReadAllLines(filePath);
                Assert.That(linesInFile.Length, Is.EqualTo(1),
                    "When a boundary condition does not have any functions, only one line is expected to be written to the file.");
                Assert.AreEqual(expectedLine, linesInFile.First(),
                    "When a boundary condition does not have any functions, the only line in the file is expected to describe the name of the boundary condition.");
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
                var filePath = Path.Combine(tempDirectory, fileName);
                File.WriteAllLines(filePath, new[] {$@"location '{boundaryConditionName}'"});

                // When
                var boundaryConditionToFunctionsMappings = bcwFile.Read(filePath);

                // Then
                Assert.That(boundaryConditionToFunctionsMappings.Count, Is.EqualTo(1),
                    "One boundary condition should have been read from the file.");
                var boundaryConditionToFunctionsMapping = boundaryConditionToFunctionsMappings.First();
                Assert.That(boundaryConditionToFunctionsMapping.Key, Is.EqualTo(boundaryConditionName),
                    $"The read boundary condition name from the file was expected to be {boundaryConditionName}.");
                Assert.That(boundaryConditionToFunctionsMapping.Value, Is.EqualTo(new List<IFunction>()),
                    "No functions should have been read from the file.");
            });
        }
    }
}