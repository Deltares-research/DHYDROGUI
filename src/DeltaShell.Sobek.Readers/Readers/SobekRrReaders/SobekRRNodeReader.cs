using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRNodeReader : SobekReader<SobekRRNode>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRNodeReader));

        public override IEnumerable<SobekRRNode> Parse(string fileContent)
        {
            const string rrNodePattern = @"NODE(?'text'.*?)node" + RegularExpression.EndOfLine;

            return (from Match rrNodeLine in RegularExpression.GetMatches(rrNodePattern, fileContent)
                    select GetSobekRRNode(rrNodeLine.Value)).ToList();
        }

        private static SobekRRNode GetSobekRRNode(string line)
        {
            //id   =          node identification
            //nm  =          name of the node 
            //ri    =          reach identification 
            //mt  =          model nodetype
            //nt   =          netter nodetype
            //ObID =      Object id 
            //px  =          position X (X coordinate)
            //py  =          position Y (Y coordinate)

            var sobekRRNode = new SobekRRNode();


            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.Id = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.Name = matches[0].Groups[label].Value;
            }

            //Branch Id
            label = "ri";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.ReachId = matches[0].Groups[label].Value;
            }

            //Object Id
            label = "ObID";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.ObjectTypeName = matches[0].Groups[label].Value;
            }

            //Netter Type
            label = "nt";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.NetterType = Convert.ToInt32((string) matches[0].Groups[label].Value);
            }

            //NodeType
            var ipos = line.IndexOf("mt", StringComparison.CurrentCulture);
            if (ipos > 0)
            {
                var bufstr = line.Substring(ipos + 3);
                bufstr = bufstr.Replace("'", "");
                var items = bufstr.SplitOnEmptySpace();

                if (items.Count() >= 2)
                {
                    var ntAsInt = Convert.ToInt32(items[1]);
                    if (Enum.IsDefined(typeof (SobekRRNodeType), ntAsInt))
                    {
                        sobekRRNode.NodeType = (SobekRRNodeType) ntAsInt;
                    }
                    else
                    {
                        log.ErrorFormat("Node type of {0} is unkown.", sobekRRNode.Id);
                    }
                }
            }

            //X
            label = "px";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.X = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Y
            label = "py";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRNode.Y = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekRRNode;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "node";
        }
    }
}
