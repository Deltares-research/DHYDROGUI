using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class BoundaryContainerTest
    {
        public static IEnumerable<TestCaseData> UpdateGridBoundaryData
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(Substitute.For<IGridBoundary>());
            }
        }

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
            Assert.That(container.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(container.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void DefinitionPerFileUsedPropertySetToTrue_ShouldDeleteAllBoundaries()
        {
            // Setup
            var container = new BoundaryContainer();
            var waveBoundary = Substitute.For<IWaveBoundary>();
            container.Boundaries.Add(waveBoundary);

            // Call
            container.DefinitionPerFileUsed = true;

            // Assert
            Assert.That(container.Boundaries, Is.Not.Null);
            Assert.That(container.Boundaries, Is.Empty);
            Assert.That(container.DefinitionPerFileUsed, Is.True);
        }

        [Test]
        public void DefinitionPerFileUsedPropertySetToFalse_ShouldNotDeleteAllBoundaries()
        {
            // Setup
            var container = new BoundaryContainer();
            var waveBoundary = Substitute.For<IWaveBoundary>();
            container.Boundaries.Add(waveBoundary);

            // Call
            container.DefinitionPerFileUsed = false;

            // Assert
            Assert.That(container.Boundaries[0], Is.SameAs(waveBoundary));
            Assert.That(container.DefinitionPerFileUsed, Is.False);
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
            var gridBoundary = Substitute.For<IGridBoundary>();

            // When
            container.UpdateGridBoundary(gridBoundary);
            IBoundarySnappingCalculator result = container.GetBoundarySnappingCalculator();

            // Assert
            Assert.That(result.GridBoundary, Is.SameAs(gridBoundary));
        }

        [Test]
        [TestCaseSource(nameof(UpdateGridBoundaryData))]
        public void GivenABoundaryContainer_WhenUpdateGridBoundaryAndGetGridBoundaryIsCalled_ThenTheGridBoundaryIsReturned(IGridBoundary gridBoundary)
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
        public void GivenABoundaryContainerWithANonNullGridBoundary_WhenUpdateGridBoundaryAndGetGridBoundaryIsCalled_ThenTheGridBoundaryIsReturned(IGridBoundary gridBoundary)
        {
            // Given
            var container = new BoundaryContainer();
            var gridBoundaryInitial = Substitute.For<IGridBoundary>();
            container.UpdateGridBoundary(gridBoundaryInitial);

            // When
            container.UpdateGridBoundary(gridBoundary);
            IGridBoundary result = container.GetGridBoundary();

            // Assert
            Assert.That(result, Is.SameAs(gridBoundary));
        }
    }
}