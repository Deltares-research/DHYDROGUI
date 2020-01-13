using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WaveModelLayerSubProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();

            // Call
            var subProvider = new WaveModelLayerSubProvider(factory);

            // Assert
            Assert.That(subProvider, Is.InstanceOf<IWaveLayerSubProvider>());
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new WaveModelLayerSubProvider(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("factory"));
        }

        [Test]
        public void CanCreateLayerFor_ValidModelAndAnyParentData_ReturnsTrue()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var model = new WaveModel();

            // Call
            bool result = subProvider.CanCreateLayerFor(model, null);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanCreateLayerFor_AnySourceDataNotAModel_ReturnsFalse()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var obj = new object();

            // Call
            bool result = subProvider.CanCreateLayerFor(obj, null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CreateLayer_ValidModelAndAnyParentData_ReturnsExpectedLayer()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var model = new WaveModel();
            var layer = Substitute.For<ILayer>();

            factory.CreateModelGroupLayer(model).Returns(layer);

            // Call
            ILayer result = subProvider.CreateLayer(model, null);

            // Assert
            Assert.That(result, Is.SameAs(layer));
            factory.Received(1).CreateModelGroupLayer(model);
        }

        [Test]
        public void CreateLayer_AnyDataNotAModel_ReturnsNull()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var obj = new object();

            // Call
            ILayer result = subProvider.CreateLayer(obj, null);

            // Assert
            Assert.That(result, Is.Null);
            factory.DidNotReceiveWithAnyArgs().CreateModelGroupLayer(null);
        }

        [Test]
        public void GenerateChildLayerObjects_NotModelAsData_ReturnsEmptyEnumerable()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var obj = new object();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(obj);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GenerateChildLayerObjects_ModelAsData_ReturnsExpectedItems()
        {
            // Setup
            var factory = Substitute.For<IWaveLayerFactory>();
            var subProvider = new WaveModelLayerSubProvider(factory);

            var waveModel = new WaveModel {OuterDomain = new WaveDomainData("Domain"),};

            // Call
            IList<object> result = subProvider.GenerateChildLayerObjects(waveModel).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(9));

            Assert.That(result, Has.Member(waveModel.BoundaryConditions));
            Assert.That(result, Has.Member(waveModel.Boundaries));
            Assert.That(result, Has.Member(waveModel.Sp2Boundaries));
            Assert.That(result, Has.Member(waveModel.Obstacles));
            Assert.That(result, Has.Member(waveModel.ObservationPoints));
            Assert.That(result, Has.Member(waveModel.ObservationCrossSections));
            Assert.That(result, Has.Member(waveModel.OuterDomain));

            Assert.That(result.Count(x => x is WaveSnappedFeaturesGroupLayerData), Is.EqualTo(1));
            Assert.That(result.Count(x => x is BoundaryMapFeaturesContainer), Is.EqualTo(1));
        }
    }
}