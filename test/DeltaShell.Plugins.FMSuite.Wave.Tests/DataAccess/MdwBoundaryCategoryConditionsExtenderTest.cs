using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class MdwBoundaryCategoryConditionsExtenderTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private const double height1 = 1.0;
        private const double period1 = 2.0;
        private const double direction1 = 3.0;
        private const double distance1 = 0;

        private const double height2 = 4.0;
        private const double period2 = 5.0;
        private const double direction2 = 6.0;
        private const double distance2 = 20;

        private const double factor = 3.3;

        private const BoundaryConditionPeriodType periodType = BoundaryConditionPeriodType.Peak;
        private readonly JonswapShape jonswapShape = new JonswapShape {PeakEnhancementFactor = factor};

        private static string SpreadingType
        {
            get
            {
                if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
                {
                    return KnownWaveBoundariesFileConstants.DegreesDefinedSpreading;
                }

                if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
                {
                    return KnownWaveBoundariesFileConstants.PowerDefinedSpreading;
                }

                throw new NotSupportedException();
            }
        }

        private static double SpreadingValue
        {
            get
            {
                if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
                {
                    return WaveSpreadingConstants.DegreesDefaultSpreading;
                }

                if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
                {
                    return WaveSpreadingConstants.PowerDefaultSpreading;
                }

                throw new NotSupportedException();
            }
        }

        private static IEnumerable<TestCaseData> DifferentShapes
        {
            get
            {
                yield return new TestCaseData(new JonswapShape {PeakEnhancementFactor = factor}, KnownWaveBoundariesFileConstants.JonswapShape, KnownWaveProperties.PeakEnhancementFactor);
                yield return new TestCaseData(new GaussShape {GaussianSpread = factor}, KnownWaveBoundariesFileConstants.GaussShape, KnownWaveProperties.GaussianSpreading);
                yield return new TestCaseData(new PiersonMoskowitzShape(), KnownWaveBoundariesFileConstants.PiersonMoskowitzShape, null);
            }
        }

        [Test]
        public void AddNewProperties_ForUniformConstantBoundary_ShouldReturn8Properties()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new UniformDataComponent<ConstantParameters<TSpreading>>(
                new ConstantParameters<TSpreading>(height1, period1, direction1, new TSpreading()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(8, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[4].Key);
            Assert.AreEqual(GetStringValue(height1), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[5].Key);
            Assert.AreEqual(GetStringValue(period1), properties[5].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[6].Key);
            Assert.AreEqual(GetStringValue(direction1), properties[6].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[7].Key);
            Assert.AreEqual(GetStringValue(SpreadingValue), properties[7].Value);
        }

        [Test]
        public void AddNewProperties_ForUniformTimeSeriesBoundary_ShouldReturn4Properties()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new UniformDataComponent<TimeDependentParameters<TSpreading>>(
                new TimeDependentParameters<TSpreading>(
                    Substitute.For<IWaveEnergyFunction<TSpreading>>()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(4, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingConstantBoundary_ShouldReturn14Properties()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(distance1, geometryDefinition);
            var constantParameters1 = new ConstantParameters<TSpreading>(height1, period1, direction1, new TSpreading());

            var supportPoint2 = new SupportPoint(distance2, geometryDefinition);
            var constantParameters2 = new ConstantParameters<TSpreading>(height2, period2, direction2, new TSpreading());

            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            dataComponent.AddParameters(supportPoint1, constantParameters1);
            dataComponent.AddParameters(supportPoint2, constantParameters2);
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(14, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[4].Key);
            Assert.AreEqual(GetDistanceStringValue(distance1), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[5].Key);
            Assert.AreEqual(GetStringValue(height1), properties[5].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[6].Key);
            Assert.AreEqual(GetStringValue(period1), properties[6].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[7].Key);
            Assert.AreEqual(GetStringValue(direction1), properties[7].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[8].Key);
            Assert.AreEqual(GetStringValue(SpreadingValue), properties[8].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[9].Key);
            Assert.AreEqual(GetDistanceStringValue(distance2), properties[9].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[10].Key);
            Assert.AreEqual(GetStringValue(height2), properties[10].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[11].Key);
            Assert.AreEqual(GetStringValue(period2), properties[11].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[12].Key);
            Assert.AreEqual(GetStringValue(direction2), properties[12].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[13].Key);
            Assert.AreEqual(GetStringValue(SpreadingValue), properties[13].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingTimeSeriesBoundary_ShouldReturn6Properties()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(distance1, geometryDefinition);
            var timeDependentParameters1 = new TimeDependentParameters<TSpreading>(
                Substitute.For<IWaveEnergyFunction<TSpreading>>());

            var supportPoint2 = new SupportPoint(distance2, geometryDefinition);
            var timeDependentParameters2 = new TimeDependentParameters<TSpreading>(
                Substitute.For<IWaveEnergyFunction<TSpreading>>());

            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            dataComponent.AddParameters(supportPoint1, timeDependentParameters1);
            dataComponent.AddParameters(supportPoint2, timeDependentParameters2);
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(6, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[4].Key);
            Assert.AreEqual(GetDistanceStringValue(distance1), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[5].Key);
            Assert.AreEqual(GetDistanceStringValue(distance2), properties[5].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingConstantBoundaryWithoutActiveSupportPoints_ShouldOnlySaveBoundaryWideData()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(4, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingTimeSeriesBoundaryWithoutActiveSupportPoints_ShouldOnlySaveBoundaryWideData()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(4, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.JonswapShape, properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(KnownWaveBoundariesFileConstants.PeakPeriodType, properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Key);
            Assert.AreEqual(SpreadingType, properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Key);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingConstantUnknownSpreadingBoundaryWithoutActiveSupportPoints_ThrowsNotSupportedException()
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<DummyConditionSpreading>>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);

            // Act
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<NotSupportedException>(Call);
            Assert.That(exception.Message, Is.EqualTo("The type of the specified dataComponent does not correspond with a supported type"));
        }

        [Test]
        [TestCaseSource(nameof(DifferentShapes))]
        public void AddNewProperties_ForDifferentShapes_ReturnsCorrectPropertiesForSpecificShape(IBoundaryConditionShape shape, string expectedShapeTypeValue, string expectedExtraProperty)
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new UniformDataComponent<ConstantParameters<TSpreading>>(
                new ConstantParameters<TSpreading>(height1, period1, direction1, new TSpreading()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();
            bool onePropertyLess = string.IsNullOrEmpty(expectedExtraProperty);

            Assert.AreEqual(onePropertyLess ? 7 : 8, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Key);
            Assert.AreEqual(expectedShapeTypeValue, properties[0].Value);
            if (!onePropertyLess)
            {
                Assert.AreEqual(expectedExtraProperty, properties[3].Key);
                Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            }
        }

        [Test]
        [TestCase(BoundaryConditionPeriodType.Peak, KnownWaveBoundariesFileConstants.PeakPeriodType)]
        [TestCase(BoundaryConditionPeriodType.Mean, KnownWaveBoundariesFileConstants.MeanPeriodType)]
        public void AddNewProperties_ForDifferentPeriodTypes_ReturnsCorrectPropertiesForSpecificPeriodType(BoundaryConditionPeriodType period, string expectedPeriodTypeValue)
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var dataComponent = new UniformDataComponent<ConstantParameters<TSpreading>>(
                new ConstantParameters<TSpreading>(height1, period1, direction1, new TSpreading()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, period, dataComponent);

            // Act
            MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            List<IniProperty> properties = section.Properties.ToList();

            Assert.AreEqual(8, properties.Count);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Key);
            Assert.AreEqual(expectedPeriodTypeValue, properties[1].Value);
        }

        private class DummyConditionSpreading : IBoundaryConditionSpreading
        {
            public void AcceptVisitor(ISpreadingVisitor visitor) {}
        }

        private static string GetStringValue(double value) => value.ToString("e7", CultureInfo.InvariantCulture);

        private static string GetDistanceStringValue(double value) => value.ToString("F7", CultureInfo.InvariantCulture);
    }

    [TestFixture]
    public class MdwBoundaryCategoryConditionsExtenderTest
    {
        [Test]
        public void AddNewProperties_SectionNull_ThrowsArgumentNullException()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Act
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(null, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySection"));
        }

        [Test]
        public void AddNewProperties_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            var section = new IniSection(KnownWaveSections.BoundarySection);

            // Act
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("conditionDefinition"));
        }

        [Test]
        public void Visit_WaveBoundaryConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => x.Arg<IBoundaryConditionVisitor>().Visit(null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaryConditionDefinition"));
        }

        [Test]
        public void Visit_GaussShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var shape = Substitute.For<IBoundaryConditionShape>();
            conditionDefinition.Shape = shape;
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => conditionDefinition.Shape.AcceptVisitor(x.Arg<IShapeVisitor>()));
            shape.When(x => x.AcceptVisitor(Arg.Any<IShapeVisitor>()))
                 .Do(x => x.Arg<IShapeVisitor>().Visit((GaussShape) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("gaussShape"));
        }

        [Test]
        public void Visit_JonswapShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var shape = Substitute.For<IBoundaryConditionShape>();
            conditionDefinition.Shape = shape;
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => conditionDefinition.Shape.AcceptVisitor(x.Arg<IShapeVisitor>()));
            shape.When(x => x.AcceptVisitor(Arg.Any<IShapeVisitor>()))
                 .Do(x => x.Arg<IShapeVisitor>().Visit((JonswapShape) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("jonswapShape"));
        }

        [Test]
        public void Visit_PiersonMoskowitzShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var shape = Substitute.For<IBoundaryConditionShape>();
            conditionDefinition.Shape = shape;
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => conditionDefinition.Shape.AcceptVisitor(x.Arg<IShapeVisitor>()));
            shape.When(x => x.AcceptVisitor(Arg.Any<IShapeVisitor>()))
                 .Do(x => x.Arg<IShapeVisitor>().Visit((PiersonMoskowitzShape) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("piersonMoskowitzShape"));
        }

        [Test]
        public void Visit_UniformDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            conditionDefinition.DataComponent = dataComponent;
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => conditionDefinition.DataComponent.AcceptVisitor(x.Arg<ISpatiallyDefinedDataComponentVisitor>()));
            dataComponent.When(x => x.AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((UniformDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("uniformDataComponent"));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            conditionDefinition.DataComponent = dataComponent;
            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => conditionDefinition.DataComponent.AcceptVisitor(x.Arg<ISpatiallyDefinedDataComponentVisitor>()));
            dataComponent.When(x => x.AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("spatiallyVaryingDataComponent"));
        }

        [Test]
        [TestCaseSource(nameof(GetFileBasedDataComponent))]
        public void AddNewProperties_ForSpatiallyVaryingFileBasedBoundary_ThrowsNotSupportException(ISpatiallyDefinedDataComponent dataComponent)
        {
            // Arrange
            var section = new IniSection(KnownWaveSections.BoundarySection);

            var conditionDefinition = new WaveBoundaryConditionDefinition(new PiersonMoskowitzShape(),
                                                                          BoundaryConditionPeriodType.Peak,
                                                                          dataComponent);

            // Act
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void Visit_ConstantParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var data = Substitute.For<IForcingTypeDefinedParameters>();

            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => data.AcceptVisitor(x.Arg<IForcingTypeDefinedParametersVisitor>()));
            data.When(x => x.AcceptVisitor(Arg.Any<IForcingTypeDefinedParametersVisitor>()))
                .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit((ConstantParameters<PowerDefinedSpreading>) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("constantParameters"));
        }

        [Test]
        public void Visit_TimeDependentParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var data = Substitute.For<IForcingTypeDefinedParameters>();

            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => data.AcceptVisitor(x.Arg<IForcingTypeDefinedParametersVisitor>()));
            data.When(x => x.AcceptVisitor(Arg.Any<IForcingTypeDefinedParametersVisitor>()))
                .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit((TimeDependentParameters<PowerDefinedSpreading>) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("timeDependentParameters"));
        }

        [Test]
        public void Visit_DegreesDefinedSpreadingNull_ThrowsArgumentNullException()
        {
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var data = Substitute.For<IBoundaryConditionSpreading>();

            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => data.AcceptVisitor(x.Arg<ISpreadingVisitor>()));
            data.When(x => x.AcceptVisitor(Arg.Any<ISpreadingVisitor>()))
                .Do(x => x.Arg<ISpreadingVisitor>().Visit((DegreesDefinedSpreading) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("degreesDefinedSpreading"));
        }

        [Test]
        public void Visit_PowerDefinedSpreadingNull_ThrowsArgumentNullException()
        {
            var section = new IniSection(KnownWaveSections.BoundarySection);
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.PeriodType = BoundaryConditionPeriodType.Mean;
            var data = Substitute.For<IBoundaryConditionSpreading>();

            conditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                               .Do(x => data.AcceptVisitor(x.Arg<ISpreadingVisitor>()));
            data.When(x => x.AcceptVisitor(Arg.Any<ISpreadingVisitor>()))
                .Do(x => x.Arg<ISpreadingVisitor>().Visit((PowerDefinedSpreading) null));

            // Call
            void Call() => MdwBoundaryCategoryConditionsExtender.AddNewProperties(section, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("powerDefinedSpreading"));
        }

        private static IEnumerable<ISpatiallyDefinedDataComponent> GetFileBasedDataComponent()
        {
            yield return new UniformDataComponent<FileBasedParameters>(new FileBasedParameters(""));

            var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            dataComponent.AddParameters(new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>()),
                                        new FileBasedParameters(""));
            yield return dataComponent;
        }
    }
}