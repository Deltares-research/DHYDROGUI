using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Delegates;
using SharpMap.Api.Editors;
using SharpMap.Editors;
using SharpMap.Editors.FallOff;
using SharpMap.Editors.Interactors.Network;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    internal class BranchToCrossSectionRelationInteractor : FeatureRelationInteractor
    {
        private List<double> fractions = new List<double>();
        private IFallOffPolicy fallOffPolicy;

        private IFeature lastFeature;
        private IGeometry lastGeometry;

        public IHydroNetwork Network { get; set; }

        #region ITopologyRule Members

        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            if (feature != lastFeature)
            {
                throw new ArgumentException("You must call FillRelatedFeature first!");
            }

            var branch = (IChannel) feature;
            var newLineString = (ILineString) newGeometry;

            IList<double> newFractions = BranchToBranchFeatureService.UpdateNewFractions(
                (ILineString) lastFeature.Geometry, newLineString,
                fractions, trackerIndices, FallOffPolicy);
            // performance improvement: test with dottrace show LineString.get_Length as a very expensive operation
            // locally store the length. This is relevant for branches with many coordinates and many cross sections.
            
        }

        /// <summary>
        /// This method updates the administration of the related objects unlike UpdateRelatedFeatures that only
        /// updates the geometry to provide the user with visual feedback.
        /// </summary>
        /// <param name="feature"></param>
        /// The parent feature. The geometry has already been updated.
        /// <param name="newGeometry"></param>
        /// The new geometry for the parent feature. In fact feature == newGeometry.Geometry
        /// <param name="trackerIndices"></param>
        /// The indices that are the source of the operation.
        /// TODO: Further Optimization. Currently t
        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            //since crosss section calculates it's geomtry based on definition + branch we only have to update the offset here.
            //we need to call InvalidateGeometry to force the CS to recalculate.

            var branch = (IChannel) feature;
            var newLineString = (ILineString) newGeometry;

            IList<double> newFractions = BranchToBranchFeatureService.UpdateNewFractions((ILineString) lastGeometry,
                                                                                         newLineString, fractions, trackerIndices, FallOffPolicy);


        }

        private IFeatureRelationInteractor CloneRule()
        {
            var branchLayerToCrossSectionTopologyRule = new BranchToCrossSectionRelationInteractor
            {
                FallOffPolicy = FallOffPolicy,
                Network = Network
            };
            return branchLayerToCrossSectionTopologyRule;
        }

        public override IFeatureRelationInteractor Activate(IFeature feature, IFeature cloneFeature, AddRelatedFeature addRelatedFeature, int level, IFallOffPolicy fallOffPolicy)
        {
            FallOffPolicy = fallOffPolicy ?? new NoFallOffPolicy();
            if (feature is IChannel)
            {
                var branch = (IChannel) feature;

                if (branch.BranchFeatures.Count > 0)
                {
                    // Only activate the rule when there is something to do.
                    var cloneRule = (BranchToCrossSectionRelationInteractor) CloneRule();
                    cloneRule.Start(branch, cloneFeature as IChannel, addRelatedFeature, level);
                    return cloneRule;
                }
            }

            return null;
        }

        private void Start(IChannel branch, IChannel cloneBranch, AddRelatedFeature addRelatedFeature, int level)
        {
            lastFeature = branch;
            lastGeometry = (IGeometry) branch.Geometry.Clone();
            fractions.Clear();
            double length = branch.Length;
        }

        private List<IFeatureRelationInteractor> activeRules = new List<IFeatureRelationInteractor>();

        public IList<IFeatureRelationInteractor> ActiveTopologyRules
        {
            get
            {
                return activeRules;
            }
        }

        public IFallOffPolicy FallOffPolicy
        {
            get
            {
                return fallOffPolicy;
            }
            set
            {
                fallOffPolicy = value;
            }
        }

        #endregion
    }
}