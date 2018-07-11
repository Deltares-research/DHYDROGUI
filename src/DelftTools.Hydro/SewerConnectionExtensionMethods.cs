using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public static class SewerConnectionExtensionMethods
    {
        /// <summary>
        /// Add structure to branch, additionaly makes certain the geometry is set.
        /// </summary>
        /// <param name="sewerConnection"></param>
        /// <param name="structure"></param>
        public static ICompositeBranchStructure AddStructureToBranch(this ISewerConnection sewerConnection, IStructure1D structure)
        {
            structure.Branch = sewerConnection;
            structure.Network = sewerConnection.Network;
            structure.Chainage = 0;

            if (sewerConnection.Geometry != null && sewerConnection.Geometry.Coordinates.Any())
            {
                structure.Geometry = new Point(sewerConnection.Geometry.Coordinates[0]);
            }
            structure.Name = sewerConnection.Name;

            return HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, sewerConnection);
        }

        public static IEnumerable<IStructure1D> GetStructuresFromBranchFeatures(this ISewerConnection sewerConnection)
        {
            var compositeStructures = sewerConnection.BranchFeatures.OfType<CompositeBranchStructure>();
            var structures = sewerConnection.BranchFeatures.OfType<IStructure1D>().Except(compositeStructures);
            return structures;
        }

        public static IEnumerable<T> GetStructuresFromBranchFeatures<T>(this ISewerConnection sewerConnection)
        {
            //Branch features are added as a composite branch structure.
            var branchStructuresT = sewerConnection.BranchFeatures.OfType<T>().ToList();
            if (!branchStructuresT.Any())
            {
                //Try as a composite structure as it should be the type added.
                var compositeStructures = sewerConnection.BranchFeatures.OfType<CompositeBranchStructure>().ToList();
                if (compositeStructures.Any())
                {
                    var compositeStructure = compositeStructures.First();
                    //Only one compositeStructure allowed per connection, so we are good to go.
                    return compositeStructure.Structures.OfType<T>();
                }
            }
            return branchStructuresT;
        }
        
        public static bool IsOrifice(this ISewerConnection sewerConnection)
        {
            return sewerConnection is SewerConnectionOrifice;
        }

        public static bool IsPipe(this ISewerConnection sewerConnection)
        {
            return sewerConnection is Pipe;
        }
    }
}