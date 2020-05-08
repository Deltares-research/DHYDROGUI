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
        private readonly IList<IGeometry> lastRelatedFeatureGeometries = new List<IGeometry>();
        private readonly List<List<IFeatureRelationInteractor>> activeLinkRules = new List<List<IFeatureRelationInteractor>>();
        private Catchment lastFeature;
        private IList<IFeature> lastRelatedFeatures;
        private IList<IFeature> lastRelatedNewFeatures;
        private Coordinate lastCoordinate;

        public override IFeatureRelationInteractor Activate(IFeature feature, IFeature cloneFeature, AddRelatedFeature addRelatedFeature, int level, IFallOffPolicy fallOffPolicy)
        {
            FallOffPolicy = new LinearFallOffPolicy();

            var catchment = feature as Catchment;
            if (catchment == null)
            {
                return null;
            }

            var cloneRule = (SubCatchmentRelationInteractor) CloneRule();
            cloneRule.Start(catchment, addRelatedFeature, level);

            return cloneRule;
        }

        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(false, lastRelatedNewFeatures, feature, newGeometry, trackerIndices);
        }

        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            UpdateOrStoreRelatedFeatures(true, lastRelatedFeatures, feature, newGeometry, trackerIndices);
        }

        private IFallOffPolicy FallOffPolicy { get; set; }

        private IFeatureRelationInteractor CloneRule()
        {
            return new SubCatchmentRelationInteractor {FallOffPolicy = FallOffPolicy};
        }

        private void Start(Catchment catchment, AddRelatedFeature addRelatedFeature, int level)
        {
            lastFeature = catchment;
            lastRelatedFeatureGeometries.Clear();
            lastRelatedFeatures = new List<IFeature>();
            lastRelatedNewFeatures = new List<IFeature>();

            lastCoordinate = (Coordinate) catchment.Geometry.Coordinates[0].Clone();

            if (catchment.SubCatchments.Count == 0)
            {
                return;
            }

            foreach (Catchment subCatchment in catchment.SubCatchments)
            {
                lastRelatedFeatures.Add(subCatchment);
                var clone = (Catchment) subCatchment.Clone();
                lastRelatedNewFeatures.Add(clone);
                lastRelatedFeatureGeometries.Add((IGeometry) clone.Geometry.Clone());

                if (addRelatedFeature == null)
                {
                    continue;
                }

                activeLinkRules.Add(new List<IFeatureRelationInteractor>());
                addRelatedFeature(activeLinkRules[activeLinkRules.Count - 1], subCatchment, clone, level);
            }
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

            double deltaX = newGeometry.Coordinates[0].X - lastCoordinate.X;
            double deltaY = newGeometry.Coordinates[0].Y - lastCoordinate.Y;

            for (var b = 0; b < catchment.SubCatchments.Count; b++)
            {
                Catchment subCatchment = catchment.SubCatchments[b];
                IGeometry geometry = lastRelatedFeatureGeometries[index];
                FallOffPolicy.Reset();

                // use the move method of FallOfPolicy that uses a source and target geometry
                if (final)
                {
                    FallOffPolicy.Move(features[index], geometry, 0, deltaX, deltaY);
                }
                else
                {
                    FallOffPolicy.Move(features[index].Geometry, geometry, 0, deltaX, deltaY);
                }

                var linkTrackerIndices = new List<int> {0};
                for (var i = 0; i < activeLinkRules[b].Count; i++)
                {
                    if (final)
                    {
                        activeLinkRules[b][i].StoreRelatedFeatures(subCatchment, features[index].Geometry, linkTrackerIndices);
                    }
                    else
                    {
                        activeLinkRules[b][i].UpdateRelatedFeatures(subCatchment, features[index].Geometry, linkTrackerIndices);
                    }
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