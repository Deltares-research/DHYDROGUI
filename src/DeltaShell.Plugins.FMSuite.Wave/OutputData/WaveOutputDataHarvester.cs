using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;

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
                    logHandler?.ReportWarning($"Could not read file: {fileInfo.Name} due to {e.Message}.");
                }
            }

            return result;
        }

        private static bool IsDiagnosticFile(FileInfo fileInfo) =>
            fileInfo.Name == "swan_bat.log" ||
            fileInfo.Name == "swn-diag.Waves";


        private static bool IsSpectraFile(FileInfo fileInfo) =>
            fileInfo.Extension == ".sp1" ||
            fileInfo.Extension == ".sp2";

    }
}