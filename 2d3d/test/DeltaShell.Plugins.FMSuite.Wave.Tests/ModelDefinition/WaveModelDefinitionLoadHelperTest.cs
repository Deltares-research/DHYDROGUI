using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class WaveModelDefinitionLoadHelperTest
    {
        [Test]
        public void TransferLoadedProperties_TargetDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => WaveModelDefinitionLoadHelper.TransferLoadedProperties(null, new WaveModelDefinition());

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("targetDefinition"));
        }

        [Test]
        public void TransferLoadedProperties_LoadedDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => WaveModelDefinitionLoadHelper.TransferLoadedProperties(new WaveModelDefinition(), null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("loadedDefinition"));
        }

        [Test]
        public void GivenModelDefinitionWithObstacles_WhenTransferLoadedProperties_ThenObstaclesTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var waveObstacles = new[]
            {
                new WaveObstacle(),
                new WaveObstacle()
            };
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.FeatureContainer.Obstacles.AddRange(waveObstacles);

            // Precondition
            Assert.That(targetDefinition.FeatureContainer.Obstacles, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.FeatureContainer.Obstacles, targetDefinition.FeatureContainer.Obstacles);
        }

        [Test]
        public void GivenModelDefinitionWithObservationPoints_WhenTransferLoadedProperties_ThenObservationPointsTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var observationPoints = new[]
            {
                new Feature2DPoint(),
                new Feature2DPoint()
            };
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.FeatureContainer.ObservationPoints.AddRange(observationPoints);

            // Precondition
            Assert.That(targetDefinition.FeatureContainer.ObservationPoints, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.FeatureContainer.ObservationPoints, targetDefinition.FeatureContainer.ObservationPoints);
        }

        [Test]
        public void GivenModelDefinitionWithObservationCrossSection_WhenTransferLoadedProperties_ThenObservationCrossSectionTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var crossSections = new[]
            {
                new Feature2D(),
                new Feature2D()
            };
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.FeatureContainer.ObservationCrossSections.AddRange(crossSections);

            // Precondition
            Assert.That(targetDefinition.FeatureContainer.ObservationPoints, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.FeatureContainer.ObservationCrossSections, targetDefinition.FeatureContainer.ObservationCrossSections);
        }

        [Test]
        public void GivenModelDefinitionWithBoundaryContainerWithBoundaryForFilesProperties_WhenTransferLoadedProperties_ThenBoundaryContainerTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var grid = Substitute.For<IGridBoundary>();

            var loadedDefinition = new WaveModelDefinition();
            IBoundaryContainer loadedBoundaryContainer = loadedDefinition.BoundaryContainer;
            loadedBoundaryContainer.DefinitionPerFileUsed = true;
            loadedBoundaryContainer.FilePathForBoundariesPerFile = "JustAFilePath";
            loadedBoundaryContainer.UpdateGridBoundary(grid);
            loadedBoundaryContainer.Boundaries.AddRange(new[]
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>()
            });

            // Precondition
            IBoundaryContainer targetBoundaryContainer = targetDefinition.BoundaryContainer;
            Assert.That(targetBoundaryContainer.DefinitionPerFileUsed, Is.False);
            Assert.That(targetBoundaryContainer.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(targetBoundaryContainer.Boundaries, Is.Empty);
            Assert.That(targetBoundaryContainer.GetGridBoundary(), Is.Null);
            Assert.That(targetBoundaryContainer.GetBoundarySnappingCalculator(), Is.Null);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetBoundaryContainer.DefinitionPerFileUsed, Is.True);
            Assert.That(targetBoundaryContainer.FilePathForBoundariesPerFile, Is.EqualTo(loadedBoundaryContainer.FilePathForBoundariesPerFile));
            Assert.That(targetBoundaryContainer.Boundaries, Is.Empty);

            Assert.That(targetBoundaryContainer.GetBoundarySnappingCalculator(), Is.Not.Null);
            Assert.That(targetBoundaryContainer.GetGridBoundary(), Is.SameAs(grid));
        }

        [Test]
        public void GivenModelDefinitionWithBoundaryContainerWithBoundaries_WhenTransferLoadedProperties_ThenBoundaryContainerTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var grid = Substitute.For<IGridBoundary>();

            var loadedDefinition = new WaveModelDefinition();
            IBoundaryContainer loadedBoundaryContainer = loadedDefinition.BoundaryContainer;
            loadedBoundaryContainer.DefinitionPerFileUsed = false;
            loadedBoundaryContainer.FilePathForBoundariesPerFile = "JustAFilePath";
            loadedBoundaryContainer.UpdateGridBoundary(grid);
            loadedBoundaryContainer.Boundaries.AddRange(new[]
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>()
            });

            // Precondition
            IBoundaryContainer targetBoundaryContainer = targetDefinition.BoundaryContainer;
            Assert.That(targetBoundaryContainer.DefinitionPerFileUsed, Is.False);
            Assert.That(targetBoundaryContainer.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(targetBoundaryContainer.Boundaries, Is.Empty);
            Assert.That(targetBoundaryContainer.GetGridBoundary(), Is.Null);
            Assert.That(targetBoundaryContainer.GetBoundarySnappingCalculator(), Is.Null);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetBoundaryContainer.DefinitionPerFileUsed, Is.False);
            Assert.That(targetBoundaryContainer.FilePathForBoundariesPerFile, Is.Empty);
            CollectionAssert.AreEqual(loadedBoundaryContainer.Boundaries, targetBoundaryContainer.Boundaries);

            Assert.That(targetBoundaryContainer.GetBoundarySnappingCalculator(), Is.Not.Null);
            Assert.That(targetBoundaryContainer.GetGridBoundary(), Is.SameAs(grid));
        }

        [Test]
        public void GivenModelDefinitionWithOuterDomain_WhenTransferLoadedProperties_ThenOuterDomainTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var outerDomain = Substitute.For<IWaveDomainData>();
            var loadedDefinition = new WaveModelDefinition { OuterDomain = outerDomain };

            // Precondition
            Assert.That(targetDefinition.OuterDomain, Is.Null);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetDefinition.OuterDomain, Is.SameAs(outerDomain));
        }

        [Test]
        public void GivenModelDefinitionWithProperties_WhenTransferLoadedProperties_ThenPropertiesTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var loadedDefinition = new WaveModelDefinition();

            WaveModelProperty firstProperty = loadedDefinition.Properties.First();
            ModelPropertyDefinition existingPropertyDefinition = firstProperty.PropertyDefinition;
            var modifiedExistingProperty = new WaveModelProperty(existingPropertyDefinition, "NewValue");
            loadedDefinition.SetModelProperty(existingPropertyDefinition.FileSectionName,
                                              existingPropertyDefinition.FilePropertyKey,
                                              modifiedExistingProperty);

            var newPropertyDefinition = new WaveModelPropertyDefinition
            {
                DataType = typeof(string),
                FileSectionName = "NewFileSection",
                FilePropertyKey = "NewFileProperty"
            };
            var newProperty = new WaveModelProperty(newPropertyDefinition, "JustAValue");
            loadedDefinition.SetModelProperty(newPropertyDefinition.FileSectionName,
                                              newPropertyDefinition.FilePropertyKey,
                                              newProperty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            int expectedNrOfProperties = loadedDefinition.Properties.Count;
            Assert.That(targetDefinition.Properties, Has.Count.EqualTo(expectedNrOfProperties));

            for (var i = 0; i < expectedNrOfProperties; i++)
            {
                WaveModelProperty targetProperty = targetDefinition.Properties[i];
                WaveModelProperty expectedProperty = loadedDefinition.Properties[i];

                Assert.That(targetProperty, Is.SameAs(expectedProperty));
            }
        }
    }
}