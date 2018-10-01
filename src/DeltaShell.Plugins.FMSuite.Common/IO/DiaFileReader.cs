using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class DiaFileReader
    {
        public static List<string> CollectAllErrorMessages(string diaFile)
        {
            var errorLine = string.Empty;
            var lineCount = 0;
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
