using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class RetentionFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RetentionFileReader));
        
        public static void ReadFile(string retentionFile, IHydroNetwork network)
        {
            if (!File.Exists(retentionFile))
                throw new FileReadingException($"Could not read file {retentionFile} properly, it doesn't exist.");

            var categories = new DelftIniReader().ReadDelftIniFile(retentionFile);
            if (categories.Count == 0)
                throw new FileReadingException($"Could not read file {retentionFile} properly, it seems empty");

            categories
                .Skip(1) // skip general section
                .Where(IsRetention)
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

            retention.UseTable = category.ReadProperty<int>(RetentionRegion.NumLevels.Key) > 1;
            if (!retention.UseTable)
            {
                retention.BedLevel = category.ReadProperty<double>(RetentionRegion.Levels.Key);
                retention.StorageArea = category.ReadProperty<double>(RetentionRegion.StorageArea.Key);
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
        
        private static bool IsRetention(DelftIniCategory category)
        {
            IDelftIniProperty useTableProperty = category.GetProperty(RetentionRegion.UseTable.Key);
            if (useTableProperty == null)
            {
                log.WarnFormat(Resources.NodeFile_The_category_does_not_contain_property, 
                               category.Name, category.LineNumber, RetentionRegion.UseTable.Key);
                return false;
            }

            return useTableProperty.ReadValue<bool>();
        }
    }
}