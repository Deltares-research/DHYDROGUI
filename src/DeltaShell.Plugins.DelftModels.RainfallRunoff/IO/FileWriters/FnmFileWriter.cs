using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters
{
    /// <summary>
    /// Writer for the <see cref="FnmFile"/>.
    /// </summary>
    public sealed class FnmFileWriter
    {
        /// <summary>
        /// Writes the provided fnm file.
        /// </summary>
        /// <param name="fnmFile"> The fnm file. </param>
        /// <param name="textWriter"> The text writer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fnmFile"/> or <paramref name="textWriter"/> is <c>null</c>.
        /// </exception>
        public void Write(FnmFile fnmFile, TextWriter textWriter)
        {
            Ensure.NotNull(fnmFile, nameof(fnmFile));
            Ensure.NotNull(textWriter, nameof(textWriter));

            foreach (string line in GetFnmFileLines(fnmFile))
            {
                textWriter.WriteLine(line);
            }
        }

        private static IEnumerable<string> GetFnmFileLines(FnmFile fnmFile)
        {
            IList<string> lines = GetHeaderLines();

            int maxFileNameLength = GetMaxFileName(fnmFile);
            int maxIndexLength = GetMaxIndex(fnmFile);
            int maxDescriptionLength = GetMaxDescription(fnmFile);

            foreach (FnmSubFile file in fnmFile.SubFiles)
            {
                string fileName = GetFileName(file).PadRight(maxFileNameLength);
                string index = GetIndex(file).PadLeft(maxIndexLength);
                string description = file.Description.PadRight(maxDescriptionLength);
                string fileType = GetFileType(file.FileType);

                lines.Add($"{fileName} * {index}. {description} {fileType}");
            }

            return lines;
        }

        private static IList<string> GetHeaderLines()
        {
            return new List<string>
            {
                "*",
                "* DELFT_3B Version 1.00",
                "* -----------------------------------------------------------------",
                "*",
                "* Last update : March 1995",
                "*",
                "* All input- and output file names (free format)",
                "*",
                "*   Namen Mappix files (*.DIR, *.TST, *.his) mogen NIET gewijzigd worden.",
                "*   Overige filenamen mogen wel gewijzigd worden.",
                "*",
                "*"
            };
        }

        private static int GetMaxDescription(FnmFile file) => file.SubFiles.Select(f => f.Description.Length).Max();

        private static int GetMaxIndex(FnmFile file) => file.SubFiles.Select(f => GetIndex(f).Length).Max();

        private static int GetMaxFileName(FnmFile file) => file.SubFiles.Select(f => GetFileName(f).Length).Max();

        private static string GetIndex(FnmSubFile file) => file.Index.ToString();

        private static string GetFileName(FnmSubFile file) => $"'{file.FileName}'";

        private static string GetFileType(FnmSubFileType fileType)
        {
            switch (fileType)
            {
                case FnmSubFileType.Input:
                    return "I";
                case FnmSubFileType.Output:
                    return "O";
                case FnmSubFileType.Undefined:
                    return string.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }
    }
}