using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRUnpavedReader : SobekReader<SobekRRUnpaved>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRUnpavedReader));

        public override IEnumerable<SobekRRUnpaved> Parse(string fileContent)
        {
            const string unpavedPattern = @"UNPV\s+?" + IdAndOptionalNamePattern + @"(?'text'.*?)unpv" + RegularExpression.EndOfLine;

                return (from Match unpavedLine in RegularExpression.GetMatches(unpavedPattern, fileContent)
                        select GetSobekUnpaved(unpavedLine.Value)).ToList();
        }

        private static SobekRRUnpaved GetSobekUnpaved(string line)
        {
            //id   =          node identification
            //na   =          number of areas (at the moment fixed at 16)
            //ar   =          area (in m2)  for all crops.  In the user interface either the total area can be specified, or the different areas per crop. In case the total area is specified, it is put at the first crop (grass).
            //ga   =          area for groundwater computations. Default = sum of crop areas. 
            //lv   =          surface level (=ground level) in m NAP 
            //co   =          computation option (1=Hellinga de Zeeuw (default), 2=Krayenhoff van de Leur, 3=Ernst)
            //rc   =          reservoir coefficient (for Krayenhoff van de Leur only); 
            //su   =          Indicator Scurve used 
            //                  su 0 = No Scurve used (Default)
            //                  su 1 ‘Scurve-id’ = Scurve used; Unpaved.Tbl contains  defniition of table with id ‘Scurve-id’.
            //sd   =          storage identification
            //ad   =          alfa-level identification (for Hellinga de Zeeuw drainage formula only)
            //ed   =          Ernst definition (for Ernst drainage formula only)
            //sp   =          seepage identification.
            //ic   =          infiltration identification
            //bt   =          soil type (from file BERGCOEF or BergCoef.Cap)
            //                  Indices >100 are from Bergcoef.Cap.
            //ig   =          initial groundwater level; constant, or as a table
            //                  ig 0 0.2 = initial groundwaterlevel as a constant, with value 0.2 m below the surface.
            //                  ig 1 'igtable1' = initial groundwater level as a table, with table identification igtable1. 
            //mg   =          maximum allowed groundwater level (in m NAP)
            //gl   =          initial depth of groundwater layer in meters (for salt computations)                      
            //ms   =          identification of the meteostation
            //is   =          initial salt concentration (mg/l) Default 100 mg/l


            var sobekUnpaved = new SobekRRUnpaved();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.Id = matches[0].Groups[label].Value;
            }

            //CropAreas
            label = "ar";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(label + @"\s*(?<" + label + ">");
            for (int i = 0; i < 16; i++ )
            {
                stringBuilder.Append(RegularExpression.Scientific + @"\s*");
            }
            stringBuilder.Append(")");
            pattern = stringBuilder.ToString();
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.CropAreas = ConvertToCropAreas(matches[0].Groups[label].Value);
            }

            //Groundwater Area
            label = "ga";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.GroundWaterArea = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Surface Level
            label = "lv";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.SurfaceLevel = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Computation Option
            label = "co";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var coAsInt = Convert.ToInt32((string) matches[0].Groups[label].Value);
                if (Enum.IsDefined(typeof(SobekUnpavedComputationOption), coAsInt))
                {
                    sobekUnpaved.ComputationOption = (SobekUnpavedComputationOption)coAsInt;
                }
                else
                {
                    log.ErrorFormat("Computation option of {0} is unkown.",sobekUnpaved.Id);
                }
            }

            //Reservoir Coefficient
            label = "rc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.ReservoirCoefficient = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Scurve Used
            label = "su";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.ScurveUsed = (Convert.ToInt32( matches[0].Groups[label].Value) == 1);
            }

            //Scurve table name
            if (sobekUnpaved.ScurveUsed)
            {
                pattern = @"su\s*1\s*'(?<tableName>" + RegularExpression.ExtendedCharacters + @")'";
                matches = RegularExpression.GetMatches(pattern, line);
                if (matches.Count == 1)
                {
                    sobekUnpaved.ScurveTableName = matches[0].Groups["tableName"].Value;
                }
            }

            //Storage Identification
            label = "sd";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.StorageId = matches[0].Groups[label].Value;
            }

            //Alfa Level Id
            label = "ad";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.AlfaLevelId = matches[0].Groups[label].Value;
            }

            //ErnstId
            label = "ed";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.ErnstId = matches[0].Groups[label].Value;
            }

            //SeepageId
            label = "sp";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.SeepageId = matches[0].Groups[label].Value;
            }


            //Infiltration Id
            label = "ic";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.InfiltrationId = matches[0].Groups[label].Value;
            }

            //SoilType
            label = "bt";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.SoilType = Convert.ToInt32((string) matches[0].Groups[label].Value);
            }

            //Initial Groundwater Level Constant
            pattern = "ig\\s0\\s*(?<value>" + RegularExpression.Scientific + ")?\\s";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var initialGroundwaterLevelConstant = Convert.ToDouble(matches[0].Groups["value"].Value, CultureInfo.InvariantCulture);

                if (Math.Abs(initialGroundwaterLevelConstant - -999.99) < 0.001)
                {
                    //seems to be the implicit agreement
                    sobekUnpaved.InitialGroundwaterLevelFromBoundary = true;
                }
                else
                {
                    sobekUnpaved.InitialGroundwaterLevelConstant = initialGroundwaterLevelConstant;    
                }
            }

            //Initial Groundwater Level Table
            pattern = "ig\\s1\\s*'(?<tableId>" + RegularExpression.ExtendedCharacters + ")?'\\s";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.InitialGroundwaterLevelTableId = matches[0].Groups["tableId"].Value;
            }

            //Maximum Groundwater Level
            label = "mg";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.MaximumGroundwaterLevel = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial Depth Groundwater Layer
            label = "gl";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.InitialDepthGroundwaterLayer = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //MeteoStation Id
            label = "ms";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.MeteoStationId = matches[0].Groups[label].Value;
            }

            //Area Ajustment Factor
            label = "aaf";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.AreaAjustmentFactor = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial Salt Concentration
            label = "is";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekUnpaved.InitialSaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekUnpaved;
        }

        private static double[] ConvertToCropAreas(string values)
        {
            var lstValues = new List<double>();

            var valuesArray = values.SplitOnEmptySpace();
            foreach (var value in valuesArray)
            {
                lstValues.Add(Convert.ToDouble(value, CultureInfo.InvariantCulture));
            }

            return lstValues.ToArray();
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "unpv";
        }
    }
}
