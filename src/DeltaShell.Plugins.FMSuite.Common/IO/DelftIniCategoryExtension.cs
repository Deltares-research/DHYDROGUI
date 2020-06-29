using System;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class DelftIniCategoryExtension
    {
        public static void AddSedimentProperty(this IDelftIniCategory category, string name, string value, string unit, string comment)
        {
            category.AddProperty(name, value, string.Format("{0,-10}{1}", string.IsNullOrEmpty(unit) ? string.Empty : "[" + unit + "]", comment));
        }
        public static bool ValidGeneralRegion(this IDelftIniCategory category, int majorVersionNr, int minorVersionNr, string fileType)
        {
            if (!category.Name.Equals(GeneralRegion.IniHeader, StringComparison.InvariantCultureIgnoreCase)) return false;

            var type = category.ReadProperty<string>(GeneralRegion.FileType.Key, true);
            if (type== default(string) || !type.Equals(fileType)) return false;

            var version = category.ReadProperty<string>(GeneralRegion.FileVersion.Key, true);
            if(version == default(string)) return false;
            if ( majorVersionNr < int.Parse(version.Split('.').First()) )
                return false;
            if( minorVersionNr < int.Parse(version.Split('.').Last()) )
                return false;
            return true;
        }
    }
}