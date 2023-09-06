using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="MigratorInstanceCreator"/> provides the methods to create configured
    /// <see cref="IniFileOperator"/> objects to be used during migration.
    /// </summary>
    public static class MigratorInstanceCreator
    {
        /// <summary>
        /// Creates the <see cref="IIniFileOperator"/> to migrate .obs files.
        /// </summary>
        /// <param name="relativeDirectory">
        /// The path from which relative paths in the .obs file should be resolved.
        /// </param>
        /// <param name="goalDirectory">
        /// The goal directory to which the .obs file and dependent files are migrated.
        /// </param>
        /// <returns>
        /// A new <see cref="IIniFileOperator"/> with which .obs files can be migrated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is null.
        /// </exception>
        public static IIniFileOperator CreateObsMigrator(string relativeDirectory, string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var obstacleFileInformationMapping =
                new Dictionary<string, IIniPropertyBehaviour>()
                {
                    {
                        KnownWaveObsProperties.PolylineFile, 
                        new NoDependentsFileMigrateBehaviour(KnownWaveObsProperties.PolylineFile,
                                                             relativeDirectory,
                                                             goalDirectory)
                    },
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>()
                {
                    {KnownWaveObsSections.ObstacleFileInformation, obstacleFileInformationMapping},
                };

            IIniPostOperationBehaviour[] postBehaviours =
            {
                new DeleteSourcePostOperationBehaviour(),
                new WriteSectionsPostOperationBehaviour(new IniWriter(), goalDirectory)
            };

            return new IniFileOperator(mapping, new IniReader(), postBehaviours);
        }

        /// <summary>
        /// Creates the <see cref="IIniFileOperator"/> to migrate .mdw files.
        /// </summary>
        /// <param name="relativeDirectory">
        /// The path from which relative paths in the .mdw file should be resolved.
        /// </param>
        /// <param name="goalDirectory">
        /// The goal directory to which the .mdw file and dependent files are migrated.
        /// </param>
        /// <returns>
        /// A new <see cref="IIniFileOperator"/> with which .mdw files can be migrated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is null.
        /// </exception>
        public static IIniFileOperator CreateMdwMigrator(string relativeDirectory,
                                                              string goalDirectory)
        {
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>()
                {
                    {KnownWaveSections.GeneralSection, CreateGeneralCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.DomainSection, CreateDomainCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.BoundarySection, CreateBoundaryCategoryMigrations(relativeDirectory, goalDirectory)},
                    {KnownWaveSections.OutputSection, CreateOutputCategoryMigrations(relativeDirectory, goalDirectory)},
                };

            IIniPostOperationBehaviour[] postBehaviours =
            {
                new DeleteSourcePostOperationBehaviour(),
                new WriteSectionsPostOperationBehaviour(new IniWriter(), goalDirectory)
            };

            return new IniFileOperator(mapping, new IniReader(), postBehaviours);
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateGeneralCategoryMigrations(string relativeDirectory,
                                                                                                               string goalDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.FlowFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.FlowFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.FlowMudFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.FlowMudFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.ObstacleFile, new IniFileMigrateBehaviour(KnownWaveProperties.ObstacleFile, relativeDirectory, goalDirectory, CreateObsMigrator(relativeDirectory, goalDirectory))},
                {KnownWaveProperties.TimeSeriesFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.TimeSeriesFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.MeteoFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.MeteoFile, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateDomainCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.Grid, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.Grid, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.BedLevelGrid, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.BedLevelGrid, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.BedLevel, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.BedLevel, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.MeteoFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.MeteoFile, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateBoundaryCategoryMigrations(string relativeDirectory,
                                                                                                                string goalDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.Spectrum, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.Spectrum, relativeDirectory, goalDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateOutputCategoryMigrations(string relativeDirectory,
                                                                                                              string goalDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.LocationFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.LocationFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.CurveFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.CurveFile, relativeDirectory, goalDirectory)},
                {KnownWaveProperties.COMFile, new NoDependentsFileMigrateBehaviour(KnownWaveProperties.COMFile, relativeDirectory, goalDirectory)},
            };
        }
    }
}