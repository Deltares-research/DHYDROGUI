using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    [TestFixture]
    [TestFixture(typeof(ConstantParameters<PowerDefinedSpreading>))]
    [TestFixture(typeof(ConstantParameters<DegreesDefinedSpreading>))]
    [TestFixture(typeof(TimeDependentParameters<PowerDefinedSpreading>))]
    [TestFixture(typeof(TimeDependentParameters<DegreesDefinedSpreading>))]
    [TestFixture(typeof(FileBasedParameters))]
    public class SpatiallyDefinedDataComponentFactoryConstructTest<TParameters>
        where TParameters : class, IForcingTypeDefinedParameters
    {
        [Test]
        public void ConstructDefaultDataComponent_UniformDataComponentWithConstantParameters_ExpectedResult()
        {
            // Setup
            var parameterFactory = new ForcingTypeDefinedParametersFactory();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            var dataComponent =
                componentFactory.ConstructDefaultDataComponent<UniformDataComponent<TParameters>>();

            // Assert
            Assert.That(dataComponent, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Not.Null);
        }

        [Test]
        public void ConstructDefaultDataComponent_SpatiallyVaryingDataComponentWithConstantParameters_ExpectedResult()
        {
            // Setup
            var parameterFactory = new ForcingTypeDefinedParametersFactory();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            var dataComponent =
                componentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TParameters>>();

            // Assert
            Assert.That(dataComponent, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Not.Null);
        }
    }
}