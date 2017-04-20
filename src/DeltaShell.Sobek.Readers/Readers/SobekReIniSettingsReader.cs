using DeltaShell.Sobek.Readers.SobekDataObjects;
using Nini.Config;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekReIniSettingsReader
    {
        static public SobekReIniSettings GetSobekReIniSettings(string path)
        {
            var sobekReIniSettings = new SobekReIniSettings();
            var source = new IniConfigSource(path);

            source.Alias.AddAlias("1", true);
            source.Alias.AddAlias("0", false);

            var general = source.Configs["General"];
            sobekReIniSettings.Salt = general.GetBoolean("Salt");
            return sobekReIniSettings;
        }
    }
}