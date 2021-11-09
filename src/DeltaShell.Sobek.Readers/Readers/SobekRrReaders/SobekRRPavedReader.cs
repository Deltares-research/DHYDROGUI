using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Sobek.Readers.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRPavedReader : SobekReader<SobekRRPaved>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRPavedReader));

        public override IEnumerable<SobekRRPaved> Parse(string fileContent)
        {
            const string pavedPattern = @"PAVE (?'text'.*?)pave" + RegularExpression.EndOfLine;

                return (from Match PavedLine in RegularExpression.GetMatches(pavedPattern, fileContent)
                        select GetSobekPaved(PavedLine.Value)).ToList();
        }

        private static SobekRRPaved GetSobekPaved(string line)
        {
            //id   =          node identification
            //ar   =          area (in m2)  
            //lv   =          street level (m NAP) 
            //sd   =          storage identification
            //ss   =          sewer system type (0=mixed, 1=separated,  2=improved separated)
            //qc   =          capacity of sewer pump (m3/s)
            //                qc 0 0.2 0.0 = capacity as a constant, with value 0.2 (mixed/rainfall sewer) and 0.0 (DWA in separated or improved separated systems). So, first value is for mixed/rainfall sewer, second value for the dry weather flow (DWA) sewer.
            //                qc 1 'qctable1' = capacity as a table, with table identification qctable1. 
            //qo   =          1 1       =          both sewer pumps discharge to open water (=default)
            //                0 0       =          both sewer pumps discharge to boundary
            //                0 1       =          rainfall or mixed part of the sewer pumps to open water, 
            //                                     DWA-part (if separated) to boundary
            //                1 0       =          rainfall or mixed part of the sewer discharges to boundary, 
            //                                     DWA-part (if separated) to open water
            //                2 2       =          both sewer pumps discharge to WWTP
            //                2 1       =          rainfall or mixed part of the sewer pumps to open water, 
            //                                     DWA-part (if separated) to WWTP
            //                                     etc. 
            //                Note: first position of record is allocated to DWA sewer, second position is allocated to mixed/rainfall sewer; 0=to boundary, 1= to openwater, 2=to WWTP.  In all other keywords the order is just the other way around!!!!
            //so   =          sewer overflow level (first value for RWA/Mixed sewer, second value for DWA sewer). If missing, the surface level will be used. The level is used to verify whether sewer overflows can occur (no overflows can occur if the related boundary or open water level is higher)
            //si   =          sewer inflow from open water/boundary possible yes/no (1=yes,0=no); first value for RWA/Mixed sewer, second value for DWA sewer). Default value is 0, meaning that no external inflow is possible.
            //ms   =          identification of the meteostation by a character id
            //                If this id is missing in the rainfall file, data from the first station in the rainfall file will be used.
            //is   =          initial salt concentration (g/m3). Default 0.
            //np   =          number of people
            //dw   =          dry weather flow identification 
            //ro   =          runoff option
            //                0 = default,  no delay (=previous situation) 
            //                1 = using runoff delay factor (as in NWRW model)
            //                2 = using Qh relation  (not yet implemented)
            //     ru   =     runoff delay factor in (1/min) (as in NWRW model)  
            //                (only needed and used if option ro 1 is specified)
            //     qh   =     reference to Qh-relation 
            //                (only needed and used if option ro 2 is specified)

            var sobekPaved = new SobekRRPaved();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.Id = matches[0].Groups[label].Value;
            }

            //Areas
            label = "ar";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.Area = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Surface Level
            label = "lv";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.StreetLevel = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Storage Identification
            label = "sd";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.StorageId = matches[0].Groups[label].Value;
            }

            //Sewer System
            label = "ss";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var ssAsInt = Convert.ToInt32(matches[0].Groups[label].Value);
                if (Enum.IsDefined(typeof(SewerSystemType), ssAsInt))
                {
                    sobekPaved.SewerSystem = (SewerSystemType)ssAsInt;
                }
                else
                {
                    log.ErrorFormat("Sewer system of {0} is unkown.", sobekPaved.Id);
                }
            }

            //Capacity Sewer Constant
            pattern = @"qc\s*0\s*(?<c1>" + RegularExpression.Scientific + @")\s*(?<c2>" + RegularExpression.Scientific +  @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.CapacitySewerConstantRainfallInM3S = Convert.ToDouble(matches[0].Groups["c1"].Value, CultureInfo.InvariantCulture);
                sobekPaved.CapacitySewerConstantDWAInM3S = Convert.ToDouble(matches[0].Groups["c2"].Value, CultureInfo.InvariantCulture);
            }

            //Capacity Sewer Table
            pattern = @"qc\s*1\s*'(?<tableName>" + RegularExpression.ExtendedCharacters + @")'";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.CapacitySewerTableId = matches[0].Groups["tableName"].Value;
            }

            //Sewer discharge type
            pattern = @"qo\s*(?<pattern>" + RegularExpression.Integer + @"\s+" + RegularExpression.Integer + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                string[] parts = matches[0].Groups["pattern"].Value.SplitOnEmptySpace();

                if (!TryGetSewerDischargeType(parts[0], out SewerDischargeType dryWeatherFlowDischargeTarget))
                {
                    log.WarnFormat(Resources.SobekRRPavedReader_Warning_UnsupportedDischargeTarget, "dry weather flow", sobekPaved.Id);
                }

                sobekPaved.DryWeatherFlowSewerPumpDischarge = dryWeatherFlowDischargeTarget;

                if (!TryGetSewerDischargeType(parts[1], out SewerDischargeType mixedRainfallDischargeTarget))
                {
                    log.WarnFormat(Resources.SobekRRPavedReader_Warning_UnsupportedDischargeTarget, "mixed/rainfall", sobekPaved.Id);
                }

                sobekPaved.MixedAndOrRainfallSewerPumpDischarge = mixedRainfallDischargeTarget;
            }

            //Sewer overflow levels
            pattern = @"so\s*(?<pattern1>" + RegularExpression.Scientific + @")\s*(?<pattern2>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.SewerOverflowLevelRWAMixed = Convert.ToDouble(matches[0].Groups["pattern1"].Value, CultureInfo.InvariantCulture);
                sobekPaved.SewerOverFlowLevelDWA = Convert.ToDouble(matches[0].Groups["pattern2"].Value, CultureInfo.InvariantCulture);
            }

            //Sewer inflow possible?
            pattern = @"si\s*(?<pattern1>" + RegularExpression.Integer + @")\s*(?<pattern2>" + RegularExpression.Integer + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.SewerInflowRWAMixed =
                    Convert.ToInt32(matches[0].Groups["pattern1"].Value, CultureInfo.InvariantCulture) == 1;
                sobekPaved.SewerInflowDWA =
                    Convert.ToInt32(matches[0].Groups["pattern2"].Value, CultureInfo.InvariantCulture) == 1;
            }

            //Meteo station
            label = "ms";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.MeteoStationId = matches[0].Groups[label].Value;
            }

            //Initial Salt Concentration
            label = "aaf";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.AreaAjustmentFactor = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial Salt Concentration
            label = "is";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.InitialSaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Number of people
            label = "np";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.NumberOfPeople = Convert.ToInt32((string) matches[0].Groups[label].Value);
            }

            //Dry Weather Flow Identification
            label = "dw";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.DryWeatherFlowId = matches[0].Groups[label].Value;
            }
            
            //Runoff option
            label = "ro";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var roAsInt = Convert.ToInt32(matches[0].Groups[label].Value);
                if (Enum.IsDefined(typeof(SpillingOption), roAsInt))
                {
                    sobekPaved.SpillingOption = (SpillingOption)roAsInt;
                }
                else
                {
                    log.ErrorFormat("Runoff option of {0} is unkown.", sobekPaved.Id);
                }
            }

            //Runoff delay factor
            label = "ru";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.SpillingRunoffCoefficient = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //QH Table
            label = "qh";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekPaved.QHTableId = matches[0].Groups[label].Value;
            }

            return sobekPaved;
        }

        private static bool TryGetSewerDischargeType(string str, out SewerDischargeType sewerDischargeType)
        {
            switch (str)
            {
                case "0":
                    sewerDischargeType = SewerDischargeType.BoundaryNode;
                    return true;
                case "2":
                    sewerDischargeType = SewerDischargeType.WWTP;
                    return true;
                default:
                    sewerDischargeType = SewerDischargeType.BoundaryNode;
                    return false;
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "pave";
        }
    }
}
