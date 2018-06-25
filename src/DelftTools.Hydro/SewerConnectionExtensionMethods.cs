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