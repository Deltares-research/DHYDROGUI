using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    public class StructureSnapRule : SnapRule
    {
        public IList<VectorLayer> StructureLayers { get; set; }

        public override SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets,
                                           Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            var structures = new List<IStructure1D>();
            var structureRenderedGeometries = new List<IGeometry>();
            var structureLayers = new List<ILayer>();
            var distances = new List<double>();

            if (StructureLayers == null)
            {
                var layers = NewFeatureLayer.Map != null
                                 ? NewFeatureLayer.Map.GetAllLayers(true).OfType<HydroRegionMapLayer>().Where(l => l.Region is IHydroNetwork)
                                 : new List<HydroRegionMapLayer>();


                StructureLayers = layers.SelectMany(l => l.Layers)
                                        .OfType<VectorLayer>()
                                        .Where(l => l.DataSource != null &&
                                                    l.DataSource.FeatureType.Implements(typeof (IStructure1D)) &&
                                                    l.CustomRenderers.Any(r => r is StructureRenderer))
                                        .ToList();
            }

            // since we use rendered geometry - skip candidates and use rendered geometry
            foreach (var layer in StructureLayers)
            {
                IList dataSourceFeatures = layer?.DataSource?.Features;
                if (dataSourceFeatures == null) 
                    continue;

                foreach (IStructure1D structure in dataSourceFeatures)
                {
                    if (!(layer.CustomRenderers.FirstOrDefault() is StructureRenderer renderer))
                        continue;

                    var renderedGeometry = renderer.GetRenderedFeatureGeometry(structure, layer);

                    if (!envelope.Intersects(renderedGeometry.EnvelopeInternal) || Equals(sourceFeature, structure))
                    {
                        continue;
                    }

                    structures.Add(structure);
                    structureRenderedGeometries.Add(renderedGeometry);
                    structureLayers.Add(layer);
                    distances.Add(envelope.Centre.Distance(renderedGeometry.EnvelopeInternal.Centre));
                }
            }

            // find index of snapped structure with a minimal distance
            var minDistance = double.MaxValue;
            var minDistanceIndex = -1;

            for (var i = 0; i < structures.Count; i++)
            {
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    minDistanceIndex = i;
                }
            }

            if (minDistanceIndex != -1)
            {
                var structure = structures[minDistanceIndex];
                var layer = structureLayers[minDistanceIndex];
                var geometry = structureRenderedGeometries[minDistanceIndex];
                var index = structure.ParentStructure.Structures.IndexOf(structure);

                // In the event that the model's network has a different coordinate system to the map,
                // transform the geometry of the retrieved structure from model coordinate system to map coordinate system before snapping
                // (the snapped geometry will then get tranformed back into model coordinate system when adding the feature to the network)
                IGeometry transformedGeometry = null;
                if (layer.CoordinateTransformation != null)
                {
                    transformedGeometry = GeometryTransform.TransformGeometry(structure.Geometry, layer.CoordinateTransformation.MathTransform);
                }
                var geometryToSnap = transformedGeometry != null ? transformedGeometry.EnvelopeInternal.Centre : structure.Geometry.EnvelopeInternal.Centre;

                return new SnapResult(geometryToSnap, structure, layer, null, index, index)
                {
                    VisibleSnaps = geometry.Coordinates.Select(c => new Point(c)).Cast<IGeometry>().ToList(),
                    Rule = this
                };
            }

            return null;
        }
    }
}