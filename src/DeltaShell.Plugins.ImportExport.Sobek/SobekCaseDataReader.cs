using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Reader for the casedesc.cmt file.
    /// </summary>
    public static class SobekCaseDataReader
    {
        private static readonly string[] fileTypes = {
            "O",
            "I",
            "OO",
            "OI",
            "IO",
            "OIO"
        };
        
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
                throw new InvalidOperationException($"{nameof(stream)} does not support reading.");
            }

            IEnumerable<string> data = ReadData(stream);
            IEnumerable<string> relativeFilePaths = CollectFilePaths(data);

            string referenceDirectory = Path.GetDirectoryName(rootFilePath);
            IEnumerable<string> absoluteFilePaths = relativeFilePaths.Select(r => GetFullPath(referenceDirectory, r));
            
            return new SobekCaseData(absoluteFilePaths);
        }

        private static IEnumerable<string> CollectFilePaths(IEnumerable<string> data)
        {
            var filePaths = new HashSet<string>();
            foreach (string line in data)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                string[] split = line.SplitOnEmptySpace();
                string fileType = split[0];
                string filePath = split[1];
                
                if (!fileTypes.Contains(fileType) || filePath.Contains("#"))
                {
                    continue;
                }

                filePath = ResolveFilePath(filePath);
                filePaths.Add(filePath);
            }

            return filePaths;
        }

        private static string GetFullPath(string directory, string r)
        {
            return Path.GetFullPath(Path.Combine(directory, r));
        }

        private static string ResolveFilePath(string filePath)
        {
            if (!filePath.StartsWith(@"\"))
            {
                return filePath;
            }

            string directory = filePath.Split(new[]
            {
                '\\'
            }, StringSplitOptions.RemoveEmptyEntries)[0];

            // Not all file paths in the case description file are relative to the case description file.
            // We assume here that file paths starting with a `\` are two directories higher,
            // so that we can make it relative to the case description file.
            // A path starting with `\` is often the work directory of a full SOBEK2 installation (with many models).
            const string rootDirectoryReplacement = @"..\..";

            filePath = filePath.Replace(@"\" + directory, rootDirectoryReplacement);

            return filePath;
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
    }
}