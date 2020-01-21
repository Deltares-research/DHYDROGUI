using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Gui;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="Feature2DLayerSubProvider"/> partially implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="IEventedList{Feature2D}"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public abstract class Feature2DLayerSubProvider : ILayerSubProvider
    {
        /// <summary>
        /// Gets the factory.
        /// </summary>
        protected IWaveLayerFactory Factory { get; }

        /// <summary>
        /// Creates a new <see cref="Feature2DLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        protected Feature2DLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            Factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEnumerable<Feature2D> features &&
                   parentData is IWaveModel model &&
                   IsCorrectFeatureSet(features, model);
        }

        /// <summary>
        /// Determines whether the provided features match the condition to
        /// create a <see cref="ILayer"/> of.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// <c>true</c> if a layer can be created by this <see cref="Feature2DLayerSubProvider"/>;
        /// <c>false</c> otherwise.
        /// </returns>
        protected abstract bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model);

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return CanCreateLayerFor(sourceData, parentData) 
                       ? CreateFeatureLayer((IWaveModel) parentData)
                       : null;
        }

        /// <summary>
        /// Creates the actual <see cref="ILayer"/>.
        /// </summary>
        /// <param name="model"> The model with which the layer should be created.</param>
        /// <returns> The created <see cref="ILayer"/>. </returns>
        protected abstract ILayer CreateFeatureLayer(IWaveModel model);

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}