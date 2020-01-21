using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui;
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
        private static IWaveModel GetConfiguredModel()
        {
            var model = Substitute.For<IWaveModel>();
            var feature = Substitute.For<IEventedList<WaveObstacle>>();

            model.Obstacles.Returns(feature);

            return model;
        }

        protected override Func<IWaveLayerFactory, ILayerSubProvider> ConstructorCall { get; } = 
            (factory) => new ObstacleLayerSubProvider(factory);

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateObstacleLayer(Model);

        protected override IWaveModel Model { get; } = GetConfiguredModel();
        protected override IEnumerable<Feature2D> RelevantFeature => Model.Obstacles;
    }
}