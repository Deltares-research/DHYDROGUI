using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Sobek.Readers.Readers
{
    //Reads Network.ntw, a netter file with information about type of nodes
    public class SobekNetworkNetterReader
    {
        public static IDictionary<string,string> ReadNodeTypes(string filePath)
        {
            return ParseNodeTypes(File.ReadLines(filePath));
        }

        public static IDictionary<string,string> ParseNodeTypes(string datFileText)
        {
            var lines = datFileText.Split(new[]{Environment.NewLine},StringSplitOptions.None);
            return ParseNodeTypes(lines);
        }

        private static IDictionary<string, string> ParseNodeTypes(IEnumerable<string> lines)
        {
            var nodes = new Dictionary<string, string>();
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
                    var value = values[valueIndex + 5].Replace("\"", "");
                    nodes[key] = value;
                    valueIndex += 13;
                }
                lineIndex++;
            }
            return nodes;
        }
    }
}
