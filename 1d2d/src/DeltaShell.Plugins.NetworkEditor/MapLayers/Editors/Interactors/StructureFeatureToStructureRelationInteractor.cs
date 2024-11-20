using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using SharpMap.Api.Delegates;
using SharpMap.Api.Editors;
using SharpMap.Editors;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class StructureFeatureToStructureRelationInteractor : FeatureRelationInteractor 
    {
        IFeature lastFeature;
        IList<IStructure1D> clonedRelatedFeatures;

        public IHydroNetwork Network { get; set; }

        public override void UpdateRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            if (feature != lastFeature)
                return;
            foreach (var cloneFeature in clonedRelatedFeatures)
            {
                cloneFeature.Geometry = GeometryHelper.SetCoordinate(cloneFeature.Geometry, 0, newGeometry.Coordinates[0]);
            }
        }

        public override void StoreRelatedFeatures(IFeature feature, IGeometry newGeometry, IList<int> trackerIndices)
        {
            if (feature != lastFeature)
                return;
            var compositeStructure = (ICompositeBranchStructure)feature;
            foreach (var structure in compositeStructure.Structures)
            {
                IGeometry geometry = (IGeometry)structure.Geometry.Clone();
                structure.Geometry = GeometryHelper.SetCoordinate(geometry, 0, newGeometry.Coordinates[0]);
            }
        }

        private IFeatureRelationInteractor CloneRule()
        {
            var structureFeatureLayerToStructureTopologyRule = new StructureFeatureToStructureRelationInteractor{Network = Network};
            return structureFeatureLayerToStructureTopologyRule;
        }

        public override IFeatureRelationInteractor Activate(IFeature feature, IFeature cloneFeature, AddRelatedFeature addRelatedFeature, int level, IFallOffPolicy fallOffPolicy)
        {
            ICompositeBranchStructure CompositeBranchStructure;
            if (null != (CompositeBranchStructure = (feature as ICompositeBranchStructure)) && CompositeBranchStructure.Structures.Count > 0)
            {
                // Only activate the rule when there is something to do.
                var cloneRule = (StructureFeatureToStructureRelationInteractor)CloneRule();
                cloneRule.Start(CompositeBranchStructure, cloneFeature as ICompositeBranchStructure, addRelatedFeature, level);
                return cloneRule;
            }
            return null;
        }

        private void Start(ICompositeBranchStructure CompositeBranchStructure, ICompositeBranchStructure cloneCompositeBranchStructure, AddRelatedFeature addRelatedFeature, int level)
        {
            lastFeature = CompositeBranchStructure;
            clonedRelatedFeatures = new List<IStructure1D>();

            foreach (var structure in CompositeBranchStructure.Structures)
            {
                var clone = CloneStructure(structure, cloneCompositeBranchStructure);
                clonedRelatedFeatures.Add(clone);
                if (null != addRelatedFeature)
                    addRelatedFeature(null, structure, clone, level);
            }
        }

        private IStructure1D CloneStructure(IStructure1D structure, ICompositeBranchStructure cloneCompositeBranchStructure)
        {
            var clone = (IStructure1D)structure.Clone();
            clone.Geometry = (IGeometry)structure.Geometry.Clone();
            clone.ParentStructure = cloneCompositeBranchStructure;
            cloneCompositeBranchStructure.Structures.Add(clone);
            return clone;
        }
    }
}