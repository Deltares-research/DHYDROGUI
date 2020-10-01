using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainerLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="BoundaryMapFeaturesContainer"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public class BoundaryMapFeaturesContainerLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="BoundaryMapFeaturesContainerLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public BoundaryMapFeaturesContainerLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IBoundaryMapFeaturesContainer &&
            parentData is IWaveModel;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IBoundaryMapFeaturesContainer container &&
            parentData is IWaveModel
                ? factory.CreateBoundaryLayer(container)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}