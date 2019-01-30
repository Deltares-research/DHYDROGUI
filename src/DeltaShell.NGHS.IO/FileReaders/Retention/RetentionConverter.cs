using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    public static class RetentionConverter
    {
        public static IList<IRetention> Convert(List<RetentionPropertiesDTO> retentionProperties, IList<string> errorMessages)
        {
            IList<IRetention> retention = new List<IRetention>();
            foreach (var retentionProperty in retentionProperties)
            {
                try
                {
                    var convertedRetention = ConvertToRetention(retentionProperty);
                    ValidateConvertedRetention(convertedRetention, retention);
                    retention.Add(convertedRetention);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            return retention;
        }

        private static IRetention ConvertToRetention(RetentionPropertiesDTO retentionPropertyDto)
        {
            return new DelftTools.Hydro.Retention
            {
                Name = retentionPropertyDto.Id,
                LongName = retentionPropertyDto.LongName,
                Branch = retentionPropertyDto.Branch,
                Chainage = retentionPropertyDto.Chainage,
                Type = retentionPropertyDto.StorageType,
                UseTable = retentionPropertyDto.UseTable,
                BedLevel = retentionPropertyDto.BedLevel,
                StreetLevel = retentionPropertyDto.StreetLevel,
                StorageArea = retentionPropertyDto.StorageArea,
                StreetStorageArea = retentionPropertyDto.StreetStorageArea,
                Geometry = retentionPropertyDto.Geometry
            };
        }

        private static void ValidateConvertedRetention(IRetention readRetention, IList<IRetention> generatedRetention)
        {
            if (!readRetention.IsDuplicateIn(generatedRetention)) return;
            var errorMessage =
                string.Format(
                    Resources.RetentionConverter_ValidateConvertedRetention_Retention_point_with_id__0__already_exists__there_cannot_be_any_duplicate_retention_ids__1_,
                    readRetention.Name, Environment.NewLine);
            throw new Exception(errorMessage);
        }

        private static bool IsDuplicateIn(this IRetention readRetention, IList<IRetention> retention)
        {
            return retention.Contains(readRetention) || retention.Any(n => n.Name == readRetention.Name);
        }
    }
}
