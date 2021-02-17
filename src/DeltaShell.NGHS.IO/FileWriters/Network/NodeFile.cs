using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class NodeFile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NodeFile));

        public static void Write(string filePath, IEnumerable<ICompartment> compartments, IEnumerable<IRetention> retentions)
        {

            var categories = new List<DelftIniCategory>();
            if (compartments != null && compartments.Any())
            {
                categories.AddRange(compartments.Select(CreateCompartmentIniCategory).ToList());
            }

            if (retentions != null && retentions.Any())
            {
                categories.AddRange(retentions.Select(GenerateRetentionStorageNode).ToList());
            }

            if (categories.Any())
            {
                var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RetentionMajorVersion,
                    GeneralRegion.RetentionMinorVersion,
                    GeneralRegion.FileTypeName.StorageNodes);
                generalRegion.AddProperty("useStreetStorage", "1");
                categories.Insert(0,generalRegion);
                new DelftIniWriter().WriteDelftIniFile(categories, filePath, false);
            }
        }

        private static DelftIniCategory GenerateRetentionStorageNode(IRetention retention)
        {
            var category = RetentionFileWriter.GenerateSpatialDataDefinition(retention);
            TypeUtils.SetPrivatePropertyValue(category, "Name","StorageNode");
            return category;
        }

        private static DelftIniCategory CreateCompartmentIniCategory(ICompartment compartment)
        {
            var iniCategory = new DelftIniCategory("StorageNode");
            iniCategory.AddProperty(RetentionRegion.Id, compartment.Name);
            iniCategory.AddProperty(RetentionRegion.Name.Key, compartment.Name);
            iniCategory.AddProperty(RetentionRegion.NodeId.Key, compartment.Name);
            iniCategory.AddProperty(KnownPropertyNames.ManholeId, compartment.ParentManhole.Name);
            iniCategory.AddProperty(RetentionRegion.UseTable.Key, false);

            iniCategory.AddProperty(RetentionRegion.BedLevel, GetValueAsStringWithFormat(compartment.BottomLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.Area, GetValueAsStringWithFormat(compartment.ManholeLength * compartment.ManholeWidth, "{0:0.0000000}"));
            iniCategory.AddProperty(RetentionRegion.StreetLevel, GetValueAsStringWithFormat(compartment.SurfaceLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.StorageType, compartment.CompartmentStorageType.GetDisplayName());
            iniCategory.AddProperty(RetentionRegion.StreetStorageArea, GetValueAsStringWithFormat(compartment.FloodableArea, "{0:0.000}"));
            iniCategory.AddProperty(KnownPropertyNames.CompartmentShape, compartment.Shape.ToString());
            return iniCategory;
        }

        private static string GetValueAsStringWithFormat(double value, string format)
        {
            return string.Format(CultureInfo.InvariantCulture, format, value);
        }

        public static List<CompartmentProperties> Read(string filePath)
        {
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();

            return categories
                .Skip(1) // skip version info
                .Where(category => //Don't read retentions here
                    !(category.Properties.Any(p => p.Name.Equals(RetentionRegion.IsRetention.Key, StringComparison.InvariantCultureIgnoreCase)) 
                      && category.ReadProperty<bool>(RetentionRegion.IsRetention.Key)))
                .Select(category =>
                {
                    var compartmentId = category.ReadProperty<string>(RetentionRegion.Id.Key);
                    var name = category.ReadProperty<string>(RetentionRegion.Name.Key);
                    var nodeId = category.ReadProperty<string>(RetentionRegion.NodeId.Key);
                    var manholeId = category.ReadProperty<string>(KnownPropertyNames.ManholeId);
                    var useTable = category.ReadProperty<bool>(RetentionRegion.UseTable.Key, true) ||
                                       category.ReadProperty<int>(RetentionRegion.UseTable.Key, true) != 0;
                    if (useTable)
                    {
                        log.Warn($"compartments with storage tables are not supported, using DEFAULT VALUES for " +
                                 $"compartment id {compartmentId} ({name}) on " +
                                 $"node id {nodeId} and " +
                                 $"manhole id {manholeId} is NOT read from the file {filePath}");
                        return new CompartmentProperties
                        {
                            CompartmentId = compartmentId,
                            Name = name,
                            NodeId = nodeId,
                            ManholeId = manholeId,
                            UseTable = false,
                            BedLevel = -10,
                            Area = 0.64,
                            StreetLevel = 0,
                            StreetStorageArea = 500,
                            CompartmentShape = CompartmentShape.Unknown,
                            CompartmentStorageType = CompartmentStorageType.Reservoir
                        };
                    }

                    return new CompartmentProperties
                    {
                        CompartmentId = compartmentId,
                        Name = name,
                        NodeId = nodeId,
                        ManholeId = manholeId,
                        UseTable = false,
                        BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel.Key),
                        Area = category.ReadProperty<double>(RetentionRegion.Area.Key),
                        StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel.Key),
                        StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key),
                        CompartmentShape = category.ReadProperty<CompartmentShape>(KnownPropertyNames.CompartmentShape,true), 
                        CompartmentStorageType = category.ReadProperty<CompartmentStorageType>(RetentionRegion.StorageType.Key, true)
                    };

                })
                .ToList();
        }
        
        private static class KnownPropertyNames
        {
            public const string ManholeId = "ManholeId";
            public const string CompartmentShape = "CompartmentShape";
        }

        public class CompartmentProperties
        {
            public string CompartmentId { get; set; }

            public string Name { get; set; }

            public string NodeId { get; set; }

            public string ManholeId { get; set; }

            public bool UseTable { get; set; }

            public double BedLevel { get; set; }

            public double Area { get; set; }

            public double StreetLevel { get; set; }
            
            public double StreetStorageArea { get; set; }

            public CompartmentShape CompartmentShape { get; set; }

            public CompartmentStorageType CompartmentStorageType { get; set; }
        }
    }
}
