using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
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
    public class WavhFileFunctionStoreGroupLayerSubProviderTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var creator = Substitute.For<IWaveLayerInstanceCreator>();

            // Call
            var provider = new WavhFileFunctionStoreGroupLayerSubProvider(creator);

            // Assert
            Assert.That(provider, Is.InstanceOf<ILayerSubProvider>());
        }

        [Test]
        public void Constructor_InstanceCreatorNull_ThrowsArgumentNullException()
        {
            void Call() => new WavhFileFunctionStoreGroupLayerSubProvider(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        public static IEnumerable<TestCaseData> CanCreateLayerForData()
        {
            const string filePath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

            var outputData = Substitute.For<IWaveOutputData>();
            string[] filePaths = { filePath };

            yield return new TestCaseData(Enumerable.Empty<string>(), outputData, false);
            yield return new TestCaseData(filePaths, null, false);
            yield return new TestCaseData(filePaths, outputData, true);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(CanCreateLayerForData))]
        public void CanCreateLayerFor_ExpectedResults(IEnumerable<string> filePaths,
                                                      object parentData,
                                                      bool expectedValue)
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WavhFileFunctionStoreGroupLayerSubProvider(instanceCreator);
            var featureContainer = Substitute.For<IWaveFeatureContainer>();

            using (var tempDir = new TemporaryDirectory())
            {
                List<WavhFileFunctionStore> functionStores =
                    filePaths.Select(tempDir.CopyTestDataFileToTempDirectory)
                             .Select(p => new WavhFileFunctionStore(p, featureContainer))
                             .ToList();

                // Call 
                bool result = provider.CanCreateLayerFor(functionStores, parentData);

                // Assert
                Assert.That(result, Is.EqualTo(expectedValue));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreateLayer_ExpectedResults()
        {
            // Setup
            const string filePath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

            var outputData = Substitute.For<IWaveOutputData>();
            string[] filePaths = { filePath };

            var layer = Substitute.For<ILayer>();
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            instanceCreator.CreateWaveOutputGroupLayer(WaveLayerNames.WavhFunctionGroupLayerName)
                           .Returns(layer);

            var provider = new WavhFileFunctionStoreGroupLayerSubProvider(instanceCreator);
            var featureContainer = Substitute.For<IWaveFeatureContainer>();

            using (var tempDir = new TemporaryDirectory())
            {
                List<WavhFileFunctionStore> functionStores =
                    filePaths.Select(tempDir.CopyTestDataFileToTempDirectory)
                             .Select(p => new WavhFileFunctionStore(p, featureContainer))
                             .ToList();

                // Call 
                ILayer result = provider.CreateLayer(functionStores, outputData);

                // Assert
                Assert.That(result, Is.SameAs(layer));
                instanceCreator.Received(1).CreateWaveOutputGroupLayer(WaveLayerNames.WavhFunctionGroupLayerName);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GenerateChildLayerObjects_ValidData_ReturnsFunctionStores()
        {
            // Setup
            const string filePath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

            string[] filePaths = { filePath };

            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            var provider = new WavhFileFunctionStoreGroupLayerSubProvider(instanceCreator);
            var featureContainer = Substitute.For<IWaveFeatureContainer>();

            using (var tempDir = new TemporaryDirectory())
            {
                List<WavhFileFunctionStore> functionStores =
                    filePaths.Select(tempDir.CopyTestDataFileToTempDirectory)
                             .Select(p => new WavhFileFunctionStore(p, featureContainer))
                             .ToList();

                // Call 
                IEnumerable<object> result = provider.GenerateChildLayerObjects(functionStores);

                // Assert
                Assert.That(result, Is.EquivalentTo(functionStores));
            }
        }

        [Test]
        public void GenerateChildLayerObjects_InvalidData_ReturnsEmptyCollection()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var provider = new WavhFileFunctionStoreGroupLayerSubProvider(instanceCreator);

            // Call
            IEnumerable<object> result = provider.GenerateChildLayerObjects(new object());

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}