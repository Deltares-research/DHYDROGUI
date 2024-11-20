using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FMClassMapFileFunctionStoreTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAClassMapFilePath_WhenCreatingFMClassMapFileFunctionStoreWithThisPath_ThenCorrectGridAndFunctionsAreConstructed()
        {
            // Given
            string classMapFilePath = TestHelper.GetTestFilePath("output_classmapfiles\\harlingen_clm.nc");

            // When
            var classMapFileFunctionStore = new FMClassMapFileFunctionStore(classMapFilePath);

            // Then
            IEventedList<IFunction> functions = classMapFileFunctionStore.Functions;
            Assert.AreEqual(2, functions.Count);

            AssertCorrectFunctionData(functions[0], "Water level (mesh2d_s1)", "mesh2d_s1", "nmesh2d_face");
            AssertCorrectFunctionData(functions[1], "Water depth at pressure points (mesh2d_waterdepth)", "mesh2d_waterdepth", "nmesh2d_face");

            UnstructuredGrid grid = classMapFileFunctionStore.Grid;
            Assert.AreEqual(16597, grid.Cells.Count);
            Assert.AreEqual(typeof(UnstructuredGrid), grid.GetType());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"output_classmapfiles\FlowFMWithTimeZones_clm.nc", "Monday, 01 January 2001 00:00:00")]
        [TestCase(@"output_classmapfiles\FlowFMWithoutTimeZones_clm.nc", "Friday, 01 May 1992 00:00:00")]
        public void OpenClassMapFileWithOrWithoutTimeZones_ShouldSetReferenceDateInFunctionsCorrectly(string classMapFilePath, string expectedReferenceDate)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string classMapFilePathTemp = tempDirectory.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(classMapFilePath));

                // Act
                var store = new FMClassMapFileFunctionStore(classMapFilePathTemp);

                // Assert
                Assert.IsInstanceOf<FMNetCdfFileFunctionStore>(store);

                string retrievedReferenceDate = ((ICoverage) store.Functions.First()).Time.Attributes["ncRefDate"];
                Assert.AreEqual(expectedReferenceDate, retrievedReferenceDate);
            }
        }

        private static void AssertCorrectFunctionData(IFunction function, string functionName, string componentName, string argumentName)
        {
            Assert.AreEqual(typeof(UnstructuredGridCellCoverage), function.GetType());
            Assert.AreEqual(functionName, function.Name);
            Assert.AreEqual(2406565, function.GetValues().Count);

            Assert.AreEqual(1, function.Components.Count);
            IVariable component = function.Components[0];
            Assert.AreEqual(componentName, component.Name);
            Assert.AreEqual(typeof(byte), component.ValueType);

            IEventedList<IVariable> arguments = function.Arguments;
            Assert.AreEqual(2, arguments.Count);
            Assert.AreEqual("Time", arguments[0].Name);
            Assert.AreEqual(argumentName, arguments[1].Name);
        }
    }
}