using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers
{
    public class HydroLinkFeatureEditor : FeatureEditor
    {
        public IHydroRegion Region { get; set; }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            return new HydroLinkInteractor(layer, feature, ((VectorLayer)layer).Style, Region);
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            var linkMapGeometry = layer.CoordinateTransformation != null
                                        ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                                        : geometry;

            // snap to source / target features
            var source = GetHydroFeature(linkMapGeometry.Coordinates[0], layer);
            var target = GetHydroFeature(linkMapGeometry.Coordinates.Last(), layer, h => source.Item1.CanLinkTo(h));

            Region.BeginEdit(string.Format("Adding link from {0} to {1}", source.Item1.Name, target.Item1.Name));

            var link = source.Item1.LinkTo(target.Item1);

            link.Name = HydroNetworkHelper.GetUniqueFeatureName(Region, link);
            link.Geometry = geometry;

            Region.EndEdit();

            return link;
        }

        private static Tuple<IHydroObject, ILayer> GetHydroFeature(Coordinate coordinate, ILayer layer, Func<IHydroObject, bool> featureCompatible = null)
        {
            var envelope = MapHelper.GetEnvelope(coordinate, (float)MapHelper.ImageToWorld(layer.Map, 1));

            var compatibleLayers = layer.Map.GetAllVisibleLayers(false)
                .Where(l => l.DataSource != null &&
                            l.DataSource.FeatureType != null &&
                            l.DataSource.FeatureType.Implements(typeof (IHydroObject)))
                .ToList();

            foreach (var compatibleLayer in compatibleLayers)
            {
                var feature = compatibleLayer.GetFeatures(envelope).OfType<IHydroObject>().FirstOrDefault();
                if(feature == null || featureCompatible != null && !featureCompatible(feature)) continue;

                return new Tuple<IHydroObject, ILayer>(feature, layer);
            }

            return null;
        }
    }
}