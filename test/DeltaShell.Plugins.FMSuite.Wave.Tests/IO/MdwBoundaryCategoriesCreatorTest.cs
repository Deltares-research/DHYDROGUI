using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class MdwBoundaryCategoriesCreatorTest
    {
        private static readonly Random random = new Random();

        private const double factor = 3.3;
        private readonly JonswapShape jonswapShape = new JonswapShape { PeakEnhancementFactor = factor };

        private const BoundaryConditionPeriodType periodType = BoundaryConditionPeriodType.Peak;

        [Test]
        public void CreateCategories_ShouldCreateACompleteCategoryForOneBoundary()
        {
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            
            // Boundary
            IWaveBoundary boundary1 = CreateWaveBoundary(out SupportPoint supportPoint1, out SupportPoint supportPoint2);
            const string boundaryName = "boundary1";
            boundary1.Name = boundaryName;

            // Add boundary to container
            var boundaries = new EventedList<IWaveBoundary> { boundary1 };
            boundaryContainer.Boundaries.Returns(boundaries);

            // Setup boundary container For boundary
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, supportPoint1, supportPoint2, out Coordinate coordinate1, out Coordinate coordinate2);

            // Act
            IEnumerable<DelftIniCategory> categories = MdwBoundaryCategoriesCreator.CreateCategories(boundaryContainer);
            
            // Assert
            DelftIniCategory createdCategory = categories.Single();
            List<DelftIniProperty> properties = createdCategory.Properties.ToList();
            
            CheckCreatedCategory(properties, boundaryName, coordinate1, coordinate2);
        }
        
        [Test]
        public void CreateCategories_ShouldCreateCategoriesForAllBoundaries()
        {
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Create boundaries
            IWaveBoundary boundary1 = CreateWaveBoundary(out SupportPoint boundary1SupportPoint1, out SupportPoint boundary1SupportPoint2);
            const string boundary1Name = "boundary1";
            boundary1.Name = boundary1Name;
            IWaveBoundary boundary2 = CreateWaveBoundary(out SupportPoint boundary2SupportPoint1, out SupportPoint boundary2SupportPoint2);
            const string boundary2Name = "boundary2";
            boundary2.Name = boundary2Name;

            // Add Boundaries to container
            var boundaries = new EventedList<IWaveBoundary> { boundary1 , boundary2};
            boundaryContainer.Boundaries.Returns(boundaries);

            // Setup boundary container for boundaries
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, boundary1SupportPoint1, boundary1SupportPoint2,
                                               out Coordinate boundary1Coordinate1, out Coordinate boundary1Coordinate2);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, boundary2SupportPoint1, boundary2SupportPoint2,
                                               out Coordinate boundary2Coordinate1, out Coordinate boundary2Coordinate2);

            // Act
            List<DelftIniCategory> categories = MdwBoundaryCategoriesCreator.CreateCategories(boundaryContainer).ToList();

            // Assert
            Assert.AreEqual(2, categories.Count);
            DelftIniCategory category1 = categories.First();
            DelftIniCategory category2 = categories.Last();
            
            CheckCreatedCategory(category1.Properties.ToList(), boundary1Name, boundary1Coordinate1, boundary1Coordinate2);
            CheckCreatedCategory(category2.Properties.ToList(), boundary2Name, boundary2Coordinate1, boundary2Coordinate2);
        }

        private IWaveBoundary CreateWaveBoundary(out SupportPoint supportPoint1, out SupportPoint supportPoint2)
        {
            var boundary1 = Substitute.For<IWaveBoundary>();

            // Geometry definition for boundary
            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            supportPoint1 = new SupportPoint(0, geometryDefinition);
            supportPoint2 = new SupportPoint(33, geometryDefinition);
            var supportPoints = new EventedList<SupportPoint>
            {
                supportPoint2,
                supportPoint1
            };
            boundary1.GeometricDefinition.SupportPoints.Returns(supportPoints);

            // Condition definition for boundary
            var dataComponent = new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                new TimeDependentParameters<PowerDefinedSpreading>(
                    Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);
            boundary1.ConditionDefinition.Returns(conditionDefinition);
            return boundary1;
        }

        private static void SetupBoundaryContainerForBoundary(IBoundarySnappingCalculator boundarySnappingCalculator, SupportPoint supportPoint1,
                                                               SupportPoint supportPoint2, out Coordinate coordinate1,
                                                               out Coordinate coordinate2)
        {
            coordinate1 = Substitute.For<Coordinate>();
            coordinate1.X = random.NextDouble();
            coordinate1.Y = random.NextDouble();
            coordinate2 = Substitute.For<Coordinate>();
            coordinate2.X = random.NextDouble();
            coordinate2.Y = random.NextDouble();
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint1).Returns(coordinate1);
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint2).Returns(coordinate2);
        }
        
        private static void CheckCreatedCategory(List<DelftIniProperty> properties, string boundary1Name, Coordinate coordinate1,
                                                 Coordinate coordinate2)
        {
            Assert.AreEqual(11, properties.Count);
            Assert.AreEqual(KnownWaveProperties.Name, properties[0].Name);
            Assert.AreEqual(boundary1Name, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.Definition, properties[1].Name);
            Assert.AreEqual("xy-coordinates", properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.StartCoordinateX, properties[2].Name);
            Assert.AreEqual(GetStringValue(coordinate1.X), properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.EndCoordinateX, properties[3].Name);
            Assert.AreEqual(GetStringValue(coordinate2.X), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.StartCoordinateY, properties[4].Name);
            Assert.AreEqual(GetStringValue(coordinate1.Y), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.EndCoordinateY, properties[5].Name);
            Assert.AreEqual(GetStringValue(coordinate2.Y), properties[5].Value);
            Assert.AreEqual(KnownWaveProperties.SpectrumSpec, properties[6].Name);
            Assert.AreEqual("parametric", properties[6].Value);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[7].Name);
            Assert.AreEqual("Jonswap", properties[7].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[8].Name);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[8].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[9].Name);
            Assert.AreEqual("Power", properties[9].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[10].Name);
            Assert.AreEqual(GetStringValue(factor), properties[10].Value);
        }

        private static string GetStringValue(double value)
        {
            return value.ToString("e7", CultureInfo.InvariantCulture);
        }
    }
}