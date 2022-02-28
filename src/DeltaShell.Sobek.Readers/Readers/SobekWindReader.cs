using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekWindReader : SobekReader<SobekWind>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWindReader));

        public override IEnumerable<SobekWind> Parse(string source)
        {
            const string structurePattern = @"(GLMT (?'text'.*?) glmt)|(MTEO (?'text'.*?)\smteo)";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, source))
            {
                var sobekWind = GetWind(structureMatch.Value);
                if (sobekWind != null)
                {
                    yield return sobekWind;
                }
            }
        }

        public static SobekWind GetWind(string record)
        {
            const string pattern = @"wv tv\s(?<vel>" + RegularExpression.CharactersAndQuote + @")\s" +
                                   @"wd td\s(?<dir>" + RegularExpression.CharactersAndQuote + @")\s";

            var sobekWind = new SobekWind
                                {
                                    Id = RegularExpression.ParseFieldAsIntegerOrString("id", record),
                                    Name = RegularExpression.ParseFieldAsString("nm", record),
                                    BranchId = RegularExpression.ParseFieldAsIntegerOrString("ci", record),
                                    Used = (RegularExpression.ParseFieldAsInt("wu", record) == 1) ? true : false
                                };
            
            var match = RegularExpression.GetFirstMatch(pattern, record);
            if (match == null)
            {
                Log.WarnFormat("Could not read wind definition (\"{0}\")",record);
                return null;
            }

            DataTable velocity, direction;

            ParseVelocity(match.Groups["vel"].Value, sobekWind, out velocity);
            ParseDirection(match.Groups["dir"].Value, sobekWind, out direction);
            if ((!sobekWind.IsConstantDirection) && (!sobekWind.IsConstantVelocity))
            {
                sobekWind.Wind = MergeWind(velocity, direction);
            }
            return sobekWind;
        }
        
        private static IFunction MergeWind(DataTable velocity, DataTable direction)
        {
            IFunction wind = new Function();

            wind.Arguments.Add(new Variable<DateTime>("time"));
            wind.Components.Add(new Variable<double>("velocity"));
            wind.Components.Add(new Variable<double>("direction"));

            List<DateTime> times = new List<DateTime>();
            foreach (DataRow row in velocity.Rows)
            {
                times.Add((DateTime) row[0]);
            }
            foreach (DataRow row in direction.Rows)
            {
                times.Add((DateTime)row[0]);
            }
            var uniqueTimes = times.Select(t => t).Distinct().OrderBy(t => t).ToArray();
            wind.Arguments[0].SetValues(uniqueTimes);

            MergeTableIntoFunctionComponent(wind, velocity, 0);
            MergeTableIntoFunctionComponent(wind, direction, 1);
            return wind;
        }

        private static void MergeTableIntoFunctionComponent(IFunction function, DataTable dataTable, int component)
        {
            IFunction interpolationFunction = new Function();
            interpolationFunction.Arguments.Add(new Variable<DateTime>("time") { ExtrapolationType = ExtrapolationType.Constant });
            interpolationFunction.Components.Add(new Variable<double>("component"));

            FunctionHelper.AddDataTableRowsToFunction(dataTable, interpolationFunction);
            var times = function.Arguments[0].Values;

            for (int i = 0; i < times.Count; i++)
            {
                function.Components[component].Values[i] =
                    interpolationFunction.Evaluate<double>(new VariableValueFilter<DateTime>(interpolationFunction.Arguments[0],
                                                                                               (DateTime)times[i]));
            }
        }

        static void ParseVelocity(string table, SobekWind sobekWind, out DataTable velocity)
        {
            velocity = null;
            if (table == null)
            {
                sobekWind.IsConstantVelocity = true;
                return;
            }
            if (table.StartsWith("0"))
            {
                sobekWind.IsConstantVelocity = true;
                sobekWind.ConstantVelocity = table.SplitOnEmptySpace()[1].Parse<double>(CultureInfo.InvariantCulture);
                return;
            }
            sobekWind.IsConstantVelocity = false;
            velocity = SobekDataTableReader.GetTable(table,
                                                             new Dictionary<string, Type>
                                                                     {
                                                                         {"datetime", typeof (DateTime)},
                                                                         {"velocity", typeof (double)}
                                                                     });
        }

        static void ParseDirection(string table, SobekWind sobekWind, out DataTable direction)
        {
            direction = null;
            if (table == null)
            {
                sobekWind.IsConstantDirection = true;
                return;
            }
            if (table.StartsWith("0"))
            {
                sobekWind.IsConstantDirection = true;
                sobekWind.ConstantDirection = table.SplitOnEmptySpace()[1].Parse<double>(CultureInfo.InvariantCulture);
                return;
            }
            sobekWind.IsConstantDirection = false;
            direction = SobekDataTableReader.GetTable(table,
                                                             new Dictionary<string, Type>
                                                                     {
                                                                         {"datetime", typeof (DateTime)},
                                                                         {"direction", typeof (double)}
                                                                     });
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "glmt";
            yield return "mteo";
        }
    }
}
