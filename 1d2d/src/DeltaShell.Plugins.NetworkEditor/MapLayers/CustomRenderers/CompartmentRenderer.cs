using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class CompartmentRenderer : IFeatureRenderer
    {
        public bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            // do not render features
            return true;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            return null;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            yield break;
        }

        public IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            yield break;
        }
    }
}