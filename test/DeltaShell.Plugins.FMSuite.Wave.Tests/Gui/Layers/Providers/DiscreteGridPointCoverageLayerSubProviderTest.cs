using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class DiscreteGridPointCoverageLayerSubProviderTest : WaveLayerSubProviderTestFixture
    {
        private static IWaveModel GetModelWithCoordinateSystem()
        {
            var model = Substitute.For<IWaveModel>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            model.CoordinateSystem = coordinateSystem;

            return model;
        }

        private static readonly Func<IEnumerable<WaveModel>> getModelsFunc = Enumerable.Empty<WaveModel>;
        private readonly IDiscreteGridPointCoverage gridCoverage = Substitute.For<IDiscreteGridPointCoverage>();
        private readonly IWaveModel model = GetModelWithCoordinateSystem();

        protected override Func<IWaveLayerFactory, IWaveLayerSubProvider> ConstructorCall { get; } =
            (factory) => new DiscreteGridPointCoverageLayerSubProvider(factory, getModelsFunc);

        protected override object GetValidSourceData() => gridCoverage;

        protected override object GetValidParentData() => model;

        protected override object GetInvalidSourceData() =>
            new object();

        protected override object GetInvalidParentData() =>
            null;

        protected override ILayer ExpectedCall(IWaveLayerFactory FactoryMock) =>
            FactoryMock.CreateGridLayer(gridCoverage, model.CoordinateSystem);
    }
}