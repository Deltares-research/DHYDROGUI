using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class RetentionFileReader
    {
        public static void ReadFile(string retentionFile, IHydroNetwork network)
        {
            if (!File.Exists(retentionFile)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", retentionFile));
            var categories = new DelftIniReader().ReadDelftIniFile(retentionFile);
            if (categories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", retentionFile));
            categories
                .Skip(1) // skip general section
                .Where(category => //Only read retentions here
                category.Properties.Any(p => p.Name.Equals(RetentionRegion.IsRetention.Key, StringComparison.InvariantCultureIgnoreCase))
                  && category.ReadProperty<bool>(RetentionRegion.IsRetention.Key)).ForEach(c =>
                {
                    var retention = new Retention();
                    retention.Name = c.ReadProperty<string>(RetentionRegion.Id.Key);
                    retention.LongName = c.ReadProperty<string>(RetentionRegion.Name.Key);
                    var branchId = c.ReadProperty<string>(RetentionRegion.BranchId.Key);
                    var branch = network.Branches.FirstOrDefault(b =>b.Name.Equals(branchId, StringComparison.InvariantCultureIgnoreCase));
                    if (branch == null) return;
                    retention.Branch = branch;
                    retention.Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(c.ReadProperty<double>(RetentionRegion.Chainage.Key));
                    var retentionType = c.ReadProperty<string>(RetentionRegion.StorageType.Key);
                    RetentionType type;
                    if(Enum.TryParse(retentionType,true, out type))
                        retention.Type = type;
                    retention.UseTable = c.ReadProperty<int>(RetentionRegion.UseTable.Key) == 1;
                    if (retention.Chainage > 0)
                    {
                        var x = c.ReadProperty<double>(RetentionRegion.X.Key);
                        var y = c.ReadProperty<double>(RetentionRegion.Y.Key);
                        retention.Geometry = new Point(x,y);
                    }
                    else
                    {
                        //iets doen met NodeId?
                        var geometryCoordinate = retention?.Branch?.Source?.Geometry?.Coordinate;
                        if(geometryCoordinate != null)
                            retention.Geometry = new Point(geometryCoordinate);
                    }

                    if (retention.UseTable)
                    {
                        var interpolationTypeString = c.ReadProperty<string>(RetentionRegion.Interpolate.Key);
                        InterpolationType t = interpolationTypeString == "block" ? InterpolationType.Constant : InterpolationType.None;
                        if (!Enum.TryParse(interpolationTypeString, true, out t))
                        {
                            //euh loggen ofzo?
                        }
                        retention.Data.Arguments[0].InterpolationType = t;
                        System.Collections.Generic.IList<double> levels = null;
                        if (c.GetPropertyValue(RetentionRegion.Levels.Key, null) != null)
                        {
                            levels = c.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key);
                        }
                        System.Collections.Generic.IList<double> storageAreas = null;
                        if (c.GetPropertyValue(RetentionRegion.StorageArea.Key, null) != null)
                        {
                            storageAreas = c.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key);
                        }
                        if (levels != null && storageAreas != null)
                        {
                            retention.Data.Arguments[0].SetValues(levels);
                            retention.Data.Components[0].SetValues(storageAreas);
                        }
                    }
                    else
                    {
                        retention.BedLevel = c.ReadProperty<double>(RetentionRegion.BedLevel.Key);
                        retention.StorageArea = c.ReadProperty<double>(RetentionRegion.Area.Key);
                        retention.StreetLevel = c.ReadProperty<double>(RetentionRegion.StreetLevel.Key);
                        retention.StreetStorageArea = c.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key);
                    }
                    retention.Branch.BranchFeatures.Add(retention);
                });



        }
    }
}