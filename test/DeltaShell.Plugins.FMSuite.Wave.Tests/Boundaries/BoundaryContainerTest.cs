using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class BoundaryContainerTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var container = new BoundaryContainer();

            // Assert
            Assert.That(container, Is.InstanceOf<IBoundaryContainer>());
            Assert.That(container, Is.InstanceOf<IBoundarySnappingCalculatorProvider>());
            Assert.That(container.Boundaries, Is.Not.Null);
            Assert.That(container.Boundaries, Is.Empty);
            Assert.That(container.GetBoundarySnappingCalculator(), Is.Null);
        }

        [Test]
        public void GivenABoundaryContainer_WhenUpdateGridBoundaryIsCalledWithNullAndGetBoundarySnappingCalculatorIsCalled_ThenCorrectCalculatorIsReturned()
        {
            // Given
            var container = new BoundaryContainer();

            // When
            container.UpdateGridBoundary(null);
            IBoundarySnappingCalculator result = container.GetBoundarySnappingCalculator();

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void GivenABoundaryContainer_WhenUpdateGridBoundaryAndGetBoundarySnappingCalculatorIsCalled_ThenCorrectCalculatorIsReturned()
        {
            // Given
            var container = new BoundaryContainer();
            var gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(3, 3);

            // When
            container.UpdateGridBoundary(gridBoundary);
            IBoundarySnappingCalculator result = container.GetBoundarySnappingCalculator();

            // Assert
            Assert.That(result.GridBoundary, Is.SameAs(gridBoundary));
        }

        [Test]
        [TestCaseSource(nameof(UpdateGridBoundaryData))]
        public void GivenABoundaryContainer_WhenUpdateGridBoundaryAndGetGridBoundaryIsCalled_ThenTheGridBoundaryIsReturned(GridBoundary gridBoundary)
        {
            // Given
            var container = new BoundaryContainer();

            // When
            container.UpdateGridBoundary(gridBoundary);
            IGridBoundary result = container.GetGridBoundary();

            // Assert
            Assert.That(result, Is.SameAs(gridBoundary));
        }

        [Test]
        [TestCaseSource(nameof(UpdateGridBoundaryData))]
        public void GivenABoundaryContainerWithANonNullGridBoundary_WhenUpdateGridBoundaryAndGetGridBoundaryIsCalled_ThenTheGridBoundaryIsReturned(GridBoundary gridBoundary)
        {
            // Given
            var container = new BoundaryContainer();
            GridBoundary gridBoundaryInitial = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(4, 4);
            container.UpdateGridBoundary(gridBoundaryInitial);

            // When
            container.UpdateGridBoundary(gridBoundary);
            IGridBoundary result = container.GetGridBoundary();

            // Assert
            Assert.That(result, Is.SameAs(gridBoundary));
        }

        private static IEnumerable<TestCaseData> UpdateGridBoundaryData
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(3, 3));
            }
        }
    }
}