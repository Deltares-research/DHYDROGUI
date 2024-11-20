using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro.Roughness
{
    public class RoughnessNetworkCoverage31LegacyLoader : LegacyLoader
    {
        private readonly IList<RoughnessNetworkCoverage> roughnessNetworkCoverages = new List<RoughnessNetworkCoverage>();

        public override void OnAfterInitialize(object entity, System.Data.IDbConnection dbConnection)
        {
            base.OnAfterInitialize(entity, dbConnection);
            roughnessNetworkCoverages.Add((RoughnessNetworkCoverage) entity);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            // Remove any faulty roughness network coverage locations (which might be present due to a former bug in the roughness importer)
            foreach (var roughnessNetworkCoverage in roughnessNetworkCoverages)
            {
                var locationsVariable = roughnessNetworkCoverage.Arguments[0];
                var locationsToRemove = locationsVariable.Values
                    .OfType<NetworkLocation>()
                    .Where(nl => nl.Chainage - nl.Branch.Length > 1.0e-7); // Chainage of network location should not be larger than the branch length (taking into account an epsilon)

                if (!locationsToRemove.Any()) continue;

                locationsVariable.RemoveValues(new IVariableValueFilter[] { new VariableValueFilter<NetworkLocation>(locationsVariable, locationsToRemove) });
            }

            roughnessNetworkCoverages.Clear();
        }
    }
}