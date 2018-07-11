using System.Collections.Generic;
using System.Linq;

namespace DelftTools.Hydro.Structures
{
    public static class ManholeExtensions
    {
        public static IEnumerable<ISewerConnection> InternalConnections(this IManhole manhole)
        {
            var network = manhole?.Network as IHydroNetwork;
            return network?.SewerConnections.Where(c => c.Source == manhole && c.Target == manhole) ?? new List<ISewerConnection>();
        }

        public static IEnumerable<IStructure1D> InternalStructures(this IManhole manhole)
        {
            var internalConnections = manhole.InternalConnections();
            var structures = internalConnections.SelectMany(s => ((SewerConnection) s).GetStructuresFromBranchFeatures()); // TODO add orifice
            return structures;
        }
        
        public static IEnumerable<IPipe> Pipes(this IManhole manhole)
        {
            var network = manhole?.Network as IHydroNetwork;
            if(network == null) return new List<IPipe>();
            var pipes = network.Pipes.ToList();
            return pipes.Where(p => p.Source == manhole || p.Target == manhole);
        }

        public static IEnumerable<OutletCompartment> OutletCompartments(this IManhole manhole)
        {
            var outletCompartments = manhole.Compartments.OfType<OutletCompartment>().ToList();
            return outletCompartments;
        }
    }
}