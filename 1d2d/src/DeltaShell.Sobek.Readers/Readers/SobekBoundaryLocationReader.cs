using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekBoundaryLocationReader : SobekReader<SobekBoundaryLocation>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekBoundaryLocationReader));

        public SobekBoundaryLocationReader()
        {
            SobekType = SobekType.Sobek212;
        }

        private SobekType sobekType;

        public SobekType SobekType
        {
            get { return sobekType; }
            set { sobekType = value; }
        }

        public override IEnumerable<SobekBoundaryLocation> Parse(string text)
        {
            string boundaryLocationsPattern =
                @"(FLBO (?'text'.*?) flbo)|" +
                @"(FLBR (?'text'.*?)\sflbr)|" +
                @"(FLNO (?'text'.*?)\sflno)|" +
                @"(FLDI (?'text'.*?)\sfldi)|" +
                @"(FLNX (?'text'.*?)\sflnx)|" +
                @"(FLBX (?'text'.*?)\sflbx)";


            if (SobekType == SobekType.SobekRE)
            {
                boundaryLocationsPattern +=
                    @"|(STBO (?'text'.*?)\sstbo)|" +
                    @"(STBR (?'text'.*?)\sstbr)";                
            }

            foreach (Match match in RegularExpression.GetMatches(boundaryLocationsPattern, text))
            {
                SobekBoundaryLocation sobekBoundaryLocation = GetBoundaryLocation(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public SobekBoundaryLocation GetBoundaryLocation(string record)
        {
            SobekBoundaryLocation sobekBoundaryLocation = new SobekBoundaryLocation();
            if (record.StartsWith("FLBR"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.Branch;
            }
            else if (record.StartsWith("FLBX"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.Branch; //RR
            }
            else if (record.StartsWith("FLDI"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.Diffuse;
            }
            else if (record.StartsWith("FLBO"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.Node;
            }
            else if (record.StartsWith("FLNO") || record.StartsWith("FLNX"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.LateralAtNode;
            }
            // --- tags STBO and STBR are only valid for Sobek RE models (for the file that is read at this level of import)
            else if (record.StartsWith("STBO"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.SaltNode;
            }
            else if (record.StartsWith("STBR"))
            {
                sobekBoundaryLocation.SobekBoundaryLocationType = SobekBoundaryLocationType.SaltLateral;
            }
            // -------------------------------------------------------------------------------------------------------------
            else
            {
                Log.WarnFormat("Boundary location record {0} unsupported, skipped.", record);
                return null;
            }
            
            sobekBoundaryLocation.Id = RegularExpression.ParseFieldAsString("id", record);
            sobekBoundaryLocation.Name = RegularExpression.ParseFieldAsString("nm", record);
            sobekBoundaryLocation.ConnectionId = RegularExpression.ParseFieldAsString("ci", record);

            if ((sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.Branch) ||
                (sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.SaltLateral||
                sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.Diffuse))
            {
                sobekBoundaryLocation.Offset = RegularExpression.ParseFieldAsDouble("lc", record);
            }
            return sobekBoundaryLocation;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "flbo";
            yield return "flbr";
            yield return "flno";
            yield return "fldi";
            yield return "flbx";
            yield return "flnx";
            if (SobekType == SobekType.SobekRE)
            {
                yield return "stbo";
                yield return "stbr";
            }
        }
    }
}
