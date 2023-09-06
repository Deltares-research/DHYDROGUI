using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniWriters
{
    public class SedMorDelftIniWriter : DelftIniWriter
    {
        protected override void WriteProperty(IniProperty property, bool writeComment = false) // flag ignored, always write comment
        {
            if (string.IsNullOrEmpty(property.Value))
            {
                return;
            }

            string line =
                string.Format("    {0,-22}= {1,-22} {2}", property.Key, property.Value,
                              property.Comment); // slightly different format to DelftIniWriter
            WriteLine(line);
        }
    }
}