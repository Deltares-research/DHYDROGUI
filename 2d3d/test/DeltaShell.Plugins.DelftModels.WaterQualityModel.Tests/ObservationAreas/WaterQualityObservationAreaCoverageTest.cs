using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.ObservationAreas
{
    [TestFixture]
    public class WaterQualityObservationAreaCoverageTest
    {
        private const int ExpectedNoDataValue = -999;

        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 1, 1);

            // call
            var coverage = new WaterQualityObservationAreaCoverage(grid);

            // assert
            Assert.IsInstanceOf<UnstructuredGridCellCoverage>(coverage);
            Assert.IsFalse(coverage.IsTimeDependent);
            Assert.AreEqual(1, coverage.Arguments.Count);
            Assert.AreEqual("cell_index", coverage.Arguments[0].Name);
            CollectionAssert.AreEqual(Enumerable.Range(0, grid.Cells.Count).ToArray(), coverage.Arguments[0].GetValues<int>().ToArray());
            Assert.AreEqual(1, coverage.Components.Count);
            Assert.AreEqual("value", coverage.Components[0].Name);
            Assert.AreEqual(typeof(int), coverage.Components[0].ValueType);
            Assert.AreEqual(ExpectedNoDataValue, coverage.Components[0].DefaultValue);
            Assert.AreEqual(ExpectedNoDataValue, coverage.Components[0].NoDataValue);
            Assert.AreEqual(new[]
            {
                ExpectedNoDataValue
            }, coverage.Components[0].NoDataValues);
            CollectionAssert.AreEqual(Enumerable.Repeat(ExpectedNoDataValue, grid.Cells.Count).ToArray(), coverage.Components[0].GetValues<int>().ToArray());
            CollectionAssert.AreEquivalent(Enumerable.Repeat<string>(null, grid.Cells.Count), coverage.GetValuesAsLabels());
        }

        [Test]
        public void Clear_NonEmptyGrid_ClearAllValuesAndSetComponentsToNoDataValue()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 1, 1);
            var coverage = new WaterQualityObservationAreaCoverage(grid);

            foreach (int i in Enumerable.Range(0, grid.Cells.Count))
            {
                coverage[i] = i;
            }

            // call
            coverage.Clear();

            // assert
            CollectionAssert.AreEqual(Enumerable.Range(0, grid.Cells.Count).ToArray(),
                                      coverage.Arguments[0].GetValues<int>().ToArray());
            CollectionAssert.AreEqual(Enumerable.Repeat(ExpectedNoDataValue, grid.Cells.Count).ToArray(),
                                      coverage.Components[0].GetValues<int>().ToArray());
        }

        [Test]
        public void Evalute_CoorinateInGrid_ReturnValue()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 1, 1);
            var coverage = new WaterQualityObservationAreaCoverage(grid);
            coverage[2] = 12;

            // call
            object evaluatedValue = coverage.Evaluate(coverage.Coordinates.ElementAt(2));

            // assert
            Assert.AreEqual(12, evaluatedValue);
        }

        [Test]
        public void Clone_CoverageWithValues_CloneCoverageAndAssociations()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 1, 1);
            var coverage = new WaterQualityObservationAreaCoverage(grid);
            coverage.SetValuesAsLabels(new[]
            {
                "Nine",
                "Seven",
                "Five",
                "Two"
            });

            // call
            var clone = (WaterQualityObservationAreaCoverage) coverage.Clone();
            IList<string> clonedValues = clone.GetValuesAsLabels();

            // assert
            Assert.AreEqual("nine", clonedValues[0]);
            Assert.AreEqual("seven", clonedValues[1]);
            Assert.AreEqual("five", clonedValues[2]);
            Assert.AreEqual("two", clonedValues[3]);
        }

        [Test]
        public void GetOutputLocations_NoAreasSpecified_ReturnEmptyDictionary()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 1, 1);
            var observationAreas = new WaterQualityObservationAreaCoverage(grid);

            // call
            Dictionary<string, IList<int>> outputLocations = observationAreas.GetOutputLocations();

            // assert
            CollectionAssert.IsEmpty(outputLocations);
        }

        [Test]
        public void GetOutputLocations_WithAreasSpecified_ReturnOnlyDefinedAreas()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 4, 1, 1);
            var observationAreas = new WaterQualityObservationAreaCoverage(grid);

            IEnumerable<int> values = Enumerable.Range(0, grid.Cells.Count - 5).Select(i => i % 3).Concat(Enumerable.Repeat(-999, 5));
            observationAreas.SetValues(values);

            observationAreas.AddLabel("Zero");
            observationAreas.AddLabel("One");
            observationAreas.AddLabel("Two");

            // call
            Dictionary<string, IList<int>> outputLocations = observationAreas.GetOutputLocations();

            // assert
            Assert.AreEqual(3, outputLocations.Count);
            CollectionAssert.AreEqual(new[]
            {
                1,
                4,
                7,
                10,
                13
            }, outputLocations["zero"]);
            CollectionAssert.AreEqual(new[]
            {
                2,
                5,
                8,
                11,
                14
            }, outputLocations["one"]);
            CollectionAssert.AreEqual(new[]
            {
                3,
                6,
                9,
                12,
                15
            }, outputLocations["two"]);
        }

        [Test]
        public void AddTwoLabelsWithDifferentCapitals_ShouldStayOneLabel()
        {
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 4, 1, 1);
            var observationAreas = new WaterQualityObservationAreaCoverage(grid);

            IEnumerable<int> values = Enumerable.Range(0, grid.Cells.Count - 5).Select(i => i % 3).Concat(Enumerable.Repeat(-999, 5));
            observationAreas.SetValues(values);

            observationAreas.AddLabel("A");

            Assert.AreEqual(1, observationAreas.Components[0].Attributes.Count);

            observationAreas.AddLabel("a");

            Assert.AreEqual(1, observationAreas.Components[0].Attributes.Count);
        }
    }
}