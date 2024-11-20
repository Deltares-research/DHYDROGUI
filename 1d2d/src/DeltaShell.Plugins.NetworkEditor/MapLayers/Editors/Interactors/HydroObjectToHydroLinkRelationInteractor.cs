using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Delegates;
using SharpMap.Api.Editors;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors;
using SharpMap.Editors.FallOff;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    /// <summary>
    /// Used to edit links connected to the hydro objects which are being moved.
    /// 
    /// Starts interacton on Activitate() then maintains a copy of all links connected to the hydro object using
    /// UpdateRelatedFeatures() and then updates original feature geometries on StoreRelatedFeatures().
    /// </summary>
    public class HydroObjectToHydroLinkRelationInteractor : FeatureRelationInteractor
    {
        private IFeature lastFeature;
        private Coordinate lastCoordinate;
        
        private IList<HydroLink> links;
        private IList<HydroLink> linksCloned;
        private IList<IGeometry> linkGeometriesCloned;
        private List<List<IFeatureRelationInteractor>> linkRules;
        private List<IHydroRegion> linkRegions;
        private OgrCoordinateSystemFactory coordinateSystemFactory;

        private IFallOffPolicy FallOffPolicy { get; set; }

        public override IFeatureRelationInteractor Activate(IFeature feature, IFeature cloneFeature, AddRelatedFeature addRelatedFeature, int level, IFallOffPolicy fallOffPolicy)
        {
            FallOffPolicy = new LinearFallOffPolicy();
            
            var hydroObject = feature as IHydroObject;
            if (hydroObject == null)
            {
                return null;
            }

            var cloneRule = (HydroObjectToHydroLinkRelationInteractor)CloneRule();
            cloneRule.Start(hydroObject, addRelatedFeature, level);
            
            return cloneRule;
        }

        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(false, linksCloned, feature, newGeometry);
        }

        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(true, links, feature, newGeometry);
        }

        private void Start(IHydroObject hydroObject, AddRelatedFeature addRelatedFeature, int level)
        {
            coordinateSystemFactory = new OgrCoordinateSystemFactory();
            lastFeature = hydroObject;
            lastCoordinate = (Coordinate) hydroObject.Geometry.InteriorPoint.Coordinates[0].Clone();

            links = new List<HydroLink>(hydroObject.Links);
            linksCloned = new List<HydroLink>(hydroObject.Links.Select(l => l.Clone()).OfType<HydroLink>());
            linkGeometriesCloned = links.Select(l => l.Geometry.Clone()).OfType<IGeometry>().ToList();
            linkRegions = links.Select(GetLinkRegion).ToList();

            linkRules = addRelatedFeature != null
                ? links.Select((l, i) => GetInteractors(l, i, addRelatedFeature, level)).ToList()
                : null;
        }

        private void UpdateOrStoreRelatedFeatures(bool final, IList<HydroLink> features, IFeature feature, IGeometry newGeometry)
        {
            var hydroObject = feature as IHydroObject;

            if(hydroObject == null || hydroObject.Links == null || !Equals(feature, lastFeature))
            {
                return;
            }

            for (var index = 0; index < links.Count; index++)
            {
                var link = links[index];
                var geometry = linkGeometriesCloned[index];
                var region = linkRegions[index];

                var localStartCoordinate = GetLocalCoordinate(lastCoordinate, hydroObject.Region?.CoordinateSystem, region.CoordinateSystem);
                Coordinate localEndCoordinate;
                
                try
                {
                    // interior point of newGeometry can become invalid, so use try catch
                    localEndCoordinate = GetLocalCoordinate(newGeometry.InteriorPoint.Coordinates[0], hydroObject.Region?.CoordinateSystem, region.CoordinateSystem);
                }
                catch (Exception)
                {
                    localEndCoordinate = localStartCoordinate.Copy();
                }

                var deltaX = localEndCoordinate.X - localStartCoordinate.X;
                var deltaY = localEndCoordinate.Y - localStartCoordinate.Y;
                
                FallOffPolicy.Reset();

                var coordinateToMove = Equals(link.Target, feature) ? geometry.Coordinates.Length - 1 : 0;
                
                // use the move method of FallOfPolicy that uses a source and target geometry
                var targetFeature = features[index];

                if (final)
                    FallOffPolicy.Move(targetFeature, geometry, coordinateToMove, deltaX, deltaY);
                else
                    FallOffPolicy.Move(targetFeature.Geometry, geometry, coordinateToMove, deltaX, deltaY);
                
                if (linkRules == null) continue;

                var linkTrackerIndices = new List<int> { coordinateToMove };
                var linkFeatureRelationInteractors = linkRules[index];

                foreach (var interactor in linkFeatureRelationInteractors)
                {
                    if (final)
                        interactor.StoreRelatedFeatures(link, targetFeature.Geometry, linkTrackerIndices);
                    else
                        interactor.UpdateRelatedFeatures(link, targetFeature.Geometry, linkTrackerIndices);
                }
            }

            if (!final) return;

            coordinateSystemFactory = null;
            linkRegions = null;
            links = null;
            linksCloned = null;
            linkGeometriesCloned = null;
            linkRules = null;
            lastFeature = null;
        }

        private IFeatureRelationInteractor CloneRule()
        {
            return new HydroObjectToHydroLinkRelationInteractor { FallOffPolicy = FallOffPolicy };
        }

        private static IHydroRegion GetLinkRegion(HydroLink link)
        {
            var region = link.Source.Region;

            while (true)
            {
                if (region.Links.Contains(link))
                {
                    return region;
                }

                var parentHydroRegion = region.Parent as IHydroRegion;
                if (parentHydroRegion == null) return null;
                
                region = parentHydroRegion;
            }
        }

        private List<IFeatureRelationInteractor> GetInteractors(IFeature link, int i, AddRelatedFeature addRelatedFeature, int level)
        {
            var featureRelationInteractors = new List<IFeatureRelationInteractor>();
            addRelatedFeature(featureRelationInteractors, link, linksCloned[i], level);
            return featureRelationInteractors;
        }

        private Coordinate GetLocalCoordinate(Coordinate coordinate, ICoordinateSystem sourceCoordinateSystem, ICoordinateSystem targetCoordinateSystem)
        {
            if (sourceCoordinateSystem == null || targetCoordinateSystem == null)
            {
                return coordinate;
            }

            var transformation = coordinateSystemFactory.CreateTransformation(sourceCoordinateSystem, targetCoordinateSystem);

            return GeometryTransform.TransformGeometry(new Point(coordinate), transformation.MathTransform).Coordinate;
        }
    }
}