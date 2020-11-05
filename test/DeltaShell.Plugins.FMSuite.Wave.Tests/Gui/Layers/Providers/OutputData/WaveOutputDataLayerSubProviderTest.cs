using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.NGHS.IO.TestUtils;
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
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            // Call 
            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

            // Assert
            Assert.That(provider, Is.InstanceOf<ILayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new WaveOutputDataLayerSubProvider(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        public static IEnumerable<TestCaseData> CanCreateLayerForData()
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
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

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

            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var layer = Substitute.For<ILayer>();

            instanceCreator.CreateWaveOutputDataLayer(outputData).Returns(layer);

            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

            // Call
            ILayer result = provider.CreateLayer(outputData, parentData);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            instanceCreator.Received(1).CreateWaveOutputDataLayer(outputData);
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
            var instanceCreator= Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

            // Call
            ILayer result = provider.CreateLayer(sourceData, parentData);

            // Assert
            Assert.That(result, Is.Null);
            instanceCreator.DidNotReceiveWithAnyArgs().CreateWaveOutputDataLayer(null);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GenerateChildLayerObjects_ReturnsWaveOutputChildObjects()
        {
            // Setup
            const string mapFilePath = "./WaveOutputDataHarvesterTest/wavm-Waves.nc";
            const string hisFilePath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

            using (var tempDir = new TemporaryDirectory())
            {
                string mapNcPath = tempDir.CopyTestDataFileToTempDirectory(mapFilePath);
                WavmFileFunctionStore[] mapStores = { new WavmFileFunctionStore(mapNcPath) };

                string hisNcPath = tempDir.CopyTestDataFileToTempDirectory(hisFilePath);
                WavhFileFunctionStore[] hisStores = { new WavhFileFunctionStore(hisNcPath) };


                var outputData = Substitute.For<IWaveOutputData>();
                outputData.WavmFileFunctionStores.Returns(mapStores);
                outputData.WavhFileFunctionStores.Returns(hisStores);

                // Call
                IList<object> result = provider.GenerateChildLayerObjects(outputData).ToList();

                // Assert
                Assert.That(result, Has.Member(mapStores));
                Assert.That(result, Has.Member(hisStores));
            }
        }

        [Test]
        public void GenerateChildLayerObjects_InvalidData_ReturnsEmptyCollection()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WaveOutputDataLayerSubProvider(instanceCreator);

            // Call
            IEnumerable<object> result = provider.GenerateChildLayerObjects(new object());

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}