using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class IniSectionExtensions
    {
        public static void AddSedimentProperty(this IniSection section, string key, string value, string unit, string comment)
        {
            var formattedComment = $"{(string.IsNullOrEmpty(unit) ? string.Empty : "[" + unit + "]"),-10}{comment}";

            var property = new IniProperty(key, value, formattedComment);

            section.AddProperty(property);
        }
    }
}