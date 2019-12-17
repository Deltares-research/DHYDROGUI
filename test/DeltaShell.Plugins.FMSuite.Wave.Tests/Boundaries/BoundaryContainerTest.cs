using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using NSubstitute;
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
        [TestCaseSource(nameof(UpdateSnappingCalculatorData))]
        public void GivenABoundaryContainer_WhenUpdateSnappingCalculatorAndGetBoundarySnappingCalculatorIsCalled_ThenTheSetCalculatorIsReturned(IBoundarySnappingCalculator newCalculator)
        {
            // Given
            var container = new BoundaryContainer();

            // When
            container.UpdateSnappingCalculator(newCalculator);
            IBoundarySnappingCalculator result = container.GetBoundarySnappingCalculator();

            // Assert
            Assert.That(result, Is.SameAs(newCalculator));
        }

        [Test]
        [TestCaseSource(nameof(UpdateSnappingCalculatorData))]
        public void GivenABoundaryContainerWithANonNullSnappingCalculator_WhenUpdateSnappingCalculatorAndGetBoundarySnappingCalculatorIsCalled_ThenTheSetCalculatorIsReturned(IBoundarySnappingCalculator newCalculator)
        {
            // Given
            var container = new BoundaryContainer();
            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            container.UpdateSnappingCalculator(calculator);

            // When
            container.UpdateSnappingCalculator(newCalculator);
            IBoundarySnappingCalculator result = container.GetBoundarySnappingCalculator();

            // Assert
            Assert.That(result, Is.SameAs(newCalculator));
        }

        private IEnumerable<TestCaseData> UpdateSnappingCalculatorData
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(Substitute.For<IBoundarySnappingCalculator>());
            }
        }
    }
}