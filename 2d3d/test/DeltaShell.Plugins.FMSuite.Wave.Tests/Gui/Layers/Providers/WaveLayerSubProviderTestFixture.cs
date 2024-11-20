using System;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class WaveLayerSubProviderTestFixture
    {
        protected IWaveLayerInstanceCreator InstanceCreatorMock;
        protected ILayer LayerMock;

        protected abstract Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; }

        [SetUp]
        public void SetUp()
        {
            InstanceCreatorMock = Substitute.For<IWaveLayerInstanceCreator>();
            LayerMock = Substitute.For<ILayer>();
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call | Assert
            Assert.DoesNotThrow(() => ConstructSubProvider());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Call || Assert
            var exception = Assert.Throws<ArgumentNullException>(() => ConstructSubProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        [Test]
        public void CanCreateLayerFor_InvalidData_ReturnsFalse()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            bool result = subProvider.CanCreateLayerFor(GetInvalidSourceData(), GetInvalidParentData());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanCreateLayerFor_ValidData_ReturnsTrue()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            bool result = subProvider.CanCreateLayerFor(GetValidSourceData(), GetValidParentData());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateLayer_ValidData_ReturnsExpectedResults()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();
            ExpectedCall(InstanceCreatorMock).Returns(LayerMock);

            // Call
            ILayer result = subProvider.CreateLayer(GetValidSourceData(), GetValidParentData());

            // Assert
            Assert.That(result, Is.SameAs(LayerMock));
            ExpectedCall(InstanceCreatorMock.Received(1));
        }

        [Test]
        public void CreateLayer_InvalidData_ReturnsNull()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            ILayer result = subProvider.CreateLayer(GetInvalidSourceData(), GetInvalidParentData());

            // Assert
            Assert.That(result, Is.Null);
            ExpectedCall(InstanceCreatorMock.DidNotReceiveWithAnyArgs());
        }

        protected ILayerSubProvider ConstructSubProvider()
        {
            return ConstructSubProvider(InstanceCreatorMock);
        }

        private ILayerSubProvider ConstructSubProvider(IWaveLayerInstanceCreator instanceCreator)
        {
            return ConstructorCall.Invoke(instanceCreator);
        }

        protected abstract object GetValidSourceData();
        protected abstract object GetValidParentData();

        protected abstract object GetInvalidSourceData();
        protected abstract object GetInvalidParentData();

        protected abstract ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock);
    }
}