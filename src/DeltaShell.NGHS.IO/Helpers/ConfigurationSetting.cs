namespace DeltaShell.NGHS.IO.Helpers
{
    public class ConfigurationSetting
    {
        private const string DefaultStringFormat = "F3";

        public ConfigurationSetting(string key, string description = null, string format = DefaultStringFormat)
        {
            Key = key;
            Format = format;
            Description = description;
        }

        public string Key { get; }
        public string Description { get; }
        public string Format { get; }
    }
}