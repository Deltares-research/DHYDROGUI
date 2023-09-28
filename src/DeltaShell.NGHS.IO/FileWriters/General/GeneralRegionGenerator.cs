using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.General
{
    public abstract class GeneralRegionGenerator
    {
        // Used by IniWriter and by BcWriter
        public static IniSection GenerateGeneralRegion(int majorVersionNr, int minorVersionNr, string fileType)
        {
            var general = new IniSection(GeneralRegion.IniHeader);
            general.AddPropertyWithOptionalComment(GeneralRegion.FileVersion.Key, string.Format("{0}.{1:D2}", majorVersionNr, minorVersionNr), GeneralRegion.FileVersion.Description.Substring(1));
            general.AddPropertyWithOptionalComment(GeneralRegion.FileType.Key, fileType, GeneralRegion.FileType.Description.Substring(1));
            return general;
        }
    }
}