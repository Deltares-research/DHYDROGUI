using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            foreach (var line in lines)
            {
                if (line == "*" || line == "")
                    continue;

                if (line.Contains("[Reach description]"))
                {
                    break; //end of information block with node data
                }
                
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
            return typeNo == 4 || typeNo == 5 ||  typeNo == 19;
        }

        public static bool IsConnectionNode(int typeNo)
        {
            return typeNo == 14;
        }

        public static bool IsExternalStructureNode(int typeNo)
        {
            return typeNo >= 7 && typeNo <= 11;
        }

        public static IDictionary<string, int> ReadBranchTypes(string filePath)
        {
            return ParseBranchTypes(File.ReadLines(filePath));
        }

        private static IDictionary<string, int> ParseBranchTypes(IEnumerable<string> lines)
        {
            var branchTypesOnIndexes = new Dictionary<int, int>();
            var branchTypes = new Dictionary<string, int>();
            var lineIndex = 0;
            var branchDescriptionIndex = 0;

            var bSetBranchNamesForTypes = false;
            foreach (var line in lines)
            {
                if (line == "*" || line == "\"*\"" || line == "")
                    continue;

                if (line.Contains("[Reach description]"))
                {
                    bSetBranchNamesForTypes = true;
                    continue; //end of information block with node data
                }

                if (line.Contains("[Model connection node]") || 
                    line.Contains("[Model connection branch]") ||
                    line.Contains("[Nodes with calculationpoint]") ||
                    line.Contains("[Reach options]") ||
                    line.Contains("[NTW properties]"))
                {
                    break; //end of information block with branch type data
                }

                if (bSetBranchNamesForTypes)
                {
                    branchDescriptionIndex = AddBranchNameAndTypeNumber(line, branchDescriptionIndex, branchTypesOnIndexes, branchTypes);
                }
                else
                {
                    if (lineIndex > 0) //first row is general info
                    {
                        AddBranchIndexNumberAndTypeNumber(line, branchTypesOnIndexes);
                    }
                }

                lineIndex++;
            }
            return branchTypes;
        }

        private static int AddBranchNameAndTypeNumber(string line, int previousIndexNumber, Dictionary<int, int> lookUpTypesOnIndexes, Dictionary<string, int> branchTypes)
        {
            var values = line.Split(new[] {','});
            var indexNumber = previousIndexNumber + 1;
            if (values.Length >= 12)
            {
                var valueString = values[0];
                if (!string.IsNullOrEmpty(valueString) && lookUpTypesOnIndexes.ContainsKey(indexNumber))
                {
                    valueString = valueString.Replace("\"", "");
                    branchTypes[valueString] = lookUpTypesOnIndexes[indexNumber];
                    return indexNumber;
                }
            }

            return previousIndexNumber;
        }

        private static void AddBranchIndexNumberAndTypeNumber(string line, Dictionary<int, int> branchTypesOnIndexes)
        {
            var values = line.Split(new[] {','});
            if (values.Length >= 20)
            {
                int key;
                int value;

                var keyString = values[2];
                if (!string.IsNullOrEmpty(keyString))
                {
                    keyString = keyString.Replace("\"", "");
                }

                var valueString = values[3];
                if (!string.IsNullOrEmpty(valueString))
                {
                    valueString = valueString.Replace("\"", "");
                }

                if (int.TryParse(keyString, out key) && int.TryParse(valueString, out value))
                {
                    branchTypesOnIndexes[key] = value;
                }
            }
        }

        public static bool IsFlowConnectionNode(int typeNo)
        {
            return typeNo == 35;
        }
    }
}
