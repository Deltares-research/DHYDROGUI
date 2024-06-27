using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="Feature2DLayerSubProvider"/> partially implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="DelftTools.Utils.Collections.Generic.IEventedList{T}"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public abstract class Feature2DLayerSubProvider : ILayerSubProvider
    {
        /// <summary>
        /// Creates a new <see cref="Feature2DLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        protected Feature2DLayerSubProvider(IWaveLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            InstanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEnumerable<Feature2D> features &&
                   parentData is IWaveModel model &&
                   IsCorrectFeatureSet(features, model.FeatureContainer);
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return CanCreateLayerFor(sourceData, parentData)
                       ? CreateFeatureLayer((IWaveModel) parentData)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        protected IWaveLayerInstanceCreator InstanceCreator { get; }

        /// <summary>
        /// Determines whether the provided features match the condition to
        /// create a <see cref="ILayer"/> of.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="featureContainer">The feature container of the model.</param>
        /// <returns>
        /// <c>true</c> if a layer can be created by this <see cref="Feature2DLayerSubProvider"/>;
        /// <c>false</c> otherwise.
        /// </returns>
        protected abstract bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveFeatureContainer featureContainer);

        /// <summary>
        /// Creates the actual <see cref="ILayer"/>.
        /// </summary>
        /// <param name="model"> The model with which the layer should be created.</param>
        /// <returns> The created <see cref="ILayer"/>. </returns>
        protected abstract ILayer CreateFeatureLayer(IWaveModel model);
    }
}