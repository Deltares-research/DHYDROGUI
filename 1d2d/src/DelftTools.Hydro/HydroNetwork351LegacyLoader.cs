using System.Collections.Generic;
using System.Data;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Collections;

namespace DelftTools.Hydro
{
    public class HydroNetwork351LegacyLoader : LegacyLoader
    {
        private readonly IList<IHydroNetwork> hydroNetworks = new List<IHydroNetwork>();

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            var hydroNetwork = entity as IHydroNetwork;
            if (hydroNetwork == null) return;

            hydroNetworks.Add(hydroNetwork);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            hydroNetworks.ForEach(n => n.MakeNamesUnique<ICompositeBranchStructure>());
        }
    }
}