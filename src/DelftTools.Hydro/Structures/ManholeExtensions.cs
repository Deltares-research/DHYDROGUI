using System.Collections.Generic;
using System.Linq;

namespace DelftTools.Hydro.Structures
{
    public static class ManholeExtensions
    {
        public static IEnumerable<ISewerConnection> GetManholeInternalConnections(this IManhole manhole)
        {
            var network = manhole?.Network as IHydroNetwork;

            if(network == null) return new List<ISewerConnection>();

            var connections = network.SewerConnections.Where(c => c.Source?.Name == manhole.Name && c.Target?.Name == manhole.Name).ToList();
            return connections;
        }

        public static IEnumerable<IPipe> GetPipesConnectedToManhole(this IManhole manhole, IEnumerable<IPipe> pipes)
        {
            var compartments = manhole.Compartments;

            var connectedPipes = new List<IPipe>();

            foreach (var pipe in pipes)
            {
                if (compartments.Contains(pipe.SourceCompartment) || compartments.Contains(pipe.TargetCompartment))
                {
                    connectedPipes.Add(pipe);
                }
            }

            return connectedPipes;
        }
    }
}