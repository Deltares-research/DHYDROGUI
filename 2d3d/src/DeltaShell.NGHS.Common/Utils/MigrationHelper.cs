namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Provides methods to help with the migration of projects.
    /// </summary>
    public static class MigrationHelper
    {
        /// <summary>
        /// Try and parse the database path from the provided <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databasePath">The database path.</param>
        /// <returns>
        /// <c>True</c> if a database path could be retrieved and stored in
        /// <paramref name="databasePath"/>; <c>False</c> if no database path
        /// could be retrieved and null is stored in <paramref name="databasePath"/>.
        /// </returns>
        public static bool TryParseDatabasePath(string connectionString, out string databasePath)
        {
            databasePath = null;
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            string[] keyValuePairs = connectionString.Split(';');

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