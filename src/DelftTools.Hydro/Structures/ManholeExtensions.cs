using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Networks;

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
        
        public static IEnumerable<IBranchFeature> GetManholeInternalBranchFeatures(this IManhole manhole)
        {
            var branchFeatures = new List<IBranchFeature>();

            var network = manhole?.Network as IHydroNetwork;
            if (network == null) return branchFeatures;

            var features = network.BranchFeatures.ToList();
            foreach (var branchFeature in features)
            {
                var branch = branchFeature.Branch;
                var sewerConnection = branch as ISewerConnection;
                if(sewerConnection == null) continue;

                var sourceCompartment = sewerConnection.SourceCompartment;
                var targetCompartment = sewerConnection.TargetCompartment;

                if (manhole.ContainsCompartmentWithName(sourceCompartment.Name)&&
                    manhole.ContainsCompartmentWithName(targetCompartment.Name))
                {
                    branchFeatures.Add(branchFeature);
                }
            }
            
            return branchFeatures;
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