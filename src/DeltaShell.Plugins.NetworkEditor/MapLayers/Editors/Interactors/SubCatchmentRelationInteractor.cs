using System.Collections.Generic;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Delegates;
using SharpMap.Api.Editors;
using SharpMap.Editors;
using SharpMap.Editors.FallOff;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class SubCatchmentRelationInteractor : FeatureRelationInteractor
    {
        private Catchment lastFeature;
        private readonly IList<IGeometry> lastRelatedFeatureGeometries = new List<IGeometry>();
        private IList<IFeature> lastRelatedFeatures;
        private IList<IFeature> lastRelatedNewFeatures;
        private Coordinate lastCoordinate;
        private IFallOffPolicy FallOffPolicy { get; set; }
        readonly List<List<IFeatureRelationInteractor>> activeLinkRules = new List<List<IFeatureRelationInteractor>>();

        private IFeatureRelationInteractor CloneRule()
        {
            return new SubCatchmentRelationInteractor { FallOffPolicy = FallOffPolicy };
        }

        public override IFeatureRelationInteractor Activate(IFeature feature, IFeature cloneFeature, AddRelatedFeature addRelatedFeature, int level, IFallOffPolicy fallOffPolicy)
        {
            FallOffPolicy = new LinearFallOffPolicy();

            var catchment = feature as Catchment;
            if (catchment == null)
            {
                return null;
            }

            var cloneRule = (SubCatchmentRelationInteractor)CloneRule();
            cloneRule.Start(catchment, addRelatedFeature, level);

            return cloneRule;
        }
        
        private void Start(Catchment catchment, AddRelatedFeature addRelatedFeature, int level)
        {
            lastFeature = catchment;
            lastRelatedFeatureGeometries.Clear();
            lastRelatedFeatures = new List<IFeature>();
            lastRelatedNewFeatures = new List<IFeature>();

            lastCoordinate = (Coordinate)catchment.Geometry.Coordinates[0].Clone();

            if (catchment.SubCatchments.Count == 0)
            {
                return;
            }

            foreach (var subCatchment in catchment.SubCatchments)
            {
                lastRelatedFeatures.Add(subCatchment);
                var clone = (Catchment)subCatchment.Clone();
                lastRelatedNewFeatures.Add(clone);
                lastRelatedFeatureGeometries.Add((IGeometry)clone.Geometry.Clone());

                if (addRelatedFeature == null)
                {
                    continue;
                }

                activeLinkRules.Add(new List<IFeatureRelationInteractor>());
                addRelatedFeature(activeLinkRules[activeLinkRules.Count - 1], subCatchment, clone, level);
            }
        }

        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(false, lastRelatedNewFeatures, feature, newGeometry, trackerIndices);
        }

        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(true, lastRelatedFeatures, feature, newGeometry, trackerIndices);
        }

        private void UpdateOrStoreRelatedFeatures(bool final, IList<IFeature> features, IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            var catchment = feature as Catchment;

            if (catchment == null || catchment.SubCatchments.Count == 0)
            {
                return;
            }

            if (!Equals(feature, lastFeature))
            {
                return;
            }

            var index = 0;

            var deltaX = newGeometry.Coordinates[0].X - lastCoordinate.X;
            var deltaY = newGeometry.Coordinates[0].Y - lastCoordinate.Y;

            for (var b = 0; b < catchment.SubCatchments.Count; b++)
            {
                var subCatchment = catchment.SubCatchments[b];
                var geometry = lastRelatedFeatureGeometries[index];
                FallOffPolicy.Reset();

                // use the move method of FallOfPolicy that uses a source and target geometry
                if (final)
                    FallOffPolicy.Move(features[index], geometry, 0, deltaX, deltaY);
                else
                {
                    FallOffPolicy.Move(features[index].Geometry, geometry, 0, deltaX, deltaY);
                }

                var linkTrackerIndices = new List<int> { 0 };
                for (var i = 0; i < activeLinkRules[b].Count; i++)
                {
                    if (final)
                        activeLinkRules[b][i].StoreRelatedFeatures(subCatchment, features[index].Geometry, linkTrackerIndices);
                    else
                        activeLinkRules[b][i].UpdateRelatedFeatures(subCatchment, features[index].Geometry, linkTrackerIndices);
                }

                index++;
            }

            if (final)
            {
                lastFeature = null;
            }
        }
    }
}