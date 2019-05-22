using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Common.IO
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
                    var mSize = grid.Size2;
                    var nSize = grid.Size1;
                    var xCoordinates = grid.X.Values;
                    var yCoordinates = grid.Y.Values;
                    
                    writer.WriteLine("Coordinate System = " + grid.Attributes[CurvilinearGrid.CoordinateSystemKey]);
                    writer.WriteLine("{0,8} {1,7}", mSize, nSize);
                    writer.WriteLine(" 0 0 0");

                    WriteCoordinates(writer, nSize, mSize, xCoordinates);
                    WriteCoordinates(writer, nSize, mSize, yCoordinates);
                }
            }
        }

        private static void WriteCoordinates(StreamWriter writer, int nSize, int mSize, IMultiDimensionalArray<double> xCoordinates)
        {
            int index = 0;
            var offset = new string(' ', 3);
            var leftIndent = new string(' ', 9);
            for (int n = 0; n < nSize; ++n)
            {
                int columnCount = 0;

                writer.Write("ETA={0,5}", n + 1);
                for (int m = 0; m < mSize; ++m)
                {
                    var x = xCoordinates[index++];
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