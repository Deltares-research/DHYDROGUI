using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataHarvester"/> implements the interface
    /// with which to obtain the relevant wave output files from a
    /// given directory.
    /// </summary>
    /// <seealso cref="IWaveOutputDataHarvester" />
    public sealed class WaveOutputDataHarvester : IWaveOutputDataHarvester
    {
        private readonly IWaveFeatureProvider featureProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveOutputDataHarvester"/> class.
        /// </summary>
        /// <param name="featureProvider">The wave model feature provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="featureProvider"/> is <c>null</c>.
        /// </exception>
        public WaveOutputDataHarvester(IWaveFeatureProvider featureProvider)
        {
            Ensure.NotNull(featureProvider, nameof(featureProvider));

            this.featureProvider = featureProvider;
        }

        public IReadOnlyList<ReadOnlyTextFileData> HarvestDiagnosticFiles(DirectoryInfo outputDataDirectory,
                                                                          ILogHandler logHandler = null) =>
            HarvestTextFiles(IsDiagnosticFile, outputDataDirectory, logHandler);

        public IReadOnlyList<ReadOnlyTextFileData> HarvestSpectraFiles(DirectoryInfo outputDataDirectory,
                                                                       ILogHandler logHandler = null) =>
            HarvestTextFiles(IsSpectraFile, outputDataDirectory, logHandler);

        public IReadOnlyList<WavmFileFunctionStore> HarvestWavmFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                                  ILogHandler logHandler = null) =>
            HarvestFiles(IsWavmFileFunctionStore, ConstructWavmFileFunctionStore, outputDataDirectory, logHandler);

        public IReadOnlyList<WavhFileFunctionStore> HarvestWavhFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                                  ILogHandler logHandler = null) =>
            HarvestFiles(IsWavhFileFunctionStore, ConstructWavhFileFunctionStore, outputDataDirectory, logHandler);

        private static IReadOnlyList<ReadOnlyTextFileData> HarvestTextFiles(Func<FileInfo, bool> isRelevantFilePredicate,
                                                                            DirectoryInfo outputDataDirectory,
                                                                            ILogHandler logHandler) =>
            HarvestFiles(isRelevantFilePredicate, ReadTextFile, outputDataDirectory, logHandler);


        private static IReadOnlyList<T> HarvestFiles<T>(Func<FileInfo, bool> isRelevantFilePredicate,
                                                        Func<FileInfo, T> constructionFunc,
                                                        DirectoryInfo outputDataDirectory,
                                                        ILogHandler logHandler)
        {
            Ensure.NotNull(outputDataDirectory, nameof(outputDataDirectory));

            var result = new List<T>();

            foreach (FileInfo fileInfo in outputDataDirectory.EnumerateFiles()
                                                             .Where(isRelevantFilePredicate))
            {
                try
                {
                    result.Add(constructionFunc.Invoke(fileInfo));
                }
                catch (Exception e) when (e is PathTooLongException ||
                                          e is IOException ||
                                          e is UnauthorizedAccessException ||
                                          e is SecurityException)
                {
                    logHandler?.ReportWarningFormat(Resources.WaveOutputDataHarvester_Could_not_read_file___0__due_to__1__, 
                                                    fileInfo.Name, e.Message);
                }
            }

            return result;
        }

        private static ReadOnlyTextFileData ReadTextFile(FileInfo fileInfo) =>
            new ReadOnlyTextFileData(fileInfo.Name, File.ReadAllText(fileInfo.FullName));

        private static WavmFileFunctionStore ConstructWavmFileFunctionStore(FileInfo fileInfo) =>
            new WavmFileFunctionStore(fileInfo.FullName);

        private WavhFileFunctionStore ConstructWavhFileFunctionStore(FileInfo fileInfo) =>
            new WavhFileFunctionStore(fileInfo.FullName, featureProvider);

        private static bool IsDiagnosticFile(FileInfo fileInfo) =>
            fileInfo.Name == WaveOutputConstants.SwanLogFileName ||
            fileInfo.Name.StartsWith(WaveOutputConstants.SwanDiagnosticFilePrefix);

        private static bool IsSpectraFile(FileInfo fileInfo) =>
            fileInfo.Extension == WaveOutputConstants.sp1Extension ||
            fileInfo.Extension == WaveOutputConstants.sp2Extension;

        private static bool IsWavmFileFunctionStore(FileInfo fileInfo) =>
            fileInfo.Name.StartsWith(WaveOutputConstants.MapFilePrefix) &&
            fileInfo.Extension == WaveOutputConstants.ncExtension;

        private static bool IsWavhFileFunctionStore(FileInfo fileInfo) =>
            fileInfo.Name.StartsWith(WaveOutputConstants.HisFilePrefix) &&
            fileInfo.Extension == WaveOutputConstants.ncExtension;
    }
}