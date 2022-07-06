using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRStorageReader : SobekReader<SobekRRStorage>
    {
        public override IEnumerable<SobekRRStorage> Parse(string fileContent)
        {
            const string pattern = @"STDF (?'text'.*?)stdf" + RegularExpression.EndOfLine;

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRStorage(line.Value)).ToList();
        }

        private static SobekRRStorage GetSobekRRStorage(string line)
        {
            var sobekRRStorage = new SobekRRStorage();

            // general info
            //id   =          storage identification
            //nm   =          name (optional)

            // for unpaved data:
            //ml   =          maximum storage on land (mm). Default 1 mm.
            //il   =          initial storage on land (mm). Default 0.

            // for paved data:
            //ms   =          maximum storage on streets (mm). Default 1 mm.
            //is   =          initial storage on streets (mm). Default 0.
            //mr   =          maximum storage sewer (mm). Default 7 mm.    
            //ir   =          initial storage in sewer (mm). Default 0.
            //                For mr and ir different sewer systems are distinghuished (mixed systems, separated systems, improved separated system).
            //                The first value is for mixed and rainfall sewer, the second value for DWA sewer.

            // for greenhouse data"
            //mk   =          maximum storage on roofs (in mm). Default 1 mm. Initial value is zero by default.
            //ik   =          initial storage on roofs (in mm). Default 0. NOT READ. Default value zero used.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.Id = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.Name = matches[0].Groups[label].Value;
            }

            //Maximum land storage
            label = "ml";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.MaxLandStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial land storage
            label = "il";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.InitialLandStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Maximum street storage
            label = "ms";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.MaxStreetStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial street storage
            label = "is";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.InitialStreetStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Maximum sewer storage
            pattern = @"mr\s*(?<pattern1>" + RegularExpression.Scientific + @")\s*(?<pattern2>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.MaxSewerStorageMixedRainfall = Convert.ToDouble(
                    matches[0].Groups["pattern1"].Value, CultureInfo.InvariantCulture);
                sobekRRStorage.MaxSewerStorageDWA = Convert.ToDouble(
                    matches[0].Groups["pattern2"].Value, CultureInfo.InvariantCulture);
            }

            //Initial sewer storage
            pattern = @"ir\s*(?<pattern1>" + RegularExpression.Scientific + @")\s*(?<pattern2>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.InitialSewerStorageMixedRainfall = Convert.ToDouble(
                    matches[0].Groups["pattern1"].Value, CultureInfo.InvariantCulture);
                sobekRRStorage.InitialSewerStorageDWA = Convert.ToDouble(
                    matches[0].Groups["pattern2"].Value, CultureInfo.InvariantCulture);
            }

            // maximum roof storage (mm)
            label = "mk";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.MaxRoofStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            // initial roof storage (mm)
            label = "ik";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRStorage.InitialRoofStorage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekRRStorage;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stdf";
        }
    }
}
