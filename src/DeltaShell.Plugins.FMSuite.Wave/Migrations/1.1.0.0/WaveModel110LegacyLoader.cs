using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="LegacyLoader"/> for the <see cref="WaveModel"/> to migrate
    /// to the directory structure associated with file format version 1.2.0.0.
    /// </summary>
    /// <seealso cref="LegacyLoader"/>
    public class WaveModel110LegacyLoader : LegacyLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveModel110LegacyLoader));

        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            base.OnAfterInitialize(entity, dbConnection);

            if (!(entity is WaveModel model))
            {
                log.ErrorFormat(Resources.WaveModel110LegacyLoader_OnAfterInitialize_Provided_entity_is_not_a__0_,
                                nameof(WaveModel));
                return;
            }

            if (!MigrationHelper.TryParseDatabasePath(
                    dbConnection.ConnectionString,
                    out string dbPath))
            {
                log.ErrorFormat(Resources.WaveModel110LegacyLoader_OnAfterInitialize_Could_not_determine_dsproj_location_from_database_connection___0_,
                                dbConnection.ConnectionString);
            }

            string modelPath = Path.Combine(dbPath + "_data", model.Name);
            string mdwPath = Path.Combine(modelPath, $"{model.Name}.mdw");

            WaveDirectoryStructureMigrationHelper.MigrateFileStructure(mdwPath);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            base.OnAfterProjectMigrated(project);

            foreach (WaveModel waveModel in GetAllWaveModelsFromProject(project))
            {
                string activityName = string.Format(Resources.WaveModel110LegacyLoader_OnAfterProjectMigrated_Unlinking_existing_wavm_nc_files_in__0__,
                                                    waveModel.Name);
                var logHandler = new LogHandler(activityName, log);
                WavmFunctionStoreMigrationHelper.DisconnectWavmFunctionStores(waveModel, logHandler);
                logHandler.LogReport();
            }
        }

        private static IEnumerable<WaveModel> GetAllWaveModelsFromProject(Project project) =>
            project.GetAllItemsRecursive().OfType<WaveModel>();
    }
}