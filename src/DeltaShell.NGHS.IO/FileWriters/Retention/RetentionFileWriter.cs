using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.IO.Ini;
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
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RetentionMajorVersion, 
                                                             GeneralRegion.RetentionMinorVersion, 
                                                             GeneralRegion.FileTypeName.Retention),
            };

            iniSections.AddRange(retentions.Select(r => GenerateSpatialDataDefinition(r, RetentionRegion.Header)));

            FileUtils.DeleteIfExists(filename);
            
            var iniFileWriter = new IniFileWriter();
            iniFileWriter.WriteIniFile(iniSections, filename);
        }

        public static IniSection GenerateSpatialDataDefinition(IRetention retention, string sectionName)
        {
            if (retention.Branch == null)
            {
                throw new FileWritingException(Resources.RetentionFileWriter_GenerateSpatialDataDefinition_Retention_does_not_have_a_valid_Branch_property);
            }

            var definition = new IniSection(sectionName);

            definition.AddPropertyWithOptionalComment(RetentionRegion.Id.Key, retention.Name, RetentionRegion.Id.Description);
            definition.AddPropertyWithOptionalComment(RetentionRegion.Name.Key, retention.LongName ?? retention.Name, RetentionRegion.Name.Description);

            if (retention.TryGetNode(out var node))
            {
                definition.AddPropertyWithOptionalComment(RetentionRegion.NodeId.Key, node.Name, RetentionRegion.BranchId.Description);
            }
            else
            {
                definition.AddPropertyWithOptionalComment(RetentionRegion.BranchId.Key, retention.Branch.Name, RetentionRegion.BranchId.Description);
                definition.AddPropertyWithOptionalCommentAndFormat(RetentionRegion.Chainage.Key, retention.Branch.GetBranchSnappedChainage(retention.Chainage), RetentionRegion.Chainage.Description, RetentionRegion.Chainage.Format);

                if (retention.Geometry?.Coordinate != null)
                {
                    definition.AddPropertyWithOptionalCommentAndFormat(RetentionRegion.X.Key, retention.Geometry.Coordinate.X, RetentionRegion.X.Description);
                    definition.AddPropertyWithOptionalCommentAndFormat(RetentionRegion.Y.Key, retention.Geometry.Coordinate.Y, RetentionRegion.Y.Description);
                }
            }

            definition.AddProperty(RetentionRegion.UseTable.Key, true, RetentionRegion.UseTable.Description);

            if (retention.UseTable)
            {
                retention.Data.AddStorageTable(definition, retention.Name);
                return definition;
            }

            definition.AddPropertyFromConfiguration(RetentionRegion.NumLevels, 1);
            definition.AddPropertyFromConfiguration(RetentionRegion.Levels, retention.BedLevel);
            definition.AddPropertyFromConfiguration(RetentionRegion.StorageArea, retention.StorageArea);
            definition.AddPropertyFromConfiguration(RetentionRegion.Interpolate, "block");

            return definition;
        }
    }
}
