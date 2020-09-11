using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="LegacyLoader"/> for the <see cref="WaveModel"/> to migrate
    /// to the directory structure associated with file format version 1.2.0.0.
    /// </summary>
    /// <seealso cref="LegacyLoader" />
    public class WaveModel110LegacyLoader : LegacyLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveModel110LegacyLoader));

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            base.OnAfterInitialize(entity, dbConnection);

            if (!(entity is WaveModel model))
            {
                log.Error($"Provided entity is not a {nameof(WaveModel)}");
                return;
            }

            if (!WaveDirectoryStructureMigrationHelper.TryParseDatabasePath(
                    dbConnection.ConnectionString, 
                    out string dbPath))
            {
                log.Error($"Could not determine dsproj location from database connection: {dbConnection.ConnectionString}");
            }

            string modelPath = Path.Combine(dbPath + "_data", model.Name);
            string mdwPath = Path.Combine(modelPath, $"{model.Name}.mdw");

            WaveDirectoryStructureMigrationHelper.Migrate(mdwPath);
            WaveDirectoryStructureMigrationHelper.UpdateWavmFileFunctionStorePaths(modelPath, model);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            base.OnAfterProjectMigrated(project);
            
            foreach (WaveModel waveModel in project.RootFolder.Models.OfType<WaveModel>())
            {
                WaveDirectoryStructureMigrationHelper.RemoveInvalidWavmFunctionStores(waveModel);
            }
        }
    }
}