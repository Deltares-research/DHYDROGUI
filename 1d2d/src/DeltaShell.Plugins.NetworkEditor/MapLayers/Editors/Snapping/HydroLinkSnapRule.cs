using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    public class HydroLinkSnapRule : ISnapRule
    {
        public int PixelGravity { get; set; }

        public bool Obligatory { get; set; }

        public SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets, Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            foreach (var candidate in candidates)
            {
                var hydroObject = candidate.Item1 as IHydroObject;
                if (hydroObject == null)
                {
                    continue;
                }

                var layer = candidate.Item2;
                
                var geometry = hydroObject is Catchment ? ((Catchment)hydroObject).InteriorPoint : hydroObject.Geometry;
                var geometryToSnap = layer.CoordinateTransformation != null
                                        ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                                        : geometry;

                var snapResult = new SnapResult(geometryToSnap.Coordinates[0], hydroObject, candidate.Item2, geometryToSnap, 0, 0)
                {
                    Rule = this
                };

                // start start
                if (sourceFeature == null)
                {
                    if (hydroObject.CanBeLinkSource)
                    {
                        return snapResult;
                    }
                    continue;
                }
                // Prevent snapping to already linked object.
                var eventedList = ((IHydroObject)sourceFeature).Links;
                if (eventedList.Any(l => Equals(l.Target, hydroObject)))
                {
                    continue;
                }

                // snap end
                var source = (IHydroObject)sourceFeature;
                var target = snapResult.SnappedFeature as IHydroObject;

                if (target != null && source.CanLinkTo(target))
                {
                    return snapResult;
                }
                
            }

            return null;
        }
    }
}