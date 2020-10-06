using System.Collections.Generic;
using System.Diagnostics;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers.OutputData
{
    [TestFixture]
    public class WaveOutputDataLayerSubProviderTest
    {
        [Test]
        public void Constructor_ValidFactory_ExpectedResults()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();

            // Call 
            var provider = new WaveOutputDataLayerSubProvider(factory);

            // Assert
            Assert.That(provider, Is.InstanceOf<ILayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new WaveOutputDataLayerSubProvider(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("factory"));
        }

        private static IEnumerable<TestCaseData> CanCreateLayerForData()
        {
            var notOutputData = new object();
            var notConnectedOutputData = Substitute.For<IWaveOutputData>();
            notConnectedOutputData.IsConnected.Returns(false);

            var connectedOutputData = Substitute.For<IWaveOutputData>();
            connectedOutputData.IsConnected.Returns(true);

            var parentData = new object();

            yield return new TestCaseData(notConnectedOutputData, parentData, false);
            yield return new TestCaseData(notConnectedOutputData, null,       false);
            yield return new TestCaseData(connectedOutputData,    parentData, true);
            yield return new TestCaseData(connectedOutputData,    null,       true);
            yield return new TestCaseData(notOutputData,          parentData, false);
            yield return new TestCaseData(notOutputData,          null,       false);
            yield return new TestCaseData(null,                   parentData, false);
            yield return new TestCaseData(null,                   null,       false);
        }

        [Test]
        [TestCaseSource(nameof(CanCreateLayerForData))]
        public void CanCreateLayerFor_ExpectedResults(object sourceData, 
                                                      object parentData, 
                                                      bool expectedValue)
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var provider = new WaveOutputDataLayerSubProvider(factory);

            // Call 
            bool result = provider.CanCreateLayerFor(sourceData, parentData);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        private static IEnumerable<TestCaseData> CreateLayerData_ValidInput()
        {
            yield return new TestCaseData(new object());
            yield return new TestCaseData(null);
        }

        [Test]
        [TestCaseSource(nameof(CreateLayerData_ValidInput))]
        public void CreateLayer_ValidInput_CreatesLayer(object parentData)
        {
            // Setup
            var outputData = Substitute.For<IWaveOutputData>();
            outputData.IsConnected.Returns(true);

            var factory = Substitute.For<IWaveLayerFactory>();
            var layer = Substitute.For<ILayer>();

            factory.CreateWaveOutputDataLayer(outputData).Returns(layer);

            var provider = new WaveOutputDataLayerSubProvider(factory);

            // Call
            ILayer result = provider.CreateLayer(outputData, parentData);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            factory.Received(1).CreateWaveOutputDataLayer(outputData);
        }

        private static IEnumerable<TestCaseData> CreateLayerData_InvalidInput()
        {
            var notOutputData = new object();
            var notConnectedOutputData = Substitute.For<IWaveOutputData>();
            notConnectedOutputData.IsConnected.Returns(false);

            var parentData = new object();

            yield return new TestCaseData(notConnectedOutputData, parentData);
            yield return new TestCaseData(notConnectedOutputData, null);
            yield return new TestCaseData(notOutputData,          parentData);
            yield return new TestCaseData(notOutputData,          null);
            yield return new TestCaseData(null,                   parentData);
            yield return new TestCaseData(null,                   null);
        }

        [Test]
        [TestCaseSource(nameof(CreateLayerData_InvalidInput))]
        public void CreateLayer_InvalidInput_ReturnsNull(object sourceData, object parentData)
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var provider = new WaveOutputDataLayerSubProvider(factory);

            // Call
            ILayer result = provider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.Null);
            factory.DidNotReceiveWithAnyArgs().CreateWaveOutputDataLayer(null);
        }

        [Test]
        public void GenerateChildLayerObjects_ReturnsWaveOutputChildObjects()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var provider = new WaveOutputDataLayerSubProvider(factory);
            var outputData = Substitute.For<IWaveOutputData>();

            // Call | Assert
            // TODO: this needs to be extended
            Assert.That(provider.GenerateChildLayerObjects(outputData), Is.Empty);
        }
    }
}