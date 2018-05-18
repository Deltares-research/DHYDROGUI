using System.Collections.Generic;
using System.Data;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;

namespace DelftTools.Hydro
{
    public class HydroNetwork351LegacyLoader : LegacyLoader
    {
        private readonly IList<HydroNetwork> hydroNetworks = new List<HydroNetwork>();

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            var hydroNetwork = entity as HydroNetwork;
            if (hydroNetwork != null) hydroNetworks.Add(hydroNetwork);
        }
        public override void OnAfterProjectMigrated(Project project)
        {
            foreach (var hydroNetwork in hydroNetworks)
            {
                hydroNetwork.EnsureCompositeBranchStructureNamesAreUnique();
            }
        }
    }
}