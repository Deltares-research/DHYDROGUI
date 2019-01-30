using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    public class RetentionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public RetentionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<IRetention> ReadRetention(string filePath, IList<IChannel> channelsList)
        {
            var errorMessages = new List<string>();
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            var retentionProperties = categories
                .Where(category => category.Name == RetentionRegion.Header)
                .Select(category => ReadPropertiesFromCategory(category, channelsList))
                .ToList();

            var retention = RetentionConverter.Convert(retentionProperties, errorMessages);
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke(Resources.RetentionFileReader_ReadRetention_While_reading_the_retention_from_file__an_error_occured, errorMessages);

            return retention;
        }

        private static RetentionPropertiesDTO ReadPropertiesFromCategory(DelftIniCategory category, IList<IChannel> channelsList)
        {
            var retentionProperties = new RetentionPropertiesDTO
            {
                Id = category.ReadProperty<string>(LocationRegion.Id.Key),
                LongName = category.ReadProperty<string>(RetentionRegion.Name.Key, true) ?? string.Empty,
                BranchName = category.ReadProperty<string>(RetentionRegion.BranchId.Key),
                Chainage = category.ReadProperty<double>(RetentionRegion.Chainage.Key),
                StorageType = category.ReadProperty<RetentionType>(RetentionRegion.StorageType.Key),
                UseTable = Convert.ToBoolean(category.ReadProperty<int>(RetentionRegion.UseTable.Key)),
                BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel.Key),
                StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel.Key),
                StorageArea = category.ReadProperty<double>(RetentionRegion.Area.Key),
                StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key)
            };

            retentionProperties.Branch = channelsList.FirstOrDefault(c => c.Name == retentionProperties.BranchName);
            if (retentionProperties.Branch == null)
            {
                var errorMessage =
                    string.Format(Resources.RetentionConverter_ConvertToRetention_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_, category.Name,
                        RetentionRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

           CalculateGeometry(retentionProperties);

           if (!retentionProperties.UseTable) return retentionProperties;

           var useTableErrorMessage = Resources
               .RetentionConverterTest_GivenARetentionDataModelWhichUsesUseTable_WhenConverting_ThenTheErrorReportIsProperlyFilled_UseTable_is_not_yet_implemented_in_the_RetentionFileReader__please_set_UseTable_to_0_to_continue_with_this_model_;
           throw new NotImplementedException(useTableErrorMessage);
        }

        private static void CalculateGeometry(RetentionPropertiesDTO retentionPropertiesDto)
        {
            retentionPropertiesDto.ResultingChainage =
                retentionPropertiesDto.Chainage / retentionPropertiesDto.Branch.Length *
                retentionPropertiesDto.Branch.Geometry.Length;
            retentionPropertiesDto.Coordinate = LengthLocationMap
                .GetLocation(retentionPropertiesDto.Branch.Geometry, retentionPropertiesDto.ResultingChainage)
                .GetCoordinate(retentionPropertiesDto.Branch.Geometry);
            retentionPropertiesDto.Geometry = new Point(retentionPropertiesDto.Coordinate);
        }
    }
}

