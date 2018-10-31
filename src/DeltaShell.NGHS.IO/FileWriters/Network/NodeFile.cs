using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class NodeFile
    {
        public static void Write(string filePath, IEnumerable<Compartment> compartments)
        {
            var categories = compartments.Select(CreateCompartmentIniCategory).ToList();
            new DelftIniWriter().WriteDelftIniFile(categories, filePath, false);
        }

        private static DelftIniCategory CreateCompartmentIniCategory(Compartment compartment)
        {
            var iniCategory = new DelftIniCategory("Retention");
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Id, compartment.Name, string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Name, compartment.ParentManhole.Name, string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.NodeId, compartment.Name, string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.ManholeId, compartment.ParentManhole.Name, string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.UseTable, "0", string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.BedLevel, GetValueAsStringWithFormat(compartment.BottomLevel, "{0:0.000}"), string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.Area, GetValueAsStringWithFormat(compartment.ManholeLength * compartment.ManholeWidth, "{0:0.0000}"), string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.StreetLevel, GetValueAsStringWithFormat(compartment.SurfaceLevel, "{0:0.000}"), string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.StorageType, "Reservoir", string.Empty));
            iniCategory.AddProperty(new DelftIniProperty(KnownPropertyNames.StreetStorageArea, "100.00", string.Empty));
            return iniCategory;
        }

        private static string GetValueAsStringWithFormat(double value, string format)
        {
            return string.Format(CultureInfo.InvariantCulture, format, value);
        }

        public static List<CompartmentProperties> Read(string filePath)
        {
            var propertiesPerCompartment = new List<CompartmentProperties>();
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();
            foreach (var category in categories)
            {
                var compartmentProperties = new CompartmentProperties
                {
                    CompartmentId = category.GetPropertyValue(KnownPropertyNames.NodeId),
                    ManholeId = category.GetPropertyValue(KnownPropertyNames.ManholeId),
                    BottomLevel = GetPropertyValueAsDouble(KnownPropertyNames.BedLevel, category),
                    Area = GetPropertyValueAsDouble(KnownPropertyNames.Area, category),
                    StreetLevel = GetPropertyValueAsDouble(KnownPropertyNames.StreetLevel, category)
                };
                propertiesPerCompartment.Add(compartmentProperties);
            }

            return propertiesPerCompartment;
        }

        private static double GetPropertyValueAsDouble(string propertyName, IDelftIniCategory category)
        {
            return double.Parse(category.GetPropertyValue(propertyName), CultureInfo.InvariantCulture);
        }

        private static class KnownPropertyNames
        {
            public const string Id = "Id";
            public const string Name = "Name";
            public const string NodeId = "NodeId";
            public const string ManholeId = "ManholeId";
            public const string UseTable = "UseTable";
            public const string BedLevel = "BedLevel";
            public const string Area = "Area";
            public const string StreetLevel = "StreetLevel";
            public const string StorageType = "StorageType";
            public const string StreetStorageArea = "StreetStorageArea";
        }

        public class CompartmentProperties
        {
            public string CompartmentId;
            public string ManholeId;
            public double BottomLevel;
            public double StreetLevel;
            public double Area;
        }
    }
}
