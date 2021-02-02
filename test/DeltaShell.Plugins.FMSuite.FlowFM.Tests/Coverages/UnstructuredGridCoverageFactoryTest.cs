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
        private readonly Random random = new Random();

        [Test]
        public void CreateVertexCoverage_DefaultComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridVertexCoverage coverage = UnstructuredGridCoverageFactory.CreateVertexCoverage("Some coverage", grid);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(16));
            Assert.That(componentValues, Is.All.EqualTo(-999d));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(16));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 16)));
        }

        [Test]
        public void CreateVertexCoverage_CustomComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();
            double[] coverageValues = GetRandomRange(16).ToArray();

            // Call
            UnstructuredGridVertexCoverage coverage = UnstructuredGridCoverageFactory.CreateVertexCoverage("Some coverage", grid, coverageValues);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(16));
            Assert.That(componentValues, Is.EqualTo(coverageValues));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(16));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 16)));
        }

        [Test]
        public void CreateCellCoverage_DefaultComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage("Some coverage", grid);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(9));
            Assert.That(componentValues, Is.All.EqualTo(-999d));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(9));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 9)));
        }

        [Test]
        public void CreateCellCoverage_CustomComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();
            double[] coverageValues = GetRandomRange(9).ToArray();

            // Call
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage("Some coverage", grid, coverageValues);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(9));
            Assert.That(componentValues, Is.EqualTo(coverageValues));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(9));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 9)));
        }

        [Test]
        public void CreateFlowLinkCoverage_DefaultComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();

            // Call
            UnstructuredGridFlowLinkCoverage coverage = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage("Some coverage", grid);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(12));
            Assert.That(componentValues, Is.All.EqualTo(-999d));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(12));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 12)));
        }

        [Test]
        public void CreateFlowLinkCoverage_CustomComponentValues_CreatesTheCorrectCoverage()
        {
            // Setup
            UnstructuredGrid grid = GetGrid();
            double[] coverageValues = GetRandomRange(12).ToArray();

            // Call
            UnstructuredGridFlowLinkCoverage coverage = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage("Some coverage", grid, coverageValues);

            // Assert
            Assert.That(coverage.Name, Is.EqualTo("Some coverage"));
            Assert.That(coverage.Grid, Is.SameAs(grid));
            Assert.That(coverage.Components[0].NoDataValue, Is.EqualTo(-999d));
            Assert.That(coverage.Components[0].DefaultValue, Is.EqualTo(-999d));

            double[] componentValues = coverage.Components[0].Values.OfType<double>().ToArray();
            Assert.That(componentValues, Has.Length.EqualTo(12));
            Assert.That(componentValues, Is.EqualTo(coverageValues));

            int[] argumentValues = coverage.Arguments[0].Values.OfType<int>().ToArray();
            Assert.That(argumentValues, Has.Length.EqualTo(12));
            Assert.That(argumentValues, Is.EqualTo(Enumerable.Range(0, 12)));
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