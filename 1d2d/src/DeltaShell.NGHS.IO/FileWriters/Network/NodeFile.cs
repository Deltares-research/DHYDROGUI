using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.IO.Ini;
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

            var iniSections = new List<IniSection>();
            if (compartments != null)
            {
                iniSections.AddRange(compartments.Select(CreateCompartmentIniSection).ToList());
            }

            if (retentions != null)
            {
                iniSections.AddRange(retentions.Select(GenerateRetentionStorageNode).ToList());
            }

            if (iniSections.Any())
            {
                var generalRegion = GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.RetentionMajorVersion,
                    GeneralRegion.RetentionMinorVersion,
                    GeneralRegion.FileTypeName.StorageNodes);
                generalRegion.AddPropertyWithOptionalComment("useStreetStorage", "1");
                iniSections.Insert(0,generalRegion);
                new IniWriter().WriteIniFile(iniSections, filePath, false);
            }
        }

        private static IniSection GenerateRetentionStorageNode(IRetention retention)
        {
            return RetentionFileWriter.GenerateSpatialDataDefinition(retention, RetentionRegion.StorageNodeHeader);
        }

        private static IniSection CreateCompartmentIniSection(ICompartment compartment)
        {
            var iniSection = new IniSection(RetentionRegion.StorageNodeHeader);
            iniSection.AddPropertyFromConfiguration(RetentionRegion.Id, compartment.Name);
            iniSection.AddPropertyFromConfiguration(RetentionRegion.Name, compartment.Name);
            iniSection.AddPropertyFromConfiguration(RetentionRegion.NodeId, compartment.Name);
            iniSection.AddPropertyWithOptionalComment(manholeId, compartment.ParentManhole.Name);
            

            iniSection.AddPropertyFromConfiguration(RetentionRegion.BedLevel, GetValueAsStringWithFormat(compartment.BottomLevel, "{0:0.000}"));
            iniSection.AddPropertyFromConfiguration(RetentionRegion.Area, GetValueAsStringWithFormat(compartment.ManholeLength * compartment.ManholeWidth, "{0:0.0000000}"));
            iniSection.AddPropertyFromConfiguration(RetentionRegion.StreetLevel, GetValueAsStringWithFormat(compartment.SurfaceLevel, "{0:0.000}"));
            iniSection.AddPropertyFromConfiguration(RetentionRegion.StorageType, compartment.CompartmentStorageType.GetDisplayName());
            iniSection.AddPropertyFromConfiguration(RetentionRegion.StreetStorageArea, GetValueAsStringWithFormat(compartment.FloodableArea, "{0:0.000}"));
            iniSection.AddPropertyWithOptionalComment(compartmentShape, compartment.Shape.ToString());
            iniSection.AddPropertyFromConfiguration(RetentionRegion.UseTable, compartment.UseTable);
            if (compartment.UseTable)
            {
                compartment.Storage.AddStorageTable(iniSection, compartment.Name);
            }
            return iniSection;
        }

        private static string GetValueAsStringWithFormat(double value, string format)
        {
            return string.Format(CultureInfo.InvariantCulture, format, value);
        }

        public static IList<CompartmentProperties> Read(string filePath)
        {
            var iniSections = new IniReader().ReadIniFile(filePath);

            return iniSections
                .Skip(1) // skip version info
                .Where(IsManHole)
                .Select(CreateCompartmentProperties)
                .ToList();
        }
        
        private static CompartmentProperties CreateCompartmentProperties(IniSection iniSection)
        {
            var properties = new CompartmentProperties
            {
                CompartmentId = iniSection.ReadProperty<string>(RetentionRegion.Id),
                Name = iniSection.ReadProperty<string>(RetentionRegion.Name),
                NodeId = iniSection.ReadProperty<string>(RetentionRegion.NodeId),
                ManholeId = iniSection.ReadProperty<string>(manholeId),
                BedLevel = iniSection.ReadProperty<double>(RetentionRegion.BedLevel),
                Area = iniSection.ReadProperty<double>(RetentionRegion.Area),
                StreetLevel = iniSection.ReadProperty<double>(RetentionRegion.StreetLevel),
                StreetStorageArea = iniSection.ReadProperty<double>(RetentionRegion.StreetStorageArea),
                CompartmentShape = iniSection.ReadProperty<CompartmentShape>(compartmentShape, true),
                CompartmentStorageType = iniSection.ReadProperty<CompartmentStorageType>(RetentionRegion.StorageType, true),
                UseTable = false
            };
            
            // useTable (mandatory) may be a bool or an int
            var useTable = iniSection.ReadProperty<bool?>(RetentionRegion.UseTable, true);
            if (!useTable.HasValue)
            {
                var intValue = iniSection.ReadProperty<int?>(RetentionRegion.UseTable);
                useTable = intValue.HasValue && intValue !=0;
            }
            properties.UseTable = useTable.Value;
            
            if (properties.UseTable)
            {
                properties.NumberOfLevels = iniSection.ReadProperty<int>(RetentionRegion.NumLevels);
                properties.Levels = iniSection.ReadPropertiesToListOfType<double>(RetentionRegion.Levels).ToArray();
                properties.StorageAreas = iniSection.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea).ToArray();
                properties.Interpolation = GetInterpolationType(iniSection.ReadProperty<string>(RetentionRegion.Interpolate));
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
        /// <param name="iniSection">The source data of the node</param>
        /// <returns>true iff the manhole id is set</returns>
        private static bool IsManHole(IniSection iniSection)
        {
            var manhole = iniSection.ReadProperty<string>(NodeFile.manholeId);
            return manhole != null;
        }
    }
}
