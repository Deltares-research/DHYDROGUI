using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class DelftIniFileParser
    {
        public static IList<DelftIniCategory> ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileReadingException($"Could not read file {filePath} properly, it doesn't exist.");

            var categories = new DelftIniReader().ReadDelftIniFile(filePath);

            if (categories.Count == 0)
                throw new FileReadingException($"Could not read file {filePath} properly, it seems empty");

            return categories;
        }
    }
}
