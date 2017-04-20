using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekSaltGlobalDispersionReader : SobekReader<SobekSaltGlobalDispersion>
    {
        // This file contains some global definitions concerning the dispersion: the type of dispersion formulation, 
        // and some other global parameters.
        // 
        // GLDS op 0 ty 0 f1 1 glds
        // or
        // GLDS op 1 ty 0 f1 1 f2 2 glds
        // or
        // GLDS op 1 ty 1 ds tt 
        // TBLE .. 
        // tble glds
        // or
        // GLDS op 2 ty 0 f1 1 f3 3 f4 4 glds
        // or
        // GLDS op 3 ty 0 f1 1 f3 3 f4 4 glds
        // or
        // GLDS op 2 ty 2 ds tt DSPN id '2' nm 'modelwide' ci '-1' ty 0 f1 1 f3 3 f4 4 dspn glds
        // Where:
        // op  = option type formulation
        // 0 = option 1 (dispersion coefficient as a function of location or time)
        // 1 = option 2 (dispersion coefficient as a linear function of the concentration- gradient: f1(x,t)+f2(x,t)*dc/dx)
        // 2 = Thatcher-Harleman formula
        // 3 = empirical formula, based on Thatcher-Harleman
        // 
        // ty  = type: 
        // 0 = constant
        // 1 = f(time)
        // 2 = f(place) (only for 'model wide' definition)
        // 
        // f1 = f1(x,t) in m2/s
        // f2  = f2(x,t) in m6/kg.s
        // f3  = f3(x,t) [-]
        // f4  = f4(x,t) [-]
        // 
        // dt tt  = dispersion table as function of the time
        //          column 1 = time
        //          the other columns are a selection of f1,f2,f3,f4, dependant of the type of dispersion chosen.
        // 
        // DSPN = keyword for the description of the model wide dispersion formulation (carrier id '-1'). Only relevant when dispersion is given as function of the location (type 2). The description of the DSPN record is given in the lod-file. In this case, constants will always be used for the model wide description.
        // Note Functions f1 through f4 als defined as f(x,t); this means that they can either depend of the place (f(x)) OR the time (f(t)), but NOT both.
        // Note When defining global dispersion, f1 through f4 can be either a constant or a function of time. The lod-file contains descriptions of Dispersion as a function of place.

        public override IEnumerable<SobekSaltGlobalDispersion> Parse(string text)
        {
            return RegularExpression.GetMatches(@"(GLDS (?'text'.*?)glds)", text)
                .Cast<Match>()
                .Select(structureMatch => ParseGlobalDispersion(structureMatch.Value))
                .Where(definition => definition != null);
        }

        private static SobekSaltGlobalDispersion ParseGlobalDispersion(string text)
        {
            const string dispersionPattern = @"(GLDS(?'text'.*?)glds)";

            var match = RegularExpression.GetFirstMatch(dispersionPattern, text);
            if (match == null)
            {
                throw new ArgumentException(string.Format("Unable to parse global friction from {0} ", text), "text");
            }
            var sobekSaltGlobalDispersion =
                new SobekSaltGlobalDispersion
                    {
                        DispersionOptionType = (DispersionOptionType)RegularExpression.ParseFieldAsInt("op", match.Value),
                        DispersionType = (DispersionType)RegularExpression.ParseFieldAsInt("ty", match.Value),
                        F1 = RegularExpression.ParseFieldAsDouble("f1", text),
                        F2 = RegularExpression.ParseFieldAsDouble("f2", text),
                        F3 = RegularExpression.ParseFieldAsDouble("f3", text),
                        F4 = RegularExpression.ParseFieldAsDouble("f4", text)
                    };

            if (sobekSaltGlobalDispersion.DispersionType == DispersionType.FunctionOfPlace)
            {
                // DSPN records in LOKDISP.DAT/DEFDIS.2 file.
                // Default DSPN record as part of GLDS; this record is optional and completely duplicates the normal F1 .. F4 fields
                const string localDispersionPattern = @"(DSPN(?'text'.*?)dspn)";
                var localMatches = RegularExpression.GetMatches(localDispersionPattern, text);
                if (localMatches.Count > 0)
                {
                    sobekSaltGlobalDispersion.SobekSaltLocalDispersion = SobekSaltLocalDispersionReader.GetLocalDispersion(localMatches[0].Value);
                }
            }
            return sobekSaltGlobalDispersion;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "glds";
        }
    }
}
