using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class ObstacleDataLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private readonly IEventedList<WaveObstacle> obstacles =
            Substitute.For<IEventedList<WaveObstacle>>();

        private static IWaveModel GetModelWithCoordinateSystem()
        {
            var model = Substitute.For<IWaveModel>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            model.CoordinateSystem = coordinateSystem;

            return model;
        }

        private readonly IWaveModel model = GetModelWithCoordinateSystem();

        protected override Func<IWaveLayerFactory, IWaveLayerSubProvider> ConstructorCall { get; } =
            (factory) => new ObstacleDataLayerSubProvider(factory);

        protected override object GetValidSourceData() => obstacles;

        protected override object GetValidParentData() => model;

        protected override object GetInvalidSourceData() => new object();

        protected override object GetInvalidParentData() => null;

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateObstacleDataLayer(obstacles, model.CoordinateSystem);

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            IWaveLayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(obstacles);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}