using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ObstacleDataLayerSubProvider"/> implements
    /// <see cref="Feature2DLayerSubProvider"/> for <see cref="WaveModel.Obstacles"/>.
    /// </summary>
    /// <seealso cref="Feature2DLayerSubProvider" />
    public class ObstacleLayerSubProvider : Feature2DLayerSubProvider
    {
        // TODO: verify behaviour of ObstacleDataLayer vs ObstacleLayer        
        /// <summary>
        /// Creates a new <see cref="ObstacleLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory"> The factory to create the layers with. </param>
        public ObstacleLayerSubProvider(IWaveLayerFactory factory) : base(factory) {}

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model) =>
            Equals(features, model.Obstacles);

        protected override ILayer CreateFeatureLayer(IWaveModel model) => 
            Factory.CreateObstacleLayer(model);
    }
}