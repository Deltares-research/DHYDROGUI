using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class ObstacleLayerSubProviderTest : Feature2DLayerSubProviderTestFixture
    {
        protected override Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; } =
            (factory) => new ObstacleLayerSubProvider(factory);

        protected override IWaveModel Model { get; } = GetConfiguredModel();
        protected override IEnumerable<Feature2D> RelevantFeature => Model.FeatureContainer.Obstacles;

        private static IWaveModel GetConfiguredModel()
        {
            var model = Substitute.For<IWaveModel>();
            var featureContainerMock = Substitute.For<IWaveFeatureContainer>();
            var feature = Substitute.For<IEventedList<WaveObstacle>>();
            
            featureContainerMock.Obstacles.Returns(feature);
            model.FeatureContainer.Returns(featureContainerMock);

            return model;
        }

        protected override ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock) =>
            instanceCreatorMock.CreateObstacleLayer(Model);
    }
}