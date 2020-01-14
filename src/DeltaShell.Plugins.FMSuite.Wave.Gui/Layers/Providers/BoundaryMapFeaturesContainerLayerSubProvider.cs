using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainerLayerSubProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="BoundaryMapFeaturesContainer"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class BoundaryMapFeaturesContainerLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="BoundaryMapFeaturesContainerLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public BoundaryMapFeaturesContainerLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is BoundaryMapFeaturesContainer &&
                   parentData is IWaveModel;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is BoundaryMapFeaturesContainer container &&
                   parentData is IWaveModel model 
                       ? factory.CreateBoundaryLayer(container, model) 
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}