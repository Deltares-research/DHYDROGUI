using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace DeltaShell.Sobek.Readers.Readers
{
    //Reads Network.ntw, a netter file with information about type of nodes
    public class SobekNetworkNetterReader
    {
        public static IDictionary<string,int> ReadNodeTypes(string filePath)
        {
            return ParseNodeTypes(File.ReadLines(filePath));
        }

        public static IDictionary<string,int> ParseNodeTypes(string datFileText)
        {
            var lines = datFileText.Split(new[]{Environment.NewLine},StringSplitOptions.None);
            return ParseNodeTypes(lines);
        }

        private static IDictionary<string, int> ParseNodeTypes(IEnumerable<string> lines)
        {
            var nodes = new Dictionary<string, int>();
            var lineIndex = 1; //first row is general info
            foreach (var line in lines)
            {
                if (line == "*" || line == "" || line == "[Reach description]")
                    continue;
                
                var values = line.Split(new[] { ',' });
                var valueIndex = 14; //first 14 values are of a branch
                while (valueIndex <= values.Count() - 13) //13 values per node
                {
                    var key = values[valueIndex].Replace("\"", "");
                    int value;
                    if (int.TryParse(values[valueIndex + 4].Replace("\"", ""), out value))
                    {
                        nodes[key] = value;
                    }
                    valueIndex += 13;
                }
                lineIndex++;
            }
            return nodes;
        }

        public static bool IsPipe(int typeNo)
        {
            return typeNo >= 4 && typeNo <= 13;
        }

        public static bool IsPreasurePipe(int typeNo)
        {
            return typeNo >= 10 && typeNo <= 13;
        }

        public static bool IsDryWeatherPipe(int typeNo)
        {
            return typeNo == 6;
        }

        public static bool IsStormWeatherPipe(int typeNo)
        {
            return typeNo == 5;
        }

        public static bool IsManhole(int typeNo)
        {
            return typeNo >= 1 && typeNo <= 11;
        }

        public static bool IsLateralSourceNode(int typeNo)
        {
            return typeNo == 4 || typeNo == 5 || typeNo == 13 || typeNo == 19;
        }

        public static IDictionary<string, int> ReadBranchTypes(string filePath)
        {
            return ParseBranchTypes(File.ReadLines(filePath));
        }

        public static IDictionary<string, int> ParseBranchTypes(string datFileText)
        {
            var lines = datFileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return ParseBranchTypes(lines);
        }

        private static IDictionary<string, int> ParseBranchTypes(IEnumerable<string> lines)
        {
            var branches = new Dictionary<string, int>();
            var lineIndex = 0; 
            foreach (var line in lines)
            {
                if (line == "*" || line == "\"*\"" || line == "" || line == "[Reach description]")
                    continue;

                if (lineIndex > 0) //first row is general info
                {
                    var values = line.Split(new[] { ',' });
                    if (values.Length >= 20)
                    {
                        var key = values[2].Replace("\"", "");
                        int value;
                        var valueString = values[3];
                        if (!string.IsNullOrEmpty(valueString))
                        {
                            valueString = valueString.Replace("\"", "");
                        }
                        if (int.TryParse(valueString, out value))
                        {
                            branches[key] = value;
                        }
                    }
                }

                lineIndex++;
            }
            return branches;
        }
    }
}
