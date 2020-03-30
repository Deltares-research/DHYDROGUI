using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class MdwBoundaryConditionPropertiesCreatorTest
    {
        [Test]
        public void AddNewProperties_ForUniformConstantBoundary()
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            const double factor = 3.3;
            var shape = new JonswapShape {PeakEnhancementFactor = factor};
            const BoundaryConditionPeriodType periodType =
                BoundaryConditionPeriodType.Peak;

            const double height = 1.0;
            const double period = 2.0;
            const double direction = 3.0;
            var dataComponent = new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(
                new ConstantParameters<PowerDefinedSpreading>(height, period, direction, new PowerDefinedSpreading()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);
            
            var boundary = new WaveBoundary("boundary1", geometryDefinition, conditionDefinition);

            MdwBoundaryConditionPropertiesCreator.AddNewProperties(category, boundary);

            List<DelftIniProperty> properties = category.Properties.ToList();

            Assert.AreEqual(8, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Name);
            Assert.AreEqual("Jonswap", properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Name);
            Assert.AreEqual(BoundaryConditionPeriodType.Peak.GetDescription(), properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Name);
            Assert.AreEqual("Power", properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Name);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[4].Name);
            Assert.AreEqual(GetStringValue(height), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[5].Name);
            Assert.AreEqual(GetStringValue(period), properties[5].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[6].Name);
            Assert.AreEqual(GetStringValue(direction), properties[6].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[7].Name);
            Assert.AreEqual(GetStringValue(WaveSpreadingConstants.PowerDefaultSpreading), properties[7].Value);

        }

        [Test]
        public void AddNewProperties_ForUniformTimeSeriesBoundary()
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            const double factor = 3.3;
            var shape = new JonswapShape { PeakEnhancementFactor = factor };
            const BoundaryConditionPeriodType periodType =
                BoundaryConditionPeriodType.Peak;
            
            var dataComponent = new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                new TimeDependentParameters<PowerDefinedSpreading>(
                    Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);
            
            var boundary = new WaveBoundary("boundary1", geometryDefinition, conditionDefinition);

            MdwBoundaryConditionPropertiesCreator.AddNewProperties(category, boundary);

            List<DelftIniProperty> properties = category.Properties.ToList();

            Assert.AreEqual(4, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Name);
            Assert.AreEqual("Jonswap", properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Name);
            Assert.AreEqual(BoundaryConditionPeriodType.Peak.GetDescription(), properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Name);
            Assert.AreEqual("Power", properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Name);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingConstantBoundary()
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            const double factor = 3.3;
            var shape = new JonswapShape { PeakEnhancementFactor = factor };
            const BoundaryConditionPeriodType periodType =
                BoundaryConditionPeriodType.Peak;
            
            const double height1 = 1.0;
            const double period1 = 2.0;
            const double direction1 = 3.0;
            const double distance1 = 0;
            var supportPoint1 = new SupportPoint(distance1, geometryDefinition);
            var constantParameters1 = new ConstantParameters<PowerDefinedSpreading>(height1, period1, direction1, new PowerDefinedSpreading());

            const double height2 = 4.0;
            const double period2 = 5.0;
            const double direction2 = 6.0;
            const double distance2 = 0;
            var supportPoint2 = new SupportPoint(distance2, geometryDefinition);
            var constantParameters2 = new ConstantParameters<PowerDefinedSpreading>(height2, period2, direction2, new PowerDefinedSpreading());

            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            dataComponent.AddParameters(supportPoint1, constantParameters1);
            dataComponent.AddParameters(supportPoint2, constantParameters2);
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);
            
            var boundary = new WaveBoundary("boundary1", geometryDefinition, conditionDefinition);

            MdwBoundaryConditionPropertiesCreator.AddNewProperties(category, boundary);

            List<DelftIniProperty> properties = category.Properties.ToList();

            Assert.AreEqual(14, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Name);
            Assert.AreEqual("Jonswap", properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Name);
            Assert.AreEqual(BoundaryConditionPeriodType.Peak.GetDescription(), properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Name);
            Assert.AreEqual("Power", properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Name);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[4].Name);
            Assert.AreEqual(GetStringValue(distance1), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[5].Name);
            Assert.AreEqual(GetStringValue(height1), properties[5].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[6].Name);
            Assert.AreEqual(GetStringValue(period1), properties[6].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[7].Name);
            Assert.AreEqual(GetStringValue(direction1), properties[7].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[8].Name);
            Assert.AreEqual(GetStringValue(WaveSpreadingConstants.PowerDefaultSpreading), properties[8].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[9].Name);
            Assert.AreEqual(GetStringValue(distance2), properties[9].Value);
            Assert.AreEqual(KnownWaveProperties.WaveHeight, properties[10].Name);
            Assert.AreEqual(GetStringValue(height2), properties[10].Value);
            Assert.AreEqual(KnownWaveProperties.Period, properties[11].Name);
            Assert.AreEqual(GetStringValue(period2), properties[11].Value);
            Assert.AreEqual(KnownWaveProperties.Direction, properties[12].Name);
            Assert.AreEqual(GetStringValue(direction2), properties[12].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingValue, properties[13].Name);
            Assert.AreEqual(GetStringValue(WaveSpreadingConstants.PowerDefaultSpreading), properties[13].Value);
        }

        [Test]
        public void AddNewProperties_ForSpatiallyVaryingTimeSeriesBoundary()
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var factor = 3.3;
            var shape = new JonswapShape { PeakEnhancementFactor = factor };
            const BoundaryConditionPeriodType periodType =
                BoundaryConditionPeriodType.Peak;
          
            const double distance1 = 0;
            var supportPoint1 = new SupportPoint(distance1, geometryDefinition);
            var timeDependentParameters1 = new TimeDependentParameters<PowerDefinedSpreading>(
                Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>());

            const double distance2 = 0;
            var supportPoint2 = new SupportPoint(distance2, geometryDefinition);
            var timeDependentParameters2 = new TimeDependentParameters<PowerDefinedSpreading>(
                Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>());

            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();
            dataComponent.AddParameters(supportPoint1, timeDependentParameters1);
            dataComponent.AddParameters(supportPoint2, timeDependentParameters2);
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);

            var boundary = new WaveBoundary("boundary1", geometryDefinition, conditionDefinition);

            MdwBoundaryConditionPropertiesCreator.AddNewProperties(category, boundary);

            List<DelftIniProperty> properties = category.Properties.ToList();

            Assert.AreEqual(6, properties.Count);
            Assert.AreEqual(KnownWaveProperties.ShapeType, properties[0].Name);
            Assert.AreEqual("Jonswap", properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.PeriodType, properties[1].Name);
            Assert.AreEqual(BoundaryConditionPeriodType.Peak.GetDescription(), properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.DirectionalSpreadingType, properties[2].Name);
            Assert.AreEqual("Power", properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.PeakEnhancementFactor, properties[3].Name);
            Assert.AreEqual(GetStringValue(factor), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[4].Name);
            Assert.AreEqual(GetStringValue(distance1), properties[4].Value);
            Assert.AreEqual(KnownWaveProperties.CondSpecAtDist, properties[5].Name);
            Assert.AreEqual(GetStringValue(distance2), properties[5].Value);

        }
        
        private static string GetStringValue(double value)
        {
            return value.ToString("e7", CultureInfo.InvariantCulture);
        }
    }
}