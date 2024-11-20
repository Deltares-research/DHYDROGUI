using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekFrictionDatFileReader : SobekReader<SobekFriction>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekFrictionDatFileReader));

        #region Regex global static members

        static string patternGlobalFriction = @"(GLFR (?'text'.*?)glfr)";
        static string patternBedFriction = @"(BDFR (?'text'.*?) bdfr)";
        static string patternCrossSectionFriction = @"(CRFR (?'text'.*?)crfr)";
        static string patternStructureFriction = @"(STFR (?'text'.*?) stfr)";
        static string patternExtraFriction = @"(XRST (?'text'.*?) xrst)";
        static string patternsExtraFriction = RegularExpression.GetExtendedCharacters("id") +
            "|" + RegularExpression.GetExtendedCharacters("nm") + 
            "|" + RegularExpression.GetInteger("ty") + 
            "|" + @"(rt rs(?<table>" + 
            RegularExpression.CharactersAndQuote + @"))\s";
        static string patternBedFrictionData = @"id\s'(?<Id>" + RegularExpression.Characters + ")'" + 
            @"(\snm\s'(?<bedFrictionDef>" + RegularExpression.CharactersAndQuote + ")')?" + 
            @"\sci\s'(?<BrancheId>" + RegularExpression.Characters + ")'";
        static string patternMainFrictionNeg = @"mf\s*(?<FrictionType>" + RegularExpression.Integer + ")" + 
            @"\smt\s*" + @"(?<FrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionPos>" + RegularExpression.CharactersAndQuote + @"))" + 
            @"\smr\s*" + @"(?<NegFrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionNeg>" + RegularExpression.CharactersAndQuote + @"))";
        static string patternFloodPlain1Friction = @"s1\s*(?<FrictionType>" + RegularExpression.Integer + ")" + 
            @"\s(c1\s*" + @"(?<FrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionPos>" + RegularExpression.CharactersAndQuote + @"))" + 
            @"\sr1\s*" + @"(?<NegFrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionNeg>" + RegularExpression.CharactersAndQuote + @")))*";
        static string patternFloodPlain2Friction = @"s2\s*(?<FrictionType>" + RegularExpression.Integer + ")" + 
            @"\s(c2\s*" + @"(?<FrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionPos>" + RegularExpression.CharactersAndQuote + @"))" + 
            @"\sr2\s*" + @"(?<NegFrictionValueType>[A-Za-z]*)" + 
            @"(\s*(?<FrictionNeg>" + RegularExpression.CharactersAndQuote + @")))*";
        static string patternCrossSectionFrictionData = @"id\s'(?<Id>" + RegularExpression.Characters + ")'" + 
            @"\snm\s'(?<bedFrictionDef>" + RegularExpression.ExtendedCharacters + ")'" + 
            @"\scs\s'(?<CSDefenition>" + RegularExpression.Characters + ")'" + 
            @"[\s\r\n\t]*(lt ys[\s\r\n\t]*(?<yValuesSections>" + RegularExpression.CharactersAndQuote + @"))" + 
            @"[\s\r\n\t]*(ft ys[\s\r\n\t]*(?<frictionValues>" + RegularExpression.CharactersAndQuote + @"))fr ys";
        static string patternStructureFrictionData = @"id\s'(?<Id>" + RegularExpression.Characters + ")'" +
            @"\sci\s'(?<StructureId>" + RegularExpression.Characters + ")'" +
            @"\smf\s*(?<MainFrictionType>" + RegularExpression.Integer + ")" +
            @"\smt\s*" +
            @"(?<FrictionValueType>[A-Za-z]*)" +
            @"(\s*(?<FrictionPos>" + RegularExpression.CharactersAndQuote + @"))" +
            @"\ss1\s(?<FrictionFloodplain1>" + RegularExpression.Integer + ")" +
            RegularExpression.CharactersAndQuote +
            @"\ss2\s(?<FrictionFloodplain2>" + RegularExpression.Integer + ")" +
            @"\ssf\s(?<GroundLayerFrictionType>" + RegularExpression.Integer + ")" +
            @"\sst\s*" +
            @"(?<GLFrictionValueType>[A-Za-z]*)" +
            @"(\s*(?<GLFrictionPos>" + RegularExpression.CharactersAndQuote + @"))";
        static string patternMainBedFriction = @"mf\s*(?<mf>" + RegularExpression.CharactersAndQuote + @")(s1)\s?";
        static string patternS1BedFriction = @"s1\s*(?<s1>" + RegularExpression.CharactersAndQuote + @")(s2)\s?";
        static string patternS2BedFriction = @"s2\s*(?<s2>" + RegularExpression.CharactersAndQuote + @")(sf|bdfr)\s?";
        static string patternInterpolation = @"PDIN\s*(?<interpolation>" + RegularExpression.Integer + ")";

        static readonly Regex regexGlobalFriction = new Regex(patternGlobalFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexBedFriction = new Regex(patternBedFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexCrossSectionFriction = new Regex(patternCrossSectionFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexStructureFriction = new Regex(patternStructureFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexExtraFriction = new Regex(patternExtraFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexSobekExtraFriction = new Regex(patternsExtraFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexSobekBedFrictionData = new Regex(patternBedFrictionData, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexMainFriction = new Regex(patternMainBedFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexMainFrictionNeg = new Regex(patternMainFrictionNeg, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexInterpolation = new Regex(patternInterpolation, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexS1BedFriction = new Regex(patternS1BedFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexFloodPlain1Friction = new Regex(patternFloodPlain1Friction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexS2BedFriction = new Regex(patternS2BedFriction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexFloodPlain2Friction = new Regex(patternFloodPlain2Friction, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexStructureFrictionData = new Regex(patternStructureFrictionData, RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex regexCrossSectionFrictionData = new Regex(patternCrossSectionFrictionData, RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// NOTE: this reader does not implement SobekReader in standard way!! Use ReadSobekFriction() as entry function
        /// </summary>
        public SobekFriction ReadSobekFriction(string filePath)
        {
            var sobekFriction = new SobekFriction();

            foreach (var sf in Read(filePath))
            {
                if (sf.GlobalBedFrictionList.Any())
                {
                    sobekFriction.GlobalBedFrictionList.AddRange(sf.GlobalBedFrictionList);
                }
                if (sf.CrossSectionFrictionList.Any())
                {
                    sobekFriction.CrossSectionFrictionList.AddRange(sf.CrossSectionFrictionList);
                }
                if (sf.SobekBedFrictionList.Any())
                {
                    sobekFriction.SobekBedFrictionList.AddRange(sf.SobekBedFrictionList);
                }
                if (sf.SobekExtraFrictionList.Any())
                {
                    sobekFriction.SobekExtraFrictionList.AddRange(sf.SobekExtraFrictionList);
                }
                if (sf.StructureFrictionList.Any())
                {
                    sobekFriction.StructureFrictionList.AddRange(sf.StructureFrictionList);
                }
            }

            return sobekFriction;
        }

        public override IEnumerable<SobekFriction> Parse(string p)
        {
            yield return GetSobekFriction(p);
        }

        #region info
        /*
            id  = id of bed friction definition
            nm  = name of the bed friction definition (not in SOBEK Urban/Rural)
            ci  = carrier id = id of the branch
            mf  = main friction type (main = main channel)
                0 = Chezy
                1 = Manning
                2 = Strickler Kn
                3 = Strickler Ks
                4 = White-Colebrook
                5
                6
                7 = De Bos and Bijkerk
            mt  = friction in positive flow direction
            mt fq = friction=f(Q)
            mt fh = C=f(h)
            mt cp = friction as a constant or as a function of the location on the branch
            For fq, fh, and cp: a constant (entered as a table) or a real table:
                0 = constant
                1 = variable
            The options fq and fh may have more dimensional tables:
                column 1 =Q or h value,
                column n = friction value on different locations along the branch for every Q or h
            Thus, the options fq and fh are a function of the location on the branch and Q (of h).
            The option cp (friction as function of the location) has a two-dimensional table:
            column 1 = location along the branch
            column 2 = friction-coefficient
            mr  = friction in negative direction:
            mr fq = friction=f(Q)
            mr fh = friction=f(h)
            mr cp = friction as a constant or as a function of the location on the branch)
            Option fq, fh, and cp may contain a constant given in a table or a table.
                0 = constant
                1 = variable
            s1  = friction for floodplain 1 (not in SOBEK Urban/Rural)
            can be either 'equal to main section', or Chezy/../Nikuradse. (0=Chezy,..,6=Equal to main section)
            Note: Engelund cannot be used for the floodplains.
            s2  = friction for floodplain 2 (not in SOBEK Urban/Rural)
            sf  = ground layer friction type (0 - 7 ) (for further details see description for mf)
            st  = friction in positive direction
            st cp = (for all friction types, for further details see description for mt)
            sr  = ground layer friction in negative direction
            sr cp for all friction types (for further details see description for mt)
            c1 cp,fq,fh = floodplain 1 friction coefficients (friction can be defined as a function of Q, of h, of the location or as a constant.
            r1 cp,fq,fh = floodplain 1 reversed flow friction coefficients as a function of Q, of h, of the location or as a constant.
            c2 cp,fq,fh = floodplain 2
            r2 cp,fq,fh = floodplain 2 reversed flow
            d9 f9 = D90
         * 
            em = flag for main section
                1 = bed friction for negative flow equals the definition for positive flow
                0 = different friction definitions for both directions
            er = flag for main section
                1 = bed friction for positive flow equals the definition for negative flow
                0 = different friction definitions for both directions
            e1 = flag for floodplain 1
                1 = bed friction for negative flow equals the definition for positive flow
                0 = different friction definitions for both directions
            e2 = flag for floodplain 1
                1 = bed friction for positive flow equals the definition for negative flow
                0 = different friction definitions for both directions
            e3 = flag for floodplain 2
            Same meaning as e1
            e4 = flag for floodplain 2
            Same meaning as e2
         * 
            CRFR = Cross section related Friction
            cs 'Crdef' = cross section definition id (only for yz profile and a-symmetrical trapezoidal) 
                to which this friction definition applies   
            lt ys = table for y values which defines the sections (in this case 2) 
                within the profile for definition of friction, flow, etc.
                Number of rows defines the number of defined sections and value per row defines 
                the start of a section and end of a section (horizontal distance increasing from the left to right ). 
                For example, the first defined section starts at Y= 0.0 (including) till Y =3.0 (not including), and so on. 
                Note: the defined sections should be based on the same coordinate system used in defining yz table.
            ft ys = table for friction values in positive direction for (in this case 2) sections 
                (division) of cross section with friction type (0-7), constant friction value, the 
                number of rows should be the same as number of sections defined in the 'lt ys'.
            fr ys = table for friction values in negative direction for (in this case 2) sections 
                (division) of cross section with friction type (0-7), constant friction value the 
                number of rows should be the same as number of sections defined in the 'lt ys'.


         */
        #endregion

        public static SobekFriction GetSobekFriction(string text)
        {
            var sobekFriction = new SobekFriction();

            var matchGlobalFriction = regexGlobalFriction.Match(text);
            if (matchGlobalFriction.Success)
            {
                var matchesGlobalBedFriction = regexBedFriction.Matches(matchGlobalFriction.Value);
                var globalBedFriction = ParseSobekBedFrictionData(matchesGlobalBedFriction);
                // the BDFR is not always present in GLFR (see Ars-7864\1\ADIGE.lit\6\friction.dat)
                if (globalBedFriction.Count > 0)
                {
                    sobekFriction.GlobalBedFrictionList = globalBedFriction;
                }
            }

            var matchesCsFriction = regexCrossSectionFriction.Matches(text);
            if (matchesCsFriction.Count > 0)
            {
                ParseSobekCrossSectionFrictionData(matchesCsFriction, sobekFriction);
            }

            var matchesBedFriction = regexBedFriction.Matches(text);
            if (matchesBedFriction.Count > 0)
            {
                sobekFriction.SobekBedFrictionList = ParseSobekBedFrictionData(matchesBedFriction);
            }

            var matchesStructureFriction = regexStructureFriction.Matches(text);
            if (matchesStructureFriction.Count > 0)
            {
                ParseSobekStructureFrictionData(matchesStructureFriction,sobekFriction);
            }

            var matchesExtraFriction = regexExtraFriction.Matches(text);
            if (matchesExtraFriction.Count > 0)
            {
                LogWarningForUnsupportedExtraResistances(matchesExtraFriction);
            }                           

            return sobekFriction;
        }

        private static void LogWarningForUnsupportedExtraResistances(MatchCollection extraFrictionCollection)
        {
            foreach (Match extraFrictionMatch in extraFrictionCollection)
            {
                MatchCollection matches = regexSobekExtraFriction.Matches(extraFrictionMatch.Value);
                foreach (Match match in matches)
                {
                    LogWarningForUnsupportedExtraResistance(match);
                }
            }
        }

        private static void LogWarningForUnsupportedExtraResistance(Match match)
        {
            string id = RegularExpression.ParseString(match, "id", null);
            if (id != null)
            {
                Log.WarnFormat(Resources.The_extra_resistance_functionality_is_not_supported_skipping_this_item_with_id_0, id);
            }
        }

        private static IList<SobekBedFriction> ParseSobekBedFrictionData(MatchCollection bedFrictionCollection)
        {
            IList<SobekBedFriction> bedFrictions = new List<SobekBedFriction>();
            foreach (Match bedFrictionMatch in bedFrictionCollection)
            {
                var matches = regexSobekBedFrictionData.Matches(bedFrictionMatch.Value);
                foreach (Match match in matches)
                {
                    var sobekBedFriction = new SobekBedFriction();

                    if (match.Groups["Id"].Success)
                    {
                        sobekBedFriction.Id = match.Groups["Id"].Value;
                    }
                    if (match.Groups["bedFrictionDef"].Success)
                    {
                        sobekBedFriction.Name = match.Groups["bedFrictionDef"].Value;
                    }
                    if (match.Groups["BrancheId"].Success)
                    {
                        sobekBedFriction.BranchId = match.Groups["BrancheId"].Value;
                    }
                    ParseMainFriction(sobekBedFriction, bedFrictionMatch.Value);
                    ParseFloodPlain1Friction(sobekBedFriction, bedFrictionMatch.Value);
                    ParseFloodPlain2Friction(sobekBedFriction, bedFrictionMatch.Value);
                    bedFrictions.Add(sobekBedFriction);
                }
            }
            return bedFrictions;
        }

        private static void ParseMainFriction(SobekBedFriction sobekBedFriction, string value)
        {
            var matches = regexMainFriction.Matches(value);
            if (1 == matches.Count)
            {
                // FrictionNeg is parsed but ignored
                var mmatches = regexMainFrictionNeg.Matches(matches[0].Value);
                if (mmatches.Count == 0)
                {
                    Log.ErrorFormat("No valid mainfriction found in {0}; ignored", value);
                    return;
                }
                var match = mmatches[0];
                ParseFrictionData(match, sobekBedFriction.MainFriction);
            }
        }

        private static void ParseFrictionData(Match match, SobekBedFrictionData friction)
        {
            if (match.Groups["FrictionType"].Success)
            {
                friction.FrictionType = (SobekBedFrictionType)Convert.ToInt32(match.Groups["FrictionType"].Value);
            }
            ParseFrictionDataDirection(match, friction.Positive, "FrictionValueType", "FrictionPos");
            ParseFrictionDataDirection(match, friction.Negative, "NegFrictionValueType", "FrictionNeg");
        }

        private static void ParseFrictionDataDirection(Match match, SobekBedFrictionDirectionData friction, string frictionValueType, string FrictionPos)
        {
            if (match.Groups[frictionValueType].Success)
            {
                if (match.Groups[frictionValueType].Value == "fh")
                {
                    friction.FunctionType = SobekFrictionFunctionType.FunctionOfH;

                    if (match.Groups[FrictionPos].Success)
                    {
                        friction.HTable = ParseMultiDimensionalFrictionTable(match.Groups[FrictionPos].Value);
                    }
                }
                if (match.Groups[frictionValueType].Value == "fq")
                {
                    friction.FunctionType = SobekFrictionFunctionType.FunctionOfQ;
                    if (match.Groups[FrictionPos].Success)
                    {
                        friction.QTable = ParseMultiDimensionalFrictionTable(match.Groups[FrictionPos].Value);
                    }
                }
                if (match.Groups[frictionValueType].Value == "cp" && match.Groups[FrictionPos].Success)
                {
                    friction.FunctionType = SobekFrictionFunctionType.Constant;
                    var cp = match.Groups[FrictionPos].Value.Trim();
                    if (cp.StartsWith("0"))
                    {
                        var results = Regex.Split(cp, @"[\t\s]+");
                        friction.FrictionConst = ConversionHelper.ToDouble(results[1]);
                    }
                    else
                    {
                        friction.FunctionType = SobekFrictionFunctionType.FunctionOfLocation;
                        friction.LocationTable = ParseLocationFrictionTable(match.Groups[FrictionPos].Value);
                    }
                }

                SetInterpolation(friction,match.Value);
            }
        }

        private static DataTable ParseMultiDimensionalFrictionTable(string table)
        {
            var dataTable = SobekDataTableReader.CreateDataTableDefinitionFromColumNames(table);
            return SobekDataTableReader.GetTable(table, dataTable);
        }

        private static DataTable ParseLocationFrictionTable(string table)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Location", typeof(double));
            dataTable.Columns.Add("Friction", typeof(double));
            return SobekDataTableReader.GetTable(table, dataTable);
        }

        private static void SetInterpolation(SobekBedFrictionDirectionData friction, string text)
        {
            var matches = regexInterpolation.Matches(text);
            if (matches.Count > 0)
            {
                if (matches[0].Groups["interpolation"].Value == "1")
                {
                    friction.Interpolation = InterpolationType.Constant;
                }
                else
                {
                    friction.Interpolation = InterpolationType.Linear;
                }
            }
        }

        private static void ParseFloodPlain1Friction(SobekBedFriction sobekBedFriction, string value)
        {
            var matches = regexS1BedFriction.Matches(value);
            if (1 == matches.Count)
            {
                var mmatches = regexFloodPlain1Friction.Matches(matches[0].Value);
                if (mmatches.Count == 0)
                {
                    return;
                }
                var match = mmatches[0];
                ParseFrictionData(match, sobekBedFriction.FloodPlain1Friction);
            }            
        }
        
        private static void ParseFloodPlain2Friction(SobekBedFriction sobekBedFriction, string value)
        {
            var matches = regexS2BedFriction.Matches(value);
            if (1 == matches.Count)
            {
                var mmatches = regexFloodPlain2Friction.Matches(matches[0].Value);
                var match = mmatches[0];

                ParseFrictionData(match, sobekBedFriction.FloodPlain2Friction);
            }
        }

        private static void ParseSobekCrossSectionFrictionData(MatchCollection csFrictionCollection, SobekFriction sobekFriction)
        {
            foreach (Match csFrictionMatch in csFrictionCollection)
            {
                var matches = regexCrossSectionFrictionData.Matches(csFrictionMatch.Value);
                foreach (Match match in matches)
                {
                    var sobekCrossSectionFriction = new SobekCrossSectionFriction();

                    if (match.Groups["Id"].Success)
                    {
                        sobekCrossSectionFriction.ID = match.Groups["Id"].Value;
                    }
                    if (match.Groups["bedFrictionDef"].Success)
                    {
                        sobekCrossSectionFriction.Name = match.Groups["bedFrictionDef"].Value;
                    }
                    if (match.Groups["CSDefenition"].Success)
                    {
                        sobekCrossSectionFriction.CrossSectionID = match.Groups["CSDefenition"].Value;
                    }
                    if (match.Groups["yValuesSections"].Success)
                    {
                        var table = SobekDataTableReader.GetTable(match.Groups["yValuesSections"].Value, new Dictionary<string, Type>
                                                      {
                                                          {"een", typeof (double)},
                                                          {"twee", typeof (double)}
                                                      });
                        foreach (var row in table.Rows)
                        {
                            sobekCrossSectionFriction.AddYSections(row);
                        }
                    }
                    if (match.Groups["frictionValues"].Success)
                    {
                        var table = SobekDataTableReader.GetTable(match.Groups["frictionValues"].Value, new Dictionary<string, Type>
                                                      {
                                                          {"een", typeof (double)},
                                                          {"twee", typeof (double)}
                                                      });
                        foreach (var row in table.Rows)
                        {
                            sobekCrossSectionFriction.AddFrictionValues(row);
                        }
                        
                    }
                    sobekFriction.CrossSectionFrictionList.Add(sobekCrossSectionFriction);
                }
            }
        }

        private static void ParseSobekStructureFrictionData(MatchCollection structureFrictionCollection, SobekFriction sobekFriction)
        {
            var frictionType = "CONST";
            foreach (Match structureFrictionMatch in structureFrictionCollection)
            {
                var matches = regexStructureFrictionData.Matches(structureFrictionMatch.Value);
                foreach (Match match in matches)
                {
                    var sobekStructureFriction = new SobekStructureFriction();

                    if (match.Groups["Id"].Success)
                    {
                        sobekStructureFriction.ID = match.Groups["Id"].Value;
                    }
                    if (match.Groups["StructureId"].Success)
                    {
                        sobekStructureFriction.StructureDefinitionID = match.Groups["StructureId"].Value;
                    }
                    if (match.Groups["MainFrictionType"].Success)
                    {
                        sobekStructureFriction.MainFrictionType = Convert.ToInt32(match.Groups["MainFrictionType"].Value);
                    }
                    if (match.Groups["FrictionValueType"].Success)
                    {
                        if (match.Groups["FrictionValueType"].Value == "fh")
                        {
                            frictionType = "H";
                        }
                        if (match.Groups["FrictionValueType"].Value == "fq")
                        {
                            frictionType = "Q";
                        }
                    }
                    if (match.Groups["FrictionPos"].Success)
                    {
                        sobekStructureFriction.AddMainPositiveFriction(match.Groups["FrictionPos"].Value, frictionType);
                    }
                    if (match.Groups["FrictionFloodplain1"].Success)
                    {
                        sobekStructureFriction.FloodPlain1FrictionType = Convert.ToInt32(match.Groups["FrictionFloodplain1"].Value);
                    }
                    if (match.Groups["FrictionFloodplain2"].Success)
                    {
                        sobekStructureFriction.FloodPlain2FrictionType = Convert.ToInt32(match.Groups["FrictionFloodplain2"].Value);
                    }

                    if (match.Groups["GroundLayerFrictionType"].Success)
                    {
                        sobekStructureFriction.GroundLayerFrictionType = Convert.ToInt32(match.Groups["GroundLayerFrictionType"].Value);
                    }
                    if (match.Groups["GLFrictionPos"].Success)
                    {
                        string[] results = Regex.Split(match.Groups["GLFrictionPos"].Value, @"[\t\s]+");
                        double friction = Convert.ToDouble(results[1],CultureInfo.InvariantCulture);
                        sobekStructureFriction.GroundLayerFrictionValue = friction;
                    }

                    sobekFriction.StructureFrictionList.Add(sobekStructureFriction);

                }
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "glfr";
            yield return "bdfr";
            yield return "crfr";
            yield return "stfr";
            yield return "xrst";
        }
    }
}
