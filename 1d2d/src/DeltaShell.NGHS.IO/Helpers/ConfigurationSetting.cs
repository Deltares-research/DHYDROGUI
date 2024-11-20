namespace DeltaShell.NGHS.IO.Helpers
{
    public class ConfigurationSetting
    {
        private const string DefaultStringFormat = "F3";

        public ConfigurationSetting(string key, string description) : this(key, null, description)
        {
        }

        public ConfigurationSetting(string key, string defaultValue = null, string description = null, string format = DefaultStringFormat)
        {
            Key = key;
            DefaultValue = defaultValue;
            Format = format;
            Description = description;
        }
        public string Key { get; private set; }
        public string DefaultValue { get; private set; }
        public string Description { get; private set; }
        public string Format { get; private set; }
    }
}