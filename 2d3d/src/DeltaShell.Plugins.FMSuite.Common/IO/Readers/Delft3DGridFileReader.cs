using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    public static class Delft3DGridFileReader
    {
        public static CurvilinearGrid Read(string path)
        {
            var dryCellValue = 0.0;
            var coordinateSystem = "Cartesian";

            var mSize = 0;
            var nSize = 0;
            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            using (var reader = new StreamReader(path))
            {
                CheckIfFileIsEmpty(reader);

                var gridSizeLineRead = false;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string currentLine = line.Trim();

                    if (string.IsNullOrEmpty(currentLine) || currentLine.StartsWith("*"))
                    {
                        continue;
                    }

                    if (currentLine.StartsWith("Coordinate"))
                    {
                        string[] fields = currentLine.Split('=');
                        if (fields.Length == 2)
                        {
                            coordinateSystem = fields[1].Trim();
                        }

                        continue;
                    }

                    if (currentLine.StartsWith("Missing"))
                    {
                        string[] fields = currentLine.Split('=');
                        if (fields.Length == 2)
                        {
                            dryCellValue = double.Parse(fields[1], CultureInfo.InvariantCulture);
                        }

                        continue;
                    }

                    if (!gridSizeLineRead)
                    {
                        IEnumerable<string> lineValues =
                            currentLine.Split(' ').Select(l => l.Trim()).Where(v => v.Length > 0);
                        string[] gridSizeString = lineValues.ToArray();
                        if (gridSizeString.Length != 2)
                        {
                            throw new Exception("Unknown file format");
                        }

                        mSize = int.Parse(gridSizeString.ElementAt(0), CultureInfo.InvariantCulture);
                        nSize = int.Parse(gridSizeString.ElementAt(1), CultureInfo.InvariantCulture);

                        gridSizeLineRead = true;

                        // skip the next line
                        if (reader.ReadLine() == null)
                        {
                            break;
                        }

                        continue;
                    }

                    if (currentLine.StartsWith("ETA"))
                    {
                        currentLine = currentLine.Substring(11);
                    }

                    IEnumerable<string> doubleValues =
                        currentLine.Split(' ').Select(l => l.Trim()).Where(v => v.Length > 0);

                    if (xCoordinates.Count < mSize * nSize)
                    {
                        xCoordinates.AddRange(doubleValues.Select(v => double.Parse(v, CultureInfo.InvariantCulture)));
                        continue;
                    }

                    yCoordinates.AddRange(doubleValues.Select(v => double.Parse(v, CultureInfo.InvariantCulture)));
                }

                SetDryCellPoints(xCoordinates, dryCellValue, yCoordinates);
            }

            // [nSize,mSize] is the shape of the data, following our convention
            // in multidimensionalarray: [rows,columns]
            var grid = new CurvilinearGrid(nSize, mSize, xCoordinates, yCoordinates, coordinateSystem) {IsTimeDependent = false};

            return grid;
        }

        private static void SetDryCellPoints(List<double> xCoordinates, double dryCellValue, List<double> yCoordinates)
        {
            for (var i = 0; i < xCoordinates.Count; ++i)
            {
                if (xCoordinates[i] == dryCellValue &&
                    yCoordinates[i] == dryCellValue)
                {
                    xCoordinates[i] = double.NaN;
                    yCoordinates[i] = double.NaN;
                }
            }
        }

        private static void CheckIfFileIsEmpty(StreamReader reader)
        {
            if (reader.EndOfStream)
            {
                throw new FormatException(string.Format("File (" + reader + ") appears to be empty"));
            }
        }
    }
}