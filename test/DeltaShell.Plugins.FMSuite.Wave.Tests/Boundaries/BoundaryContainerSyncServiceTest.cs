using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class BoundaryContainerSyncServiceTest
    {
        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryContainerSyncService(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModelWithABoundaryContainerSyncService_WhenTheGridOfTheOuterDomainIsChanged_ThenTheBoundaryContainerIsUpdated()
        {
            // Given
            using (var model = new WaveModel())
            {
                IBoundaryContainer boundaryContainer = model.BoundaryContainer;
                boundaryContainer.Boundaries.Add(Substitute.For<IWaveBoundary>());

                IBoundarySnappingCalculator initialCalculator = boundaryContainer.GetBoundarySnappingCalculator();

                // When
                model.OuterDomain.Grid = new CurvilinearGrid(10, 10, new List<double>(), new List<double>(), null);

                // Then
                Assert.That(boundaryContainer.GetBoundarySnappingCalculator(), Is.Not.SameAs(initialCalculator));
                Assert.That(boundaryContainer.Boundaries, Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModelWithABoundaryContainerSyncService_WhenTheOuterDomainIsChanged_ThenTheBoundaryContainerIsUpdated()
        {
            // Given
            using (var model = new WaveModel())
            {
                IBoundaryContainer boundaryContainer = model.BoundaryContainer;
                boundaryContainer.Boundaries.Add(Substitute.For<IWaveBoundary>());

                IBoundarySnappingCalculator initialCalculator = boundaryContainer.GetBoundarySnappingCalculator();

                var domainData = new WaveDomainData("name");
                domainData.Grid = new CurvilinearGrid(10, 10, new List<double>(), new List<double>(), null);

                // When
                model.OuterDomain = domainData;

                // Then
                Assert.That(boundaryContainer.GetBoundarySnappingCalculator(), Is.Not.SameAs(initialCalculator));
                Assert.That(boundaryContainer.Boundaries, Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModelWithABoundaryContainerSyncService_WhenTheOuterDomainIsChanged_ThenTheBoundaryContainerDoesNotListenToTheOldDomain()
        {
            // Given
            using (var model = new WaveModel())
            {
                IBoundaryContainer boundaryContainer = model.BoundaryContainer;

                WaveDomainData oldDomain = model.OuterDomain;

                // When
                model.OuterDomain = new WaveDomainData("name");

                var boundary = Substitute.For<IWaveBoundary>();
                boundaryContainer.Boundaries.Add(boundary);
                IBoundarySnappingCalculator calculator = boundaryContainer.GetBoundarySnappingCalculator();

                // Then
                oldDomain.Grid = new CurvilinearGrid(10, 10, new List<double>(), new List<double>(), null);

                Assert.That(boundaryContainer.Boundaries, Has.Count.EqualTo(1));
                Assert.That(boundaryContainer.Boundaries.First(), Is.SameAs(boundary));
                Assert.That(boundaryContainer.GetBoundarySnappingCalculator(), Is.SameAs(calculator));
            }
        }
    }
}