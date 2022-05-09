using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class DiaFileReader
    {
        public static string Read(string diaFilePath)
        {
            var diaFileContent = string.Empty;
            using (var fileStream = new FileStream(diaFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                diaFileContent = textReader.ReadToEnd();
            }

            return diaFileContent;
        }
        public static List<string> CollectAllErrorMessages(string diaFile)
        {
            var errorLine = string.Empty;
            var errorMessages = new List<string>();

            using (var reader = new StreamReader(diaFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (!string.IsNullOrEmpty(line) && ((line.Contains("ERROR")) || !string.IsNullOrEmpty(errorLine)))
                    {
                        if (!line.Contains("FATAL"))
                        {
                            if (line.StartsWith(""))
                            {
                                line = line.Substring(1);
                            }
                            errorLine += line;
                        }
                        else
                        {
                            errorMessages.Add(errorLine);
                        }
                    }
                }
                reader.Close();
            }

            return errorMessages;
        }
    }
}
