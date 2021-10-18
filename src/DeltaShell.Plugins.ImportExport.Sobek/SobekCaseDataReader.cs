using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Reader for the casedesc.cmt file.
    /// </summary>
    public static class SobekCaseDataReader
    {
        /// <summary>
        /// Reads the <see cref="SobekCaseData"/> from the specified <paramref name="stream"/>
        /// </summary>
        /// <param name="stream"> The stream. </param>
        /// <param name="rootFilePath"> The root file path used with the relative paths. </param>
        /// <returns>
        /// The <see cref="SobekCaseData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> or <paramref name="rootFilePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="stream"/> does not support reading.
        /// </exception>
        public static SobekCaseData Read(Stream stream, string rootFilePath)
        {
            Ensure.NotNull(stream, nameof(stream));
            Ensure.NotNull(rootFilePath, nameof(rootFilePath));

            if (!stream.CanRead)
            {
                throw new InvalidOperationException("The current file stream does not support reading.");
            }

            string[] lines = ReadData(stream).ToArray();

            var caseData = new SobekCaseData();

            string windFilePathLine = lines.FirstOrDefault(l => l.ContainsCaseInsensitive(".wdc") || l.ContainsCaseInsensitive(".wnd"));
            caseData.WindDataPath = GetAbsolutePath(windFilePathLine, rootFilePath);

            string buiFilePathLine = lines.FirstOrDefault(l => l.ContainsCaseInsensitive(".bui"));
            caseData.BuiDataPath = GetAbsolutePath(buiFilePathLine, rootFilePath);

            return caseData;
        }

        private static IEnumerable<string> ReadData(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static string GetAbsolutePath(string line, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            string caseDir = Path.GetDirectoryName(rootPath);

            string fixedDir = Path.Combine(caseDir, @"..\..\FIXED\");
            if (!Directory.Exists(fixedDir))
            {
                fixedDir = Path.Combine(caseDir, @"..\FIXED\");
            }

            string relativeFilePath = line.Split(' ')[1];
            string fileName = Path.GetFileName(relativeFilePath);
            return Path.GetFullPath(Path.Combine(fixedDir, fileName));
        }
    }
}