using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;

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

            FileUtils.DeleteIfExists(filename);
            
            var iniFileWriter = new IniFileWriter();
            iniFileWriter.WriteIniFile(categories, filename);
        }

        public static DelftIniCategory GenerateSpatialDataDefinition(IRetention retention)
        {
            if (retention.Branch == null)
            {
                throw new FileWritingException(Resources.RetentionFileWriter_GenerateSpatialDataDefinition_Retention_does_not_have_a_valid_Branch_property);
            }

            var definition = new DelftIniCategory(RetentionRegion.Header);

            definition.AddProperty(RetentionRegion.Id.Key, retention.Name, RetentionRegion.Id.Description);
            definition.AddProperty(RetentionRegion.Name.Key, retention.LongName ?? retention.Name, RetentionRegion.Name.Description);

            if (retention.TryGetNode(out var node))
            {
                definition.AddProperty(RetentionRegion.NodeId.Key, node.Name, RetentionRegion.BranchId.Description);
            }
            else
            {
                definition.AddProperty(RetentionRegion.BranchId.Key, retention.Branch.Name, RetentionRegion.BranchId.Description);
                definition.AddProperty(RetentionRegion.Chainage.Key, retention.Branch.GetBranchSnappedChainage(retention.Chainage), RetentionRegion.Chainage.Description, RetentionRegion.Chainage.Format);

                if (retention.Geometry?.Coordinate != null)
                {
                    definition.AddProperty(RetentionRegion.X.Key, retention.Geometry.Coordinate.X, RetentionRegion.X.Description);
                    definition.AddProperty(RetentionRegion.Y.Key, retention.Geometry.Coordinate.Y, RetentionRegion.Y.Description);
                }
            }

            definition.AddProperty(RetentionRegion.UseTable.Key, true, RetentionRegion.UseTable.Description);

            if (retention.UseTable)
            {
                retention.Data.AddStorageTable(definition, retention.Name);
                return definition;
            }

            definition.AddProperty(RetentionRegion.NumLevels, 1);
            definition.AddProperty(RetentionRegion.Levels, retention.BedLevel);
            definition.AddProperty(RetentionRegion.StorageArea, retention.StorageArea);
            definition.AddProperty(RetentionRegion.Interpolate, "block");

            return definition;
        }
    }
}
