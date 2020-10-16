using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;

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
                throw new ArgumentException($"The specified {nameof(mdwPath)} at {mdwPath} does not exist.");
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
                    {"PolylineFile", new CollectPropertyValueBehaviour("PolylineFile", hashSet, relativeDirectory)},
                };

            var mapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>()
                {
                    {"ObstacleFileInformation", obstacleFileInformationMapping},
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
                    {"General", CreateGeneralCategoryMigrations(hashSet, relativeDirectory)},
                    {"Domain", CreateDomainCategoryMigrations(hashSet, relativeDirectory)},
                    {"Boundary", CreateBoundaryCategoryMigrations(hashSet, relativeDirectory)},
                    {"Output", CreateOutputCategoryMigrations(hashSet, relativeDirectory)},
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
                {"FlowFile", new CollectPropertyValueBehaviour("FlowFile", hashSet, relativeDirectory)},
                {"FlowMudFile", new CollectPropertyValueBehaviour("FlowMudFile", hashSet, relativeDirectory)},
                {"ObstacleFile", new CollectPropertyValueWithDependentsBehaviour("ObstacleFile",
                                                                        relativeDirectory,
                                                                        CreateObsCollector(hashSet, relativeDirectory))},
                {"TSeriesFile", new CollectPropertyValueBehaviour("TSeriesFile", hashSet, relativeDirectory)},
                {"MeteoFile", new CollectPropertyValueBehaviour("MeteoFile", hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateDomainCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"Grid", new CollectPropertyValueBehaviour("Grid", hashSet, relativeDirectory)},
                {"BedLevelGrid", new CollectPropertyValueBehaviour("BedLevelGrid", hashSet, relativeDirectory)},
                {"BedLevel", new CollectPropertyValueBehaviour("BedLevel", hashSet, relativeDirectory)},
                {"MeteoFile", new CollectPropertyValueBehaviour("MeteoFile", hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateBoundaryCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"Spectrum", new CollectPropertyValueBehaviour("Spectrum", hashSet, relativeDirectory)},
            };
        }

        private static IReadOnlyDictionary<string, IDelftIniPropertyBehaviour> CreateOutputCategoryMigrations(HashSet<string> hashSet, string relativeDirectory)
        {
            return new Dictionary<string, IDelftIniPropertyBehaviour>
            {
                {"LocationFile", new CollectPropertyValueBehaviour("LocationFile", hashSet, relativeDirectory)},
                {"CurveFile", new CollectPropertyValueBehaviour("CurveFile", hashSet, relativeDirectory)},
                {"COMFile", new CollectPropertyValueBehaviour("COMFile", hashSet, relativeDirectory)},
            };
        }
    }
}