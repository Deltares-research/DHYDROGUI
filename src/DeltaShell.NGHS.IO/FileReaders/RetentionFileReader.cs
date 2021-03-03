using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class RetentionFileReader
    {
        public static void ReadFile(string retentionFile, IHydroNetwork network)
        {
            if (!File.Exists(retentionFile))
                throw new FileReadingException($"Could not read file {retentionFile} properly, it doesn't exist.");

            var categories = new DelftIniReader().ReadDelftIniFile(retentionFile);
            if (categories.Count == 0)
                throw new FileReadingException($"Could not read file {retentionFile} properly, it seems empty");

            categories
                .Skip(1) // skip general section
                .Where(c => //Only read retentions here
                    c.Properties.Any(p => p.Name.Equals(RetentionRegion.IsRetention.Key, StringComparison.InvariantCultureIgnoreCase))
                    && c.ReadProperty<bool>(RetentionRegion.IsRetention.Key))
                .Select(c => ReadRetention(network, c))
                .Where(r => r != null)
                .ForEach(r =>
                {
                    r.Branch?.BranchFeatures.Add(r);
                });
        }

        private static Retention ReadRetention(INetwork network, DelftIniCategory category)
        {
            var retention = new Retention
            {
                Name = category.ReadProperty<string>(RetentionRegion.Id.Key),
                LongName = category.ReadProperty<string>(RetentionRegion.Name.Key)
            };

            var branchId = category.ReadProperty<string>(RetentionRegion.BranchId.Key);
            var branch = network.Branches.FirstOrDefault(b => string.Equals(b.Name, branchId, StringComparison.InvariantCultureIgnoreCase));
            if (branch == null)
                return retention;

            retention.Branch = branch;
            retention.Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(RetentionRegion.Chainage.Key));

            if (retention.Chainage > 0)
            {
                var x = category.ReadProperty<double>(RetentionRegion.X.Key);
                var y = category.ReadProperty<double>(RetentionRegion.Y.Key);
                retention.Geometry = new Point(x, y);
            }
            else
            {
                //iets doen met NodeId?
                var geometryCoordinate = retention?.Branch?.Source?.Geometry?.Coordinate;
                if (geometryCoordinate != null)
                    retention.Geometry = new Point(geometryCoordinate);
            }

            var retentionType = category.ReadProperty<string>(RetentionRegion.StorageType.Key);
            if (Enum.TryParse(retentionType, true, out RetentionType type))
                retention.Type = type;

            retention.UseTable = category.ReadProperty<int>(RetentionRegion.UseTable.Key) == 1;

            if (!retention.UseTable)
            {
                retention.BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel.Key);
                retention.StorageArea = category.ReadProperty<double>(RetentionRegion.Area.Key);
                retention.StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel.Key);
                retention.StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key);
                return retention;
            }

            var interpolationTypeString = category.ReadProperty<string>(RetentionRegion.Interpolate.Key);
            retention.Data.Arguments[0].InterpolationType = GetInterpolationType(interpolationTypeString);

            var levels = category.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key, true);
            var storageAreas = category.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key, true);

            if (levels == null || storageAreas == null)
                return retention;

            retention.Data.Arguments[0].SetValues(levels);
            retention.Data.Components[0].SetValues(storageAreas);

            return retention;
        }

        private static InterpolationType GetInterpolationType(string interpolationTypeString)
        {
            switch (interpolationTypeString?.ToLower())
            {
                case "block": return InterpolationType.Constant;
                case "linear": return InterpolationType.Linear;
                default: return InterpolationType.None;
            }
        }
    }
}