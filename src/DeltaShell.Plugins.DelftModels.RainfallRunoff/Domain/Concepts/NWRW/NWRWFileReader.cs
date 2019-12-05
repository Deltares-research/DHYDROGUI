using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    public class NWRWFileReader : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NWRWFileReader));
        public IEnumerable<NWRWData> ReadNWRWFile(string nwrwFile)
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
                    
                    if (!IsValidNWRWLine(line))
                    {
                        errors.Add($"Line {LineNumber} is not a valid NWRW line.");
                    }
                    else
                    {
                        yield return ParseDataFromNWRWLine(line, LineNumber, errors);
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

        private NWRWData ParseDataFromNWRWLine(string line, int lineNumber, ICollection<string> parseErrors)
        {
            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.NWRW,
                /*Name = nwrwData.Name,
                Description = nwrwData.LongName*/
            };
            var data = new NWRWData(catchment);

            var values = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < values.Length; i++)
            {
                switch (values[i].ToLower())
                {
                    case NWRWRegion.NWRWKey:
                        break;
                    case NWRWRegion.IdKey:
                        data.Name = ParseString(values[++i], LineNumber);
                        break;
                    case NWRWRegion.SurfaceLevelKey:
                    //    data.SurfaceLevel = ParseDouble(values[++i], LineNumber, parseErrors);
                        break;
                    case NWRWRegion.AreaKey:
                    //    data.ClosedPavedWithSlope = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.ClosedPavedFlat = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.ClosedPavedFlatStretched = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.OpenPavedWithSlope = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.OpenPavedFlat = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.OpenPavedFlatStretched = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.RoofWithSlope = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.RoofFlat = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.RoofFlatStretched = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.UnpavedWithSlope = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.UnpavedFlat = ParseInt(values[++i], LineNumber, parseErrors);
                    //    data.UnpavedFlatStretched = ParseInt(values[++i], LineNumber, parseErrors);
                       break;
                    case NWRWRegion.NumberOfPeopleKey:
                        data.NumberOfPeople = ParseInt(values[++i], LineNumber, parseErrors);
                        break;
                    case NWRWRegion.DryWeatherFlowIdKey:
                        data.DryWeatherFlowId = ParseString(values[++i], LineNumber);
                        break;
                    case NWRWRegion.MeteostationIdKey:
                        data.MeteoStationName = ParseString(values[++i], LineNumber);
                        break;
                    case NWRWRegion.NumberOfSpecialAreasKey:
                        data.NumberOfSpecialAreas = ParseInt(values[++i], LineNumber, parseErrors);
                        data.SpecialAreas = new List<NWRWSpecialArea>();
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas.Add(new NWRWSpecialArea());
                        }
                        break;
                    case NWRWRegion.SpecialAreaKey:
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas[j].Area = ParseInt(values[++i], LineNumber, parseErrors);
                        }
                        break;
                    case NWRWRegion.SpecialInflowReferenceKey:
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas[j].SpecialInflowReference = ParseString(values[++i], LineNumber);
                        }
                        break;
                    case NWRWRegion.AreaAdjustmentFactorKey:
                        data.AreaAdjustmentFactor = ParseInt(values[++i], LineNumber, parseErrors);
                        i++;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return data;
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

        private bool IsValidNWRWLine(string line)
        {
            return line.StartsWith("nwrw", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
