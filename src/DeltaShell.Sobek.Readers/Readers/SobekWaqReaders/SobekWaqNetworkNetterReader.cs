using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqNetworkNetterReader
    {
        # region Sobek212

        /// <summary>
        /// NetterBranchObject contains information about Netter 'Branch Object'
        /// 
        /// </summary>
        public struct NetterBranchObject
        {
            /// <summary>
            /// Id of 'Branch' and is assumed to be unique for the read network
            /// </summary>
            /// <example>
            /// "Br 1"
            /// </example>
            public string Id;
            
            /// <summary>
            /// Index number of the corresponding 'Reach'
            /// </summary>
            /// <example>
            /// 1
            /// </example>
            public int ReachIndex;

            /// <summary>
            /// Id of the parent of the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "SBK_CHANNEL"
            /// </example>
            public string ParentType;
            
            /// <summary>
            /// Id of the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "SBK_CHANNEL_SWTTYPE2"
            /// </example>
            public string UserDefinedType;

            public NetterBranchObject(string id, int reachIndex, string parentType, string userDefinedType)
            {
                Id = id;
                ReachIndex = reachIndex;
                ParentType = parentType;
                UserDefinedType = userDefinedType;
            }
        }

        /// <summary>
        /// NetterBranchObject contains information about Netter 'Node Object'
        /// </summary>
        public struct NetterNodeObject
        {
            /// <summary>
            /// Id of node and is assumed to be unique for the read network
            /// </summary>
            /// <example>
            /// "N 1"
            /// </example>
            public string Id;

            /// <summary>
            /// Id of the parent of the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "SBK_BOUNDARY"
            /// </example>
            public string ParentType;

            /// <summary>
            /// Id of the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "SBK_BOUNDARY_BNTYPE1"
            /// </example>
            public string UserDefinedType;

            public NetterNodeObject(string id, string parentType, string userDefinedType)
            {
                Id = id;
                ParentType = parentType;
                UserDefinedType = userDefinedType;
            }
        }

        /// <summary>
        /// Read method for finding user defined types of nodes in the Sobek 212 Netter file "NETWORK.NTW".
        /// </summary>
        /// <example>
        /// File "NETWORK.NTW" starts like this:
        /// 
        /// "NTW6.6","D:\SOBEK212\NewTest.lit\CMTWORK\ntrpluv.ini","SOBEK-LITE-DELWAQ, edit network"
        /// "b1","L1",1,23,"SBK_CHANNEL","SBK_CHANNEL_BRANCHTYPENORMAL",0,0,0,0,368.089888978055,0,0,0,"N1","Node1","",1,65,"SBK_BOUNDARY","SBK_BOUNDARY_BNTYPE1",254902.698666233,471482.595349359,0,0,"SYS_DEFAULT",0,"LS1","LateralSource1","",1,67,"SBK_LATERALFLOW","SBK_LATERALFLOW_LSTYPE1",255205.071885633,471692.501524965,0,368.089888978055,"SYS_DEFAULT",0
        /// </example>
        /// <param name="filePath">The path to "NETWORK.NTW"</param>
        /// <returns>A list of objects with relevant fields: branch id, parent type and user defined type id</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IEnumerable<NetterNodeObject> ReadNodeUserDefinedTypeIds(string filePath)
        {
            string defFileText = File.ReadAllText(filePath, Encoding.Default);
            return ParseNodeTypeIds(defFileText);
        }

        /// <summary>
        /// Read method for finding user defined types of branches in the Sobek 212 Netter file "NETWORK.NTW".
        /// </summary>
        /// <example>
        /// File "NETWORK.NTW" starts like this:
        /// 
        /// "NTW6.6","D:\SOBEK212\NewTest.lit\CMTWORK\ntrpluv.ini","SOBEK-LITE-DELWAQ, edit network"
        /// "b1","L1",1,23,"SBK_CHANNEL","SBK_CHANNEL_BRANCHTYPENORMAL",0,0,0,0,368.089888978055,0,0,0,"N1","Node1","",1,65,"SBK_BOUNDARY","SBK_BOUNDARY_BNTYPE1",254902.698666233,471482.595349359,0,0,"SYS_DEFAULT",0,"LS1","LateralSource1","",1,67,"SBK_LATERALFLOW","SBK_LATERALFLOW_LSTYPE1",255205.071885633,471692.501524965,0,368.089888978055,"SYS_DEFAULT",0
        /// </example>
        /// <param name="filePath">The path to "NETWORK.NTW"</param>
        /// <returns>A list of objects with relevant fields: branch id, reach index, parent type and user defined type id</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IEnumerable<NetterBranchObject> ReadBranchTypeIds(string filePath)
        {
            string defFileText = File.ReadAllText(filePath, Encoding.Default);
            return ParseBranchTypeIds(defFileText);
        }


        /// <summary>
        /// Read method for finding reaches in the Sobek 212 Netter file "NETWORK.NTW".
        /// </summary>
        /// <example>
        /// A reach description segment is defined in the file "NETWORK.NTW" as follows:
        /// 
        /// [Reach description]
        ///  4 
        /// "r1","Reach1","N1","N3",0,2,254902.698666233,471482.595349359,255445.558183461,471859.446068411,660.84258329427,0,100,-1
        /// "r2","Reach2","N3","N2",0,2,255445.558183461,471859.446068411,256011.036847239,472221.222758701,671.303577247563,0,100,-1
        /// "r3","Reach3","N3","N2a",0,2,255445.558183461,471859.446068411,255551.114200699,472424.72214699,575.047057021267,0,100,-1
        /// "r4","Reach4","N3","N4",0,2,255445.558183461,471815.963293136,255612.28365428,471859.446068411,172.302450259536,0,100,-1
        /// </example>
        /// <param name="filePath">The path to "NETWORK.NTW"</param>
        /// <returns>A dictionary with reach index as key and reach id as value</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IDictionary<string, int> ReadReachIds(string filePath)
        {
            string defFileText = File.ReadAllText(filePath, Encoding.Default);
            return ParseReachIds(defFileText);
        }

        private static IEnumerable<NetterNodeObject> ParseNodeTypeIds(string datFileText)
        {
            var lines = datFileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var nodes = new List<NetterNodeObject>();

            var lineIndex = 1; //first row is general info
            var line = "ready to start";
            while (lineIndex < lines.Count() && line.Replace("\"", "") != "*" && line != "" && line != "[Reach description]")
            {
                line = lines[lineIndex];
                var values = line.Split(new[] { ',' });
                if (values.Count() == 40)
                {
                    var valueIndex = 14; //first 14 values are of a branch
                    while (valueIndex <= values.Count() - 13) //13 values per node
                    {
                        var nodeId = values[valueIndex].Replace("\"", "");
                        var parentType = values[valueIndex + 5].Replace("\"", "");
                        var userDefinedType = values[valueIndex + 6].Replace("\"", "");
                        if ((nodeId != "") && (nodes.Where(n => n.Id == nodeId).Count() == 0)) 
                        {
                            nodes.Add(new NetterNodeObject(nodeId, parentType, userDefinedType));
                        }
                        valueIndex += 13;
                    }
                }
                else if (line.Replace("\"", "") != "*" && line != "" && !line.StartsWith("["))
                {
                    throw new FormatException("no valid data was found");
                }
                lineIndex++;
            }
            return nodes;
        }

        private static IEnumerable<NetterBranchObject> ParseBranchTypeIds(string datFileText)
        {
            var lines = datFileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var branches = new List<NetterBranchObject>();

            var lineIndex = 1; //first row is general info
            var line = "ready to start";
            while (lineIndex < lines.Count() && line.Replace("\"", "") != "*" && line != "" && !line.StartsWith("["))
            {
                line = lines[lineIndex];
                var values = line.Split(new[] { ',' });
                //first 14 values are of a branch
                if (values.Count() == 40)
                {
                    var branchId = values[0].Replace("\"", "");
                    int reachIndex;
                    if (int.TryParse(values[2].Replace("\"", ""), out reachIndex))
                    {
                        var parentType = values[4].Replace("\"", "");
                        var userDefinedType = values[5].Replace("\"", "");
                        if (branchId != "")
                        {
                            branches.Add(new NetterBranchObject(branchId, reachIndex, parentType, userDefinedType));
                        }
                    }
                    else
                    {
                        throw new FormatException("no valid data was found");
                    }
                }
                else if (line.Replace("\"", "") != "*" && line != "" && !line.StartsWith("["))
                {
                    throw new FormatException("no valid data was found");
                }
                lineIndex++;
            }
            return branches;
        }

        private static IDictionary<string, int> ParseReachIds(string datFileText)
        {
            var lines = datFileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var reaches = new Dictionary<string, int>();

            // search start of Reach section
            var line = lines.Where(l => l.Contains("[Reach description]")).FirstOrDefault();
            if (line == null)
            {
                throw new FormatException("no valid data was found");
            }

            // skip empty row(s)
            var lineIndex = lines.ToList().IndexOf(line);
            lineIndex++;
            line = "";
            while (lineIndex < lines.Count() && line == "")
            {
                line = lines[lineIndex];
                lineIndex++;
            }

            // read number of reaches
            int numberOfReaches;
            if (int.TryParse(line, out numberOfReaches))
            {
                if (lineIndex + numberOfReaches >= lines.Count())
                {
                    throw new FormatException("no valid data was found");
                }
                lineIndex--;
                for (int reachIndex = 1; (reachIndex <= numberOfReaches); reachIndex++)
                {
                    line = lines[lineIndex + reachIndex];
                    var values = line.Split(new[] { ',' });
                    // we are interested in the first value
                    if (values.Count() == 0)
                    {
                        throw new FormatException("no valid data was found");
                    }
                    var key = values[0].Replace("\"", "");
                    var value = reachIndex;
                    reaches[key] = value;
                }
            }
            else
            {
                throw new FormatException("no valid data was found");
            }
            return reaches;
        }

        # endregion
    }
}
