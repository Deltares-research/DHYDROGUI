using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Coverages
{
    [TestFixture]
    public class UnstructuredGridCoverageFactoryTest
    {
        private readonly Random random = new Random(21);

        [Test]
        public void CreateVertexCoverage_ComponentValuesGiven_CreatesTheCorrectCoverage()
        {
            // Setup
            const string coverageName = "Some coverage";
            const double noDataValue = -999d;
            const double defaultValue = -999d;
            const int nValues = 16;

            UnstructuredGrid grid = GetGrid();
            double[] coverageValues = GetRandomRange(nValues).ToArray();

            // Call
            UnstructuredGridVertexCoverage coverage = UnstructuredGridCoverageFactory.CreateVertexCoverage(coverageName, grid, coverageValues);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo(coverageName));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(noDataValue));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(defaultValue));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(nValues));
            Assert.That(componentValues, Is.EqualTo(coverageValues));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(nValues));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, nValues)));
        }

        [Test]
        public void CreateCellCoverage_CreatesTheCorrectCoverage()
        {
            // Setup
            const string coverageName = "Some coverage";
            const double noDataValue = -999d;
            const double defaultValue = -999d;
            const int nValues = 9;

            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage(coverageName, grid);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo(coverageName));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(noDataValue));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(defaultValue));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(nValues));
            Assert.That(componentValues, Is.All.EqualTo(defaultValue));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(nValues));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, nValues)));
        }

        [Test]
        public void CreateCellCoverage_DefaultValueGiven_CreatesTheCorrectCoverage()
        {
            // Setup
            const string coverageName = "Some coverage";
            const double noDataValue = -999d;
            const double defaultValue = 7d;
            const int nValues = 9;

            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage(coverageName, grid, defaultValue: defaultValue);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo(coverageName));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(noDataValue));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(defaultValue));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(nValues));
            Assert.That(componentValues, Is.All.EqualTo(defaultValue));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(nValues));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, nValues)));
        }

        [Test]
        public void CreateCellCoverage_ComponentValuesGiven_CreatesTheCorrectCoverage()
        {
            // Setup
            const string coverageName = "Some coverage";
            const double noDataValue = -999d;
            const double defaultValue = -999d;
            const int nValues = 9;

            UnstructuredGrid grid = GetGrid();
            double[] coverageValues = GetRandomRange(nValues).ToArray();

            // Call
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage(coverageName, grid, coverageValues);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo(coverageName));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(noDataValue));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(defaultValue));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(nValues));
            Assert.That(componentValues, Is.EqualTo(coverageValues));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(nValues));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, nValues)));
        }

        [Test]
        public void CreateFlowLinkCoverage_CreatesTheCorrectCoverage()
        {
            // Setup
            const string coverageName = "Some coverage";
            const double noDataValue = -999d;
            const double defaultValue = -999d;
            const int nValues = 12;

            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridFlowLinkCoverage coverage = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage(coverageName, grid);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo(coverageName));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(noDataValue));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(defaultValue));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(nValues));
            Assert.That(componentValues, Is.All.EqualTo(defaultValue));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(nValues));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, nValues)));
        }

        private static UnstructuredGrid GetGrid() => UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

        private IEnumerable<double> GetRandomRange(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return random.Next();
            }
        }
    }
}