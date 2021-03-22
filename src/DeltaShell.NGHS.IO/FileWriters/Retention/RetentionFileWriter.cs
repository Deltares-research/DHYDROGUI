using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Retention
{
    public static class RetentionFileWriter
    {
        public static void WriteFile(string filename, IEnumerable<IRetention> retentions)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RetentionMajorVersion, 
                                                             GeneralRegion.RetentionMinorVersion, 
                                                             GeneralRegion.FileTypeName.Retention),
            };

            categories.AddRange(retentions.Select(GenerateSpatialDataDefinition));

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        public static DelftIniCategory GenerateSpatialDataDefinition(IRetention retention)
        {
            if (retention.Branch == null) throw new FileWritingException("Retention does not have a valid Branch property");
            var definition = new DelftIniCategory(RetentionRegion.Header);

            definition.AddProperty(RetentionRegion.Id.Key, retention.Name, RetentionRegion.Id.Description);
            definition.AddProperty(RetentionRegion.Name.Key, retention.LongName ?? retention.Name, RetentionRegion.Name.Description);
            definition.AddProperty(RetentionRegion.IsRetention.Key, "true", RetentionRegion.IsRetention.Description);

            definition.AddProperty(RetentionRegion.BranchId.Key, retention.Branch.Name, RetentionRegion.BranchId.Description);
            if (retention.Chainage > 0 && retention.Geometry?.Coordinate != null)
            {
                definition.AddProperty(RetentionRegion.X.Key, retention.Geometry.Coordinate.X, RetentionRegion.X.Description);
                definition.AddProperty(RetentionRegion.Y.Key, retention.Geometry.Coordinate.Y, RetentionRegion.Y.Description);
            }
            else
            {
                definition.AddProperty(RetentionRegion.NodeId.Key, retention.Branch.Source.Name, RetentionRegion.BranchId.Description);
            }

            definition.AddProperty(RetentionRegion.Chainage.Key, retention.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(retention.Chainage), RetentionRegion.Chainage.Description, RetentionRegion.Chainage.Format);
            
            definition.AddProperty(RetentionRegion.StorageType.Key, retention.Type.ToString(), RetentionRegion.StorageType.Description);
            definition.AddProperty(RetentionRegion.UseTable.Key, retention.UseTable ? 1 : 0, RetentionRegion.UseTable.Description);
            if (retention.UseTable)
            {
                retention.Data.AddStorageTable(definition, retention.Name);
                return definition;
            }
            else
            {
                definition.AddProperty(RetentionRegion.BedLevel.Key, retention.BedLevel, RetentionRegion.BedLevel.Description, RetentionRegion.BedLevel.Format);
                definition.AddProperty(RetentionRegion.Area.Key, retention.StorageArea, RetentionRegion.Area.Description, RetentionRegion.BedLevel.Format);
                definition.AddProperty(RetentionRegion.StreetLevel.Key, retention.StreetLevel, RetentionRegion.StreetLevel.Description, RetentionRegion.BedLevel.Format);
                definition.AddProperty(RetentionRegion.StreetStorageArea.Key, retention.StreetStorageArea, RetentionRegion.StreetStorageArea.Description, RetentionRegion.BedLevel.Format);
            }


            return definition;
        }
    }
}
