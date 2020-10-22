using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="MigratorInstanceCreator"/> provides the methods to create configured
    /// <see cref="DelftIniFileOperator"/> objects to be used during migration.
    /// </summary>
    public static class MigratorInstanceCreator
    {
        /// <summary>
        /// Creates the <see cref="IDelftIniFileOperator"/> to migrate .obs files.
        /// </summary>
        /// <param name="relativeDirectory">
        /// The path from which relative paths in the .obs file should be resolved.
        /// </param>
        /// <param name="goalDirectory">
        /// The goal directory to which the .obs file and dependent files are migrated.
        /// </param>
        /// <returns>
        /// A new <see cref="IDelftIniFileOperator"/> with which .obs files can be migrated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is null.
        /// </exception>
        public static IDelftIniFileOperator CreateObsMigrator(string relativeDirectory, string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var obstacleFileInformationMapping =
                new Dictionary<string, IDelftIniPropertyBehaviour>()
                {
                    {
                        "PolylineFile", new NoDependentsFileMigrateBehaviour("PolylineFile",
                                                                             relativeDirectory,
                                                                             goalDirectory)
                    },
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {"ObstacleFileInformation", obstacleFileInformationMapping},
                };

            IDelftIniPostOperationBehaviour[] postBehaviours =
            {
                new DeleteSourcePostOperationBehaviour(),
                new WriteCategoriesPostOperationBehaviour(new DelftIniWriter(), goalDirectory)
            };

            return new DelftIniFileOperator(mapping, new DelftIniReader(), postBehaviours);
        }

        /// <summary>
        /// Creates the <see cref="IDelftIniFileOperator"/> to migrate .mdw files.
        /// </summary>
        /// <param name="relativeDirectory">
        /// The path from which relative paths in the .mdw file should be resolved.
        /// </param>
        /// <param name="goalDirectory">
        /// The goal directory to which the .mdw file and dependent files are migrated.
        /// </param>
        /// <returns>
        /// A new <see cref="IDelftIniFileOperator"/> with which .mdw files can be migrated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is null.
        /// </exception>
        public static IDelftIniFileOperator CreateMdwMigrator(string relativeDirectory,
                                                              string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {"General", CreateGeneralCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Domain", CreateDomainCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Boundary", CreateBoundaryCategoryMigrations(relativeDirectory, goalDirectory)},
                    {"Output", CreateOutputCategoryMigrations(relativeDirectory, goalDirectory)},
                };

            IDelftIniPostOperationBehaviour[] postBehaviours =
            {
                new DeleteSourcePostOperationBehaviour(),
                new WriteCategoriesPostOperationBehaviour(new DelftIniWriter(), goalDirectory)
            };

            return new DelftIniFileOperator(mapping, new DelftIniReader(), postBehaviours);
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateGeneralCategoryMigrations(string relativeDirectory,
                                                                                                               string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"FlowFile", new NoDependentsFileMigrateBehaviour("FlowFile", relativeDirectory, goalDirectory)},
                {"FlowMudFile", new NoDependentsFileMigrateBehaviour("FlowMudFile", relativeDirectory, goalDirectory)},
                {"ObstacleFile", new DelftIniFileMigrateBehaviour("ObstacleFile", relativeDirectory, goalDirectory, CreateObsMigrator(relativeDirectory, goalDirectory))},
                {"TSeriesFile", new NoDependentsFileMigrateBehaviour("TSeriesFile", relativeDirectory, goalDirectory)},
                {"MeteoFile", new NoDependentsFileMigrateBehaviour("MeteoFile", relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateDomainCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"Grid", new NoDependentsFileMigrateBehaviour("Grid", relativeDirectory, goalDirectory)},
                {"BedLevelGrid", new NoDependentsFileMigrateBehaviour("BedLevelGrid", relativeDirectory, goalDirectory)},
                {"BedLevel", new NoDependentsFileMigrateBehaviour("BedLevel", relativeDirectory, goalDirectory)},
                {"MeteoFile", new NoDependentsFileMigrateBehaviour("MeteoFile", relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateBoundaryCategoryMigrations(string relativeDirectory,
                                                                                                                string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"Spectrum", new NoDependentsFileMigrateBehaviour("Spectrum", relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateOutputCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"LocationFile", new NoDependentsFileMigrateBehaviour("LocationFile", relativeDirectory, goalDirectory)},
                {"CurveFile", new NoDependentsFileMigrateBehaviour("CurveFile", relativeDirectory, goalDirectory)},
                {"COMFile", new NoDependentsFileMigrateBehaviour("COMFile", relativeDirectory, goalDirectory)},
            };
        }
    }
}