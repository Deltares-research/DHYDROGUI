using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using log4net;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    public static class RetentionConverter
    {
        public static IList<IRetention> Convert(IEnumerable<DelftIniCategory> categories, IList<IChannel> channelsList,
            IList<string> errorMessages)
        {
            IList<IRetention> retention = new List<IRetention>();
            foreach (var retentionCategory in categories.Where(category => category.Name == RetentionRegion.Header))
            {
                try
                {
                    var convertedRetention = ConvertToRetention(retentionCategory, channelsList);
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

        private static IRetention ConvertToRetention(IDelftIniCategory category, IList<IChannel> channelsList)
        {
            var id = category.ReadProperty<string>(LocationRegion.Id.Key);
            var longName = category.ReadProperty<string>(RetentionRegion.Name.Key, true) ?? string.Empty;
            var branchName = category.ReadProperty<string>(RetentionRegion.BranchId.Key);
            var branch = channelsList.FirstOrDefault(c => c.Name == branchName);
            if (branch == null)
            {
                var errorMessage =
                    string.Format(Resources.RetentionConverter_ConvertToRetention_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_, category.Name,
                        RetentionRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }
            var chainage = category.ReadProperty<double>(RetentionRegion.Chainage.Key);
            var storageType = category.ReadProperty<RetentionType>(RetentionRegion.StorageType.Key);
            var useTableAsInt = category.ReadProperty<int>(RetentionRegion.UseTable.Key);
           
            var resultingChainage = chainage / branch.Length * branch.Geometry.Length;
            var coordinate = LengthLocationMap.GetLocation(branch.Geometry, resultingChainage).GetCoordinate(branch.Geometry);
            var geometry = new Point(coordinate);

            var useTable = System.Convert.ToBoolean(useTableAsInt);
            if (!useTable)
            {
                var bedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel.Key);
                var streetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel.Key);
                var storageArea = category.ReadProperty<double>(RetentionRegion.Area.Key);
                var streetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key);

                return new DelftTools.Hydro.Retention
                {
                    Name = id,
                    LongName = longName,
                    Branch = branch,
                    Chainage = chainage,
                    Type = storageType,
                    UseTable = false,
                    BedLevel = bedLevel,
                    StreetLevel = streetLevel,
                    StorageArea = storageArea,
                    StreetStorageArea = streetStorageArea,
                    Geometry = geometry,
                };
            }

            var useTableErrorMessage = Resources.RetentionConverterTest_GivenARetentionDataModelWhichUsesUseTable_WhenConverting_ThenTheErrorReportIsProperlyFilled_UseTable_is_not_yet_implemented_in_the_RetentionFileReader__please_set_UseTable_to_0_to_continue_with_this_model_;
            throw new NotImplementedException(useTableErrorMessage);
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
