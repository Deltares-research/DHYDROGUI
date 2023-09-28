using System;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class IniSectionExtensions
    {
        public static void AddSedimentProperty(this IniSection iniSection, string name, string value, string unit, string comment)
        {
            iniSection.AddPropertyWithOptionalComment(name, value, $"{(string.IsNullOrEmpty(unit) ? string.Empty : "[" + unit + "]"),-10}{comment}");
        }
        public static bool ValidGeneralRegion(this IniSection iniSection, int majorVersionNr, int minorVersionNr, string fileType)
        {
            if (!iniSection.Name.Equals(GeneralRegion.IniHeader, StringComparison.InvariantCultureIgnoreCase)) return false;

            var type = iniSection.ReadProperty<string>(GeneralRegion.FileType.Key, true);
            if (type== default(string) || !type.Equals(fileType)) return false;

            var version = iniSection.ReadProperty<string>(GeneralRegion.FileVersion.Key, true);
            if(version == default(string)) return false;
            if ( majorVersionNr < int.Parse(version.Split('.').First()) )
                return false;
            if( minorVersionNr < int.Parse(version.Split('.').Last()) )
                return false;
            return true;
        }
    }
}