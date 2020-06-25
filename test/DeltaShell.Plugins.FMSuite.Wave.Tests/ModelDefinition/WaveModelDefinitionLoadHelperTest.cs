using System;
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

            var observationPoints = new[] {new Feature2DPoint(), new Feature2DPoint()};
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

            var crossSections = new[] {new Feature2D(), new Feature2D()};
            var loadedDefinition = new WaveModelDefinition();
            loadedDefinition.ObservationCrossSections.AddRange(crossSections);

            // Precondition
            Assert.That(targetDefinition.ObservationPoints, Is.Empty);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            CollectionAssert.AreEqual(loadedDefinition.ObservationCrossSections, targetDefinition.ObservationCrossSections);
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
            loadedBoundaryContainer.Boundaries.AddRange(new[] {Substitute.For<IWaveBoundary>(), Substitute.For<IWaveBoundary>()});

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
            loadedBoundaryContainer.Boundaries.AddRange(new[] {Substitute.For<IWaveBoundary>(), Substitute.For<IWaveBoundary>()});

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
            var loadedDefinition = new WaveModelDefinition
            {
                OuterDomain =  outerDomain
            };
        
            // Precondition
            Assert.That(targetDefinition.OuterDomain, Is.Null);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetDefinition.OuterDomain, Is.SameAs(outerDomain));
        }

        [Test]
        public void GivenModelDefinitionWithWaveInputFieldData_WhenTransferLoadedProperties_ThenWaveInputFieldDataTransferred()
        {
            // Given
            var targetDefinition = new WaveModelDefinition();

            var random = new Random(21);
            var inputFieldData = new WaveInputFieldData
            {
                HydroDataType = InputFieldDataType.TimeVarying,
                WindDataType= InputFieldDataType.TimeVarying,
                WaterLevelConstant = random.NextDouble(),
                VelocityXConstant = random.NextDouble(),
                VelocityYConstant = random.NextDouble(),
                WindSpeedConstant = random.NextDouble(),
                WindDirectionConstant = random.NextDouble()
            };
            
            var loadedDefinition = new WaveModelDefinition
            {
                TimePointData = inputFieldData
            };

            // Precondition
            WaveInputFieldData targetInputFieldData = targetDefinition.TimePointData;
            Assert.That(targetInputFieldData.HydroDataType, Is.EqualTo(InputFieldDataType.Constant));
            Assert.That(targetInputFieldData.WindDataType, Is.EqualTo(InputFieldDataType.Constant));
            Assert.That(targetInputFieldData.WaterLevelConstant, Is.EqualTo(0.0));
            Assert.That(targetInputFieldData.VelocityXConstant, Is.EqualTo(0.0));
            Assert.That(targetInputFieldData.VelocityYConstant, Is.EqualTo(0.0));
            Assert.That(targetInputFieldData.WindSpeedConstant, Is.EqualTo(0.0));
            Assert.That(targetInputFieldData.WindDirectionConstant, Is.EqualTo(0.0));

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetInputFieldData.HydroDataType, Is.EqualTo(inputFieldData.HydroDataType));
            Assert.That(targetInputFieldData.WindDataType, Is.EqualTo(inputFieldData.WindDataType));
            Assert.That(targetInputFieldData.WaterLevelConstant, Is.EqualTo(inputFieldData.WaterLevelConstant));
            Assert.That(targetInputFieldData.VelocityXConstant, Is.EqualTo(inputFieldData.VelocityXConstant));
            Assert.That(targetInputFieldData.VelocityYConstant, Is.EqualTo(inputFieldData.VelocityYConstant));
            Assert.That(targetInputFieldData.WindSpeedConstant, Is.EqualTo(inputFieldData.WindSpeedConstant));
            Assert.That(targetInputFieldData.WindDirectionConstant, Is.EqualTo(inputFieldData.WindDirectionConstant));
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
            if (targetDefinition == null)
            {
                throw new ArgumentNullException(nameof(targetDefinition));
            }

            if (loadedDefinition == null)
            {
                throw new ArgumentNullException(nameof(loadedDefinition));
            }

            targetDefinition.OuterDomain = loadedDefinition.OuterDomain;

            targetDefinition.ObservationPoints.AddRange(loadedDefinition.ObservationPoints);
            targetDefinition.ObservationCrossSections.AddRange(loadedDefinition.ObservationCrossSections);
            targetDefinition.Obstacles.AddRange(loadedDefinition.Obstacles);

            TransferTimePointData(targetDefinition.TimePointData, loadedDefinition.TimePointData);
            TransferBoundaryContainer(targetDefinition.BoundaryContainer, loadedDefinition.BoundaryContainer);
        }

        private static void TransferTimePointData(WaveInputFieldData targetTimePointData, WaveInputFieldData loadedTimePointData)
        {
            targetTimePointData.HydroDataType = loadedTimePointData.HydroDataType;
            targetTimePointData.WindDataType = loadedTimePointData.WindDataType;
            targetTimePointData.WaterLevelConstant = loadedTimePointData.WaterLevelConstant;
            targetTimePointData.VelocityXConstant = loadedTimePointData.VelocityXConstant;
            targetTimePointData.VelocityYConstant = loadedTimePointData.VelocityYConstant;
            targetTimePointData.WindSpeedConstant = loadedTimePointData.WindSpeedConstant;
            targetTimePointData.WindDirectionConstant = loadedTimePointData.WindDirectionConstant;
        }

        private static void TransferBoundaryContainer(IBoundaryContainer targetBoundaryContainer, IBoundaryContainer loadedBoundaryContainer)
        {
            targetBoundaryContainer.UpdateGridBoundary(loadedBoundaryContainer.GetGridBoundary());
            if (loadedBoundaryContainer.DefinitionPerFileUsed)
            {
                targetBoundaryContainer.DefinitionPerFileUsed = true;
                targetBoundaryContainer.FilePathForBoundariesPerFile = loadedBoundaryContainer.FilePathForBoundariesPerFile;
            }
            else
            {
                targetBoundaryContainer.Boundaries.AddRange(loadedBoundaryContainer.Boundaries);
            }
        }
    }
}