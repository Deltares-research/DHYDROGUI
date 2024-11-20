using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of <see cref="GroupableFeature2DPolygon"/>.
    /// </summary>
    internal abstract class GroupableFeature2DPolygonsLayerProvider : FeaturesLayerProvider<GroupableFeature2DPolygon>
    {
        /// <inheritdoc/>
        public override bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IEventedList<GroupableFeature2DPolygon> features &&
            parentData is HydroArea hydroArea &&
            Equals(features, GetLayerFeatures(hydroArea));

        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.DataSource.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
            {
                if (!(geometry is IPolygon))
                {
                    if (geometry.Coordinates.Length < 4)
                    {
                        return null;
                    }

                    geometry = new Polygon(new LinearRing(geometry.Coordinates));
                }

                var newFeature = new GroupableFeature2DPolygon { Geometry = geometry };
                provider.Features.Add(newFeature);

                return newFeature;
            };

            return layer;
        }
    }
}