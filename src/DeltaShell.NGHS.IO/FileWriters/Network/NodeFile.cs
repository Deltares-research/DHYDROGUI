using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            

            iniCategory.AddProperty(RetentionRegion.BedLevel, GetValueAsStringWithFormat(compartment.BottomLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.Area, GetValueAsStringWithFormat(compartment.ManholeLength * compartment.ManholeWidth, "{0:0.0000000}"));
            iniCategory.AddProperty(RetentionRegion.StreetLevel, GetValueAsStringWithFormat(compartment.SurfaceLevel, "{0:0.000}"));
            iniCategory.AddProperty(RetentionRegion.StorageType, compartment.CompartmentStorageType.GetDisplayName());
            iniCategory.AddProperty(RetentionRegion.StreetStorageArea, GetValueAsStringWithFormat(compartment.FloodableArea, "{0:0.000}"));
            iniCategory.AddProperty(KnownPropertyNames.CompartmentShape, compartment.Shape.ToString());
            iniCategory.AddProperty(RetentionRegion.UseTable, compartment.UseTable);
            if (compartment.UseTable)
            {
                AddStorageTable(compartment, iniCategory);
            }
            return iniCategory;
        }

        private static void AddStorageTable(ICompartment compartment, DelftIniCategory iniCategory)
        {
            if (compartment.Storage == null || compartment.Storage.Arguments == null || compartment.Storage.Arguments.Count <= 0 ||
                compartment.Storage.Components == null || compartment.Storage.Components.Count <= 0)
            {
                return;
            }

            var levels = compartment.Storage.Arguments[0].Values as IList<double>;
            if (levels == null)
            {
                var cannotWriteRetentionArea = string.Format("Cannot write compartment storage area with id : {0} because levels / heights in table is not defined as a list of doubles.", compartment.Name);
                throw new FileWritingException(cannotWriteRetentionArea);
            }

            var storageAreas = compartment.Storage.Components[0].Values as IList<double>;
            if (storageAreas == null)
            {
                var cannotWriteRetentionArea = string.Format("Cannot write compartment storage area with id : {0} because storage areas in table is not defined as a list of doubles.", compartment.Name);
                throw new FileWritingException(cannotWriteRetentionArea);
            }

            var interpolateType = compartment.Storage.Arguments[0].InterpolationType;

            if (interpolateType == InterpolationType.None)
            {
                var cannotWriteRetentionArea = string.Format("Cannot write compartment storage area with id : {0} because interpolation type is set to 'None'. Core cannot handle this type", compartment.Name);
                throw new FileWritingException(cannotWriteRetentionArea);
            }

            iniCategory.AddProperty(RetentionRegion.NumLevels, levels.Count);
            iniCategory.AddProperty(RetentionRegion.Levels.Key, levels, RetentionRegion.Levels.Description, RetentionRegion.Levels.Format);
            iniCategory.AddProperty(RetentionRegion.StorageArea.Key, storageAreas, RetentionRegion.StorageArea.Description, RetentionRegion.StorageArea.Format);
            iniCategory.AddProperty(RetentionRegion.Interpolate, interpolateType == InterpolationType.Linear ? "linear" : "block");
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
                .Select(category => CreateCompartmentProperties(filePath, category))
                .ToList();
        }

        private static CompartmentProperties CreateCompartmentProperties(string filePath, DelftIniCategory category)
        {
            var properties = new CompartmentProperties
            {
                CompartmentId = category.ReadProperty<string>(RetentionRegion.Id.Key),
                Name = category.ReadProperty<string>(RetentionRegion.Name.Key),
                NodeId = category.ReadProperty<string>(RetentionRegion.NodeId.Key),
                ManholeId = category.ReadProperty<string>(KnownPropertyNames.ManholeId),
                UseTable = false
            };
            
            var useTable = category.ReadProperty<bool>(RetentionRegion.UseTable.Key, true) ||
                           category.ReadProperty<int>(RetentionRegion.UseTable.Key, true) != 0;
            if (useTable)
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
                properties.NumberOfLevels = category.ReadProperty<int>(RetentionRegion.NumLevels.Key);
                properties.Levels = category.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key).ToArray();
                properties.StorageAreas = category.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key).ToArray();
                properties.Interpolation = GetInterpolationType(category.ReadProperty<string>(RetentionRegion.Interpolate.Key));
            }
            else
            {
                properties.BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel.Key);
                properties.Area = category.ReadProperty<double>(RetentionRegion.Area.Key);
                properties.StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel.Key);
                properties.StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea.Key);
                properties.CompartmentShape = category.ReadProperty<CompartmentShape>(KnownPropertyNames.CompartmentShape, true);
                properties.CompartmentStorageType = category.ReadProperty<CompartmentStorageType>(RetentionRegion.StorageType.Key, true);
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

            public int NumberOfLevels { get; set; }
            public double[] Levels { get; set; }
            public double[] StorageAreas { get; set; }
            public InterpolationType Interpolation { get; set; }= InterpolationType.Linear;
        }
    }
}
