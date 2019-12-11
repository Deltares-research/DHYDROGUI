using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
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
            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.NWRW,
                /*Name = nwrwData.Name,
                Description = nwrwData.LongName*/
            };
            var data = new NwrwData(catchment);

            var values = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < values.Length; i++)
            {
                switch (values[i].ToLower())
                {
                    case NwrwRegion.NwrwKey:
                        break;
                    case NwrwRegion.IdKey:
                        data.Name = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwRegion.SurfaceLevelKey:
                    //    data.SurfaceLevel = ParseDouble(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwRegion.AreaKey:
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
                    case NwrwRegion.NumberOfPeopleKey:
                        data.NumberOfPeople = ParseInt(values[++i], LineNumber, parseErrors);
                        break;
                    case NwrwRegion.DryWeatherFlowIdKey:
                        data.DryWeatherFlowId = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwRegion.MeteostationIdKey:
                        data.MeteoStationName = ParseString(values[++i], LineNumber);
                        break;
                    case NwrwRegion.NumberOfSpecialAreasKey:
                        data.NumberOfSpecialAreas = ParseInt(values[++i], LineNumber, parseErrors);
                        data.SpecialAreas = new List<NwrwSpecialArea>();
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas.Add(new NwrwSpecialArea());
                        }
                        break;
                    case NwrwRegion.SpecialAreaKey:
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas[j].Area = ParseInt(values[++i], LineNumber, parseErrors);
                        }
                        break;
                    case NwrwRegion.SpecialInflowReferenceKey:
                        for (int j = 0; j < data.NumberOfSpecialAreas; j++)
                        {
                            data.SpecialAreas[j].SpecialInflowReference = ParseString(values[++i], LineNumber);
                        }
                        break;
                    case NwrwRegion.AreaAdjustmentFactorKey:
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

        private bool IsValidNwrwLine(string line)
        {
            return line.StartsWith("nwrw", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
