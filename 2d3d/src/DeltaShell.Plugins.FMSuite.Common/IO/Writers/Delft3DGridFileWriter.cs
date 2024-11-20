using System;
using System.IO;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Writers
{
    public static class Delft3DGridFileWriter
    {
        private const double dryCellValue = 0.0;
        private const int maxNrColumnsPerLine = 5;

        public static void Write(CurvilinearGrid grid, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                using (CultureUtils.SwitchToInvariantCulture())
                {
                    int mSize = grid.Size2;
                    int nSize = grid.Size1;
                    IMultiDimensionalArray<double> xCoordinates = grid.X.Values;
                    IMultiDimensionalArray<double> yCoordinates = grid.Y.Values;

                    writer.WriteLine("Coordinate System = " + grid.Attributes[CurvilinearGrid.CoordinateSystemKey]);
                    writer.WriteLine("{0,8} {1,7}", mSize, nSize);
                    writer.WriteLine(" 0 0 0");

                    WriteCoordinates(writer, nSize, mSize, xCoordinates);
                    WriteCoordinates(writer, nSize, mSize, yCoordinates);
                }
            }
        }

        private static void WriteCoordinates(StreamWriter writer, int nSize, int mSize,
                                             IMultiDimensionalArray<double> xCoordinates)
        {
            var index = 0;
            var offset = new string(' ', 3);
            var leftIndent = new string(' ', 9);
            for (var n = 0; n < nSize; ++n)
            {
                var columnCount = 0;

                writer.Write("ETA={0,5}", n + 1);
                for (var m = 0; m < mSize; ++m)
                {
                    double x = xCoordinates[index++];
                    writer.Write(offset + "{0,23:E20}", double.IsNaN(x) ? dryCellValue : x);
                    if (++columnCount == maxNrColumnsPerLine)
                    {
                        columnCount = 0;
                        if (m < mSize - 1) // not if the last one..
                        {
                            writer.Write(Environment.NewLine);
                            writer.Write(leftIndent);
                        }
                    }
                }

                writer.Write(Environment.NewLine);
            }
        }
    }
}