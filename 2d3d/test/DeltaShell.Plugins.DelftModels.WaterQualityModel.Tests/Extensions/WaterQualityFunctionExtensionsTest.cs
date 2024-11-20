using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Extentions
{
    [TestFixture]
    public class WaterQualityFunctionExtensionsTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsConst()
        {
            IFunction constFunction = WaterQualityFunctionFactory.CreateConst("a", 1.0, "a", "a", "a");

            Assert.IsTrue(constFunction.IsConst());
            Assert.IsFalse(constFunction.IsTimeSeries());
            Assert.IsFalse(constFunction.IsNetworkCoverage());
            Assert.IsFalse(constFunction.IsUnstructuredGridCellCoverage());
            Assert.IsFalse(constFunction.IsFromHydroDynamics());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsTimeSeries()
        {
            IFunction timeSeries = WaterQualityFunctionFactory.CreateTimeSeries("a", 1.0, "a", "a", "a");

            Assert.IsFalse(timeSeries.IsConst());
            Assert.IsTrue(timeSeries.IsTimeSeries());
            Assert.IsFalse(timeSeries.IsNetworkCoverage());
            Assert.IsFalse(timeSeries.IsUnstructuredGridCellCoverage());
            Assert.IsFalse(timeSeries.IsFromHydroDynamics());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsNetworkCoverage()
        {
            IFunction networkCoverage = WaterQualityFunctionFactory.CreateNetworkCoverage("a", 1.0, "a", "a", "a");

            Assert.IsFalse(networkCoverage.IsConst());
            Assert.IsFalse(networkCoverage.IsTimeSeries());
            Assert.IsTrue(networkCoverage.IsNetworkCoverage());
            Assert.IsFalse(networkCoverage.IsUnstructuredGridCellCoverage());
            Assert.IsFalse(networkCoverage.IsFromHydroDynamics());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsUnstructuredGridCellCoverage()
        {
            IFunction cellCoverage = WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("a", 1.0, "a", "a", "a");

            Assert.IsFalse(cellCoverage.IsConst());
            Assert.IsFalse(cellCoverage.IsTimeSeries());
            Assert.IsFalse(cellCoverage.IsNetworkCoverage());
            Assert.IsTrue(cellCoverage.IsUnstructuredGridCellCoverage());
            Assert.IsFalse(cellCoverage.IsFromHydroDynamics());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsFromHydroDynamics()
        {
            FunctionFromHydroDynamics function = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("a", 1.0, "a", "a", "a");

            Assert.IsFalse(function.IsConst());
            Assert.IsFalse(function.IsTimeSeries());
            Assert.IsFalse(function.IsNetworkCoverage());
            Assert.IsFalse(function.IsUnstructuredGridCellCoverage());
            Assert.IsTrue(function.IsFromHydroDynamics());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Test_IsSegmentFile()
        {
            SegmentFileFunction function = WaterQualityFunctionFactory.CreateSegmentFunction("a", 1.0, "a", "a", "a", string.Empty);

            Assert.IsFalse(function.IsConst());
            Assert.IsFalse(function.IsTimeSeries());
            Assert.IsFalse(function.IsNetworkCoverage());
            Assert.IsFalse(function.IsUnstructuredGridCellCoverage());
            Assert.IsFalse(function.IsFromHydroDynamics());
            Assert.IsTrue(function.IsSegmentFile());
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ClearCoverageForTimeIndependent()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(1000, 1000, 10, 10);

            var rng = new Random(0);
            IEnumerable<double> randomData = Enumerable.Repeat(0, grid.Cells.Count).Select(v => 12.34 * rng.NextDouble());

            const double noDataValue = -999.0;

            var gridCellCoverage = new UnstructuredGridCellCoverage(grid, false);
            gridCellCoverage.Components[0].NoDataValue = noDataValue;
            gridCellCoverage.SetValues(randomData);

            // call
            TestHelper.AssertIsFasterThan(3500, () => gridCellCoverage.ClearCoverage());

            // assert
            Assert.AreEqual(1, gridCellCoverage.Arguments.Count);
            int[] argumentValues = gridCellCoverage.Arguments[0].GetValues<int>().ToArray();
            Assert.AreEqual(grid.Cells.Count, argumentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, grid.Cells.Count), argumentValues);

            Assert.AreEqual(1, gridCellCoverage.Components.Count);
            double[] componentValues = gridCellCoverage.Components[0].GetValues<double>().ToArray();
            Assert.AreEqual(grid.Cells.Count, componentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Repeat(noDataValue, grid.Cells.Count), componentValues);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ClearCoverageForTimeDependent()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(1000, 1000, 10, 10);

            var rng = new Random(0);

            const double noDataValue = -999.0;

            var gridCellCoverage = new UnstructuredGridCellCoverage(grid, true);
            gridCellCoverage.Components[0].NoDataValue = noDataValue;
            gridCellCoverage[new DateTime(2015, 2, 25, 16, 15, 33)] = Enumerable.Repeat(0, grid.Cells.Count).Select(v => 12.34 * rng.NextDouble());
            gridCellCoverage[new DateTime(2015, 2, 25, 16, 16, 44)] = Enumerable.Repeat(0, grid.Cells.Count).Select(v => 12.34 * rng.NextDouble());

            // call
            TestHelper.AssertIsFasterThan(3500, () => gridCellCoverage.ClearCoverage());

            // assert
            Assert.AreEqual(2, gridCellCoverage.Arguments.Count);
            DateTime[] timeValues = gridCellCoverage.Arguments[0].GetValues<DateTime>().ToArray();
            Assert.IsEmpty(timeValues);
            int[] argumentValues = gridCellCoverage.Arguments[1].GetValues<int>().ToArray();
            Assert.AreEqual(grid.Cells.Count, argumentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, grid.Cells.Count), argumentValues);

            Assert.AreEqual(1, gridCellCoverage.Components.Count);
            double[] componentValues = gridCellCoverage.Components[0].GetValues<double>().ToArray();
            Assert.IsEmpty(componentValues);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestAssigningNewGridToGridCellCoverage()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);

            var rng = new Random(0);
            IEnumerable<double> randomData = Enumerable.Repeat(0, grid.Cells.Count).Select(v => 12.34 * rng.NextDouble());

            const double noDataValue = -999.0;

            var gridCellCoverage = new UnstructuredGridCellCoverage(grid, false);
            gridCellCoverage.Components[0].NoDataValue = noDataValue;
            gridCellCoverage.SetValues(randomData);

            UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(7, 2, 10, 10);

            // call
            gridCellCoverage.AssignNewGridToCoverage(newGrid);

            // assert
            Assert.AreSame(newGrid, gridCellCoverage.Grid);

            Assert.AreEqual(1, gridCellCoverage.Arguments.Count);
            int[] argumentValues = gridCellCoverage.Arguments[0].GetValues<int>().ToArray();
            Assert.AreEqual(newGrid.Cells.Count, argumentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, newGrid.Cells.Count), argumentValues);

            Assert.AreEqual(1, gridCellCoverage.Components.Count);
            double[] componentValues = gridCellCoverage.Components[0].GetValues<double>().ToArray();
            Assert.AreEqual(newGrid.Cells.Count, componentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Repeat(noDataValue, newGrid.Cells.Count), componentValues);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestAssignNewGridToCoverageWithoutClearingData()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);

            var rng = new Random(0);
            double[] randomData = Enumerable.Repeat(0, grid.Cells.Count).Select(v => 12.34 * rng.NextDouble()).ToArray();

            const double noDataValue = -999.0;

            var gridCellCoverage = new UnstructuredGridCellCoverage(grid, false);
            gridCellCoverage.Components[0].NoDataValue = noDataValue;
            gridCellCoverage.SetValues(randomData);

            UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 5, 5); // Identical grid cells, but different size.

            // call
            gridCellCoverage.AssignNewGridToCoverage(newGrid, false);

            // assert
            Assert.AreSame(newGrid, gridCellCoverage.Grid);

            Assert.AreEqual(1, gridCellCoverage.Arguments.Count);
            int[] argumentValues = gridCellCoverage.Arguments[0].GetValues<int>().ToArray();
            Assert.AreEqual(newGrid.Cells.Count, argumentValues.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, newGrid.Cells.Count), argumentValues);

            Assert.AreEqual(1, gridCellCoverage.Components.Count);
            double[] componentValues = gridCellCoverage.Components[0].GetValues<double>().ToArray();
            Assert.AreEqual(newGrid.Cells.Count, componentValues.Length);
            CollectionAssert.AreEqual(randomData, componentValues);
        }
    }
}