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
            if (!(manhole?.Network is IHydroNetwork network)) yield break;

            foreach (var connection in network.SewerConnections)
            {
                if (connection is IPipe) continue;

                if (connection.Source == manhole && connection.Target == manhole)
                {
                    yield return connection;
                }
            }
        }

        public static IEnumerable<IStructure1D> InternalStructures(this IManhole manhole)
        {
            return manhole.InternalConnections().SelectMany(s => s.GetStructuresFromBranchFeatures());
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
            var outletCompartments = manhole?.Compartments?.OfType<OutletCompartment>();
            return outletCompartments;
        }

        public static IEnumerable<IPipe> IncomingPipes(this IManhole manhole)
        {
            var pipes = manhole.Pipes();
            return pipes.Where(p => p.Target == manhole);
        }

        public static IEnumerable<IPipe> OutgoingPipes(this IManhole manhole)
        {
            var pipes = manhole.Pipes();
            return pipes.Where(p => p.Source == manhole);
        }

        // TODO: Move to a good location
        public static bool IndexInRange(this ICollection shapes, int index)
        {
            return index >= 0 && index < shapes.Count;
        }
    }
}