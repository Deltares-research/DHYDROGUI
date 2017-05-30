using System;
using System.IO;
using DelftTools.Utils.RegularExpressions;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public static class WaveModelFileHelper
    {
        public static string ImportIntoModelDirectory(string modelDir, string absolutePath)
        {
            var fileName = Path.GetFileName(absolutePath);
            var uniqueFileName = GetUniqueTargetFileName(modelDir, fileName);
            var targetFile = Path.Combine(modelDir, uniqueFileName);
            File.Copy(absolutePath, targetFile);

            return uniqueFileName;
        }

        private const string NameBasePattern = @"((?<base>.+)_(?<suffix>\d+))";
        private static string GetUniqueTargetFileName(string targetDirectory, string fileName)
        {
            while (File.Exists(Path.Combine(targetDirectory, fileName)))
            {
                var nameBase = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var match = RegularExpression.GetFirstMatch(NameBasePattern, nameBase);

                if (match != null && match.Success)
                {
                    // increment
                    var newSuffix = int.Parse(match.Groups["suffix"].Value) + 1; // always >0
                    int suffixSize = Math.Max((int)Math.Log10(newSuffix) + 1, match.Groups["suffix"].Value.Length);
                    var suffixFormat = new string('0', suffixSize);
                    fileName = match.Groups["base"] + "_" + newSuffix.ToString(suffixFormat) + extension;
                    continue;
                }

                // append
                fileName = nameBase + "_001" + extension;
            }

            return fileName;
        }
    }
}
