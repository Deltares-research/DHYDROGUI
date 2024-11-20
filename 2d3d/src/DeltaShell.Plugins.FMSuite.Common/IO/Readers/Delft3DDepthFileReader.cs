using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    public static class Delft3DDepthFileReader
    {
        public static IEnumerable<double> Read(string path, int sizeN, int sizeM)
        {
            var lineCount = 0;

            var columnCount = 0;
            var lastRowCounter = 0;

            var values = new List<double>();

            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCount++;
                    if (line == string.Empty || line.StartsWith("*"))
                    {
                        continue;
                    }

                    List<string> doubleValues = line.Split(new[]
                    {
                        ' '
                    }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (var i = 0; i < doubleValues.Count; i++)
                    {
                        double val;
                        if (!double.TryParse(doubleValues[i], NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                        {
                            throw new Exception(
                                string.Format("Failed to parse value from \'{0}\' on line nr. {1} in file {2}",
                                              doubleValues[i], lineCount, path));
                        }

                        columnCount++;
                        if (columnCount == sizeM + 1)
                        {
                            // skip last column, often -999 though not always
                            columnCount = 0;
                            continue;
                        }

                        if (values.Count < sizeN * sizeM)
                        {
                            values.Add(val);
                        }
                        else
                        {
                            lastRowCounter++; // skip last row, often -999 though not always
                        }
                    }
                }
            }

            if (lastRowCounter != sizeM)
            {
                throw new Exception(string.Format(
                                        "Unexpected format of depth file {0}, expecting a total of {1}x{2} values (including end-of-row/column symbols)",
                                        path, sizeM + 1, sizeN + 1));
            }

            return values;
        }
    }
}