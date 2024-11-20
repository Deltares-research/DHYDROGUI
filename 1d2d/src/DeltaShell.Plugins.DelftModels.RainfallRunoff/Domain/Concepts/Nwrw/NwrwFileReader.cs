using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.Utils.Extensions;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwFileReader));

        public IEnumerable<NwrwData> ReadNwrwFile(string nwrwFile)
        {
            OpenInputFile(nwrwFile);
            var errors = new List<string>();
            try
            {
                string line;

                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (!IsValidNwrwLine(line))
                    {
                        errors.Add($"Line {LineNumber} is not a valid NWRW line.");
                    }
                    else
                    {
                        yield return ParseDataFromNwrwLine(line, LineNumber, errors);
                    }
                }
            }
            finally
            {
                CloseInputFile();
                if (errors.Any())
                    Log.Error($"Reading NWRW file did not end fine, we had the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }

        private NwrwData ParseDataFromNwrwLine(string line, int lineNumber, ICollection<string> parseErrors)
        {
            // We either need the rrModel here, or we have to restructure.
            // Add this point, we don't have to right info to create a NwrwData

            var nwrwData = new NwrwData(null);

            var values = line.SplitOnEmptySpace();

            for (int i = 0; i < values.Length; i++)
            {
                switch (values[i].ToLower())
                {
                    case NwrwKeywords.Pluv_3b_NWRW:
                    case NwrwKeywords.Pluv_3b_nwrw:
                        break;
                    case NwrwKeywords.Pluv_id:
                        nwrwData.Name = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwKeywords.Pluv_3b_sl:
                        nwrwData.LateralSurface = ParseDouble(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwKeywords.Pluv_3b_ar:
                        foreach (NwrwSurfaceType nwrwSurfaceType in NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder)
                        {
                            nwrwData.SurfaceLevelDict[nwrwSurfaceType] = ParseDouble(values[++i], LineNumber, parseErrors);
                        }
                        break;
                    case NwrwKeywords.Pluv_3b_np:
                        break;
                    case NwrwKeywords.Pluv_3b_dw:
                        break;
                    case NwrwKeywords.Pluv_3b_ms:
                        nwrwData.MeteoStationName = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwKeywords.Pluv_3b_na:
                        nwrwData.NumberOfSpecialAreas = ParseInt(values[++i], LineNumber, parseErrors);
                        nwrwData.SpecialAreas = new List<NwrwSpecialArea>();
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas.Add(new NwrwSpecialArea());
                        }
                        break;
                    case NwrwKeywords.Pluv_3b_aa:
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas[j].Area = ParseInt(values[++i], LineNumber, parseErrors);
                        }
                        break;
                    case NwrwKeywords.Pluv_3b_nw:
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas[j].SpecialInflowReference = ParseString(values[++i], LineNumber);
                        }
                        break;
                    case NwrwKeywords.Pluv_3b_aaf:
                        nwrwData.AreaAdjustmentFactor = ParseInt(values[++i], LineNumber, parseErrors);
                        i++;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return nwrwData;
        }


        private double ParseDouble(string doubleString, int lineNumber, ICollection<string> parseErrors)
        {
            double value;
            if (!double.TryParse(doubleString, out value))
            {
                parseErrors.Add($"Could not parse {doubleString} on line {lineNumber}");
            }
            return value;
        }

        private Int32 ParseInt(string intString, int lineNumber, ICollection<string> parseErrors)
        {
            Int32 value;
            if (!Int32.TryParse(intString, out value))
            {
                parseErrors.Add($"Could not parse {intString} on line {lineNumber}");
            }
            return value;
        }

        private string ParseString(string stringValue, int lineNumber)
        {
            return stringValue.Replace("'", String.Empty).Replace("\"", String.Empty);
        }

        private bool IsValidNwrwLine(string line)
        {
            return line.StartsWith("nwrw", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
