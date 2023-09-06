using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

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
                        KnownWaveObsProperties.PolylineFile, 
                        new NoDependentsFileMigrateBehaviour(KnownWaveObsProperties.PolylineFile,
                                                             relativeDirectory,
                                                             goalDirectory)
                    },
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {KnownWaveObsSections.ObstacleFileInformation, obstacleFileInformationMapping},
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
                    {KnownWaveSections.GeneralSection, CreateGeneralCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.DomainSection, CreateDomainCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.BoundarySection, CreateBoundaryCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.OutputSection, CreateOutputCategoryMigrations(relativeDirectory, goalDirectory)},
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
                {KnownWaveProperties.FlowFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.FlowFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.FlowMudFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.FlowMudFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.ObstacleFile, new DelftIniFileMigrateBehaviour(KnownWaveProperties.ObstacleFile, relativeDirectory, goalDirectory, CreateObsMigrator(relativeDirectory, goalDirectory))},
                {KnownWaveProperties.TimeSeriesFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.TimeSeriesFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.MeteoFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.MeteoFile, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateDomainCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.Grid, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.Grid, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.BedLevelGrid, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.BedLevelGrid, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.BedLevel, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.BedLevel, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.MeteoFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.MeteoFile, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateBoundaryCategoryMigrations(string relativeDirectory,
                                                                                                                string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.Spectrum, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.Spectrum, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateOutputCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.LocationFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.LocationFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.CurveFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.CurveFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.COMFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.COMFile, relativeDirectory, goalDirectory)},
            };
        }
    }
}