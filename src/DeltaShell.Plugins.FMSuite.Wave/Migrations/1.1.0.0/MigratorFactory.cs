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
                        "PolyLineFile", new NoDependentsFileMigrateBehaviour("PolyLineFile",
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
    }
}