using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
        private const string manholeId = "ManholeId";
        private const string compartmentShape = "CompartmentShape";

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
            iniCategory.AddProperty(RetentionRegion.Name, compartment.Name);
            iniCategory.AddProperty(RetentionRegion.NodeId, compartment.Name);
            iniCategory.AddProperty(manholeId, compartment.ParentManhole.Name);
            

            iniCategory.AddProperty(RetentionRegion.BedLevel, GetValueAsStringWithFormat(compartment.BottomLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.Area, GetValueAsStringWithFormat(compartment.ManholeLength * compartment.ManholeWidth, "{0:0.0000000}"));
            iniCategory.AddProperty(RetentionRegion.StreetLevel, GetValueAsStringWithFormat(compartment.SurfaceLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.StorageType, compartment.CompartmentStorageType.GetDisplayName());
            iniCategory.AddProperty(RetentionRegion.StreetStorageArea, GetValueAsStringWithFormat(compartment.FloodableArea, "{0:0.000}"));
            iniCategory.AddProperty(compartmentShape, compartment.Shape.ToString());
            iniCategory.AddProperty(RetentionRegion.UseTable, compartment.UseTable);
            if (compartment.UseTable)
            {
                compartment.Storage.AddStorageTable(iniCategory, compartment.Name);
            }
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
                    !(category.Properties.Any(p => string.Equals(p.Name, RetentionRegion.IsRetention.Key, StringComparison.InvariantCultureIgnoreCase)) 
                      && category.ReadProperty<bool>(RetentionRegion.IsRetention)))
                .Select(category => CreateCompartmentProperties(filePath, category))
                .ToList();
        }

        private static CompartmentProperties CreateCompartmentProperties(string filePath, DelftIniCategory category)
        {
            var properties = new CompartmentProperties
            {
                CompartmentId = category.ReadProperty<string>(RetentionRegion.Id),
                Name = category.ReadProperty<string>(RetentionRegion.Name),
                NodeId = category.ReadProperty<string>(RetentionRegion.NodeId),
                ManholeId = category.ReadProperty<string>(manholeId),
                UseTable = false
            };

            var useTable = category.ReadProperty<bool?>(RetentionRegion.UseTable, true);
            if (!useTable.HasValue)
            {
                var intValue = category.ReadProperty<int?>(RetentionRegion.UseTable, true);
                useTable = intValue.HasValue && intValue !=0;
            }

            if (useTable.Value)
            {
                log.Warn($"compartments with storage tables are not supported, using DEFAULT VALUES for " +
                         $"compartment id {properties.CompartmentId} ({properties.Name}) on " +
                         $"node id {properties.NodeId} and " +
                         $"manhole id {properties.ManholeId} is NOT read from the file {filePath}");
                
                properties.BedLevel = -10;
                properties.Area = 0.64;
                properties.StreetLevel = 0;
                properties.StreetStorageArea = 500;
                properties.CompartmentShape = CompartmentShape.Unknown;
                properties.CompartmentStorageType = CompartmentStorageType.Reservoir;
                properties.UseTable = true;
                properties.NumberOfLevels = category.ReadProperty<int>(RetentionRegion.NumLevels);
                properties.Levels = category.ReadPropertiesToListOfType<double>(RetentionRegion.Levels).ToArray();
                properties.StorageAreas = category.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea).ToArray();
                properties.Interpolation = GetInterpolationType(category.ReadProperty<string>(RetentionRegion.Interpolate));
            }
            else
            {
                properties.BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel);
                properties.Area = category.ReadProperty<double>(RetentionRegion.Area);
                properties.StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel);
                properties.StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea);
                properties.CompartmentShape = category.ReadProperty<CompartmentShape>(compartmentShape, true);
                properties.CompartmentStorageType = category.ReadProperty<CompartmentStorageType>(RetentionRegion.StorageType, true);
            }

            return properties;
        }
        private static InterpolationType GetInterpolationType(string interpolationTypeString)
        {
            switch (interpolationTypeString?.ToLower())
            {
                case "block":  return InterpolationType.Constant;
                case "linear": return InterpolationType.Linear;
                default:       return InterpolationType.None;
            }
        }
    }
}
