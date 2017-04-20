using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqUserDefinedTypesReader
    {
        # region Sobek212

        /// <summary>
        /// NetterUserDefinedTypeObject contains information about a Netter 'User Defined Object'
        /// </summary>
        public struct NetterUserDefinedTypeObject
        {
            /// <summary>
            /// Id of the 'User Defined Object', and is assumed to be unique for the read network
            /// </summary>
            /// <example>
            /// "SBK_BOUNDARY_BNTYPE1"
            /// </example>
            public string TypeId;
            
            /// <summary>
            /// Name of the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "BN Type 1"
            /// </example>
            public string TypeName;
            
            /// <summary>
            /// Fraction assigned to the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "Boundary Flow"
            /// </example>
            public string Fraction;

            /// <summary>
            /// Surface water type assigned to the 'User Defined Object'
            /// </summary>
            /// <example>
            /// "Normal"
            /// </example>
            public string SurfaceWaterType;

            public NetterUserDefinedTypeObject(string typeId, string typeName, string fraction, string surfaceWaterType)
            {
                TypeId = typeId;
                TypeName = typeName;
                Fraction = fraction;
                SurfaceWaterType = surfaceWaterType;
            }
        }

        /// <summary>
        /// Read method for user node types in the Sobek 212 Netter file "NTRPLUV.OBJ".
        /// </summary>
        /// <example>
        /// File "NTRPLUV.OBJ" has the following structure:
        /// 
        /// [User Project Identification]
        /// Version=3.10
        /// 
        /// [User Node Types]
        /// Number of Types=5
        /// 1 ParentID=SBK_BOUNDARY
        /// 1 Type=BNType1
        /// 1 ID=SBK_BOUNDARY_BNTYPE1
        /// 1 BNAname=sbk_boundary_bntype1
        /// 1 In Use=-1
        /// 1 Visible=-1
        /// 1 LabelInactive=0
        /// 1 SymbolType=9
        /// 1 BorderColor=0
        /// 1 FillColor=16744576
        /// 1 Syst=SYS_DEFAULT
        /// 1 IncludeInScaleData=-1
        /// 1 IncludeInTableView=0
        /// 1 MonitorStation=0
        /// 1 SurfaceWaterType=
        /// 1 Allow boundaries=-1
        /// 1 Number of boundaries=1
        /// 1 Boundary 1=Boundary Flow
        /// 1 Metafile=
        /// 1 PictureActive=0
        /// 1 WLM function=
        /// 2 ParentID=SBK_BOUNDARY
        /// etc.
        /// </example>
        /// <param name="filePath">The path to "NTRPLUV.OBJ"</param>
        /// <returns>A list of NetterUserDefinedTypeObject</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IEnumerable<NetterUserDefinedTypeObject> ReadUserNodeTypes(string filePath)
        {
            string defFileText = File.ReadAllText(filePath, Encoding.Default);
            return ParseUserTypes(defFileText, true);
        }

        /// <summary>
        /// Read method for user branch types in the Sobek 212 Netter file "NTRPLUV.OBJ".
        /// </summary>
        /// <example>
        /// File "NTRPLUV.OBJ" has the following structure:
        /// 
        /// [User Project Identification]
        /// Version=3.10
        /// 
        /// [User Node Types]
        /// Number of Types=5
        /// 1 ParentID=SBK_BOUNDARY
        /// etc.
        /// 
        /// [User Branch Types]
        /// Number of Types=3
        /// 1 ParentID=SBK_CHANNEL
        /// 1 Type=BranchTypeNormal
        /// 1 ID=SBK_CHANNEL_BRANCHTYPENORMAL
        /// 1 Number of boundaries=0
        /// etc
        /// 2 ParentID=SBK_CHANNEL&LAT
        /// 2 Type=BranchTypeTest
        /// 2 ID=SBK_CHANNEL&LAT_BRANCHTYPETEST
        /// 2 Number of boundaries=1
        /// 2 Boundary 1=Lateral inflow
        /// etc.
        /// </example>
        /// <param name="filePath">The path to "NTRPLUV.OBJ"</param>
        /// <returns>A list of NetterUserDefinedTypeObject</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IEnumerable<NetterUserDefinedTypeObject> ReadUserBranchTypes(string filePath)
        {
            string defFileText = File.ReadAllText(filePath, Encoding.Default);
            return ParseUserTypes(defFileText, false);
        }

        private static IEnumerable<string> GetSection(string datFileText, string section)
        {
            var sectionText = SobekWaqReaderHelper.GetTextBlock(datFileText, "\\[" + section + "\\]","(\r\n *\r\n|$|\\[)");
            if (sectionText.Length == 0)
            {
                throw new FormatException(String.Format("Section [{0}] was not found", section));
            }

            return SobekWaqReaderHelper.GetTextLines(sectionText);
        }

        private static IEnumerable<NetterUserDefinedTypeObject> ParseUserTypes(string datFileText, bool nodes)
        {
            string section = nodes ? "User Node Types" : "User Branch Types";
            
            var lines = GetSection(datFileText, section);
            var nodeTypes = new List<NetterUserDefinedTypeObject>();

            const string searchText = "Number of Types=";
            var searchLine = lines.Where(l => l.StartsWith(searchText)).FirstOrDefault();
            if (searchLine == null)
            {
                throw new FormatException(String.Format("'{0}' was not found in section [{1}]", searchText, section));
            }
            int numberOfTypes = int.Parse(searchLine.Substring(searchText.Length));
            if (numberOfTypes > 0)
            {
                for (int typeIndex = 1; typeIndex <= numberOfTypes; typeIndex++)
                {
                    var typeId = GetValue(lines, typeIndex + " " + "ID" +"=", section);
                    var typeName = GetValue(lines, typeIndex + " " + "Type" +"=", section);
                    var fraction = "";
                    var surfaceWaterType = "";
                    if (nodes)
                    {
                        fraction = GetValue(lines, typeIndex + " " + "Boundary 1" + "=", section);
                    }
                    else
                    {
                        int numberOfBoundaries;
                        if (int.TryParse(GetValue(lines, typeIndex + " " + "Number of boundaries" +"=", section), out numberOfBoundaries))
                        {
                          if (numberOfBoundaries == 1) // always 0 or 1
                          {
                              fraction = GetValue(lines, typeIndex + " " + "Boundary 1" + "=", section);
                          }
                        }
                        else
                        {
                            throw new FormatException("no valid data was found");
                        }
                       surfaceWaterType = GetValue(lines, typeIndex + " " + "SurfaceWaterType" +"=", section);
                    }

                    nodeTypes.Add(new NetterUserDefinedTypeObject(typeId, typeName, fraction, surfaceWaterType));
                }
            }
            return nodeTypes;
        }

        private static string GetValue(IEnumerable<string> lines, string searchText, string section)
        {
            var searchLine = lines.Where(l => l.StartsWith(searchText)).FirstOrDefault();
            if (searchLine == null)
            {
                throw new FormatException(String.Format("'{0}' was not found in section [{1}]", searchText, section));
            }
            return searchLine.Substring(searchText.Length);
        }

        # endregion
    }
}
