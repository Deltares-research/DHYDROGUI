using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
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
    public class WavhFileFunctionStoreLayerSubProviderTest
    {
        [Test]
        public void Constructor_ReturnsExpectedResult()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            IEnumerable<WaveModel> GetModels()
            {
                return Enumerable.Empty<WaveModel>();
            }

            // Call
            var subProvider = new WavhFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Assert
            Assert.That(subProvider, Is.InstanceOf<ILayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IEnumerable<WaveModel> GetModels()
            {
                return Enumerable.Empty<WaveModel>();
            }

            // Call | Assert
            void Call()
            {
                new WavhFileFunctionStoreLayerSubProvider(null, GetModels);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("instanceCreator"));
        }

        [Test]
        public void Constructor_GetModelsNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            // Call | Assert
            void Call()
            {
                new WavhFileFunctionStoreLayerSubProvider(instanceCreator, null);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void CreateLayer_InvalidSourceAndParentData_ReturnsNull()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            IEnumerable<WaveModel> GetModels()
            {
                return Enumerable.Empty<WaveModel>();
            }

            var subProvider = new WavhFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            ILayer result = subProvider.CreateLayer(new object(), new object());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GenerateChildLayerObjects_InvalidData_ReturnsEmptyEnumerable()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            IEnumerable<WaveModel> GetModels()
            {
                return Enumerable.Empty<WaveModel>();
            }

            var subProvider = new WavhFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(new object());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GenerateChildLayerObjects_HasNoParentModel_ResultIncludesFunctionsAndGrid()
        {
            // Setup
            var store = new TestWavhFileFunctionStore
            {
                Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>(),
                    Substitute.For<IFunction>()
                })
            };

            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            IEnumerable<IWaveModel> GetModels()
            {
                return Enumerable.Empty<IWaveModel>();
            }

            var subProvider = new WavhFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            IList<object> result = subProvider.GenerateChildLayerObjects(store).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));

            foreach (IFunction func in store.Functions)
            {
                Assert.That(result, Contains.Item(func));
            }
        }

        [Test]
        public void GenerateChildLayerObjects_HasParentModel_ResultIncludesFunctions()
        {
            // Setup
            var store = new TestWavhFileFunctionStore
            {
                Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>(),
                    Substitute.For<IFunction>()
                })
            };

            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            var model = Substitute.For<IWaveModel>();
            model.WaveOutputData.WavhFileFunctionStores.Returns(new EventedList<IWavhFileFunctionStore> {store});

            IEnumerable<IWaveModel> GetModels()
            {
                return new[]
                {
                    model
                };
            }

            var subProvider = new WavhFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            IList<object> result = subProvider.GenerateChildLayerObjects(store).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));

            foreach (IFunction func in store.Functions)
            {
                Assert.That(result, Contains.Item(func));
            }
        }

        [Test]
        public void CanCreateLayerFor_InvalidSourceAndParentData_ReturnsFalse()
        {
            // Setup
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            IEnumerable<WaveModel> GetModels()
            {
                return Enumerable.Empty<WaveModel>();
            }

            var subProvider = new WavmFileFunctionStoreLayerSubProvider(instanceCreator, GetModels);

            // Call
            bool result = subProvider.CanCreateLayerFor(new object(), new object());

            // Assert
            Assert.That(result, Is.False);
        }
    }
}