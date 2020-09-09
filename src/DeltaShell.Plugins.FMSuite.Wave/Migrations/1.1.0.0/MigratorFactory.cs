using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="MigratorFactory"/> provides the methods to create configured
    /// <see cref="DelftIniMigrator"/> objects to be used during migration.
    /// </summary>
    public static class MigratorFactory
    {
        public static IDelftIniMigrator CreateObsMigrator(string relativeDirectory,
                                                          string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var obstacleFileInformationMapping =
                new Dictionary<string, IMigrationBehaviour>()
                {
                    {
                        "PolylineFile", new NoDependentsFileMigrateBehaviour("PolylineFile",
                                                                             relativeDirectory,
                                                                             goalDirectory)
                    },
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>()
                {
                    {"ObstacleFileInformation", obstacleFileInformationMapping},
                };

            return new DelftIniMigrator(mapping, new DelftIniReader(), new DelftIniWriter());
        }

        public static IDelftIniMigrator CreateMdwMigrator(string relativeDirectory,
                                                          string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>()
                {
                    {"General",  CreateGeneralCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Domain",   CreateDomainCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Boundary", CreateBoundaryCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Output",   CreateOutputCategoryMigrations(relativeDirectory, goalDirectory)},
                };

            return new DelftIniMigrator(mapping, new DelftIniReader(), new DelftIniWriter());
        }

        private static IReadOnlyDictionary<string, IMigrationBehaviour> 
            CreateGeneralCategoryMigrations(string relativeDirectory, string goalDirectory)
        {
            return new Dictionary<string, IMigrationBehaviour>
            {
                { "FlowFile",     new NoDependentsFileMigrateBehaviour("FlowFile", relativeDirectory, goalDirectory) },
                { "FlowMudFile",  new NoDependentsFileMigrateBehaviour("FlowMudFile", relativeDirectory, goalDirectory) },
                { "ObstacleFile", new DelftIniFileMigrateBehaviour("ObstacleFile", relativeDirectory, goalDirectory, CreateObsMigrator(relativeDirectory, goalDirectory)) },
                { "TSeriesFile",  new NoDependentsFileMigrateBehaviour("TSeriesFile", relativeDirectory, goalDirectory) },
                { "MeteoFile",    new NoDependentsFileMigrateBehaviour("MeteoFile", relativeDirectory, goalDirectory) },
            };
        }

        private static IReadOnlyDictionary<string, IMigrationBehaviour>
            CreateDomainCategoryMigrations(string relativeDirectory, string goalDirectory)
        {
            return new Dictionary<string, IMigrationBehaviour>
            {
                { "Grid",         new NoDependentsFileMigrateBehaviour("Grid", relativeDirectory, goalDirectory) },
                { "BedLevelGrid", new NoDependentsFileMigrateBehaviour("BedLevelGrid", relativeDirectory, goalDirectory) },
                { "BedLevel",     new NoDependentsFileMigrateBehaviour("BedLevel", relativeDirectory, goalDirectory) },
                { "MeteoFile",    new NoDependentsFileMigrateBehaviour("MeteoFile", relativeDirectory, goalDirectory) },
            };
        }

        private static IReadOnlyDictionary<string, IMigrationBehaviour>
            CreateBoundaryCategoryMigrations(string relativeDirectory, string goalDirectory)
        {
            return new Dictionary<string, IMigrationBehaviour>
            {
                { "Spectrum", new NoDependentsFileMigrateBehaviour("Spectrum", relativeDirectory, goalDirectory) },
            };
        }

        private static IReadOnlyDictionary<string, IMigrationBehaviour>
            CreateOutputCategoryMigrations(string relativeDirectory, string goalDirectory)
        {
            return new Dictionary<string, IMigrationBehaviour>
            {
                { "LocationFile", new NoDependentsFileMigrateBehaviour("LocationFile", relativeDirectory, goalDirectory) },
                { "CurveFile",    new NoDependentsFileMigrateBehaviour("CurveFile", relativeDirectory, goalDirectory) },
                // Adjust COM file migration behaviour
                { "COMFile",      new NoDependentsFileMigrateBehaviour("COMFile", relativeDirectory, goalDirectory) },
            };
        }
    }
}