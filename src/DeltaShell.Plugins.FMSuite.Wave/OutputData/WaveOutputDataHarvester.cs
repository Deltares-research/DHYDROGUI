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
    public class WaveOutputDataHarvester : IWaveOutputDataHarvester
    {
        private static ReadOnlyTextFileData ReadTextFile(FileInfo fileInfo) =>
            new ReadOnlyTextFileData(fileInfo.Name, File.ReadAllText(fileInfo.FullName));

        public IReadOnlyList<ReadOnlyTextFileData> HarvestDiagnosticFiles(DirectoryInfo outputDataDirectory,
                                                                          ILogHandler logHandler = null) =>
            HarvestTextFiles(IsDiagnosticFile, outputDataDirectory, logHandler);

        public IReadOnlyList<ReadOnlyTextFileData> HarvestSpectraFiles(DirectoryInfo outputDataDirectory,
                                                                       ILogHandler logHandler = null) =>
            HarvestTextFiles(IsSpectraFile, outputDataDirectory, logHandler);

        public IReadOnlyList<WavmFileFunctionStore> HarvestWavmFileFunctionStores(DirectoryInfo outputDataDirectory, ILogHandler logHandler = null)
        {
            Ensure.NotNull(outputDataDirectory, nameof(outputDataDirectory));

            var result = new List<WavmFileFunctionStore>();

            foreach (FileInfo fileInfo in outputDataDirectory.EnumerateFiles()
                                                             .Where(IsWavmFileFunctionStore))
            {
                // TODO: see if we can cache this somehow.
                // could consider calculating a hash value here, and use that to determine whether we 
                // can reuse the existing nc file or create a new one.
                try
                {
                    result.Add(new WavmFileFunctionStore(fileInfo.FullName));
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

        private static IReadOnlyList<ReadOnlyTextFileData> HarvestTextFiles(Func<FileInfo, bool> isRelevantFilePredicate,
                                                                            DirectoryInfo outputDataDirectory,
                                                                            ILogHandler logHandler)
        {
            Ensure.NotNull(outputDataDirectory, nameof(outputDataDirectory));

            var result = new List<ReadOnlyTextFileData>();

            foreach (FileInfo fileInfo in outputDataDirectory.EnumerateFiles()
                                                             .Where(isRelevantFilePredicate))
            {
                try
                {
                    result.Add(ReadTextFile(fileInfo));
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

        private static bool IsDiagnosticFile(FileInfo fileInfo) =>
            fileInfo.Name == WaveOutputConstants.SwanLogFileName ||
            fileInfo.Name == WaveOutputConstants.SwanDiagnosticFileName;


        private static bool IsSpectraFile(FileInfo fileInfo) =>
            fileInfo.Extension == WaveOutputConstants.sp1Extension ||
            fileInfo.Extension == WaveOutputConstants.sp2Extension;

        private static bool IsWavmFileFunctionStore(FileInfo fileInfo) =>
            fileInfo.Name.StartsWith(WaveOutputConstants.MapFilePrefix) &&
            fileInfo.Extension == WaveOutputConstants.ncExtension;
    }
}