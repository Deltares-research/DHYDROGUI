using System.Data;
using System.IO;
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

            if (!TryObtainDatabasePath(dbConnection.ConnectionString, out string dbPath))
            {
                log.Error($"Could not determine dsproj location from database connection: {dbConnection.ConnectionString}");
            }

            string mdwPath = Path.Combine(dbPath + "_data", model.Name, $"{model.Name}.mdw");

            WaveDirectoryStructureMigrationHelper.Migrate(mdwPath);
        }

        private static bool TryObtainDatabasePath(string connectionString, 
                                                  out string databasePath)
        {
            string[] keyValuePairs = connectionString.Split(';');

            databasePath = null;

            foreach (string keyValueString in keyValuePairs)
            {
                if (TryParseDataSource(keyValueString, out string parsedPath))
                {
                    databasePath = parsedPath;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseDataSource(string keyValueString, out string parsedDataSourcePath)
        {
            parsedDataSourcePath = null;

            string[] keyValuePair = keyValueString.Split('=');
            string key = keyValuePair[0].Trim();
            string value = keyValuePair[1].Trim();

            if (key != "Data Source")
            {
                return false;
            }

            parsedDataSourcePath = value;
            return true;
        }
    }
}