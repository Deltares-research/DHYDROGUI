using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public abstract class BranchFeatureRenderer: IFeatureRenderer, ICloneable
    {
        protected readonly Dictionary<IFeature, ILineString> updatedBranchFeatures;
        protected Envelope lastEnvelope;
        protected VectorLayer vectorLayer;

        protected BranchFeatureRenderer()
        {
            updatedBranchFeatures = new Dictionary<IFeature, ILineString>();
        }

        /// <summary>
        /// Called for each feature that needs to be rendered. CrossSectionRenderer assumes it is always 
        /// added to a vectorlayer.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="g"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public abstract bool Render(IFeature feature, Graphics g, ILayer layer);

        public virtual IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            return layer.CoordinateTransformation != null
                ? GeometryTransform.TransformGeometry(feature.Geometry, layer.CoordinateTransformation.MathTransform)
                : feature.Geometry;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            return layer.GetFeatures(geometry, false);
        }

        public virtual IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            return layer.GetFeatures(box);
        }


        /// <summary>
        /// Clones the custom renderer. This allows the Network Editor to use custom renderers for
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();
    }
}