using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
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
        public static ICompositeBranchStructure AddStructureToBranch(this ISewerConnection sewerConnection, IStructure1D structure, bool generateUniqueName = true)
        {
            structure.Branch = sewerConnection;
            structure.Network = sewerConnection.Network;

            if (!sewerConnection.IsInternalConnection() && sewerConnection.Geometry != null &&
                sewerConnection.Geometry.Coordinates.Any())
            {
                var x = (sewerConnection.Geometry.Coordinates[0].X + sewerConnection.Geometry.Coordinates[1].X) / 2;
                var y = (sewerConnection.Geometry.Coordinates[0].Y + sewerConnection.Geometry.Coordinates[1].Y) / 2;
                structure.Geometry = new Point(x, y);

                var dx = x - sewerConnection.Geometry.Coordinates[0].X;
                var dy = y - sewerConnection.Geometry.Coordinates[0].Y;
                structure.Chainage = Math.Sqrt(dx * dx + dy * dy);
            }

            if (structure.Name == null) structure.Name = sewerConnection.Name;

            return HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, sewerConnection, generateUniqueName);
        }

        public static IEnumerable<IStructure1D> GetStructuresFromBranchFeatures(this ISewerConnection sewerConnection)
        {
            return sewerConnection.BranchFeatures.Where(f => f is IStructure1D && !(f is ICompositeBranchStructure)).Cast<IStructure1D>();
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

        public static bool IsPipe(this ISewerConnection sewerConnection)
        {
            return sewerConnection is Pipe;
        }


        public static bool IsInternalConnection(this ISewerConnection sewerConnection)
        {
            return sewerConnection.Source != null && sewerConnection.Source == sewerConnection.Target;
        }

        /// <summary>
        /// Returns special connections between manholes. E.g. 'persleidingen', 'overstort leidingen'
        /// </summary>
        /// <param name="sewerConnection"></param>
        /// <returns></returns>
        public static bool IsSpecialConnection(this ISewerConnection sewerConnection)
        {
            return !sewerConnection.IsPipe() && !sewerConnection.IsInternalConnection();
        }
    }
}