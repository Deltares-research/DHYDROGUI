using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public static class FmCrossSectionDefinitionExporter
    {
        public static void Export(string filePath, CrossSectionDefinitionStandard[] crossSectionDefinitions)
        {
            File.Create(filePath);
        }
    }
}
