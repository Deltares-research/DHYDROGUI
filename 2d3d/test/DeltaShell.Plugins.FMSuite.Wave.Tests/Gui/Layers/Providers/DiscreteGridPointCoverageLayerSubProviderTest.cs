using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
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
        private static readonly Func<IEnumerable<WaveModel>> getModelsFunc = Enumerable.Empty<WaveModel>;
        private readonly IDiscreteGridPointCoverage gridCoverage = Substitute.For<IDiscreteGridPointCoverage>();
        private readonly IWaveModel model = GetModelWithCoordinateSystem();

        protected override Func<IWaveLayerInstanceCreator, ILayerSubProvider> ConstructorCall { get; } =
            (factory) => new DiscreteGridPointCoverageLayerSubProvider(factory, getModelsFunc);

        [Test]
        public void Constructor_GetWaveModelsFuncNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new DiscreteGridPointCoverageLayerSubProvider(InstanceCreatorMock, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("getWaveModelsFunc"));
        }

        [Test]
        public void GenerateChildLayerObjects_ReturnsEmptyEnumerable()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();

            // Call
            IEnumerable<object> result = subProvider.GenerateChildLayerObjects(gridCoverage);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CreateLayer_ParentWavmFileFunctionStore_NoCoordinateSystem()
        {
            // Setup
            ILayerSubProvider subProvider = ConstructSubProvider();
            var parent = Substitute.For<IWavmFileFunctionStore>();

            InstanceCreatorMock.CreateGridLayer(gridCoverage, null).Returns(LayerMock);

            // Call
            ILayer layer = subProvider.CreateLayer(gridCoverage, parent);

            // Assert
            Assert.That(layer, Is.SameAs(LayerMock));
            InstanceCreatorMock.Received(1).CreateGridLayer(gridCoverage, null);
        }

        private static IWaveModel GetModelWithCoordinateSystem()
        {
            var model = Substitute.For<IWaveModel>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            model.CoordinateSystem = coordinateSystem;

            return model;
        }

        protected override object GetValidSourceData() => gridCoverage;

        protected override object GetValidParentData() => model;

        protected override object GetInvalidSourceData() =>
            new object();

        protected override object GetInvalidParentData() =>
            null;

        protected override ILayer ExpectedCall(IWaveLayerInstanceCreator instanceCreatorMock) =>
            instanceCreatorMock.CreateGridLayer(gridCoverage, model.CoordinateSystem);
    }
}