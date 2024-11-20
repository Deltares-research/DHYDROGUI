using System;
using System.IO;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public static class WaveModelFileHelper
    {
        private const string NameBasePattern = @"((?<base>.+)_(?<suffix>\d+))";

        public static string ImportIntoModelDirectory(string modelDir, string absolutePath)
        {
            string fileName = Path.GetFileName(absolutePath);
            string uniqueFileName = GetUniqueTargetFileName(modelDir, fileName);
            string targetFile = Path.Combine(modelDir, uniqueFileName);
            File.Copy(absolutePath, targetFile);

            return uniqueFileName;
        }

        private static string GetUniqueTargetFileName(string targetDirectory, string fileName)
        {
            while (File.Exists(Path.Combine(targetDirectory, fileName)))
            {
                string nameBase = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                Match match = RegularExpression.GetFirstMatch(NameBasePattern, nameBase);

                if (match != null && match.Success)
                {
                    // increment
                    int newSuffix = int.Parse(match.Groups["suffix"].Value) + 1; // always >0
                    int suffixSize = Math.Max((int) Math.Log10(newSuffix) + 1, match.Groups["suffix"].Value.Length);
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