using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class DelftIniCategoryExtension
    {
        public static void AddSedimentProperty(this IDelftIniCategory category, string name, string value, string unit, string comment)
        {
            IEnumerable<string> namesList = new List<string>
            {
                SedimentFile.Name.Key,
                SedimentFile.SedConc,
                SedimentFile.SedThick
            };

            category.AddProperty(name, namesList.Contains(name) ? string.Format("#{0}#", value) : value, string.Format("{0,-10}{1}", string.IsNullOrEmpty(unit)? string.Empty : "[" + unit + "]", comment));
        }
    }
}