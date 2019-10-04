using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
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
            definition.AddProperty(RetentionRegion.Name.Key, retention.LongName, RetentionRegion.Name.Description);

            definition.AddProperty(RetentionRegion.BranchId.Key, retention.Branch.Name, RetentionRegion.BranchId.Description);
            definition.AddProperty(RetentionRegion.Chainage.Key, retention.Chainage, RetentionRegion.Chainage.Description, RetentionRegion.Chainage.Format);
            
            definition.AddProperty(RetentionRegion.StorageType.Key, retention.Type.ToString(), RetentionRegion.StorageType.Description);
            definition.AddProperty(RetentionRegion.UseTable.Key, retention.UseTable ? 1 : 0, RetentionRegion.UseTable.Description);
            if (retention.UseTable)
            {
                if (retention.Data == null || retention.Data.Arguments == null || retention.Data.Arguments.Count <= 0 ||
                    retention.Data.Components == null || retention.Data.Components.Count <= 0) return definition;

                var levels = retention.Data.Arguments[0].Values as IList<double>;
                if (levels == null)
                {
                    var cannotWriteRetentionArea = string.Format("Cannot write retention area with id : {0} because levels / heighes in table is not defined as a list of doubles.", retention.Name);
                    throw new FileWritingException(cannotWriteRetentionArea);
                }
                

                var storageAreas = retention.Data.Components[0].Values as IList<double>;
                if (storageAreas == null)
                {
                    var cannotWriteRetentionArea = string.Format("Cannot write retention area with id : {0} because storage areas in table is not defined as a list of doubles.", retention.Name);
                    throw new FileWritingException(cannotWriteRetentionArea);
                }
                

                var interpolateType = retention.Data.Arguments[0].InterpolationType;

                if (interpolateType == InterpolationType.None)
                {
                    var cannotWriteRetentionArea = string.Format("Cannot write retention area with id : {0} because interpolation type is set to 'None'. Core cannot handle this type", retention.Name);
                    throw new FileWritingException(cannotWriteRetentionArea);
                }
                
                definition.AddProperty(RetentionRegion.NumLevels.Key, levels.Count, RetentionRegion.NumLevels.Description);
                definition.AddProperty(RetentionRegion.Levels.Key, levels, RetentionRegion.Levels.Description, RetentionRegion.Levels.Format);
                definition.AddProperty(RetentionRegion.StorageArea.Key, storageAreas, RetentionRegion.StorageArea.Description, RetentionRegion.StorageArea.Format);
                definition.AddProperty(RetentionRegion.Interpolate.Key, interpolateType == InterpolationType.Linear ? 0:1, RetentionRegion.Interpolate.Description);
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
