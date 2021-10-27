using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Common.Extensions;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekRetentionsReader : SobekReader<Retention>
    {
        // NODES.DAT

        // NODE id '25' ty 1 ws 0 ss 0 wl 0 ml 1  node
        // NODE id '24' ty 1 ws 0 ss 0 wl 0 ml 1  node
        // NODE id '6' ty 1 ws 10000 ss 0 wl 22 ml 999999 node

        // Where:
        //id = node id
        //ty = type water on street
        //0 = connection node, is not a retention!!!!
        //1 = reservoir
        //2 = closed
        //3 = loss
        //ws = retention area (manhole)
        //ss = street retention area
        //wl = bed level storage reservoir (manhole)
        //ml = street level
        //NODE id '0-62' ty 1 ct sw PDIN 1 0 ' ' pdin
        //TBLE 
        //16.28 2 <
        //20.84 2 <
        //tble ct ss PDIN 0 0 ' ' pdin
        //TBLE
        //20.85  100 <
        //21.9  150 <
        //tble  node
        //Where:
        //ct sw TBLE .. tble = table for retention in well
        //ct ss TBLE .. tble = table for retention at street
        //PDIN 1 0 ' ' pdin = block function
        //PDIN 0 0 ' ' pdin = linear interpolation

        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRetentionsReader));

        private Dictionary<Retention, string> structures = new Dictionary<Retention, string>();
        private Dictionary<Retention, string> secondStructures = new Dictionary<Retention, string>();

        public bool Sobek2Import { get; set; }

        public override IEnumerable<Retention> Parse(string text)
        {
            string lateralConditionPattern = Sobek2Import ? @"(NODE (?'text'.*?) node)" : @"(FLBR (?'text'.*?) flbr)";

            var matches = RegularExpression.GetMatches(lateralConditionPattern, text);

            foreach (Match match in matches)
            {
                Retention retention = GetRetention(match.Value);
                if (retention != null)
                {
                    yield return retention;
                }
            }
        }

        public Retention GetRetention(string line)
        {
            line = line.Replace("ct sw", "ctsw");
            line = line.Replace("ct ss", "ctss");
            line = line.Replace("dc lt 5", "dclt5");
            line = line.Replace("wl ow", "wlow");
            string[] words = line.SplitOnEmptySpace();

            if (words.Length == 0)
            {
                return null;
            }

            bool valid = Sobek2Import;


            var isConnectionNode = IsConnectionNode(words);
            if (isConnectionNode) return null;

            Retention retention = new Retention();

            for (int i = 0; i < words.Length - 1; i++)
            {
                string key = words[i].ToUpper();
                string value = words[i + 1].Trim('\'');

                if (value.StartsWith("."))
                {
                    value = "0" + value;
                }
                if (words[i + 1].StartsWith("'"))
                {
                    value = ConcatinateStringValues(words, i, value);
                }

                switch (key)
                {
                    case "ID":
                        retention.Name = value;
                        break;
                    case "TY":
                        int type = 0;
                        if (Int32.TryParse(value, out type))
                        {
                            switch (type)
                            {
                                case 1:
                                    retention.Type = RetentionType.Reservoir;
                                    break;
                                case 2:
                                    retention.Type = RetentionType.Closed;
                                    break;
                                case 3:
                                    retention.Type = RetentionType.Loss;
                                    break;
                            }
                        }
                        break;
                    case "WS":
                        retention.StorageArea = ParseAndGetValue(value);
                        break;
                    case "SS":
                        if (Sobek2Import)
                        {
                            retention.StreetStorageArea = ParseAndGetValue(value);   
                        }
                        else
                        {
                            retention.StreetStorageArea = retention.StorageArea;
                        }
                        break;
                    case "AR":
                        //ar is runoff area
                        retention.StorageArea = ParseAndGetValue(value);
                        if (!Sobek2Import)
                        {
                            retention.StreetStorageArea = retention.StorageArea;
                            retention.StreetLevel = 999999.0;
                        }
                        break;
                    case "WL":
                        retention.BedLevel = ParseAndGetValue(value);
                        if (!Sobek2Import)
                        {
                            retention.StreetLevel = 999999.0;
                        }
                        break;
                    case "ML":
                        retention.StreetLevel = Sobek2Import ? ParseAndGetValue(value) : 999999.0;
                        break;
                    case "BL":
                        //sobek RE bed level
                        retention.BedLevel = ParseAndGetValue(value);
                        break;
                    case "CTSW":
                        var swMatch = RegularExpression.GetFirstMatch(
                                @"ctsw\s*(?<table>" + RegularExpression.CharactersAndQuote + @")", line);
                        if (swMatch == null)
                        {
                            Log.WarnFormat("Could not read CTSW of retention definition with id {0} (\"{1}\")", retention.Id, line);
                            return null;
                        }
                        SetTableToFunction(SobekDataTableReader.GetTable(swMatch.Groups["table"].Value,
                                                                                         StorageStructure),
                                                           retention.Data);
                        retention.UseTable = true;
                        break;
                    case "CTSS":
                        var ssMatch =
                            RegularExpression.GetFirstMatch(
                                @"ctss\s*(?<table>" + RegularExpression.CharactersAndQuote + @")", line);
                        if (ssMatch == null)
                        {
                            Log.WarnFormat("Could not read CTSS of retention definition with id {0} (\"{1}\")", retention.Id, line);
                            return null;
                        }
                        SetTableToFunction(SobekDataTableReader.GetTable(ssMatch.Groups["table"].Value,
                                                                                         StorageStructure),
                                                           retention.Data);
                        break;
                    case "PDIN":
                        int pdin = 0;
                        if (Int32.TryParse(value, out pdin))
                        {
                            switch (pdin)
                            {
                                case 1:
                                    retention.Data.Arguments[0].InterpolationType = InterpolationType.Constant;
                                    break;
                                case 0:
                                    retention.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                                    break;
                            }
                        }
                        break;
                    case "DCLT5":
                        valid = true;
                        break;
                    case "SD":
                        // FLBR id '201' sc 0 dc lt 5 9.9999e+009 9.9999e+009 s2 '103' ar 551056 bl 44.82 ih 44.82 u1 1 ca 0 0 0 0 cj '-1' '-1' '-1' '-1' cb 1 1 1 0 ck '01' '119' '5596' '-1' lt 0 sd '102' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr
                        if (!value.Equals("") && !value.Equals("-1"))
                        {
                            RetentionStructures[retention] = value;
                        }
                        break;
                    case "S2":
                        // FLBR id '201' sc 0 dc lt 5 9.9999e+009 9.9999e+009 s2 '103' ar 551056 bl 44.82 ih 44.82 u1 1 ca 0 0 0 0 cj '-1' '-1' '-1' '-1' cb 1 1 1 0 ck '01' '119' '5596' '-1' lt 0 sd '102' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr
                        if (!value.Equals("") && !value.Equals("-1"))
                        {
                            SecondRetentionStructures[retention] = value;
                        }
                        break;
                    case "WLOW":
                        // table with water levels outside the lateral structure
                        //wl ow 0= constant as a table,
                        //wl ow 1= 'real' table
                        break;
                }
            }

            return valid ? retention : null;
        }

        private bool IsConnectionNode(string[] words)
        {
            for (int i = 0; i < words.Length - 1; i++)
            {
                string key = words[i].ToUpper();
                string value = words[i + 1].Trim('\'');
                if (key.Equals("TY"))
                {
                    int type = 0;
                    if (int.TryParse(value, out type))
                    {
                        return (type == 0);
                    }
                }
            }
            return false;
        }

        private double ParseAndGetValue(string stringValue)
        {
            return Double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
                   value != 0
                ? value
                : 0;
        }

        private string ConcatinateStringValues(string[] words, int i, string value)
        {
            int start = i + 1;
            for (int j = 0; j < 5; j++)
            {
                if (words[start + j].EndsWith(("'")))
                {
                    int end = start + j;
                    StringBuilder result = new StringBuilder();
                    for (int k = 0; k < end- start; k++)
                    {
                        result.Append(" ");
                        result.Append((words[start +1 + k]).Trim('\''));
                    }
                    value = value + result;
                    break;
                }
            }
            return value;
        }

        private static DataTable StorageStructure
        {
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("height", typeof(double));
                dataTable.Columns.Add("storage", typeof(double));
                return dataTable;
            }
        }

        // TODO: DRY, remove this or DataTableHelper.SetTableToFunction
        private static void SetTableToFunction(DataTable dataTable, IFunction function)
        {
            var values = new SortedDictionary<object, object>();

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var argumentValue = dataTable.Rows[i][0];

                if (!values.ContainsKey(argumentValue))
                {
                    values[argumentValue] = dataTable.Rows[i][1];
                }
                else
                {
                    Log.WarnFormat("Duplicate entry during import {0}: {1} with both value {2} and {3}.", dataTable.TableName, argumentValue, function[argumentValue], dataTable.Rows[i][1]);
                }
            }

            var arrayArgumentValues = function.Arguments[0].Values;
            var arrayComponentValues = function.Components[0].Values;

            arrayArgumentValues.FireEvents = false;
            arrayComponentValues.FireEvents = false;
            arrayArgumentValues.AddRange(values.Keys.ToArray());
            arrayComponentValues.AddRange(values.Values.ToArray());
            arrayArgumentValues.FireEvents = true;
            arrayComponentValues.FireEvents = true;
        }

        public Dictionary<Retention, string> RetentionStructures
        {
            get { return structures; }
        }

        public Dictionary<Retention, string> SecondRetentionStructures
        {
            get { return secondStructures; }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "node";
            yield return "flbr";
        }
    }
}
