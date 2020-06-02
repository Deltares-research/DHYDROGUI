using System;
using System.Collections.Generic;
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
            return new HydroLinkInteractor(layer, feature, ((VectorLayer) layer).Style, Region);
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            IGeometry linkMapGeometry = layer.CoordinateTransformation != null
                                            ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                                            : geometry;

            // snape to source / target features
            Tuple<IHydroObject, ILayer> source = GetHydroFeature(linkMapGeometry.Coordinates[0], layer);
            Tuple<IHydroObject, ILayer> target = GetHydroFeature(linkMapGeometry.Coordinates.Last(), layer);

            Region.BeginEdit(new DefaultEditAction(string.Format("Adding link from {0} to {1}", source.Item1.Name, target.Item1.Name)));

            HydroLink link = source.Item1.LinkTo(target.Item1);

            link.Name = HydroNetworkHelper.GetUniqueFeatureName(Region, link);
            link.Geometry = geometry;

            Region.EndEdit();

            return link;
        }

        private static Tuple<IHydroObject, ILayer> GetHydroFeature(Coordinate coordinate, ILayer layer)
        {
            Envelope envelope = MapHelper.GetEnvelope(coordinate, (float) MapHelper.ImageToWorld(layer.Map, 1));

            List<ILayer> compatibleLayers = layer.Map.GetAllVisibleLayers(false)
                                                 .Where(l => l.DataSource != null &&
                                                             l.DataSource.FeatureType != null &&
                                                             l.DataSource.FeatureType.Implements(typeof(IHydroObject)))
                                                 .ToList();

            foreach (ILayer compatibleLayer in compatibleLayers)
            {
                IHydroObject feature = compatibleLayer.GetFeatures(envelope).OfType<IHydroObject>().FirstOrDefault();
                if (feature == null)
                {
                    continue;
                }

                return new Tuple<IHydroObject, ILayer>(feature, layer);
            }

            return null;
        }
    }
}