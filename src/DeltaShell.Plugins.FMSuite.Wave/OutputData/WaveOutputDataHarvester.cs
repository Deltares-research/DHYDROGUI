using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataHarvester"/> implements the interface
    /// with which to obtain the relevant wave output files from a
    /// given directory.
    /// </summary>
    /// <seealso cref="IWaveOutputDataHarvester"/>
    public sealed class WaveOutputDataHarvester : IWaveOutputDataHarvester
    {
        private readonly IWaveFeatureContainer featureContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveOutputDataHarvester"/> class.
        /// </summary>
        /// <param name="featureContainer">The wave model feature container.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="featureContainer"/> is <c>null</c>.
        /// </exception>
        public WaveOutputDataHarvester(IWaveFeatureContainer featureContainer)
        {
            Ensure.NotNull(featureContainer, nameof(featureContainer));

            this.featureContainer = featureContainer;
        }

        public IReadOnlyList<ReadOnlyTextFileData> HarvestDiagnosticFiles(DirectoryInfo outputDataDirectory,
                                                                          ILogHandler logHandler = null)
        {
            return HarvestTextFiles(IsDiagnosticFile, outputDataDirectory, logHandler);
        }

        public IReadOnlyList<ReadOnlyTextFileData> HarvestSpectraFiles(DirectoryInfo outputDataDirectory,
                                                                       ILogHandler logHandler = null)
        {
            return HarvestTextFiles(IsSpectraFile, outputDataDirectory, logHandler);
        }
        
        public IReadOnlyList<ReadOnlyTextFileData> HarvestSwanFiles(DirectoryInfo outputDataDirectory,
                                                                    ILogHandler logHandler = null)
        {
            return HarvestTextFiles(IsSwanFile, outputDataDirectory, logHandler);
        }

        public IReadOnlyList<IWavmFileFunctionStore> HarvestWavmFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                                   ILogHandler logHandler = null)
        {
            return HarvestFiles(IsWavmFileFunctionStore, ConstructWavmFileFunctionStore, outputDataDirectory, logHandler);
        }

        public IReadOnlyList<IWavhFileFunctionStore> HarvestWavhFileFunctionStores(DirectoryInfo outputDataDirectory,
                                                                                   ILogHandler logHandler = null)
        {
            return HarvestFiles(IsWavhFileFunctionStore, ConstructWavhFileFunctionStore, outputDataDirectory, logHandler);
        }

        private static IReadOnlyList<ReadOnlyTextFileData> HarvestTextFiles(Func<FileInfo, bool> isRelevantFilePredicate,
                                                                            DirectoryInfo outputDataDirectory,
                                                                            ILogHandler logHandler)
        {
            return HarvestFiles(isRelevantFilePredicate, ReadTextFile, outputDataDirectory, logHandler);
        }

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

        private static ReadOnlyTextFileData ReadTextFile(FileInfo fileInfo)
        {
            return new ReadOnlyTextFileData(fileInfo.Name, 
                                            File.ReadAllText(fileInfo.FullName), 
                                            ReadOnlyTextFileDataType.Default);
        }

        private static WavmFileFunctionStore ConstructWavmFileFunctionStore(FileInfo fileInfo)
        {
            return new WavmFileFunctionStore(fileInfo.FullName);
        }

        private WavhFileFunctionStore ConstructWavhFileFunctionStore(FileInfo fileInfo)
        {
            return new WavhFileFunctionStore(fileInfo.FullName, featureContainer);
        }

        private static bool IsDiagnosticFile(FileInfo fileInfo)
        {
            return fileInfo.Name == WaveOutputConstants.SwanLogFileName ||
                   fileInfo.Name.StartsWith(WaveOutputConstants.SwanDiagnosticFilePrefix);
        }

        private static bool IsSpectraFile(FileInfo fileInfo)
        {
            return fileInfo.Extension == WaveOutputConstants.sp1Extension ||
                   fileInfo.Extension == WaveOutputConstants.sp2Extension;
        }
        
        private static bool IsSwanFile(FileInfo fileInfo)
        {
            return fileInfo.Name.StartsWith(WaveOutputConstants.SwanInputFilePrefix) &&
                   fileInfo.Extension == string.Empty;
        }

        private static bool IsWavmFileFunctionStore(FileInfo fileInfo)
        {
            return fileInfo.Name.StartsWith(WaveOutputConstants.MapFilePrefix) &&
                   fileInfo.Extension == WaveOutputConstants.ncExtension;
        }

        private static bool IsWavhFileFunctionStore(FileInfo fileInfo)
        {
            return fileInfo.Name.StartsWith(WaveOutputConstants.HisFilePrefix) &&
                   fileInfo.Extension == WaveOutputConstants.ncExtension;
        }
    }
}