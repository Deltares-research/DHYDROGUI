using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;

namespace DelftTools.Hydro.Structures
{
    public static class ManholeExtensions
    {
        public static IEnumerable<ISewerConnection> InternalConnections(this IManhole manhole)
        {
            return manhole.IncomingBranches
                          .Concat(manhole.OutgoingBranches)
                          .OfType<ISewerConnection>()
                          .Where(b => b.Source == b.Target)
                          .Distinct();
        }

        public static IEnumerable<IStructure1D> InternalStructures(this IManhole manhole)
        {
            return manhole.InternalConnections().SelectMany(s => s.GetStructuresFromBranchFeatures());
        }
        
        public static IEnumerable<IPipe> Pipes(this IManhole manhole)
        {
            return manhole.IncomingBranches.Concat(manhole.OutgoingBranches).OfType<IPipe>();
        }

        public static IEnumerable<OutletCompartment> OutletCompartments(this IManhole manhole)
        {
            return manhole?.Compartments?.OfType<OutletCompartment>();
        }

        public static IEnumerable<IPipe> IncomingPipes(this IManhole manhole)
        {
            return manhole.Pipes().Where(p => p.Target == manhole);
        }

        public static IEnumerable<IPipe> OutgoingPipes(this IManhole manhole)
        {
            return manhole.Pipes().Where(p => p.Source == manhole);
        }

        // TODO: Move to a good location
        public static bool IndexInRange(this ICollection shapes, int index)
        {
            return index >= 0 && index < shapes.Count;
        }
    }
}