using System;
using System.Data.SQLite;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._2._0._0;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations
{
    /// <summary>
    /// Migrates the database to the current version of the <see cref="WaveApplicationPlugin"/>.
    /// </summary>
    public static class WavesMigrator
    {
        /// <summary>
        /// Migrates the database to the current version of the <see cref="WaveApplicationPlugin"/>.
        /// </summary>
        /// <param name="projectPath">The path of the project file.</param>
        /// <param name="projectVersion">The version of the project.</param>
        /// <param name="currentVersion"> The current version of the <see cref="WaveApplicationPlugin"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="projectPath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="projectVersion"/> or <paramref name="currentVersion"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the file at the specified <paramref name="projectPath"/> is not found.
        /// </exception>
        public static void Migrate(string projectPath, Version projectVersion, Version currentVersion)
        {
            Ensure.NotNullOrEmpty(projectPath, nameof(projectPath));
            Ensure.NotNull(projectVersion, nameof(projectVersion));
            Ensure.NotNull(currentVersion, nameof(currentVersion));

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException($"Project file does not exist: {projectPath}", projectPath);
            }

            if (projectVersion > currentVersion)
            {
                throw new ArgumentException($"The project version ({projectVersion}) cannot be higher than the current application version ({currentVersion}).");
            }

            if (projectVersion == currentVersion)
            {
                return;
            }

            using (var dbConnection = new SQLiteConnection($"Data Source={projectPath};"))
            {
                dbConnection.Open();

                WaveModel120Migrator.Migrate(dbConnection, projectVersion);
            }
        }
    }
}