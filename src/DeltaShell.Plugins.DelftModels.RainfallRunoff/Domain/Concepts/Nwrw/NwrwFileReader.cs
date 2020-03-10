using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
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
                if(errors.Any())
                    Log.Error($"Reading NWRW file did not end fine, we had the following errors: {Environment.NewLine}{string.Join(Environment.NewLine,errors)}");
            }
        }

        private NwrwData ParseDataFromNwrwLine(string line, int lineNumber, ICollection<string> parseErrors)
        {
            // We either need the rrModel here, or we have to restructure.
            // Add this point, we don't have to right info to create a NwrwData

            var nwrwData = new NwrwData(null);

            var values = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < values.Length; i++)
            {
                switch (values[i].ToLower())
                {
                    case NwrwKeywords.NwrwOpeningKey:
                    case NwrwKeywords.NwrwClosingKey:
                        break;
                    case NwrwKeywords.IdKey:
                        nwrwData.Name = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwKeywords.SurfaceLevelKey:
                        nwrwData.LateralSurface = ParseDouble(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwKeywords.AreaKey:
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedWithSlope] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlatStretch] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.OpenPavedWithSlope] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlat] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlatStretched] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.RoofWithSlope] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.RoofFlat] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.RoofFlatStretched] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.UnpavedWithSlope] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlat] = ParseDouble(values[++i], LineNumber, parseErrors);
                        nwrwData.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlatStretched] = ParseDouble(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwKeywords.FirstNumberOfUnitsKey:
                        //nwrwData.NumberOfInhabitants = ParseInt(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwKeywords.FirstDryWeatherFlowIdKey:
                        //nwrwData.DryWeatherFlowIdInhabitant = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwKeywords.MeteostationIdKey:
                        nwrwData.MeteoStationName = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwKeywords.NumberOfSpecialAreasKey:
                        nwrwData.NumberOfSpecialAreas = ParseInt(values[++i], LineNumber, parseErrors);
                        nwrwData.SpecialAreas = new List<NwrwSpecialArea>();
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas.Add(new NwrwSpecialArea());
                        }
                        break;
                    case NwrwKeywords.SpecialAreaKey:
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas[j].Area = ParseInt(values[++i], LineNumber, parseErrors);
                        }
                        break;
                    case NwrwKeywords.SpecialInflowReferenceKey:
                        for (int j = 0; j < nwrwData.NumberOfSpecialAreas; j++)
                        {
                            nwrwData.SpecialAreas[j].SpecialInflowReference = ParseString(values[++i], LineNumber);
                        }
                        break;
                    case NwrwKeywords.AreaAdjustmentFactorKey:
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
