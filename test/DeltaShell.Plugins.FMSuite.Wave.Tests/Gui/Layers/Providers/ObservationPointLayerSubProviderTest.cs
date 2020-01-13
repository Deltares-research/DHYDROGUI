using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class ObservationPointLayerSubProviderTest : Feature2DLayerSubProviderTestFixture
    {
        private static IWaveModel GetConfiguredModel()
        {
            var model = Substitute.For<IWaveModel>();
            var feature = Substitute.For<IEventedList<Feature2DPoint>>();

            model.ObservationPoints.Returns(feature);

            return model;
        }

        protected override Func<IWaveLayerFactory, IWaveLayerSubProvider> ConstructorCall { get; } = 
            (factory) => new ObservationPointLayerSubProvider(factory);

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateObservationPointsLayer(Model);

        protected override IWaveModel Model { get; } = GetConfiguredModel();
        protected override IEnumerable<Feature2D> RelevantFeature => Model.ObservationPoints;
    }
}