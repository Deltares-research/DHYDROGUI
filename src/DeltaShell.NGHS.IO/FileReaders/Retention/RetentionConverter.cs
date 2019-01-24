using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using log4net;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    public static class RetentionConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RetentionConverter));

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
                    $"Unable to parse {category.Name} property: {RetentionRegion.BranchId.Key}, Branch not found in Network.{Environment.NewLine}";
                throw new Exception(errorMessage);
            }
            var chainage = category.ReadProperty<double>(RetentionRegion.Chainage.Key);
            var storageType = category.ReadProperty<RetentionType>(RetentionRegion.StorageType.Key);
            var useTableAsInt = category.ReadProperty<int>(RetentionRegion.UseTable.Key);
            var useTable = System.Convert.ToBoolean(useTableAsInt);

            var resultingChainage = chainage / branch.Length * branch.Geometry.Length;
            var geometry = new Point(
                LengthLocationMap.GetLocation(branch.Geometry, resultingChainage).GetCoordinate(branch.Geometry));

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

            Log.ErrorFormat("UseTable is not yet implemented in the RetentionFileReader, please set UseTable to 0 to continue with this model.");
            return new DelftTools.Hydro.Retention();
        }

        private static void ValidateConvertedRetention(IRetention readRetentionPoint, IList<IRetention> generatedRetentionPoint)
        {
            if (!readRetentionPoint.IsDuplicateIn(generatedRetentionPoint)) return;
            var errorMessage =
                $"Retention point with id {readRetentionPoint.Name} already exists, there cannot be any duplicate retention ids.{Environment.NewLine}";
            throw new Exception(errorMessage);
        }

        private static bool IsDuplicateIn(this IRetention readRetention, IList<IRetention> retention)
        {
            return retention.Contains(readRetention) || retention.Any(n => n.Name == readRetention.Name);
        }
    }
}
