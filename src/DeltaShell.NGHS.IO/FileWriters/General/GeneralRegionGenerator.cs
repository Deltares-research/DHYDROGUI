using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.General
{
    public abstract class GeneralRegionGenerator
    {
        // Used by DelftIniWriter and by DelftBcWriter
        public static DelftIniCategory GenerateGeneralRegion(int majorVersionNr, int minorVersionNr, string fileType)
        {
            var general = new DelftIniCategory(GeneralRegion.IniHeader);
            general.AddProperty(GeneralRegion.FileVersion.Key, string.Format("{0}.{1:D2}", majorVersionNr, minorVersionNr), GeneralRegion.FileVersion.Description.Substring(1));
            general.AddProperty(GeneralRegion.FileType.Key, fileType, GeneralRegion.FileType.Description.Substring(1));
            return general;
        }
    }
}