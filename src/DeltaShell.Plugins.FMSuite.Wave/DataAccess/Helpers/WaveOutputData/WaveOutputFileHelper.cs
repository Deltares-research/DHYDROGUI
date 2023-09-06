using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
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

            IDelftIniFileOperator collector = CreateMdwCollector(hashSet, relativeDirectory);

            collector.Invoke(mdwPathInfo.Open(FileMode.Open), mdwPath, null);

            return hashSet;
        }

        private static IDelftIniFileOperator CreateObsCollector(HashSet<string> hashSet, string relativeDirectory)
        {
            var obstacleFileInformationMapping =
                new Dictionary<string, IDelftIniPropertyBehaviour>()
                {
                    {KnownWaveObsProperties.PolylineFile, new CollectPropertyValueBehaviour(KnownWaveObsProperties.PolylineFile, hashSet, relativeDirectory)},
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {KnownWaveObsSections.ObstacleFileInformation, obstacleFileInformationMapping},
                };

            IDelftIniPostOperationBehaviour[] postBehaviours =
            {
                new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory)
            };

            return new DelftIniFileOperator(mapping, new DelftIniReader(), postBehaviours);
        }

        private static IDelftIniFileOperator CreateMdwCollector(HashSet<string> hashSet, string relativeDirectory)
        {
            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {KnownWaveSections.GeneralSection, CreateGeneralCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.DomainSection, CreateDomainCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.BoundarySection, CreateBoundaryCategoryMigrations(hashSet, relativeDirectory)},
                    {KnownWaveSections.OutputSection, CreateOutputCategoryMigrations(hashSet, relativeDirectory)},
                };

            IDelftIniPostOperationBehaviour[] postBehaviours =
            {
                new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory),
            };

            return new DelftIniFileOperator(mapping, new DelftIniReader(), postBehaviours);
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateGeneralCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
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

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateDomainCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.Grid, new CollectPropertyValueBehaviour(KnownWaveProperties.Grid, hashSet, relativeDirectory)},
                {KnownWaveProperties.BedLevelGrid, new CollectPropertyValueBehaviour(KnownWaveProperties.BedLevelGrid, hashSet, relativeDirectory)},
                {KnownWaveProperties.BedLevel, new CollectPropertyValueBehaviour(KnownWaveProperties.BedLevel, hashSet, relativeDirectory)},
                {KnownWaveProperties.MeteoFile, new CollectPropertyValueBehaviour(KnownWaveProperties.MeteoFile, hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateBoundaryCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.Spectrum, new CollectPropertyValueBehaviour(KnownWaveProperties.Spectrum, hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateOutputCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {KnownWaveProperties.LocationFile, new CollectPropertyValueBehaviour(KnownWaveProperties.LocationFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.CurveFile, new CollectPropertyValueBehaviour(KnownWaveProperties.CurveFile, hashSet, relativeDirectory)},
                {KnownWaveProperties.COMFile, new CollectPropertyValueBehaviour(KnownWaveProperties.COMFile, hashSet, relativeDirectory)},
            };
        }
    }
}