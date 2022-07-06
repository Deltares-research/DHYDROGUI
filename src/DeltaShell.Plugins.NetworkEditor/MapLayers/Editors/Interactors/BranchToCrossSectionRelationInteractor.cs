using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Delegates;
using SharpMap.Api.Editors;
using SharpMap.Editors;
using SharpMap.Editors.FallOff;
using SharpMap.Editors.Interactors.Network;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    class BranchToCrossSectionRelationInteractor : FeatureRelationInteractor
    {
        List<double> fractions = new List<double>();
        private IFallOffPolicy fallOffPolicy;

        IFeature lastFeature;
        IGeometry lastGeometry;
        IList<ICrossSection> originalRelatedFeatures;
        IList<ICrossSection> clonedRelatedFeatures;

        public IHydroNetwork Network { get; set; }

        #region ITopologyRule Members


        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            if (feature != lastFeature)
            {
                throw new ArgumentException("You must call FillRelatedFeature first!");
            }

            var branch = (IChannel)feature;
            ILineString newLineString = (ILineString)newGeometry;

            IList<double> newFractions = BranchToBranchFeatureService.UpdateNewFractions(
                (ILineString)lastFeature.Geometry, newLineString,
                fractions, trackerIndices, FallOffPolicy);
            // performance improvement: test with dottrace show LineString.get_Length as a very expensive operation
            // locally store the length. This is relevant for branches with many coordinates and many cross sections.
            for (int i = 0; i < newFractions.Count; i++)
            {
                UpdateCrossSectionGeometry(clonedRelatedFeatures[i], newLineString, i, newFractions, branch);
            }
        }

        private void UpdateCrossSectionGeometry(ICrossSection crossSection, ILineString newLineString, int i, IList<double> newFractions, IChannel branch)
        {
            if (!branch.IsLengthCustom)
            {
                crossSection.Chainage = BranchFeature.SnapChainage(newLineString.Length, newLineString.Length * newFractions[i]);
            }
            else
            {
                crossSection.Chainage = crossSection.Chainage; //hack: invalidate geometry
            }
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
        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            //since crosss section calculates it's geomtry based on definition + branch we only have to update the offset here.
            //we need to call InvalidateGeometry to force the CS to recalculate.

            var branch = (IChannel)feature;
            var newLineString = (ILineString)newGeometry;

            var newFractions = BranchToBranchFeatureService.UpdateNewFractions((ILineString)lastGeometry,
                                                                               newLineString, fractions, trackerIndices, FallOffPolicy);

            // Only update non geometry based cross sections
            int fractionIndex = 0;
            foreach (var crossSection in branch.CrossSections.Where(c => !c.GeometryBased))
            {
                UpdateCrossSectionGeometry(crossSection, newLineString, fractionIndex++, newFractions, branch);
            }
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
                var branch = (IChannel)feature;
                
                if (branch.BranchFeatures.Count > 0)
                {
                    // Only activate the rule when there is something to do.
                    var cloneRule = (BranchToCrossSectionRelationInteractor)CloneRule();
                    cloneRule.Start(branch, cloneFeature as IChannel, addRelatedFeature, level);
                    return cloneRule;
                }
            }
            return null;
        }

        private void Start(IChannel branch, IChannel cloneBranch, AddRelatedFeature addRelatedFeature, int level)
        {
            lastFeature = branch;
            lastGeometry = (IGeometry)branch.Geometry.Clone();
            originalRelatedFeatures = new List<ICrossSection>();
            clonedRelatedFeatures = new List<ICrossSection>();
            fractions.Clear();
            double length = branch.Length;
            foreach (var crossSection in branch.CrossSections)
            {
                if (!crossSection.Definition.GeometryBased)
                {
                    fractions.Add(crossSection.Chainage / length); // = optimization of GeometryHelper.LineStringGetFraction
                    originalRelatedFeatures.Add(crossSection);
                    var clone = (ICrossSection)crossSection.Clone();
                    clone.Branch = cloneBranch;
                    cloneBranch.BranchFeatures.Add(clone);
                    clonedRelatedFeatures.Add(clone);
                    if (null != addRelatedFeature)
                        addRelatedFeature(activeRules, crossSection, clone, level);
                }
            }
        }

        List<IFeatureRelationInteractor> activeRules = new List<IFeatureRelationInteractor>();
        public IList<IFeatureRelationInteractor> ActiveTopologyRules
        {
            get { return activeRules; }
        }

        public IFallOffPolicy FallOffPolicy
        {
            get { return fallOffPolicy; }
            set { fallOffPolicy = value; }
        }

        #endregion
    }
}