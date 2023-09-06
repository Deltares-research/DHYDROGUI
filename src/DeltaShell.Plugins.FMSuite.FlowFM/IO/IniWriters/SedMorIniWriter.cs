using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.IniWriters
{
    public class SedMorIniWriter : IniWriter
    {
        protected override void WriteProperty(IniProperty property, bool writeComment = false) // flag ignored, always write comment
        {
            if (string.IsNullOrEmpty(property.Value))
            {
                return;
            }

            string line =
                string.Format("    {0,-22}= {1,-22} {2}", property.Key, property.Value,
                              property.Comment); // slightly different format to IniWriter
            WriteLine(line);
        }
    }
}