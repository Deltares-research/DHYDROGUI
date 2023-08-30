using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class NodeFile
    {
        private const string manholeId = "ManholeId";
        private const string compartmentShape = "CompartmentShape";

        public static void Write(string filePath, IEnumerable<ICompartment> compartments, IEnumerable<IRetention> retentions)
        {

            var categories = new List<DelftIniCategory>();
            if (compartments != null)
            {
                categories.AddRange(compartments.Select(CreateCompartmentIniCategory).ToList());
            }

            if (retentions != null)
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
            TypeUtils.SetPrivatePropertyValue(category, "Name", RetentionRegion.StorageNodeHeader);
            return category;
        }

        private static DelftIniCategory CreateCompartmentIniCategory(ICompartment compartment)
        {
            var iniCategory = new DelftIniCategory(RetentionRegion.StorageNodeHeader);
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

        public static IList<CompartmentProperties> Read(string filePath)
        {
            var categories = new DelftIniReader().ReadDelftIniFile(filePath);

            return categories
                .Skip(1) // skip version info
                .Where(IsManHole)
                .Select(CreateCompartmentProperties)
                .ToList();
        }
        
        private static CompartmentProperties CreateCompartmentProperties(DelftIniCategory category)
        {
            var properties = new CompartmentProperties
            {
                CompartmentId = category.ReadProperty<string>(RetentionRegion.Id),
                Name = category.ReadProperty<string>(RetentionRegion.Name),
                NodeId = category.ReadProperty<string>(RetentionRegion.NodeId),
                ManholeId = category.ReadProperty<string>(manholeId),
                BedLevel = category.ReadProperty<double>(RetentionRegion.BedLevel),
                Area = category.ReadProperty<double>(RetentionRegion.Area),
                StreetLevel = category.ReadProperty<double>(RetentionRegion.StreetLevel),
                StreetStorageArea = category.ReadProperty<double>(RetentionRegion.StreetStorageArea),
                CompartmentShape = category.ReadProperty<CompartmentShape>(compartmentShape, true),
                CompartmentStorageType = category.ReadProperty<CompartmentStorageType>(RetentionRegion.StorageType, true),
                UseTable = false
            };
            
            // useTable (mandatory) may be a bool or an int
            var useTable = category.ReadProperty<bool?>(RetentionRegion.UseTable, true);
            if (!useTable.HasValue)
            {
                var intValue = category.ReadProperty<int?>(RetentionRegion.UseTable);
                useTable = intValue.HasValue && intValue !=0;
            }
            properties.UseTable = useTable.Value;
            
            if (properties.UseTable)
            {
                properties.NumberOfLevels = category.ReadProperty<int>(RetentionRegion.NumLevels);
                properties.Levels = category.ReadPropertiesToListOfType<double>(RetentionRegion.Levels).ToArray();
                properties.StorageAreas = category.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea).ToArray();
                properties.Interpolation = GetInterpolationType(category.ReadProperty<string>(RetentionRegion.Interpolate));
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
        
        /// <summary>
        /// A node is associated with a manhole if the manhole id is set. 
        /// </summary>
        /// <param name="category">The source data of the node</param>
        /// <returns>true iff the manhole id is set</returns>
        private static bool IsManHole(DelftIniCategory category)
        {
            var manhole = category.ReadProperty<string>(NodeFile.manholeId);
            return manhole != null;
        }
    }
}
