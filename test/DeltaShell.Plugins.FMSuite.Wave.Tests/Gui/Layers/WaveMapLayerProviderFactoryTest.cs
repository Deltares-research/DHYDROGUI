using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveMapLayerProviderFactoryTest
    {
        [Test]
        public void GetSubProviders_ReturnsExpectedResults()
        {
            // Call
            IList<IWaveLayerSubProvider> result = WaveMapLayerProviderFactory.GetSubProviders(Enumerable.Empty<WaveModel>)?.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(12));

            Assert.That(result.Any(x => x is BoundaryMapFeaturesContainerLayerSubProvider), 
                        $"Expected one {nameof(BoundaryMapFeaturesContainerLayerSubProvider)}");
            Assert.That(result.Any(x => x is DiscreteGridPointCoverageLayerSubProvider),
                        $"Expected one {nameof(DiscreteGridPointCoverageLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObservationCrossSectionLayerSubProvider),
                        $"Expected one {nameof(ObservationCrossSectionLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObservationPointLayerSubProvider),
                        $"Expected one {nameof(ObservationPointLayerSubProvider)}");
            Assert.That(result.Any(x => x is ObstacleLayerSubProvider),
                        $"Expected one {nameof(ObstacleLayerSubProvider)}");
            Assert.That(result.Any(x => x is Sp2BoundaryLayerSubProvider),
                        $"Expected one {nameof(Sp2BoundaryLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveBoundaryConditionLayerSubProvider),
                        $"Expected one {nameof(WaveBoundaryConditionLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveBoundaryLayerSubProvider),
                        $"Expected one {nameof(WaveBoundaryLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveDomainDataLayerSubProvider),
                        $"Expected one {nameof(WaveDomainDataLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveModelLayerSubProvider),
                        $"Expected one {nameof(WaveModelLayerSubProvider)}");
            Assert.That(result.Any(x => x is WaveSnappedFeaturesGroupLayerDataLayerSubProvider),
                        $"Expected one {nameof(WaveSnappedFeaturesGroupLayerDataLayerSubProvider)}");
            Assert.That(result.Any(x => x is WavmFileFunctionStoreLayerSubProvider),
                        $"Expected one {nameof(WavmFileFunctionStoreLayerSubProvider)}");

        }

        [Test]
        public void GetSubProviders_GetWaveModelsNull_ThrowsArgumentNullException()
        {
            void Call() => WaveMapLayerProviderFactory.GetSubProviders(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void ConstructMapLayerProvider_ReturnsExpectedResults()
        {
            // Call
            IMapLayerProvider result = WaveMapLayerProviderFactory.ConstructMapLayerProvider(Enumerable.Empty<WaveModel>);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ConstructMapLayerProvider_GetWaveModelsNull_ThrowsArgumentNullException()
        {
            void Call() => WaveMapLayerProviderFactory.ConstructMapLayerProvider(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }
    }
}