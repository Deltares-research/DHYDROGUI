using System;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class SedMorIniWriter : IniWriter
    {
        protected override void WriteProperty(IniProperty property, bool writeComment = false) // flag ignored, always write comment
        {
            if (string.IsNullOrEmpty(property.Value)) return;

            var line = $"    {property.Key,-22}= {property.Value,-22} {property.Comment}"; // slightly different format to IniWriter
            WriteLine(line);
        }
    }
}