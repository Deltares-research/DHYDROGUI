using System.Data;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Legacy loader for <see cref="FlowFMApplicationPlugin"/> version 1.3.0.
    /// This legacy loader removes the data items for the spatial coverages from the data base.
    /// </summary>
    /// <seealso cref="LegacyLoader"/>
    public class WaterFlowFMModel130LegacyLoader : LegacyLoader
    {
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            base.OnAfterInitialize(entity, dbConnection);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            base.OnAfterProjectMigrated(project);
        }
    }
}