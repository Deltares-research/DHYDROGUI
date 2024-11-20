using System;
using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    /// <summary>
    /// <see cref="WaveOutputFileHelper"/> provides methods to help interact
    /// with the output data on disk.
    /// </summary>
    public static class WaveOutputFileHelper
    {
        /// <summary>
        /// Collects the input file names from the specified working directory
        /// mdw file at the <see cref="mdwPath"/>.
        /// </summary>
        /// <param name="mdwPath">The working directory .mdw path.</param>
        /// <returns>
        /// A set of the input file names as strings found in the working directory
        /// mdw file.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified mdwPath does not exist.
        /// </exception>
        public static HashSet<string> CollectInputFileNamesFromWorkingDirectoryMdw(string mdwPath)
        {
            Ensure.NotNullOrEmpty(mdwPath, nameof(mdwPath));

            var mdwPathInfo = new FileInfo(mdwPath);

            if (!mdwPathInfo.Exists)
            {
                throw new ArgumentException(string.Format(Resources.WaveOutputFileHelper_CollectInputFileNamesFromWorkingDirectoryMdw_The_specified__0__at__1__does_not_exist_, 
                                                          nameof(mdwPath), mdwPath));
            }

            var hashSet = new HashSet<string>();
            string relativeDirectory = Path.GetDirectoryName(mdwPath);

            IIniFileOperator collector = CreateMdwCollector(hashSet, relativeDirectory);

            collector.Invoke(mdwPathInfo.Open(FileMode.Open), mdwPath, null);

            return hashSet;
        }

        private static IIniFileOperator CreateObsCollector(HashSet<string> hashSet, string relativeDirectory)
        {
            var obstacleFileInformationMapping =
                new Dictionary<string, IIniPropertyBehaviour>()
                {
                    {KnownWaveObsProperties.PolylineFile, new CollectPropertyValueBehaviour(KnownWaveObsProperties.PolylineFile, hashSet, relativeDirectory)},
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>()
                {
                    {KnownWaveObsSections.ObstacleFileInformation, obstacleFileInformationMapping},
                };

            IIniPostOperationBehaviour[] postBehaviours =
            {
                new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory)
            };

            return new IniFileOperator(mapping, new IniReader(), postBehaviours);
        }

        private static IIniFileOperator CreateMdwCollector(HashSet<string> hashSet, string relativeDirectory)
        {
            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>()
                {
                    {KnownWaveSections.GeneralSection, CreateGeneralCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.DomainSection, CreateDomainCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.BoundarySection, CreateBoundaryCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.OutputSection, CreateOutputCategoryMigrations(hashSet, relativeDirectory)},
                };

            IIniPostOperationBehaviour[] postBehaviours =
            {
                new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory),
            };

            return new IniFileOperator(mapping, new IniReader(), postBehaviours);
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateGeneralCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.FlowFile, new CollectPropertyValueBehaviour(KnownWaveProperties.FlowFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.FlowMudFile, new CollectPropertyValueBehaviour(KnownWaveProperties.FlowMudFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.ObstacleFile, new CollectPropertyValueWithDependentsBehaviour(KnownWaveProperties.ObstacleFile,
                                                                        relativeDirectory,
                                                                        CreateObsCollector(hashSet, relativeDirectory))},
                {KnownWaveProperties.TimeSeriesFile, new CollectPropertyValueBehaviour(KnownWaveProperties.TimeSeriesFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.MeteoFile, new CollectPropertyValueBehaviour(KnownWaveProperties.MeteoFile, hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateDomainCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.Grid, new CollectPropertyValueBehaviour(KnownWaveProperties.Grid, hashSet, relativeDirectory)},
                {KnownWaveProperties.BedLevelGrid, new CollectPropertyValueBehaviour(KnownWaveProperties.BedLevelGrid, hashSet, relativeDirectory)},
                {KnownWaveProperties.BedLevel, new CollectPropertyValueBehaviour(KnownWaveProperties.BedLevel, hashSet, relativeDirectory)},
                {KnownWaveProperties.MeteoFile, new CollectPropertyValueBehaviour(KnownWaveProperties.MeteoFile, hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateBoundaryCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.Spectrum, new CollectPropertyValueBehaviour(KnownWaveProperties.Spectrum, hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IIniPropertyBehaviour> CreateOutputCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IIniPropertyBehaviour>
            {
                {KnownWaveProperties.LocationFile, new CollectPropertyValueBehaviour(KnownWaveProperties.LocationFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.CurveFile, new CollectPropertyValueBehaviour(KnownWaveProperties.CurveFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.COMFile, new CollectPropertyValueBehaviour(KnownWaveProperties.COMFile, hashSet, relativeDirectory)},
            };
        }
    }
}