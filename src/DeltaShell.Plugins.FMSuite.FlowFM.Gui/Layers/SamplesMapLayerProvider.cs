using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// Class for providing map layers for <see cref="Samples"/>
    /// </summary>
    public static class SamplesMapLayerProvider
    {
        /// <summary>
        /// Create a new layer for the given <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">The samples to create a new layer for.</param>
        /// <returns>The created layer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="samples"/> is <c>null</c>.</exception>
        public static ILayer Create(Samples samples)
        {
            Ensure.NotNull(samples, nameof(samples));

            var featureCollection = new PointCloudFeatureProvider() { PointCloud = samples.AsPointCloud() };

            ILayer layer = SharpMapLayerFactory.CreateLayer(featureCollection);
            layer.Name = samples.Name;
            layer.ReadOnly = true;

            return layer;
        }
    }
}