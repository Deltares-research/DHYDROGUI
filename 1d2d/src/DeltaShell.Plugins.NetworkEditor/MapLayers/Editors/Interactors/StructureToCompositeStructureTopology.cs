using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    class StructureToCompositeStructureTopology<T> where T : IStructure1D
    {
        public IHydroNetwork Network { set; get; }
        public IEnumerable<ICompositeBranchStructure> CompositeStructures { get; set; }
        public ILayer Layer { get; set; }
        public double Tolerance { get; set; }

        /// <summary>
        /// Called when a structure geometry is created in the editor. The structure will be 
        /// connected to the StructureFeature.
        /// </summary>
        /// <param name="t">The newly created structure</param>
        /// <param name="snapResult"></param>
        public void OnStructureAdded(T t, SnapResult snapResult)
        {
            ConnectStructureToStructureFeature(t, snapResult);
            HydroNetworkHelper.RemoveUnusedCompositeStructures(Network);
        }

        /// <summary>
        /// connects branchFeature to the first branch is in range. This is not correct it should 
        /// take the nearest or only use a very small tolerance. Updating coordinates is primarily 
        /// the responsibility of the snapping layer.
        /// </summary>
        /// <param name="t">Assumed its <see cref="IFeature.Geometry"/> is not null.</param>
        /// <param name="snapResult"></param>
        public void ConnectStructureToStructureFeature(T t, SnapResult snapResult)
        {
            ICompositeBranchStructure newParentCompositeStructure = null;
            //  if available use snapping results.
            int newIndex = -1;
            if (null != snapResult)
            {
                var newSnappedStructure = snapResult.SnappedFeature as IStructure1D;
                if (newSnappedStructure != null)
                {
                    newParentCompositeStructure = newSnappedStructure.ParentStructure;
                    newIndex = snapResult.SnapIndexNext;
                }
            }

            // if snap tool failed we will effectively add the structure to the end of the list.
            if (newParentCompositeStructure == null)
            {
                var limit = Layer == null ? Tolerance : MapHelper.ImageToWorld(Layer.Map, 1);
                newParentCompositeStructure = CompositeStructures.FirstOrDefault(cs => cs.Geometry.Distance(t.Geometry) < limit);
            }

            var oldParentCompositeStructure = CompositeStructures.FirstOrDefault(s => s.Structures.Contains(t));
            if (null != newParentCompositeStructure)
            {
                // existing or new structure is snapped to a composite structure
                AddStructureToExistingCompositeStructure(newParentCompositeStructure, t, oldParentCompositeStructure, newIndex);
            }
            else
            {
                AddStructureToNewCompositeStructure(t);
            }
        }

        private void AddStructureToExistingCompositeStructure(ICompositeBranchStructure newParentCompositeBranchStructure, T t, ICompositeBranchStructure oldParentCompositeStructure, int newIndex)
        {
            var oldIndex = -1;
            if (oldParentCompositeStructure != null)
            {
                oldIndex = oldParentCompositeStructure.Structures.IndexOf(t);
                if ((oldIndex == newIndex) && (oldParentCompositeStructure == newParentCompositeBranchStructure))
                {
                    // structure is dropped at the location it already is in; do nothing.
                    return;
                }
            }
            if (oldParentCompositeStructure != null)
            {
                oldParentCompositeStructure.Structures.Remove(t);
            }
            // attach structure if it is moved to another structureFeature
            ICompositeBranchStructure parentStructure = null;
            if (oldParentCompositeStructure != newParentCompositeBranchStructure)
            {
                parentStructure = newParentCompositeBranchStructure;
            }
                // attach structure if it is relocated wihtin a structureFeature.
            else if (oldIndex != newIndex)
            {
                parentStructure = oldParentCompositeStructure;
            }
            t.ParentStructure = parentStructure;
            if (-1 != newIndex)
                parentStructure?.Structures.Insert(newIndex, t);
            else
                parentStructure?.Structures.Add(t);
        }

        private void AddStructureToNewCompositeStructure(IStructure1D structure)
        {
            IBranch branch = structure.Branch;

            var oldParentCompositeStructure = CompositeStructures.FirstOrDefault(s => s.Structures.Contains(structure));
            if (oldParentCompositeStructure != null)
            {
                oldParentCompositeStructure.Structures.Remove(structure);
            }

            // Connection could not be made to an existing StructureFeature; Add a new StructureFeature at this location
            var newCompositeStructure = new CompositeBranchStructure
            {
                Branch = branch,
                Network = branch.Network,
                Chainage = structure.Chainage,
                Geometry = (IGeometry)structure.Geometry?.Clone()
            };

            // make new composite structure names unique
            newCompositeStructure.Name = HydroNetworkHelper.GetUniqueFeatureName(newCompositeStructure.Network as HydroNetwork, newCompositeStructure);
            
            // always connect structure to branch because of property changed events
            structure.Branch =  branch; 
            branch.BranchFeatures.Add(newCompositeStructure);
            newCompositeStructure.Chainage = structure.Chainage;
            structure.ParentStructure = newCompositeStructure;
            newCompositeStructure.Structures.Add(structure);
        }
    }
}