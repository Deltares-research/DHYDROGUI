using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ObstacleLayerSubProvider"/> implements
    /// <see cref="Feature2DLayerSubProvider"/> for <see cref="WaveModel.Obstacles"/>.
    /// </summary>
    /// <seealso cref="Feature2DLayerSubProvider"/>
    public class ObstacleLayerSubProvider : Feature2DLayerSubProvider
    {
        /// <summary>
        /// Creates a new <see cref="ObstacleLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator"> The factory to create the layers with. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public ObstacleLayerSubProvider(IWaveLayerInstanceCreator instanceCreator) : base(instanceCreator) {}

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveFeatureContainer featureContainer) =>
            Equals(features, featureContainer.Obstacles);

        protected override ILayer CreateFeatureLayer(IWaveModel model) =>
            InstanceCreator.CreateObstacleLayer(model);
    }
}