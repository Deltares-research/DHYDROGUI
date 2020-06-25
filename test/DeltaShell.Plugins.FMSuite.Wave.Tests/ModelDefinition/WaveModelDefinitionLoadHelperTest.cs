using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class WaveModelDefinitionLoadHelperTest
    {
        [Test]
        public void GivenModelDefinitionWithObstacles_WhenTransferLoadedProperties_ThenObstaclesTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var waveObstacles = new[] {new WaveObstacle(), new WaveObstacle()};
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.Obstacles.AddRange(waveObstacles);

            // Precondition
            Assert.That(targetDefinition.Obstacles, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.Obstacles, targetDefinition.Obstacles);
        }

        [Test]
        public void GivenModelDefinitionWithObservationPoints_WhenTransferLoadedProperties_ThenObservationPointsTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var observationPoints = new[] { new Feature2DPoint(), new Feature2DPoint() };
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.ObservationPoints.AddRange(observationPoints);

            // Precondition
            Assert.That(targetDefinition.ObservationPoints, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.ObservationPoints, targetDefinition.ObservationPoints);
        }

        [Test]
        public void GivenModelDefinitionWithObservationCrossSection_WhenTransferLoadedProperties_ThenObservationCrossSectionTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var crossSections = new[] { new Feature2D(), new Feature2D() };
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.ObservationCrossSections.AddRange(crossSections);

            // Precondition
            Assert.That(targetDefinition.ObservationPoints, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.ObservationCrossSections, targetDefinition.ObservationCrossSections);
        }
    }

    public static class WaveModelDefinitionLoadHelper
    {
        /// <summary>
        /// Transfers the definitions from the <paramref name="loadedDefinition"/> to the <paramref name="targetDefinition"/>.
        /// </summary>
        /// <param name="targetDefinition">The <see cref="WaveModelDefinition"/> to transfer the properties to.</param>
        /// <param name="loadedDefinition">The <see cref="WaveModelDefinition"/> that contains the properties to transfer.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public static void TransferLoadedProperties(WaveModelDefinition targetDefinition, WaveModelDefinition loadedDefinition)
        {
            targetDefinition.ObservationPoints.AddRange(loadedDefinition.ObservationPoints);
            targetDefinition.ObservationCrossSections.AddRange(loadedDefinition.ObservationCrossSections);
            targetDefinition.Obstacles.AddRange(loadedDefinition.Obstacles);
        }
    }
}