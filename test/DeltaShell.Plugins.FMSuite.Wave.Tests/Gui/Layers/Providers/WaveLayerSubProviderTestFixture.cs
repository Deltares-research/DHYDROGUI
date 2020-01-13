using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public abstract class WaveLayerSubProviderTestFixture
    {
        protected IWaveLayerSubProvider ConstructSubProvider()
        {
            return ConstructSubProvider(FactoryMock);
        }

        private IWaveLayerSubProvider ConstructSubProvider(IWaveLayerFactory factory)
        {
            return ConstructorCall.Invoke(factory);
        }

        protected abstract Func<IWaveLayerFactory, IWaveLayerSubProvider> ConstructorCall { get; }

        protected IWaveLayerFactory FactoryMock;
        protected ILayer LayerMock;

        [SetUp]
        public void SetUp()
        {
            FactoryMock = Substitute.For<IWaveLayerFactory>();
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
            Assert.That(exception.ParamName, Is.EqualTo("factory"));
        }

        protected abstract object GetValidSourceData();
        protected abstract object GetValidParentData();

        protected abstract object GetInvalidSourceData();
        protected abstract object GetInvalidParentData();

        [Test]
        public void CanCreateLayerFor_InvalidData_ReturnsFalse()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();

            // Call
            bool result = subProvider.CanCreateLayerFor(GetInvalidSourceData(), GetInvalidParentData());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanCreateLayerFor_ValidData_ReturnsTrue()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();

            // Call
            bool result = subProvider.CanCreateLayerFor(GetValidSourceData(), GetValidParentData());

            // Assert
            Assert.That(result, Is.True);
        }

        protected abstract ILayer ExpectedCall(IWaveLayerFactory FactoryMock);

        [Test]
        public void CreateLayer_ValidData_ReturnsExpectedResults()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();
            ExpectedCall(FactoryMock).Returns(LayerMock);

            // Call
            ILayer result = subProvider.CreateLayer(GetValidSourceData(), GetValidParentData());

            // Assert
            Assert.That(result, Is.SameAs(LayerMock));
            ExpectedCall(FactoryMock.Received(1));
        }

        [Test]
        public void CreateLayer_InvalidData_ReturnsNull()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();

            // Call
            ILayer result = subProvider.CreateLayer(GetInvalidSourceData(), GetInvalidParentData());

            // Assert
            Assert.That(result, Is.Null);
            ExpectedCall(FactoryMock.DidNotReceiveWithAnyArgs());
        }
    }
}