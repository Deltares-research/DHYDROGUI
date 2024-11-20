using System;
using System.IO;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Writers
{
    public static class Delft3DDepthFileWriter
    {
        private const double MissingValue = -999.0;
        private const int MaxNrOfEntriesInRow = 12;

        public static void Write(double[] depthValues, int sizeN, int sizeM, string targetFile)
        {
            if (depthValues.Length != sizeN * sizeM)
            {
                throw new NotSupportedException("nr. of depth values should match grid size");
            }

            using (var writer = new StreamWriter(targetFile))
            {
                using (CultureUtils.SwitchToInvariantCulture())
                {
                    var index = 0;
                    const string spacing = "  ";
                    for (var n = 0; n < sizeN; ++n)
                    {
                        for (var m = 0; m < sizeM; ++m)
                        {
                            if (m != 0)
                            {
                                writer.Write(m % MaxNrOfEntriesInRow == 0 ? Environment.NewLine : spacing);
                            }

                            writer.Write("{0,15:F8}", depthValues[index++]);
                        }

                        // write -999 column:
                        writer.Write(string.Format(spacing + "{0,15:F8}", MissingValue) + Environment.NewLine);
                    }

                    // write -999 row:
                    for (var m = 0; m < sizeM + 1; ++m)
                    {
                        if (m != 0)
                        {
                            writer.Write(m % MaxNrOfEntriesInRow == 0 ? Environment.NewLine : spacing);
                        }

                        writer.Write("{0,15:F8}", MissingValue);
                    }
                }
            }
        }
    }
}