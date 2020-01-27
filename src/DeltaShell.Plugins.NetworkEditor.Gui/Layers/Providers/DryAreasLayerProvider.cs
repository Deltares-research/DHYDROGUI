using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for collections
    /// of dry areas.
    /// </summary>
    public class DryAreasLayerProvider : GroupableFeaturesLayerProvider<GroupableFeature2DPolygon>
    {
        /// <inheritdoc/>
        protected override ILayer CreateLayer(HydroArea hydroArea)
        {
            ILayer layer = base.CreateLayer(hydroArea);
            layer.DataSource.AddNewFeatureFromGeometryDelegate = (provider, geometry) =>
            {
                if (!(geometry is IPolygon))
                {
                    if (geometry.Coordinates.Length < 4) return null;
                    geometry = new Polygon(new LinearRing(geometry.Coordinates));
                }
                var newFeature = new GroupableFeature2DPolygon { Geometry = geometry };
                provider.Features.Add(newFeature);

                return newFeature;
            };

            return layer;
        }

        /// <inheritdoc/>
        protected override string GetLayerName()
        {
            return HydroArea.DryAreasPluralName;
        }

        /// <inheritdoc/>
        protected override VectorStyle GetVectorStyle()
        {
            return HydroAreaLayerStyles.DryAreaStyle;
        }

        /// <inheritdoc/>
        protected override string GetFeatureTypeName()
        {
            return "DryArea";
        }

        /// <inheritdoc/>
        protected override IEventedList<GroupableFeature2DPolygon> GetLayerFeatures(HydroArea hydroArea)
        {
            return hydroArea.DryAreas;
        }
    }
}