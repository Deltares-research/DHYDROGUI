using System;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Wind;
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
            var loadedMeteoData = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXYP,
                XYVectorFilePath = nameof(WaveMeteoData.XYVectorFilePath),
                XComponentFilePath = nameof(WaveMeteoData.XComponentFilePath),
                YComponentFilePath = nameof(WaveMeteoData.YComponentFilePath),
                HasSpiderWeb = true,
                SpiderWebFilePath = nameof(WaveMeteoData.SpiderWebFilePath)
            };

            var loadedInputFieldData = new WaveInputFieldData
            {
                HydroDataType = InputFieldDataType.TimeVarying,
                WindDataType= InputFieldDataType.TimeVarying,
                WaterLevelConstant = random.NextDouble(),
                VelocityXConstant = random.NextDouble(),
                VelocityYConstant = random.NextDouble(),
                WindSpeedConstant = random.NextDouble(),
                WindDirectionConstant = random.NextDouble(),
                InputFields = Substitute.For<IFunction>(),
                MeteoData = loadedMeteoData
            };
            
            var loadedDefinition = new WaveModelDefinition
            {
                TimePointData = loadedInputFieldData
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
            Assert.That(targetInputFieldData.InputFields, Is.Not.SameAs(loadedInputFieldData.InputFields));

            WaveMeteoData targetMeteoData = targetInputFieldData.MeteoData;
            Assert.That(targetMeteoData.FileType, Is.EqualTo(WindDefinitionType.WindXY));
            Assert.That(targetMeteoData.XYVectorFilePath, Is.Null);
            Assert.That(targetMeteoData.XComponentFilePath, Is.Null);
            Assert.That(targetMeteoData.YComponentFilePath, Is.Null);
            Assert.That(targetMeteoData.HasSpiderWeb, Is.False);
            Assert.That(targetMeteoData.SpiderWebFilePath, Is.Null);

            // When
            WaveModelDefinitionLoadHelper.TransferLoadedProperties(targetDefinition, loadedDefinition);

            // Then
            Assert.That(targetInputFieldData.HydroDataType, Is.EqualTo(loadedInputFieldData.HydroDataType));
            Assert.That(targetInputFieldData.WindDataType, Is.EqualTo(loadedInputFieldData.WindDataType));
            Assert.That(targetInputFieldData.WaterLevelConstant, Is.EqualTo(loadedInputFieldData.WaterLevelConstant));
            Assert.That(targetInputFieldData.VelocityXConstant, Is.EqualTo(loadedInputFieldData.VelocityXConstant));
            Assert.That(targetInputFieldData.VelocityYConstant, Is.EqualTo(loadedInputFieldData.VelocityYConstant));
            Assert.That(targetInputFieldData.WindSpeedConstant, Is.EqualTo(loadedInputFieldData.WindSpeedConstant));
            Assert.That(targetInputFieldData.WindDirectionConstant, Is.EqualTo(loadedInputFieldData.WindDirectionConstant));
            Assert.That(targetInputFieldData.InputFields, Is.SameAs(loadedInputFieldData.InputFields));

            Assert.That(targetMeteoData.FileType, Is.EqualTo(loadedMeteoData.FileType));
            Assert.That(targetMeteoData.XYVectorFilePath, Is.EqualTo(loadedMeteoData.XYVectorFilePath));
            Assert.That(targetMeteoData.XComponentFilePath, Is.EqualTo(loadedMeteoData.XComponentFilePath));
            Assert.That(targetMeteoData.YComponentFilePath, Is.EqualTo(loadedMeteoData.YComponentFilePath));
            Assert.That(targetMeteoData.HasSpiderWeb, Is.EqualTo(loadedMeteoData.HasSpiderWeb));
            Assert.That(targetMeteoData.SpiderWebFilePath, Is.EqualTo(loadedMeteoData.SpiderWebFilePath));
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
            loadedDefinition.SetModelProperty(existingPropertyDefinition.FileCategoryName, 
                                              existingPropertyDefinition.FilePropertyName, 
                                              modifiedExistingProperty);

            var newPropertyDefinition = new WaveModelPropertyDefinition
            {
                DataType = typeof(string),
                FileCategoryName = "NewFileCategory",
                FilePropertyName = "NewFileProperty"
            };
            var newProperty = new WaveModelProperty(newPropertyDefinition, "JustAValue");
            loadedDefinition.SetModelProperty(newPropertyDefinition.FileCategoryName, 
                                              newPropertyDefinition.FilePropertyName, 
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